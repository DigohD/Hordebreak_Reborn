using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.Entity.Components.Excavatable;
using FNZ.Shared.Model.Entity.Components.Polygon;
using FNZ.Shared.Model.Entity.Components.TileObject;
using FNZ.Shared.Model.World;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Analytics;

namespace FNZ.Shared.Utils.CollisionUtils
{
	/*
     *  Notes of concern on transfer from FarNorthZ:
     *  
     *  Unsure if the northEdgeObject and eastEdgeObject entities are fetched correctly
     * 
     *  Vector2's have been converted to float2's, WHERE NECESSARY!. magnitude and normalize has been exchanged to math.-functions
     *  
     *  Biggest issue is the conversion from Tile to tile positions and data fetches from chunks. For example, where
     *  null checks for tiles were made prior, we now see if the chunk with a given tile position exists. This might not
     *  be what we want. Previously, Lists contained Tiles, whereas now they contain tile positions.
     *  
     */
	public class FNECollisionUtils
	{
		public struct LineCircleCollisionResult
		{
			public bool hit;
			public float hitDist;
			public Vector2 normal;
		}

		// Structure that stores the results of the PolygonCollision function
		public struct PolygonCollisionResult
		{
			// Are the polygons going to intersect forward in time?
			public bool WillIntersect;
			// Are the polygons currently intersecting?
			public bool Intersect;
			// The translation to apply to the first polygon to push the polygons apart.
			public Vector2 MinimumTranslationVector;
		}

		public struct PolygonCircleHitResult
		{
			public bool hit;
			public List<VerticeHitResult> verticeHitResults;
		}

		public struct VerticeHitResult
		{
			public float distanceToLine;
			public Vector2 normal;
			public bool leftOfVertice;
		}

		public struct PolyCircleHitResult
		{
			public bool hit;
			public Vector2 adjustVector;
		}

		private static Stack<List<int2>> raycastHitLists = new Stack<List<int2>>();
		private static List<int2> PopRayCastHitList()
		{
			if (raycastHitLists.Count == 0) return new List<int2>();
			else return raycastHitLists.Pop();
		}
		private static void RecycleRayCastHitList(List<int2> toRecycle)
		{
			toRecycle.Clear();
			raycastHitLists.Push(toRecycle);
		}

		public static FNERayCastHitStruct RayCastOnPolygons(Vector2 start, Vector2 end, List<FNEEntity> entitiesToCheck)
		{
			FNEEntity candidate = null;
			float candHitDist = 1000000;

			bool hit = false;

			float hitX = 0;
			float hitY = 0;

			foreach (FNEEntity entity in entitiesToCheck)
			{
				PolygonComponentShared comp = entity.GetComponent<PolygonComponentShared>();

				FNEPolygon polygon = entity.GetComponent<PolygonComponentShared>().GetWorldPolygon();

				// go through each of the vertices, plus
				// the next vertex in the list
				int next = 0;
				for (int current = 0; current < polygon.Points.Count; current++)
				{

					// get next vertex in list
					// if we've hit the end, wrap around to 0
					next = current + 1;
					if (next == polygon.Points.Count) next = 0;

					// get the PVectors at our current position
					// this makes our if statement a little cleaner
					Vector2 vc = polygon.Points[current];    // c for "current"
					Vector2 vn = polygon.Points[next];       // n for "next"

					if (get_line_intersection(start, end, vc, vn, out hitX, out hitY))
					{
						hit = true;
						float hitDist = (new Vector2(hitX, hitY) - start).magnitude;

						if (hitDist < candHitDist)
						{
							candidate = entity;
							candHitDist = hitDist;
						}
					}
				}
			}

			FNERayCastHitStruct ret;
			ret.IsHit = hit;
			ret.HitEntity = candidate;
			ret.HitLocation = start + ((end - start).normalized * candHitDist);

			return ret;
		}

		public static FNERayCastHitStruct RayCastForEnemiesInModel(float2 start, float2 end, GameWorld world)
		{
			List<int2> raycastHitList = PopRayCastHitList();

			bool hit = false;
			FNEEntity candidate = null;
			float candHitDist = 100000;

			FNERayCastHitStruct ret = default;

			GetBresenhamTilesFromLine3Wide((int)start.x, (int)start.y, (int)end.x, (int)end.y, world, raycastHitList);

			for (int i = raycastHitList.Count - 1; i >= 0; i--)
			{
				if (world.GetWorldChunk<WorldChunk>(raycastHitList[i]) == null)
				{
					raycastHitList.RemoveAt(i);
				}
			}

			LineCircleCollisionResult lineColRes = new LineCircleCollisionResult();
			bool shouldReturn = false;
			foreach (var tilePos in raycastHitList)
			{
				var tileEnemies = world.GetTileEnemies(tilePos);
				if (tileEnemies != null)
					foreach (var e in tileEnemies)
					{
						if (e.IsDead)
							continue;

						float radius = e.Data.GetComponentData<EnemyStatsComponentData>().hitboxRadius;
						//first check if our start point is within hitbox of target
						if (math.length(start - e.Position) <= radius)
						{
							ret.IsHit = true;
							ret.HitEntity = e;
							ret.HitLocation = start;
							shouldReturn = true;
							break;
						}

						LineCircleCollision(start, end, e.Position, radius, ref lineColRes);

						if (lineColRes.hit)
						{
							hit = true;

							if (lineColRes.hitDist < candHitDist)
							{
								candidate = e;
								candHitDist = lineColRes.hitDist;
							}
						}
					}

				if (shouldReturn) break;
			}

			if (shouldReturn)
			{
				RecycleRayCastHitList(raycastHitList);
				return ret;
			}

			RecycleRayCastHitList(raycastHitList);

			ret.IsHit = hit;
			ret.HitEntity = candidate;
			ret.HitLocation = start + (math.normalize(end - start) * candHitDist);
			return ret;
		}

		public static FNERayCastHitStruct RayCastForPlayersInModel(float2 start, float2 end, GameWorld world, long playerToIgnore)
		{
			List<int2> raycastHitList = PopRayCastHitList();

			bool hit = false;
			FNEEntity candidate = null;
			float candHitDist = 100000;

			FNERayCastHitStruct ret = default;

			GetBresenhamTilesFromLine3Wide((int)start.x, (int)start.y, (int)end.x, (int)end.y, world, raycastHitList);

			for (int i = raycastHitList.Count - 1; i >= 0; i--)
			{
				if (world.GetWorldChunk<WorldChunk>(raycastHitList[i]) == null)
				{
					raycastHitList.RemoveAt(i);
				}
			}

			LineCircleCollisionResult lineColRes = new LineCircleCollisionResult();
			bool shouldReturn = false;
			foreach (var tilePos in raycastHitList)
			{
				var tilePlayers = world.GetTilePlayers(tilePos);
				if (tilePlayers != null)
					foreach (var p in tilePlayers)
					{
						if (p.NetId == playerToIgnore) continue;

						float radius = DefaultValues.PLAYER_RADIUS;
						//first check if our start point is within hitbox of target
						if (math.length(start - p.Position) <= radius * 6)
						{
							ret.IsHit = true;
							ret.HitEntity = p;
							ret.HitLocation = start;
							shouldReturn = true;
							break;
						}

						LineCircleCollision(start, end, p.Position, radius * 6, ref lineColRes);

						if (lineColRes.hit)
						{
							hit = true;

							if (lineColRes.hitDist < candHitDist)
							{
								candidate = p;
								candHitDist = lineColRes.hitDist;
							}
						}
					}

				if (shouldReturn) break;
			}

			if (shouldReturn)
			{
				RecycleRayCastHitList(raycastHitList);
				return ret;
			}

			RecycleRayCastHitList(raycastHitList);

			ret.IsHit = hit;
			ret.HitEntity = candidate;
			ret.HitLocation = start + (math.normalize(end - start) * candHitDist);
			return ret;
		}

		public static List<FNERayCastHitStruct> RayCastAllEnemiesInModel(float2 start, float2 end, GameWorld world, float widthMultiplier = 1f)
		{
			List<int2> raycastHitList = PopRayCastHitList();

			List<FNERayCastHitStruct> hits = new List<FNERayCastHitStruct>();

			FNERayCastHitStruct ret;

			if (world.GetWorldChunk<WorldChunk>(start) != null)
			{
				GetBresenhamTilesFromLine3Wide((int)start.x, (int)start.y, (int)end.x, (int)end.y, world, raycastHitList);

				for (int i = raycastHitList.Count - 1; i >= 0; i--)
				{
					if (world.GetWorldChunk<WorldChunk>(raycastHitList[i]) == null)
					{
						raycastHitList.RemoveAt(i);
					}
				}
				bool shouldRecycle = false;
				foreach (var tilePos in raycastHitList)
				{
					var tileEnemies = world.GetTileEnemies(tilePos);
					if (tileEnemies != null)
						foreach (var e in tileEnemies)
						{
							float radius = e.Data.GetComponentData<EnemyStatsComponentData>().hitboxRadius * widthMultiplier;
							//first check if our start point is within hitbox of target
							if (math.length(start - e.Position) <= radius * 6)
							{
								ret.IsHit = true;
								ret.HitEntity = e;
								ret.HitLocation = start;
								hits.Add(ret);
								shouldRecycle = true;
							}

							LineCircleCollisionResult lineColRes = new LineCircleCollisionResult();
							LineCircleCollision(start, end, e.Position, radius * 6, ref lineColRes);

							if (lineColRes.hit)
							{
								hits.Add(new FNERayCastHitStruct(
									true,
									e.Position,
									e
								));
							}
						}

					if (shouldRecycle) break;
				}
			}

			RecycleRayCastHitList(raycastHitList);

			return hits;
		}

		public static FNERayCastHitStruct ExcavatorRayCastForWallsAndTileObjectsInModel(float2 start, float2 end, GameWorld world)
		{
			List<int2> raycastHitList = PopRayCastHitList();
			List<FNEEntity> entitiesToCheck = new List<FNEEntity>();

			if (world.GetWorldChunk<WorldChunk>(start) != null)
			{
				GetBresenhamTilesFromLine3Wide((int)start.x, (int)start.y, (int)end.x, (int)end.y, world, raycastHitList);

				for (int i = raycastHitList.Count - 1; i >= 0; i--)
				{
					if (world.GetWorldChunk<WorldChunk>(raycastHitList[i]) == null)
					{
						raycastHitList.RemoveAt(i);
					}
				}

				foreach (var tilePos in raycastHitList)
				{
					var tileEntity = world.GetTileObject(tilePos.x, tilePos.y);
					if (tileEntity != null && tileEntity.HasComponent<ExcavatableComponentShared>() && tileEntity.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(tileEntity);
					}

					var southEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.SOUTH);
					if (southEdgeObject != null && southEdgeObject.HasComponent<ExcavatableComponentShared>() && southEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(southEdgeObject);
					}

					var westEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
					if (westEdgeObject != null && westEdgeObject.HasComponent<ExcavatableComponentShared>() && westEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(westEdgeObject);
					}

					var northEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y + 1, GameWorld.EdgeObjectDirection.SOUTH);
					if (northEdgeObject != null && northEdgeObject.HasComponent<ExcavatableComponentShared>() && northEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(northEdgeObject);
					}

					var eastEdgeObject = world.GetEdgeObject(tilePos.x + 1, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
					if (eastEdgeObject != null && eastEdgeObject.HasComponent<ExcavatableComponentShared>() && eastEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(eastEdgeObject);
					}
				}
			}

			FNERayCastHitStruct hitStruct = RayCastOnPolygons(start, end, entitiesToCheck);

			RecycleRayCastHitList(raycastHitList);

			return hitStruct;
		}

		//change to FNERayCastHitStruct as return value if hit location becomes of interest
		public static FNERayCastHitStruct ProjectileRayCastForWallsAndTileObjectsInModel(float2 start, float2 end, GameWorld world)
		{
			List<int2> raycastHitList = PopRayCastHitList();
			// @TODO(Anders E): Find out why this is null, we must push a null list to the rayCastHitPool but i dont see how
			if (raycastHitList == null)
			{
				raycastHitList = new List<int2>();
			}
			
			List<FNEEntity> entitiesToCheck = new List<FNEEntity>();

			if (world.GetWorldChunk<WorldChunk>(start) != null)
			{
				GetBresenhamTilesFromLine3Wide((int)start.x, (int)start.y, (int)end.x, (int)end.y, world, raycastHitList);

				for (int i = raycastHitList.Count - 1; i >= 0; i--)
				{
					if (world.GetWorldChunk<WorldChunk>(raycastHitList[i]) == null)
					{
						raycastHitList.RemoveAt(i);
					}
				}

				foreach (var tilePos in raycastHitList)
				{
					var tileEntity = world.GetTileObject(tilePos.x, tilePos.y);
					if (tileEntity != null && tileEntity.GetComponent<TileObjectComponentShared>().hittable && tileEntity.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(tileEntity);
					}

					var southEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.SOUTH);
					if (southEdgeObject != null && southEdgeObject.GetComponent<EdgeObjectComponentShared>().IsHittable && southEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(southEdgeObject);
					}

					var westEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
					if (westEdgeObject != null && westEdgeObject.GetComponent<EdgeObjectComponentShared>().IsHittable && westEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(westEdgeObject);
					}

					var northEdgeObject = world.GetEdgeObject(tilePos.x, tilePos.y + 1, GameWorld.EdgeObjectDirection.SOUTH);
					if (northEdgeObject != null && northEdgeObject.GetComponent<EdgeObjectComponentShared>().IsHittable && northEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(northEdgeObject);
					}

					var eastEdgeObject = world.GetEdgeObject(tilePos.x + 1, tilePos.y, GameWorld.EdgeObjectDirection.WEST);
					if (eastEdgeObject != null && eastEdgeObject.GetComponent<EdgeObjectComponentShared>().IsHittable && eastEdgeObject.HasComponent<PolygonComponentShared>())
					{
						entitiesToCheck.Add(eastEdgeObject);
					}
				}
			}

			FNERayCastHitStruct hitStruct = RayCastOnPolygons(start, end, entitiesToCheck);

			RecycleRayCastHitList(raycastHitList);

			return hitStruct;
		}

		public static void LineCircleCollision(Vector2 start, Vector2 end, Vector2 circlePos,
			float circleRadius, ref LineCircleCollisionResult result)
		{
			// https://www.ludu.co/course/linjar-algebra/projektion-reflektion

			Vector2 u = circlePos - start;
			Vector2 v = end - start;

			Vector2 proj = (Vector2.Dot(u, v) / (v.magnitude * v.magnitude)) * v;

			Vector2 hitNormal = (u - proj);

			//pythagoras theorem to get hit location.

			float hitDist = proj.magnitude - Mathf.Sqrt(circleRadius * circleRadius - hitNormal.magnitude * hitNormal.magnitude);

			//check that hitDist is within raycast range AND (projection point is in front of raycast OR raycast start position is inside circle) AND hitLocation is within raycast range
			if ((v).magnitude >= hitDist && (Vector2.Dot(proj, v) > 0 || (start - circlePos).magnitude <= circleRadius) && hitNormal.magnitude <= circleRadius)
			{
				result.hit = true;
				result.hitDist = hitDist;
			}
			else
			{
				result.hit = false;
			}

			result.normal = hitNormal;
		}


		// Check if polygon A is going to collide with polygon B.
		// The last parameter is the *relative* velocity 
		// of the polygons (i.e. velocityA - velocityB)
		public static PolygonCollisionResult PolygonCollision(FNEPolygon polygonA,
									  FNEPolygon polygonB, Vector2 velocity)
		{
			PolygonCollisionResult result = new PolygonCollisionResult();
			result.Intersect = true;
			result.WillIntersect = true;

			int edgeCountA = polygonA.Edges.Count;
			int edgeCountB = polygonB.Edges.Count;
			float minIntervalDistance = float.PositiveInfinity;
			Vector2 translationAxis = new Vector2();
			Vector2 edge;

			// Loop through all the edges of both polygons
			for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
			{
				if (edgeIndex < edgeCountA)
				{
					edge = polygonA.Edges[edgeIndex];
				}
				else
				{
					edge = polygonB.Edges[edgeIndex - edgeCountA];
				}

				// ===== 1. Find if the polygons are currently intersecting =====

				// Find the axis perpendicular to the current edge
				Vector2 axis = new Vector2(-edge.y, edge.x);
				axis.Normalize();

				// Find the projection of the polygon on the current axis
				float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
				ProjectPolygon(axis, polygonA, ref minA, ref maxA);
				ProjectPolygon(axis, polygonB, ref minB, ref maxB);

				// Check if the polygon projections are currentlty intersecting
				if (IntervalDistance(minA, maxA, minB, maxB) > 0)
					result.Intersect = false;

				// ===== 2. Now find if the polygons *will* intersect =====

				// Project the velocity on the current axis
				float velocityProjection = Vector2.Dot(axis, velocity);

				// Get the projection of polygon A during the movement
				if (velocityProjection < 0)
				{
					minA += velocityProjection;
				}
				else
				{
					maxA += velocityProjection;
				}

				// Do the same test as above for the new projection
				float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
				if (intervalDistance > 0) result.WillIntersect = false;

				// If the polygons are not intersecting and won't intersect, exit the loop
				if (!result.Intersect && !result.WillIntersect) break;

				// Check if the current interval distance is the minimum one. If so store
				// the interval distance and the current distance.
				// This will be used to calculate the minimum translation vector
				intervalDistance = Mathf.Abs(intervalDistance);
				if (intervalDistance < minIntervalDistance)
				{
					minIntervalDistance = intervalDistance;
					translationAxis = axis;

					Vector2 d = polygonA.Center - polygonB.Center;
					if (Vector2.Dot(d, translationAxis) < 0)
						translationAxis = -translationAxis;
				}
			}

			// The minimum translation vector
			// can be used to push the polygons appart.
			if (result.WillIntersect)
				result.MinimumTranslationVector =
					   translationAxis * minIntervalDistance;

			return result;
		}

		// Calculate the distance between [minA, maxA] and [minB, maxB]
		// The distance will be negative if the intervals overlap
		private static float IntervalDistance(float minA, float maxA, float minB, float maxB)
		{
			if (minA < minB)
			{
				return minB - maxA;
			}
			else
			{
				return minA - maxB;
			}
		}

		// Calculate the projection of a polygon on an axis
		// and returns it as a [min, max] interval
		private static void ProjectPolygon(Vector2 axis, FNEPolygon polygon,
								   ref float min, ref float max)
		{
			// To project a point on an axis use the dot product
			float dotProduct = Vector2.Dot(axis, polygon.Points[0]);
			min = dotProduct;
			max = dotProduct;
			for (int i = 0; i < polygon.Points.Count; i++)
			{
				dotProduct = Vector2.Dot(polygon.Points[i], axis);
				if (dotProduct < min)
				{
					min = dotProduct;
				}
				else
				{
					if (dotProduct > max)
					{
						max = dotProduct;
					}
				}
			}
		}

		public static float2 RotateVector(float2 v, float degrees)
		{
			float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
			float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

			float tx = v.x;
			float ty = v.y;
			v.x = (cos * tx) - (sin * ty);
			v.y = (sin * tx) + (cos * ty);
			return v;
		}



		public static PolyCircleHitResult PolyCircleIntersects(FNEPolygon polygon, Vector2 circlePos, float r)
		{
			var res = new PolyCircleHitResult();
			res.adjustVector = new Vector2();

			Vector2 circleCenter = circlePos;
			float circleRadius = r;

			Vector2 closestPoint = Vector2.zero;
			float closestDistance = float.MaxValue;

			foreach (var point in polygon.Points)
			{
				float distance = Vector2.Distance(circleCenter, point);

				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestPoint = point;
				}
			}

			Vector2 previousPoint = polygon.Points[polygon.Points.Count - 1];

			foreach (Vector2 currentPoint in polygon.Points)
			{
				Vector2 edge = currentPoint - previousPoint;

				float dot = Vector2.Dot(edge, circleCenter - previousPoint);
				float interp = dot / Vector2.Dot(edge, edge);

				if (0.0f < interp && interp < 1.0f)
				{
					Vector2 point = previousPoint + edge * interp;
					float distance = Vector2.Distance(circleCenter, point);

					if (distance < closestDistance)
					{
						closestDistance = distance;
						closestPoint = point;
					}
				}

				previousPoint = currentPoint;
			}

			if (closestDistance < circleRadius)
			{
				float penetrationDistance = circleRadius - closestDistance;
				Vector2 normalizedVector = (closestPoint - circleCenter) / closestDistance;
				res.adjustVector = (normalizedVector * penetrationDistance);
			}

			res.hit = res.adjustVector != Vector2.zero;

			return res;
		}

		// http://www.jeffreythompson.org/collision-detection/poly-circle.php
		// http://www.jeffreythompson.org/collision-detection/license.php
		public static PolygonCircleHitResult PolygonCircleCollision(FNEPolygon polygon, Vector2 circlePos, float r, Vector2 relativeCircleVelocity)
		{
			PolygonCircleHitResult colRes = new PolygonCircleHitResult();
			colRes.verticeHitResults = new List<VerticeHitResult>();
			//colRes.hitDists = new List<Vector2>();
			//colRes.distancesToLine = new List<float>();

			LineCircleCollisionResult lineCol = new LineCircleCollisionResult();



			// go through each of the vertices, plus
			// the next vertex in the list
			int next = 0;
			for (int current = 0; current < polygon.Points.Count; current++)
			{

				// get next vertex in list
				// if we've hit the end, wrap around to 0
				next = current + 1;
				if (next == polygon.Points.Count) next = 0;

				// get the PVectors at our current position
				// this makes our if statement a little cleaner
				Vector2 vc = polygon.Points[current];    // c for "current"
				Vector2 vn = polygon.Points[next];       // n for "next"

				// check for collision between the circle and
				// a line formed between the two vertices
				LineCircleCollision(vc, vn, circlePos + relativeCircleVelocity, r, ref lineCol);

				if (lineCol.hit)
				{
					VerticeHitResult verticeHitRes = new VerticeHitResult();

					Vector2 v = vn - vc;

					Vector2 circlePosToVn = vn - circlePos;
					Vector2 proj = v.normalized * Vector2.Dot(circlePosToVn, v.normalized);

					float distanceToLine = Mathf.Sqrt(
						circlePosToVn.magnitude * circlePosToVn.magnitude
						- proj.magnitude * proj.magnitude);

					verticeHitRes.distanceToLine = distanceToLine;

					bool isLeftOfVertice = -lineCol.normal.x * v.y + lineCol.normal.y * v.x > 0;
					if (!isLeftOfVertice)
					{
						lineCol.normal *= -1;
					}

					colRes.hit = true;
					verticeHitRes.normal = lineCol.normal;
					verticeHitRes.leftOfVertice = isLeftOfVertice;

					colRes.verticeHitResults.Add(verticeHitRes);

				}
			}

			if (colRes.hit)
			{
				return colRes;
			}

			// the above algorithm only checks if the circle
			// is touching the edges of the polygon – in most
			// cases this is enough, but you can un-comment the
			// following code to also test if the center of the
			// circle is inside the polygon

			if (polygonPointCollision(polygon, circlePos)) return colRes;

			// otherwise, after all that, return false
			return colRes;
		}

		// POLYGON/POINT
		// only needed if you're going to check if the circle
		// is INSIDE the polygon
		private static bool polygonPointCollision(FNEPolygon polygon, Vector2 point)
		{
			bool collision = false;

			// go through each of the vertices, plus the next
			// vertex in the list
			int next = 0;
			for (int current = 0; current < polygon.Points.Count; current++)
			{

				// get next vertex in list
				// if we've hit the end, wrap around to 0
				next = current + 1;
				if (next == polygon.Points.Count) next = 0;

				// get the PVectors at our current position
				// this makes our if statement a little cleaner
				Vector2 vc = polygon.Points[current];    // c for "current"
				Vector2 vn = polygon.Points[next];       // n for "next"

				// compare position, flip 'collision' variable
				// back and forth
				if (((vc.y > point.y && vn.y < point.y) || (vc.y < point.y && vn.y > point.y)) &&
					 (point.x < (vn.x - vc.x) * (point.y - vc.y) / (vn.y - vc.y) + vc.x))
				{
					collision = !collision;
				}
			}
			return collision;
		}

		private static bool get_line_intersection(Vector2 line1start, Vector2 line1end,
		Vector2 line2start, Vector2 line2end, out float hit_x, out float hit_y)
		{
			return get_line_intersection(line1start.x, line1start.y, line1end.x, line1end.y,
				line2start.x, line2start.y, line2end.x, line2end.y, out hit_x, out hit_y);
		}

		public static bool get_line_intersection(float line1start_x, float line1start_y, float line1end_x, float line1end_y,
		float line2start_x, float line2start_y, float line2end_x, float line2end_y, out float hit_x, out float hit_y)
		{
			float s1_x, s1_y, s2_x, s2_y;
			s1_x = line1end_x - line1start_x; s1_y = line1end_y - line1start_y;
			s2_x = line2end_x - line2start_x; s2_y = line2end_y - line2start_y;

			float s, t;
			s = (-s1_y * (line1start_x - line2start_x) + s1_x * (line1start_y - line2start_y)) / (-s2_x * s1_y + s1_x * s2_y);
			t = (s2_x * (line1start_y - line2start_y) - s2_y * (line1start_x - line2start_x)) / (-s2_x * s1_y + s1_x * s2_y);

			if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
			{
				// Collision detected
				hit_x = line1start_x + (t * s1_x);
				hit_y = line1start_y + (t * s1_y);
				return true;
			}

			hit_x = 0;
			hit_y = 0;
			return false; // No collision
		}

		public static Vector2 RadianToVector2(float radian)
		{
			return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
		}

		public static Vector2 DegreeToVector2(float degree)
		{
			return RadianToVector2(degree * Mathf.Deg2Rad);
		}

		public static void GetBresenhamTilesFromLine3Wide(int x, int y, int x2, int y2, GameWorld world, List<int2> output)
		{
			GetBresenhamTilesFromLine(x, y, x2, y2, world, output);

			int tilesLength = output.Count;

			if (fastAbs(x - x2) > fastAbs(y - y2))
			{
				if (x < x2)
				{
					AddTileIfNotNull(world, new int2(x - 1, y - 1), output);
					AddTileIfNotNull(world, new int2(x - 1, y), output);
					AddTileIfNotNull(world, new int2(x - 1, y + 1), output);

					AddTileIfNotNull(world, new int2(x2 + 1, y2 - 1), output);
					AddTileIfNotNull(world, new int2(x2 + 1, y2), output);
					AddTileIfNotNull(world, new int2(x2 + 1, y2 + 1), output);
				}
				else
				{
					AddTileIfNotNull(world, new int2(x + 1, y - 1), output);
					AddTileIfNotNull(world, new int2(x + 1, y), output);
					AddTileIfNotNull(world, new int2(x + 1, y + 1), output);

					AddTileIfNotNull(world, new int2(x2 - 1, y2 - 1), output);
					AddTileIfNotNull(world, new int2(x2 - 1, y2), output);
					AddTileIfNotNull(world, new int2(x2 - 1, y2 + 1), output);
				}


				for (int i = 0; i < tilesLength; i++)
				{
					AddTileIfNotNull(world, new int2(output[i].x, output[i].y + 1), output);
					AddTileIfNotNull(world, new int2(output[i].x, output[i].y - 1), output);
				}
			}
			else
			{
				if (y < y2)
				{
					AddTileIfNotNull(world, new int2(x - 1, y - 1), output);
					AddTileIfNotNull(world, new int2(x, y - 1), output);
					AddTileIfNotNull(world, new int2(x + 1, y - 1), output);

					AddTileIfNotNull(world, new int2(x2 - 1, y2 + 1), output);
					AddTileIfNotNull(world, new int2(x2, y2 + 1), output);
					AddTileIfNotNull(world, new int2(x2 + 1, y2 + 1), output);
				}
				else
				{
					AddTileIfNotNull(world, new int2(x - 1, y + 1), output);
					AddTileIfNotNull(world, new int2(x, y + 1), output);
					AddTileIfNotNull(world, new int2(x + 1, y + 1), output);

					AddTileIfNotNull(world, new int2(x2 - 1, y2 - 1), output);
					AddTileIfNotNull(world, new int2(x2, y2 - 1), output);
					AddTileIfNotNull(world, new int2(x2 + 1, y2 - 1), output);
				}


				for (int i = 0; i < tilesLength; i++)
				{
					AddTileIfNotNull(world, new int2(output[i].x - 1, output[i].y), output);
					AddTileIfNotNull(world, new int2(output[i].x + 1, output[i].y), output);
				}
			}
		}

		private static void AddTileIfNotNull(GameWorld world, int2 tilePos, List<int2> output)
		{
			if (world.GetWorldChunk<WorldChunk>(tilePos) != null)
			{
				output.Add(tilePos);
			}
		}

		public static void GetBresenhamTilesFromLine(int x, int y, int x2, int y2, GameWorld world, List<int2> output)
		{
			int w = x2 - x;
			int h = y2 - y;
			int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
			if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
			if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
			if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
			int longest = fastAbs(w);
			int shortest = fastAbs(h);
			if (!(longest > shortest))
			{
				longest = fastAbs(h);
				shortest = fastAbs(w);
				if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
				dx2 = 0;
			}
			int numerator = longest >> 1;
			for (int i = 0; i <= longest; i++)
			{
				if (world.GetWorldChunk<WorldChunk>(new int2(x, y)) == null)
				{
					break;
				}
				output.Add(new int2(x, y));
				numerator += shortest;
				if (!(numerator < longest))
				{
					numerator -= longest;
					x += dx1;
					y += dy1;
				}
				else
				{
					x += dx2;
					y += dy2;
				}
			}
		}

		public static int fastAbs(int v)
		{
			return (v ^ (v >> 31)) - (v >> 31);
		}

		public static bool Intersects(FNECircle circle, FNERectangle rect)
		{
			Vector2 rectCenter = rect.GetCenterPoint();

			float circleDistanceX = Mathf.Abs(circle.Position.x - rectCenter.x);
			float circleDistanceY = Mathf.Abs(circle.Position.y - rectCenter.y);

			if (circleDistanceX > (rect.Width / 2.0f + circle.Radius)) { return false; }
			if (circleDistanceY > (rect.Height / 2.0f + circle.Radius)) { return false; }

			if (circleDistanceX <= (rect.Width / 2.0f)) { return true; }
			if (circleDistanceY <= (rect.Height / 2.0f)) { return true; }

			float cornerDistanceSQ = Mathf.Pow(circleDistanceX - rect.Width / 2.0f, 2)
				+ Mathf.Pow(circleDistanceY - rect.Height / 2.0f, 2);

			return (cornerDistanceSQ <= Mathf.Pow(circle.Radius, 2));
		}
	}

}