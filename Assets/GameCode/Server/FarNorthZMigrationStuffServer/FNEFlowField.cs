using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.World;
using FNZ.Shared.Constants;
using FNZ.Shared.Model.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class FNEFlowField
	{
		public FF_Node[,] graph;
		public VectorFieldNode[,] vectorField;

		public Vector2 sourcePosition;

		public int worldStartX;
		public int worldStartY;
		public int gridSizeX;
		public int gridSizeY;
		public int gridSize;

		private int radius;

		public FNEFlowField(Vector2 sourcePosition, int radius)
		{
			this.sourcePosition = sourcePosition;

			int tileWorldX = (int)sourcePosition.x;
			int tileWorldY = (int)sourcePosition.y;

			this.worldStartX = tileWorldX - radius;
			this.worldStartY = tileWorldY - radius;

			if (worldStartX < 0) worldStartX = 0;
			if (worldStartY < 0) worldStartY = 0;

			this.gridSizeX = radius * 2;
			this.gridSizeY = radius * 2;

			if (worldStartX + gridSizeX > GameServer.World.WIDTH) gridSizeX = GameServer.World.WIDTH - worldStartX;
			if (worldStartY + gridSizeY > GameServer.World.HEIGHT) gridSizeY = GameServer.World.HEIGHT - worldStartY;

			gridSize = gridSizeX * gridSizeY;

			this.radius = radius;

			RegenerateFlowField();
		}

		public void RegenerateFlowField()
		{
			vectorField = new VectorFieldNode[gridSizeX, gridSizeY];
			graph = new FF_Node[gridSizeX, gridSizeY];

			GenerateDistanceField(GameServer.World, (int)sourcePosition.x, (int)sourcePosition.y);
			GenerateVectorField(this.sourcePosition);
		}

		public FNEFlowField(ServerWorld world, Vector2 startWorldPos, Vector2 targetWorldPos)
		{
			this.sourcePosition = targetWorldPos;

			DetermineFlowFieldGridBoundary(startWorldPos, targetWorldPos);

			gridSize = gridSizeX * gridSizeY;

			vectorField = new VectorFieldNode[gridSizeX, gridSizeY];
			graph = new FF_Node[gridSizeX, gridSizeY];

			GenerateDistanceField(world, worldStartX + radius, worldStartY + radius);
			GenerateVectorField(targetWorldPos);
		}

		public VectorFieldNode GetVectorFieldDirection(int tileWorldX, int tileWorldY)
		{
			int tileX = tileWorldX - worldStartX;
			int tileY = tileWorldY - worldStartY;

			if (tileX < 0 || tileY < 0 || tileX >= gridSizeX || tileY >= gridSizeY)
				return new VectorFieldNode();

			return vectorField[tileX, tileY];
		}

		private void DetermineFlowFieldGridBoundary(Vector2 startWorldPos, Vector2 targetWorldPos)
		{
			Vector2 newStart = startWorldPos + (startWorldPos - targetWorldPos).normalized * 10.0f;
			Vector2 newTarget = targetWorldPos + (targetWorldPos - startWorldPos).normalized * 10.0f;
			Vector2 between = newTarget - newStart;
			Vector2 midPoint = newStart + between.normalized * (between.magnitude / 2.0f);

			radius = (int)(midPoint - newStart).magnitude;

			gridSizeX = radius * 2;
			gridSizeY = radius * 2;

			worldStartX = (int)(midPoint.x) - radius;
			worldStartY = (int)(midPoint.y) - radius;
		}

		private void GenerateDistanceField(ServerWorld world, int tileWorldX, int tileWorldY)
		{
			FNEHeap<FF_Node> openSet = new FNEHeap<FF_Node>(gridSizeX * gridSizeY);

			for (int y = worldStartY; y < worldStartY + gridSizeY; y++)
			{
				for (int x = worldStartX; x < worldStartX + gridSizeX; x++)
				{
					int gridX = x - worldStartX;
					int gridY = y - worldStartY;

					graph[gridX, gridY] = new FF_Node(x, y)
					{
						gridCost = 1, //GameServer.World.GetTileBlocking(tileWorldX, tileWorldY) ? (byte)255 : (byte)1, //This (for some reason) sometimes return true on an empty tile. Commented for now.
						pathDistanceCost = 65535,
						parent = null
					};

					openSet.Add(graph[gridX, gridY]);
				}
			}

			int goalX = (int)(sourcePosition.x) - worldStartX;
			int goalY = (int)(sourcePosition.y) - worldStartY;

			FF_Node startNode = graph[goalX, goalY];
			if (startNode == null) return;
			startNode.pathDistanceCost = 0;
			openSet.UpdateItem(startNode);

			while (openSet.Count > 0)
			{
				FF_Node currentNode = openSet.RemoveFirst();

				int2 currentTile = new int2(currentNode.gridX, currentNode.gridY);
				var currentTileObject = GameServer.World.GetTileObject(currentTile.x, currentTile.y);

				foreach (var neighbour in world.GetTileStraightNeighbors(currentNode.gridX, currentNode.gridY))
				{
					if (!FFContainsTile(neighbour))
					{
						continue;
					}

					int tileX = neighbour.x - worldStartX;
					int tileY = neighbour.y - worldStartY;

					FF_Node neighbourNode = graph[tileX, tileY];

					//if (neighbourNode.gridCost == 255)
					//{
					//    continue;
					//}

					byte wallCost = GetCostBetweenTiles(currentTile, neighbour);

					short tileObjectCost = GetCostOfTileObject(neighbour);

					float newDistance = currentNode.totalCost + GetDistance(currentNode, neighbourNode) + wallCost + tileObjectCost;

					if (newDistance >= 255)
					{
						continue;
					}

					if (newDistance < neighbourNode.pathDistanceCost)
					{
						neighbourNode.pathDistanceCost = newDistance;
						neighbourNode.parent = currentNode;

						if (wallCost > 0)
						{
							neighbourNode.entityToAttackToNextNode = GetEdgeObjBetweenTiles(currentTile, neighbour);
						}
						else if (currentTileObject != null && currentTileObject.Data.blocking
							&& !currentTileObject.Data.smallCollisionBox)
						{
							neighbourNode.entityToAttackToNextNode = currentTileObject;
						}
						else
						{
							neighbourNode.entityToAttackToNextNode = null;
						}

						openSet.UpdateItem(neighbourNode);
					}
				}
			}
		}

		private byte GetCostBetweenTiles(int2 currentTile, int2 neighbourTile)
		{
			int2 south = new int2(currentTile.x, currentTile.y - 1);
			int2 west = new int2(currentTile.x - 1, currentTile.y);
			int2 north = new int2(currentTile.x, currentTile.y + 1);
			int2 east = new int2(currentTile.x + 1, currentTile.y);

			if (neighbourTile.x == west.x && neighbourTile.y == west.y) //neighborTile == West
			{
				if (GameServer.World.IsTileWestEdgeOccupied(currentTile))
				{
					var wall = GameServer.World.GetEdgeObject(currentTile.x, currentTile.y, 
						Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
					
					return wall.GetComponent<EdgeObjectComponentServer>().IsPassable ? (byte) 0 : wall.Data.pathingCost;
				}
			}
			else if (neighbourTile.x == north.x && neighbourTile.y == north.y) //neighborTile == North
			{
				if (GameServer.World.IsTileNorthEdgeOccupied(currentTile))
				{
					var wall = GameServer.World.GetEdgeObject(north.x, north.y,
						Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
					
					return wall.GetComponent<EdgeObjectComponentServer>().IsPassable ? (byte) 0 : wall.Data.pathingCost;
				}
			}
			else if (neighbourTile.x == east.x && neighbourTile.y == east.y) //neighborTile == East
			{
				if (GameServer.World.IsTileEastEdgeOccupied(currentTile))
				{
					var wall = GameServer.World.GetEdgeObject(east.x, east.y,
						Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
					
					return wall.GetComponent<EdgeObjectComponentServer>().IsPassable ? (byte) 0 : wall.Data.pathingCost;
				}
			}
			else if (neighbourTile.x == south.x && neighbourTile.y == south.y) //neighborTile == South
			{
				if (GameServer.World.IsTileSouthEdgeOccupied(currentTile))
				{
					var wall = GameServer.World.GetEdgeObject(currentTile.x, currentTile.y,
						Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
					
					return wall.GetComponent<EdgeObjectComponentServer>().IsPassable ? (byte) 0 : wall.Data.pathingCost;
				}
			}

			return 0;
		}

		private byte GetCostOfTileObject(int2 neighbour)
		{
			var tileObject = GameServer.World.GetTileObject(neighbour.x, neighbour.y);
			var isBlocking = GameServer.World.IsTileBlocking(neighbour.x, neighbour.y);
			if (isBlocking != null && isBlocking.Value)
			{
				return 255;
			}

			if (tileObject != null && !tileObject.Data.smallCollisionBox)
			{
				return tileObject.Data.pathingCost;
			}

			return 0;
		}

		private FNEEntity GetEdgeObjBetweenTiles(int2 currentTile, int2 neighbourTile)
		{
			int2 south = new int2(currentTile.x, currentTile.y - 1);
			int2 west = new int2(currentTile.x - 1, currentTile.y);
			int2 north = new int2(currentTile.x, currentTile.y + 1);
			int2 east = new int2(currentTile.x + 1, currentTile.y);

			if (neighbourTile.x == west.x && neighbourTile.y == west.y) //neighborTile == West
			{
				return GameServer.World.GetEdgeObject(currentTile.x, currentTile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
			}
			else if (neighbourTile.x == north.x && neighbourTile.y == north.y) //neighborTile == North
			{
				return GameServer.World.GetEdgeObject(north.x, north.y, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
			}
			else if (neighbourTile.x == east.x && neighbourTile.y == east.y) //neighborTile == East
			{
				return GameServer.World.GetEdgeObject(east.x, east.y, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST);
			}
			else if (neighbourTile.x == south.x && neighbourTile.y == south.y) //neighborTile == South
			{
				return GameServer.World.GetEdgeObject(currentTile.x, currentTile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH);
			}

			return null;
		}

		private void GenerateVectorField(Vector2 goal)
		{
			int tileX = (int)goal.x - worldStartX;
			int tileY = (int)goal.y - worldStartY;

			if (tileX < 0 || tileY < 0 || tileX >= gridSizeX || tileY >= gridSizeY) return;

			FF_Node goalNode = graph[tileX, tileY];

			for (int y = 0; y < gridSizeY; y++)
			{
				for (int x = 0; x < gridSizeX; x++)
				{
					FF_Node node = graph[x, y];

					if (node == null)
					{
						Debug.Log("NULL");
					}

					if (node.gridCost == 255) continue;

					VectorFieldNode goalDirection = new VectorFieldNode();

					if (node == goalNode || node.pathDistanceCost == 65535)
					{
						vectorField[x, y] = goalDirection;
						continue;
					}

					goalDirection.vector = CalculateDirectionToGoal(node);
					goalDirection.breakWall = node.entityToAttackToNextNode;
					vectorField[x, y] = goalDirection;
				}
			}
		}

		private Vector2 CalculateDirectionToGoal(FF_Node node)
		{
			var currentTile = new int2(node.gridX, node.gridY);

			var edgeObjects = GameServer.World.GetStraightDirectionsEdgeObjects(currentTile);
			var tileObjects = GameServer.World.GetStraightNeighborTileObjects(currentTile);
			
			int2 south = new int2(currentTile.x, currentTile.y - 1);
			int2 west = new int2(currentTile.x - 1, currentTile.y);
			int2 north = new int2(currentTile.x, currentTile.y + 1);
			int2 east = new int2(currentTile.x + 1, currentTile.y);

			// Check if we have a wall to attack
			if (node.entityToAttackToNextNode != null)
			{
				if (node.entityToAttackToNextNode == edgeObjects[TileCardinalDirectionConstants.WEST] || (node.entityToAttackToNextNode == tileObjects[TileCardinalDirectionConstants.WEST]))
				{
					return new Vector2(-1, 0);
				}
				else if (node.entityToAttackToNextNode == edgeObjects[TileCardinalDirectionConstants.NORTH] || (node.entityToAttackToNextNode == tileObjects[TileCardinalDirectionConstants.NORTH]))
				{
					return new Vector2(0, 1);
				}
				else if (node.entityToAttackToNextNode == edgeObjects[TileCardinalDirectionConstants.EAST] || (node.entityToAttackToNextNode == tileObjects[TileCardinalDirectionConstants.EAST]))
				{
					return new Vector2(1, 0);
				}
				else if (node.entityToAttackToNextNode == edgeObjects[TileCardinalDirectionConstants.SOUTH] || (node.entityToAttackToNextNode == tileObjects[TileCardinalDirectionConstants.SOUTH]))
				{
					return new Vector2(0, -1);
				}
			}

			float westCost = node.totalCost;
			float eastCost = node.totalCost;
			float northCost = node.totalCost;
			float southCost = node.totalCost;

			if (FFContainsTile(west)) westCost = GetNode(west).totalCost;
			if (FFContainsTile(east)) eastCost = GetNode(east).totalCost + 0.0001f;
			if (FFContainsTile(north)) northCost = GetNode(north).totalCost + 0.0001f;
			if (FFContainsTile(south)) southCost = GetNode(south).totalCost;

			if (ShouldBlockVector(west, edgeObjects[TileCardinalDirectionConstants.WEST])) westCost = node.totalCost;
			if (ShouldBlockVector(east, edgeObjects[TileCardinalDirectionConstants.EAST])) eastCost = node.totalCost;
			if (ShouldBlockVector(north, edgeObjects[TileCardinalDirectionConstants.NORTH])) northCost = node.totalCost;
			if (ShouldBlockVector(south, edgeObjects[TileCardinalDirectionConstants.SOUTH])) southCost = node.totalCost;

			Vector2 goalDirection = new Vector2(westCost - eastCost, southCost - northCost);

			if (goalDirection.y > 0 && (ShouldBlockVector(north, edgeObjects[TileCardinalDirectionConstants.NORTH])))
			{
				goalDirection.y = 0;
			}
			else if (goalDirection.y < 0 && (ShouldBlockVector(south, edgeObjects[TileCardinalDirectionConstants.SOUTH])))
			{
				goalDirection.y = 0;
			}
			else if (goalDirection.x > 0 && (ShouldBlockVector(east, edgeObjects[TileCardinalDirectionConstants.EAST])))
			{
				goalDirection.x = 0;
			}
			else if (goalDirection.x < 0 && (ShouldBlockVector(west, edgeObjects[TileCardinalDirectionConstants.WEST])))
			{
				goalDirection.x = 0;
			}
			else if (goalDirection.x > 0 && goalDirection.y > 0 && //Heading northeast
				(GameServer.World.GetTileObjectBlocking(currentTile.x + 1, currentTile.y + 1) ||
				GameServer.World.GetEdgeObject(currentTile.x + 1, currentTile.y + 1, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH) != null ||
				GameServer.World.GetEdgeObject(currentTile.x + 1, currentTile.y + 1, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST) != null
				))
			{
				if (northCost > eastCost)
				{
					goalDirection.y = 0;
				}
				else
				{
					goalDirection.x = 0;
				}
			}
			else if (goalDirection.x < 0 && goalDirection.y > 0 && //Heading northwest
				(GameServer.World.GetTileObjectBlocking(currentTile.x - 1, currentTile.y + 1) ||
				GameServer.World.GetEdgeObject(currentTile.x - 1, currentTile.y + 1, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH) != null ||
				GameServer.World.GetEdgeObject(currentTile.x, currentTile.y + 1, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST) != null
				))
			{
				if (westCost > northCost)
				{
					goalDirection.x = 0;
				}
				else
				{
					goalDirection.y = 0;
				}
			}
			else if (goalDirection.x < 0 && goalDirection.y < 0 && //Heading southwest
				(GameServer.World.GetTileObjectBlocking(currentTile.x - 1, currentTile.y - 1) ||
				GameServer.World.GetEdgeObject(currentTile.x - 1, currentTile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH) != null ||
				GameServer.World.GetEdgeObject(currentTile.x, currentTile.y - 1, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST) != null
				))
			{
				if (westCost > southCost)
				{
					goalDirection.x = 0;
				}
				else
				{
					goalDirection.y = 0;
				}
			}
			else if (goalDirection.x > 0 && goalDirection.y < 0 && //Heading southeast
				(GameServer.World.GetTileObjectBlocking(currentTile.x + 1, currentTile.y - 1) ||
				GameServer.World.GetEdgeObject(currentTile.x + 1, currentTile.y - 1, Shared.Model.World.GameWorld.EdgeObjectDirection.WEST) != null ||
				GameServer.World.GetEdgeObject(currentTile.x + 1, currentTile.y, Shared.Model.World.GameWorld.EdgeObjectDirection.SOUTH) != null
				))
			{
				if (eastCost > southCost)
				{
					goalDirection.x = 0;
				}
				else
				{
					goalDirection.y = 0;
				}
			}

			//if (goalDirection.magnitude == 0 && (
			//    currentTile.GetNorthNeighbour() == null 
			//    || currentTile.GetNorthNeighbour().blocking 
			//    || currentTile.GetSouthNeighbour() == null
			//    || currentTile.GetSouthNeighbour().blocking 
			//    || FNEUtil.NeighbourTileHasBlockingEdgeObject(currentTile, currentTile.GetNorthNeighbour()) 
			//    || FNEUtil.NeighbourTileHasBlockingEdgeObject(currentTile, currentTile.GetSouthNeighbour())))
			//{
			//    goalDirection.x = 1;
			//}
			//else if (goalDirection.magnitude == 0 && (
			//    currentTile.GetWestNeighbour() == null
			//    || currentTile.GetWestNeighbour().blocking 
			//    || currentTile.GetEastNeighbour() == null
			//    || currentTile.GetEastNeighbour().blocking 
			//    || FNEUtil.NeighbourTileHasBlockingEdgeObject(currentTile, currentTile.GetWestNeighbour()) 
			//    || FNEUtil.NeighbourTileHasBlockingEdgeObject(currentTile, currentTile.GetEastNeighbour())))
			//{
			//    goalDirection.y = 1;
			//}

			return goalDirection.normalized;
		}

		private bool ShouldBlockVector(int2 tile, FNEEntity edgeObject)
		{
			var tileObject = GameServer.World.GetTileObject(tile.x, tile.y);
			var isTileBlocking = GameServer.World.IsTileBlocking(tile.x, tile.y).GetValueOrDefault();
			return (isTileBlocking || (tileObject != null && !tileObject.Data.smallCollisionBox && tileObject.Data.pathingCost > 0) || (edgeObject != null && !edgeObject.GetComponent<EdgeObjectComponentServer>().IsPassable));
		}

		private FF_Node GetNode(int2 tile)
		{
			int tileX = tile.x - worldStartX;
			int tileY = tile.y - worldStartY;

			return graph[tileX, tileY];
		}

		private float GetDistance(FF_Node node1, FF_Node node2)
		{
			float dstX = Mathf.Abs(node1.gridX - node2.gridX);
			float dstY = Mathf.Abs(node1.gridY - node2.gridY);

			if (dstX > dstY)
				return 1.4f * dstY + (dstX - dstY);
			return 1.4f * dstX + (dstY - dstX);
		}

		private bool FFContainsTile(int2 tile)
		{
			Vector2 gridPos = new Vector2(worldStartX, worldStartY);

			Vector2 tilePosInGrid = new Vector2(tile.x, tile.y) - gridPos;

			if (tilePosInGrid.x < 0 || tilePosInGrid.y < 0 || tilePosInGrid.x >= gridSizeX || tilePosInGrid.y >= gridSizeY)
			{
				return false;
			}

			return graph[(int)tilePosInGrid.x, (int)tilePosInGrid.y] != null;
		}
	}
}