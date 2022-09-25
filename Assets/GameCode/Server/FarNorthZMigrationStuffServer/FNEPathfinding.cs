using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Model.World;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FNEPathfinding
	{
		public static bool HasLineOfSight(int2 tile1, int2 tile2)
		{
			if (tile1.x == tile2.x && tile1.y == tile2.y) return true;

			int deltaX = Mathf.Abs(tile2.x - tile1.x);
			int deltaY;

			int2 currentTile, goalTile;

			if (tile1.x <= tile2.x)
			{
				currentTile = tile1;
				goalTile = tile2;
				deltaY = tile2.y - tile1.y;
			}
			else
			{
				currentTile = tile2;
				goalTile = tile1;
				deltaY = tile1.y - tile2.y;
			}

			float kvot = Mathf.Abs((float)deltaX / (float)deltaY);
			float TO = 0;

			int tempSafeGuard = 0;
			while (tempSafeGuard < 1000)
			{
				if ((TO < 0 || deltaX == 0) && deltaY > 0)
				{
					var north = new int2(currentTile.x, currentTile.y + 1);
					var northEO = GameServer.World.GetEdgeObject(north.x, north.y, GameWorld.EdgeObjectDirection.SOUTH);

					// Traverse Up through World grid
					if ((northEO != null && !northEO.GetComponent<EdgeObjectComponentServer>().IsPassable) ||
						GameServer.World.GetTileObject(north.x, north.y) != null && !GameServer.World.GetTileObject(north.x, north.y).Data.seeThrough)
					{
						return false;
					}

					currentTile = north;
					TO += kvot;
				}
				else if ((TO < 0 || deltaX == 0) && deltaY < 0)
				{
					var south = new int2(currentTile.x, currentTile.y - 1);
					var southEO = GameServer.World.GetEdgeObject(currentTile.x, currentTile.y, GameWorld.EdgeObjectDirection.SOUTH);

					// Traverse Down through World grid
					if ((southEO != null && !southEO.GetComponent<EdgeObjectComponentServer>().IsPassable) ||
						GameServer.World.GetTileObject(south.x, south.y) != null && !GameServer.World.GetTileObject(south.x, south.y).Data.seeThrough)
					{
						return false;
					}

					currentTile = south;
					TO += kvot;
				}
				else
				{
					var east = new int2(currentTile.x + 1, currentTile.y);
					var eastEO = GameServer.World.GetEdgeObject(east.x, east.y, GameWorld.EdgeObjectDirection.WEST);

					// traverse Right through World grid
					if ((eastEO != null && !eastEO.GetComponent<EdgeObjectComponentServer>().IsPassable) ||
						GameServer.World.GetTileObject(east.x, east.y) != null && !GameServer.World.GetTileObject(east.x, east.y).Data.seeThrough)
					{
						return false;
					}

					currentTile = east;
					TO -= 1;
				}

				if (currentTile.x == goalTile.x && currentTile.y == goalTile.y) //currentTile == goalTile
				{
					return true;
				}

				tempSafeGuard++;
			}

			Debug.LogError("ERROR: Vicinity between tiles failed after 1000 iterations!");
			return false;
		}

		public static List<FNEPoint> FindPath(int visitedTilesBeforeSurrender, Vector2 startWorldPos, Vector2 targetWorldPos)
		{
			return FindPathImpl(visitedTilesBeforeSurrender, startWorldPos, targetWorldPos);
		}

		private static List<FNEPoint> FindPathImpl(int visitedTilesBeforeSurrender, Vector2 startWorldPos, Vector2 targetWorldPos)
		{
			int tileStartPosX = (int)(startWorldPos.x);
			int tileStartPosY = (int)(startWorldPos.y);

			int tileEndPosX = (int)(targetWorldPos.x);
			int tileEndPosY = (int)(targetWorldPos.y);

			if (visitedTilesBeforeSurrender != -1 && (startWorldPos - targetWorldPos).magnitude > visitedTilesBeforeSurrender)
			{
				return new List<FNEPoint>();
			}

			FF_Node startNode = new FF_Node(tileStartPosX, tileStartPosY);
			FF_Node targetNode = new FF_Node(tileEndPosX, tileEndPosY);

			startNode.pathDistanceCost = GetDistance(startNode, targetNode);
			startNode.gridCost = 0;

			FNEHeap<FF_Node> openSet;
			FNEHeap<FF_Node> closedSet;

			if (visitedTilesBeforeSurrender != -1)
			{
				openSet = new FNEHeap<FF_Node>((visitedTilesBeforeSurrender * 2) + 6);
				closedSet = new FNEHeap<FF_Node>((visitedTilesBeforeSurrender * 2) + 6);
			}
			else
			{
				throw new System.Exception("Pathfinding did not recieve a valid grid or visitedTilesBeforeSurrender.");
			}

			openSet.Add(startNode);

			int visitedCounter = 0;

			while (openSet.Count > 0)
			{
				if (visitedTilesBeforeSurrender != -1 && visitedCounter >= visitedTilesBeforeSurrender)
				{
					return new List<FNEPoint>();
				}

				visitedCounter++;

				FF_Node currentNode = openSet.RemoveFirst();

				closedSet.Add(currentNode);

				if (currentNode.Equals(targetNode))
				{
					return RetracePath(startNode, currentNode);
				}

				var currentTile = new int2(currentNode.gridX, currentNode.gridY);
				var neighbours = GameServer.World.GetTileStraightNeighbors(currentTile.x, currentTile.y);
				foreach (var neighbor in GameServer.World.GetTileDiagonalNeighbors(currentTile.x, currentTile.y))
					neighbours.Add(neighbor);

				foreach (var neighbourTile in neighbours)
				{
					var neighbourNode = new FF_Node(neighbourTile.x, neighbourTile.y);

					if (neighbourNode.Equals(targetNode) && GameServer.World.GetTileObject(neighbourTile.x, neighbourTile.y) != null &&
						!NeighbourTileHasBlockingEdgeObject(currentTile, neighbourTile))
					{
						return RetracePath(startNode, currentNode);
					}

					if (GameServer.World.GetTileObjectBlocking(neighbourTile.x, neighbourTile.y) || closedSet.Contains(neighbourNode) ||
						NeighbourTileHasBlockingEdgeObject(currentTile, neighbourTile))
					{
						continue;
					}

					float newMovementCostToNeighbour = currentNode.gridCost + GetDistance(currentNode, neighbourNode) + neighbourNode.gridCost;

					if (!openSet.Contains(neighbourNode))
					{
						openSet.Add(neighbourNode);
					}
					else if (newMovementCostToNeighbour >= neighbourNode.gridCost)
					{
						continue;
					}

					neighbourNode.gridCost = (byte)newMovementCostToNeighbour;
					neighbourNode.pathDistanceCost = GetDistance(neighbourNode, targetNode);
					neighbourNode.parent = currentNode;

					openSet.UpdateItem(neighbourNode);
				}
			}

			return new List<FNEPoint>();
		}

		private static List<FNEPoint> RetracePath(FF_Node startNode, FF_Node endNode)
		{
			List<FF_Node> path = new List<FF_Node>();
			FF_Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}

			path.Reverse();

			List<FNEPoint> result = new List<FNEPoint>();

			if (path != null)
			{
				foreach (FF_Node node in path)
				{
					result.Add(new FNEPoint(node.gridX, node.gridY));
				}
			}

			return result;
		}

		private static float GetDistance(FF_Node node1, FF_Node node2)
		{
			float dstX = Mathf.Abs(node1.gridX - node2.gridX);
			float dstY = Mathf.Abs(node1.gridY - node2.gridY);

			if (dstX > dstY)
				return 1.4f * dstY + (dstX - dstY);
			return 1.4f * dstX + (dstY - dstX);
		}

		private static bool NeighbourTileHasBlockingEdgeObject(int2 currentTile, int2 neighbourTile)
		{
			int2 south = new int2(currentTile.x, currentTile.y - 1);
			int2 west = new int2(currentTile.x - 1, currentTile.y);
			int2 north = new int2(currentTile.x, currentTile.y + 1);
			int2 east = new int2(currentTile.x + 1, currentTile.y);

			int2 southWest = new int2(currentTile.x - 1, currentTile.y - 1);
			int2 northWest = new int2(currentTile.x - 1, currentTile.y + 1);
			int2 northEast = new int2(currentTile.x + 1, currentTile.y + 1);
			int2 southEast = new int2(currentTile.x + 1, currentTile.y - 1);

			if (neighbourTile.x == west.x && neighbourTile.y == west.y) //neighborTile == West
			{
				if (GameServer.World.IsTileWestEdgeOccupied(currentTile)) return true;
			}
			else if (neighbourTile.x == northWest.x && neighbourTile.y == northWest.y) //neighborTile == NorthWest
			{
				if (GameServer.World.IsTileWestEdgeOccupied(north) || GameServer.World.IsTileSouthEdgeOccupied(north) ||
					GameServer.World.IsTileWestEdgeOccupied(currentTile) || GameServer.World.IsTileSouthEdgeOccupied(northWest)) return true;
			}
			else if (neighbourTile.x == north.x && neighbourTile.y == north.y) //neighborTile == North
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(north)) return true;
			}
			else if (neighbourTile.x == northEast.x && neighbourTile.y == northEast.y) //neighborTile == NorthEast
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(northEast) || GameServer.World.IsTileWestEdgeOccupied(northEast) ||
					GameServer.World.IsTileSouthEdgeOccupied(north) || GameServer.World.IsTileWestEdgeOccupied(east)) return true;
			}
			else if (neighbourTile.x == east.x && neighbourTile.y == east.y) //neighborTile == East
			{
				if (GameServer.World.IsTileWestEdgeOccupied(east)) return true;
			}
			else if (neighbourTile.x == southEast.x && neighbourTile.y == southEast.y) //neighborTile == SouthEast
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(east) || GameServer.World.IsTileWestEdgeOccupied(east) ||
					GameServer.World.IsTileWestEdgeOccupied(southEast) || GameServer.World.IsTileSouthEdgeOccupied(currentTile)) return true;
			}
			else if (neighbourTile.x == south.x && neighbourTile.y == south.y) //neightborTile == South
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(currentTile)) return true;
			}
			else if (neighbourTile.x == southWest.x && neighbourTile.y == southWest.y) //neighborTile == SouthWest
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(west) || GameServer.World.IsTileWestEdgeOccupied(south) ||
					GameServer.World.IsTileSouthEdgeOccupied(currentTile) || GameServer.World.IsTileWestEdgeOccupied(currentTile)) return true;
			}

			return false;
		}
	}

}