using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.Entity.Components.TileObject;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.AI
{
	public partial class AIComponentServer
	{
		List<FNEPoint> hidingPath;
		int2 m_FleeVectorFixed = new int2();

		private void Hiding()
		{
			if (m_Path != null)
				FollowPath();
			else
			{
				if (!ScanForEnemies())
					m_CurrentBehaviour = EnemyBehaviour.Chill;
				else
				{
					if (m_DistanceToTarget > m_AggroRange * 0.7f)
					{
						m_CurrentBehaviour = EnemyBehaviour.Chill;
						return;
					}

					m_FleeVector.x = (int)ParentEntity.Position.x - (int)m_TargetPlayer.Position.x;
					m_FleeVector.y = (int)ParentEntity.Position.y - (int)m_TargetPlayer.Position.y;

					if (m_FleeVector.x < 0) m_FleeVectorFixed.x = -1;
					else if (m_FleeVector.x > 0) m_FleeVectorFixed.x = 1;
					else m_FleeVectorFixed.x = 0;

					if (m_FleeVector.y < 0) m_FleeVectorFixed.y = -1;
					else if (m_FleeVector.y > 0) m_FleeVectorFixed.y = 1;
					else m_FleeVectorFixed.y = 0;

					if (m_DistanceToTarget > m_AggroRange / 4)
					{
						//if we are already in a hiding spot
						if (TileHasHittableTileObject((int2)ParentEntity.Position + m_FleeVectorFixed) ||
							TileHasHittableEdgeObjects((int2)ParentEntity.Position + m_FleeVectorFixed))
						{
							m_CurrentBehaviour = EnemyBehaviour.Chill;
							return;
						}
					}

					int2 tile;
					int2 neighborTile;
					bool foundHidingSpot = false;
					for (int i = 0; i < m_AggroRange; i++)
					{
						tile = (int2)ParentEntity.Position + (m_FleeVectorFixed * i);

						if (TileHasHittableTileObject(tile) || TileHasHittableEdgeObjects(tile))
						{
							if (m_FleeVectorFixed.x == 0 || m_FleeVectorFixed.y == 0)
							{
								if (CheckHidingSpotsStraight(tile, m_FleeVectorFixed.x == 0))
								{
									foundHidingSpot = true;
									break;
								}
							}
							else
							{
								if (CheckHidingSpotsDiagonal(tile))
								{
									foundHidingSpot = true;
									break;
								}
							}
						}

						for (int j = 1; j <= i; j++)
						{
							if (m_FleeVectorFixed.x == 0 || m_FleeVectorFixed.y == 0)
							{
								neighborTile = m_FleeVectorFixed.x == 0 ? new int2(tile.x - j, tile.y) : new int2(tile.x, tile.y - j);
								if (TileHasHittableTileObject(neighborTile) || TileHasHittableEdgeObjects(neighborTile))
								{
									if (CheckHidingSpotsStraight(neighborTile, m_FleeVectorFixed.x == 0))
									{
										foundHidingSpot = true;
										break;
									}
								}

								neighborTile = m_FleeVectorFixed.x == 0 ? new int2(tile.x + j, tile.y) : new int2(tile.x, tile.y + j);
								if (TileHasHittableTileObject(neighborTile) || TileHasHittableEdgeObjects(neighborTile))
								{
									if (CheckHidingSpotsStraight(neighborTile, m_FleeVectorFixed.x == 0))
									{
										foundHidingSpot = true;
										break;
									}
								}
							}
							else
							{
								neighborTile = new int2(tile.x - (m_FleeVectorFixed.x * j), tile.y);
								if (TileHasHittableTileObject(neighborTile))
								{
									if (CheckHidingSpotsDiagonal(neighborTile))
									{
										foundHidingSpot = true;
										break;
									}
								}

								neighborTile = new int2(tile.x, tile.y - (m_FleeVectorFixed.y * j));
								if (TileHasHittableTileObject(neighborTile))
								{
									if (CheckHidingSpotsDiagonal(neighborTile))
									{
										foundHidingSpot = true;
										break;
									}
								}
							}
						}

						if (foundHidingSpot) break;
					}

					if (!foundHidingSpot)
					{
						m_CurrentBehaviour = EnemyBehaviour.Fleeing;
					}
				}

			}
		}

		private bool CheckHidingSpotsDiagonal(int2 coverTile)
		{
			int2 hidingTile = coverTile + m_FleeVectorFixed;
			var hidingSpot1 = TileHasHittableTileObject(hidingTile);
			var hidingSpot2 = TileHasHittableTileObject(hidingTile.x - m_FleeVectorFixed.x, hidingTile.y);
			var hidingSpot3 = TileHasHittableTileObject(hidingTile.x, hidingTile.y - m_FleeVectorFixed.y);

			if (!hidingSpot1)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, (float2)hidingTile);
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}
			else if (!hidingSpot2)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, new float2(hidingTile.x - m_FleeVectorFixed.x, hidingTile.y));
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}
			else if (!hidingSpot3)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, new float2(hidingTile.x, hidingTile.y - m_FleeVectorFixed.y));
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}

			return false;
		}

		private bool CheckHidingSpotsStraight(int2 coverTile, bool vertical)
		{
			int2 hidingTile = coverTile + m_FleeVectorFixed;
			var hidingSpot1 = TileHasHittableTileObject(hidingTile);
			var hidingSpot2 = vertical ?
				TileHasHittableTileObject(hidingTile.x - 1, hidingTile.y) :
				TileHasHittableTileObject(hidingTile.x, hidingTile.y - 1);
			var hidingSpot3 = vertical ?
				TileHasHittableTileObject(hidingTile.x + 1, hidingTile.y) :
				TileHasHittableTileObject(hidingTile.x, hidingTile.y + 1);

			if (!hidingSpot1)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, (float2)hidingTile);
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}
			else if (!hidingSpot2)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, new float2(hidingTile.x - m_FleeVectorFixed.x, hidingTile.y));
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}
			else if (!hidingSpot3)
			{
				hidingPath = FNEPathfinding.FindPath(500, ParentEntity.Position, new float2(hidingTile.x, hidingTile.y - m_FleeVectorFixed.y));
				if (hidingPath.Count != 0)
				{
					SetNewPath(hidingPath);
					return true;
				}
			}

			return false;
		}

		private bool TileHasHittableTileObject(int2 tile)
		{
			var go = _world.GetTileObject(tile.x, tile.y);
			if (go != null)
			{
				if (go.GetComponent<TileObjectComponentServer>().hittable)
					return true;
			}

			return false;
		}

		private bool TileHasHittableTileObject(int tileX, int tileY)
		{
			var go = _world.GetTileObject(tileX, tileY);
			if (go != null)
			{
				if (go.GetComponent<TileObjectComponentServer>().hittable)
					return true;
			}

			return false;
		}

		private bool TileHasHittableEdgeObjects(int2 tile)
		{
			if (Mathf.Abs(m_FleeVector.x) >= Mathf.Abs(m_FleeVector.y))
			{
				if (m_FleeVector.x < 0 && _world.IsTileEastEdgeOccupied(tile))
				{
					var eastWall = _world.GetEdgeObject(tile.x + 1, tile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
					if (eastWall.GetComponent<EdgeObjectComponentServer>().IsHittable)
						return true;
				}
				else if (m_FleeVector.x > 0 && _world.IsTileWestEdgeOccupied(tile))
				{
					var westWall =_world.GetEdgeObject(tile.x, tile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
					if (westWall.GetComponent<EdgeObjectComponentServer>().IsHittable)
						return true;
				}
			}

			if (Mathf.Abs(m_FleeVector.y) >= Mathf.Abs(m_FleeVector.x))
			{
				if (m_FleeVector.y < 0 && _world.IsTileNorthEdgeOccupied(tile))
				{
					var northWall =_world.GetEdgeObject(tile.x, tile.y + 1, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
					if (northWall.GetComponent<EdgeObjectComponentServer>().IsHittable)
						return true;
				}
				else if (m_FleeVector.y > 0 && _world.IsTileSouthEdgeOccupied(tile))
				{
					var southWall = _world.GetEdgeObject(tile.x, tile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
					if (southWall.GetComponent<EdgeObjectComponentServer>().IsHittable)
						return true;
				}
			}

			return false;
		}

	}
}
