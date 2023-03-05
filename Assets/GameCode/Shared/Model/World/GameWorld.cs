using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using System.Collections.Generic;
using FNZ.Shared.Model.Entity.Components.EdgeObject;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World
{
	public abstract class GameWorld
	{
		public enum EdgeObjectDirection
		{
			SOUTH = 0,
			WEST = 1
		}

		// The size of a chunk in tiles (64 means the chunk is 64x64 tiles large)
		//public byte CHUNK_SIZE;

		// The total width in tiles for the terrain
		public int WIDTH;
		// The total height in tiles for the terrain
		public int HEIGHT;
		
		protected WorldChunk m_Chunk;
		public int WorldInstanceIndex;

		public virtual void InitializeWorld<T>() where T : WorldChunk {}

		public void SetChunk<T>(T chunk) where T : WorldChunk
		{
			m_Chunk = chunk;
		}

		public T GetWorldChunk<T>() where T : WorldChunk
		{
			return m_Chunk as T;
		}

		public ChunkCellData GetChunkCellData(int worldX, int worldY)
		{
			if (m_Chunk == null) return null;
			return m_Chunk.ChunkCells[worldX, worldY];
		}

		public virtual void RemoveChunk()
		{
			m_Chunk = null;
		}

		public void AddTileObject(FNEEntity tileObject)
		{
			if (m_Chunk == null) return;
			m_Chunk.AddTileObject(tileObject);
		}

		public void RemoveTileObject(FNEEntity tileObject)
		{
			if (m_Chunk == null) return;
			m_Chunk.RemoveTileObject(tileObject);
		}

		public int GetFlatArrayIndexFromPos(float tileX, float tileY)
		{
			return GetFlatArrayIndexFromPos((int) tileX, (int) tileY);
		}
		public int GetFlatArrayIndexFromPos(int tileX, int tileY)
        {
			return tileX % WIDTH + ((tileY % HEIGHT) * WIDTH);
		}

		public bool? IsTileBlocking(int tileX, int tileY)
		{
			return m_Chunk?.BlockingTiles[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public FNEEntity GetTileObject(int tileX, int tileY)
		{
			if (m_Chunk == null) return null;
			return m_Chunk.TileObjects[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public List<FNEEntity> GetStraightNeighborTileObjects(int2 currentTile)
		{
			var neighborTileObjects = new List<FNEEntity>();

			foreach (var neighbor in GetTileStraightNeighbors(currentTile.x, currentTile.y))
			{
				neighborTileObjects.Add(GetTileObject(neighbor.x, neighbor.y));
			}

			return neighborTileObjects;
		}

		public List<FNEEntity> GetDiagonalNeighborTileObjects(int2 currentTile)
		{
			var neighborTileObjects = new List<FNEEntity>();

			foreach (var neighbor in GetTileDiagonalNeighbors(currentTile.x, currentTile.y))
			{
				neighborTileObjects.Add(GetTileObject(neighbor.x, neighbor.y));
			}

			return neighborTileObjects;
		}

		public void AddEdgeObject(FNEEntity edgeObject)
		{			
			if (m_Chunk == null) return;

			bool isWest = edgeObject.Position.x % 1 == 0;
			bool isSouth = edgeObject.Position.y % 1 == 0;

			if (isWest)
			{
				m_Chunk.AddWestEdgeObject(edgeObject);
			}
			else if (isSouth)
			{
				m_Chunk.AddSouthEdgeObject(edgeObject);
			}
			else
			{
				Debug.LogError("WTF, Wall is neither West nor South!");
			}
		}

		public void RemoveEdgeObject(FNEEntity edgeObject)
		{
			if (m_Chunk == null) return;

			bool isWest = edgeObject.Position.x % 1 == 0;
			bool isSouth = edgeObject.Position.y % 1 == 0;

			if (isWest)
			{
				m_Chunk.RemoveWestEdgeObject(edgeObject);
			}
			else if (isSouth)
			{
				m_Chunk.RemoveSouthEdgeObject(edgeObject);
			}
		}

		public FNEEntity GetEdgeObject(int tileX, int tileY, EdgeObjectDirection dir)
		{
			if (m_Chunk == null)
				return null;
			if (dir == EdgeObjectDirection.SOUTH)
				return m_Chunk.SouthEdgeObjects[GetFlatArrayIndexFromPos(tileX, tileY)];
			else
				return m_Chunk.WestEdgeObjects[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public FNEEntity GetEdgeObjectAtPosition(float2 pos)
		{
			if (pos.x < 0 || pos.y < 0 || pos.x >= WIDTH | pos.y >= HEIGHT) return null;

			if (pos.x % 1 != 0)
			{
				return GetEdgeObject((int)pos.x, (int)pos.y, EdgeObjectDirection.SOUTH);
			}
			else
			{
				return GetEdgeObject((int)pos.x, (int)pos.y, EdgeObjectDirection.WEST);
			}
		}

		public List<FNEEntity> GetStraightDirectionsEdgeObjects(int2 tilePos)
		{
			return new List<FNEEntity>()
			{
				m_Chunk?.SouthEdgeObjects[GetFlatArrayIndexFromPos(tilePos.x, tilePos.y)],
				m_Chunk?.WestEdgeObjects[GetFlatArrayIndexFromPos(tilePos.x, tilePos.y)],
				m_Chunk?.SouthEdgeObjects[GetFlatArrayIndexFromPos(tilePos.x, tilePos.y + 1)],
				m_Chunk?.WestEdgeObjects[GetFlatArrayIndexFromPos(tilePos.x + 1, tilePos.y)]
			};
		}

		public void AddTileRoom(int2 tilePos, long roomId)
		{
			m_Chunk.AddTileRoom(tilePos, roomId);
		}

		public void RemoveTileRoom(int2 tilePos)
		{
			m_Chunk.RemoveTileRoom(tilePos);
		}

		public long GetTileRoom(float2 position)
		{
			if (m_Chunk == null)
				return 0;

			return m_Chunk.TileRooms[(int) position.x + (int) position.y * WIDTH];
		}

		public string GetTileId(int tileX, int tileY)
		{
			var id = GetTileIdCode(tileX, tileY);

			if (id == 0)
				return string.Empty;

			return IdTranslator.Instance.GetId<TileData>(id);
		}

		public TileData GetTileData(int tileX, int tileY)
		{
			return DataBank.Instance.GetData<TileData>(GetTileId(tileX, tileY));
		}

		public ushort GetTileIdCode(int tileX, int tileY)
		{
			if (m_Chunk == null)
				return 0;

			return m_Chunk.TileIdCodes[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public bool GetTileObjectBlocking(int tileX, int tileY)
		{
			if (m_Chunk == null)
				return false;
			return m_Chunk.TileObjectBlockingList[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public bool GetBlockedTile(int tileX, int tileY)
		{
			if (m_Chunk == null)
				return false;
			return m_Chunk.BlockingTiles[GetFlatArrayIndexFromPos(tileX, tileY)];
		}

		public bool GetTileBlocking(int2 tile)
		{
			return m_Chunk.TileObjectBlockingList[GetFlatArrayIndexFromPos(tile.x, tile.y)];
		}

		public List<FNEEntity> GetTileEnemies(int2 tilePos)
		{
			var cc = GetChunkCellData(tilePos.x, tilePos.y);
			if (cc == null) return null;
			return cc.GetEnemies();
		}

		public void AddEnemyToTile(int2 tilePos, FNEEntity enemy)
		{
			var cc = GetChunkCellData(tilePos.x, tilePos.y);
			if (cc == null)
            {
				Debug.LogWarning($"No chunk found when trying to add enemy to tile on position: {tilePos}");
				return;
            }

			cc.AddEnemy(enemy);
		}

		public void AddEnemyToTile(FNEEntity enemy)
		{
			AddEnemyToTile(new int2((int)enemy.Position.x, (int)enemy.Position.y), enemy);
		}

		public void RemoveEnemyFromTile(FNEEntity enemy)
		{
			RemoveEnemyFromTile(new int2((int)enemy.Position.x, (int)enemy.Position.y), enemy);
		}

		public void RemoveEnemyFromTile(int2 tilePos, FNEEntity enemy)
		{
			var cc = GetChunkCellData(tilePos.x, tilePos.y);

			if (cc == null)
			{
				Debug.LogWarning($"No chunk found when trying to remove enemy on tile at position: {tilePos}");
				return;
			}

			cc.RemoveEnemy(enemy);		
		}

		public void AddPlayerToTile(FNEEntity player)
		{
			var tileX = (int)player.Position.x;
			var tileY = (int)player.Position.y;

			var cc = GetChunkCellData(tileX, tileY);
			cc.AddPlayer(player);
		}

		public List<FNEEntity> GetTilePlayers(int2 tilePos)
		{
			var cc = GetChunkCellData(tilePos.x, tilePos.y);
			if (cc == null) return null;
			return cc.GetPlayers();
		}

		public bool IsAnySurroundingTileInRadiusNull(int2 origin, int radius)
		{

			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT)
						return true;
				}
			}

			return false;
		}

		public List<int2> GetSurroundingTilesInRadius(int2 origin, int radius)
		{
			List<int2> surrounding = new List<int2>();

			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (origin.x + x >= 0 && origin.x + x < WIDTH && origin.y + y >= 0 && origin.y + y < HEIGHT)
						surrounding.Add(new int2(origin.x + x, origin.y + y));
				}
			}

			return surrounding;
		}

		// TODO: Do not return lists
		public List<int2> GetTileStraightNeighbors(int x, int y)
		{
			return new List<int2>
			{
				new int2(x, y - 1), //South
				new int2(x - 1, y), //West
				new int2(x, y + 1), //North
				new int2(x + 1, y)  //East
			};
		}

		// TODO: Do not return lists
		public List<int2> GetTileDiagonalNeighbors(int x, int y)
		{
			return new List<int2>
			{
				new int2(x - 1, y - 1), //SouthWest
				new int2(x - 1, y + 1), //NorthWest
				new int2(x + 1, y + 1), //NorthEast
				new int2(x + 1, y - 1)  //SouthEast
			};
		}

		public bool IsTileWestEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y, EdgeObjectDirection.WEST) != null;
		}

		public bool IsTileEastEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x + 1, tilePos.y, EdgeObjectDirection.WEST) != null;
		}

		public bool IsTileNorthEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y + 1, EdgeObjectDirection.SOUTH) != null;
		}

		public bool IsTileSouthEdgeOccupied(int2 tilePos)
		{
			return GetEdgeObject(tilePos.x, tilePos.y, EdgeObjectDirection.SOUTH) != null;
		}

		public bool IsTileWestEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y, EdgeObjectDirection.WEST);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range < eo.Data.seeThroughRange)
			{
				return true;
			}

			return false;
		}
		
		public bool IsTileSouthEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y, EdgeObjectDirection.SOUTH);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range < eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
		
		public bool IsTileEastEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x + 1, tilePos.y, EdgeObjectDirection.WEST);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range <= eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
		
		public bool IsTileNorthEdgeSeeThrough(int2 tilePos, int range)
		{
			var eo = GetEdgeObject(tilePos.x, tilePos.y + 1, EdgeObjectDirection.SOUTH);
			if (eo == null)
			{
				return true;
			}

			var eoComp = eo.GetComponent<EdgeObjectComponentShared>();
			
			if(eoComp.IsSeethrough && range <= eo.Data.seeThroughRange)
			{
				return true;
			}
			
			return false;
		}
	}
	
}