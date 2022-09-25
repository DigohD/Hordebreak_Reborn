using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.Model.Entity.Components.Polygon;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils.CollisionUtils;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{
	public class PlayerCollisionSystem
	{
		private PlayerController m_PlayerController;
		private FNEEntity m_Player;

		private bool ghostCheat;

		private List<List<Vector2>> allPolygonSlideVectors = new List<List<Vector2>>();
		private List<FNEPolygon> polygons = new List<FNEPolygon>();

		public PlayerCollisionSystem(PlayerController playerController)
		{
			m_PlayerController = playerController;
			m_Player = playerController.GetPlayerEntity();
			ghostCheat = false;
		}

		public void Update()
		{
			RunCheats();
			RunCollisionsCheck();
		}

		private void RunCheats()
		{
			if ((UnityEngine.Input.GetKey(KeyCode.LeftShift) && UnityEngine.Input.GetKeyDown(KeyCode.Comma)) || UnityEngine.Input.GetKeyDown(KeyCode.F9))
			{
				if (ghostCheat)
					ghostCheat = false;
				else
					ghostCheat = true;
			}
		}

		private void RunCollisionsCheck()
		{
			var playerMovement = m_PlayerController.GetPlayerMovementSystem();

			Vector2 position = m_Player.Position;
			Vector2 desiredVelocity = playerMovement.GetVelocity();

			if (ghostCheat)
			{
				playerMovement.Velocity = desiredVelocity;
				return;
			}

			float playerRadius = m_PlayerController.Radius;

			List<FNEPolygon> polygonsToCheck = GetSurroundingPolygons(position, 2, GameClient.World);
			List<FNECircle> circlesToCheck = GetSurroundingCircles(position, 2, GameClient.World);

			foreach (var circle in circlesToCheck)
			{
				Vector2 pos = circle.Position;
				float radius = circle.Radius;
				Vector2 between = (position - pos);
				if (between.magnitude <= (radius + playerRadius))
				{
					float dot = Vector2.Dot(desiredVelocity, between.normalized);
					Vector2 projected = between.normalized * dot;
					if (dot > 0) desiredVelocity = projected;
					else desiredVelocity = desiredVelocity - projected;
					break;
				}
			}

			playerMovement.Velocity = GetAdjustedVelocity(
				position,
				desiredVelocity,
				playerRadius,
				polygonsToCheck
			);
		}

		private List<FNECircle> GetSurroundingCircles(float2 position, int radiusInTiles, GameWorld world)
		{
			List<FNECircle> result = new List<FNECircle>();

			List<int2> tiles = world.GetSurroundingTilesInRadius(new int2((int)position.x, (int)position.y), radiusInTiles);

			foreach (var tilePos in tiles)
			{
				var tileEnemies = world.GetTileEnemies(tilePos);
				if (tileEnemies != null)
					foreach (var enemy in tileEnemies)
					{
						FNECircle c = new FNECircle(enemy.Position, enemy.Data.GetComponentData<EnemyStatsComponentData>().scale);
						result.Add(c);
					}
			}

			return result;
		}

		private List<FNEPolygon> GetSurroundingPolygons(float2 position, int radiusInTiles, GameWorld world)
		{
			polygons.Clear();

			List<int2> tiles = world.GetSurroundingTilesInRadius(new int2((int)position.x, (int)position.y), radiusInTiles);

			foreach (var tilePos in tiles)
			{
				var tileBlocked = world.GetBlockedTile(tilePos.x, tilePos.y);
				var tileEntity = world.GetTileObject(tilePos.x, tilePos.y);

				PolygonComponentClient polyComp = null;
				if (tileEntity != null && tileEntity.Data.blocking && (polyComp = tileEntity.GetComponent<PolygonComponentClient>()) != null)
				{
					FNEPolygon polygon = polyComp.GetWorldPolygon(Vector2.zero);
					polygons.Add(polygon);
				}else if (tileBlocked)
                {
					FNEPolygon polygon = new FNEPolygon();

					polygon.Points.Add(tilePos);
					polygon.Points.Add(new float2(tilePos.x, tilePos.y + 1));
					polygon.Points.Add(new float2(tilePos.x + 1, tilePos.y + 1));
					polygon.Points.Add(new float2(tilePos.x + 1, tilePos.y));

					polygon.BuildEdges();
					polygons.Add(polygon);
				}

				foreach (var edgeObj in world.GetStraightDirectionsEdgeObjects(tilePos))
				{
					if (edgeObj == null || !edgeObj.Data.blocking)
					{
						if (edgeObj != null && edgeObj.HasComponent<EdgeObjectComponentClient>())
						{
							if (edgeObj.GetComponent<EdgeObjectComponentClient>().IsPassable)
								continue;
						}
						else
							continue;
					}

					polyComp = edgeObj.GetComponent<PolygonComponentClient>();
					if (polyComp == null) continue;
					FNEPolygon polygon = polyComp.GetWorldPolygon(Vector2.zero);
					polygons.Add(polygon);
				}
			}

			return polygons;
		}

		private Vector2 GetAdjustedVelocity(Vector2 position, Vector2 desiredVelocity,
		   float radius, List<FNEPolygon> polygonsToCheck)
		{
			allPolygonSlideVectors.Clear();

			foreach (var polygon in polygonsToCheck)
			{
				var point0 = new float3(polygon.Points[0].x, polygon.Points[0].y, 0);
				var point1 = new float3(polygon.Points[1].x, polygon.Points[1].y, 0);
				var point2 = new float3(polygon.Points[2].x, polygon.Points[2].y, 0);
				var point3 = new float3(polygon.Points[3].x, polygon.Points[3].y, 0);

				Debug.DrawLine(point0, point1, Color.green, 0.0f, false);
				Debug.DrawLine(point1, point2, Color.green, 0.0f, false);
				Debug.DrawLine(point2, point3, Color.green, 0.0f, false);
				Debug.DrawLine(point3, point0, Color.green, 0.0f, false);

				var hitResult = FNECollisionUtils.PolygonCircleCollision(
						polygon,
						position,
						Mathf.Sqrt(radius * radius * 2),
						desiredVelocity
				);

				List<Vector2> polygonSlideVectors = new List<Vector2>();

				if (hitResult.hit)
				{
					foreach (var verticeHitResult in hitResult.verticeHitResults)
					{
						float dot = Vector2.Dot(desiredVelocity, verticeHitResult.normal.normalized);
						Vector2 projected = verticeHitResult.normal.normalized * dot;
						if (verticeHitResult.leftOfVertice && dot > 0)
						{
							continue;
						}
						else if (dot > 0 || verticeHitResult.distanceToLine < radius)
						{
							polygonSlideVectors.Add(projected);
						}
						else
						{
							polygonSlideVectors.Add(desiredVelocity - projected);
						}
					}

					if (polygonSlideVectors.Count > 0)
						allPolygonSlideVectors.Add(polygonSlideVectors);
				}
			}

			if (allPolygonSlideVectors.Count == 0)
			{
				return desiredVelocity;
			}

			List<Vector2> firstPolyVecs = allPolygonSlideVectors[0];
			allPolygonSlideVectors.RemoveAt(0);

			foreach (var firstPolyVec in firstPolyVecs)
			{
				bool polyMatchFound = true;
				foreach (var polyVecs in allPolygonSlideVectors)
				{
					bool found = false;
					foreach (var polySlideVec in polyVecs)
					{
						if (firstPolyVec == polySlideVec)
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						polyMatchFound = false;
						break;
					}
				}

				if (polyMatchFound)
				{
					return firstPolyVec;
				}
			}

			return Vector3.zero;
		}

		private Vector2 GetSlidingVector(Vector2 desiredVelocity, Vector2 normal, bool isLeftOfVertice, float distToVertice, float playerRadius)
		{
			float dot = Vector2.Dot(desiredVelocity, normal);
			Vector2 projected = normal * dot;
			if (isLeftOfVertice && dot > 0)
			{
				return Vector2.zero;
			}
			else if (dot > 0 || distToVertice < playerRadius)
			{
				return projected;
			}
			else
			{
				return (desiredVelocity - projected);
			}
		}
	}
}