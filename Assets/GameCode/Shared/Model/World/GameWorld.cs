using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
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
		public byte CHUNK_SIZE;
		// The number of chunks in a row of the terrain
		public int HEIGHT_IN_CHUNKS;
		// The number of chunks in a column of the terrain
		public int WIDTH_IN_CHUNKS;
		// The total width in tiles for the terrain
		public int WIDTH;
		// The total height in tiles for the terrain
		public int HEIGHT;

		protected WorldChunk[,] m_Chunks;
		//protected WorldChunk m_Chunk;

		public virtual void InitializeWorld<T>() where T : WorldChunk
		{
			m_Chunks = new T[WIDTH, HEIGHT];
		}

		public void SetChunk<T>(byte chunkX, byte chunkY, T chunk) where T : WorldChunk
		{
			m_Chunks[chunkX, chunkY] = chunk;
		}

		public T GetWorldChunk<T>(float2 position) where T : WorldChunk
		{
			int worldX = (int)position.x;
			int worldY = (int)position.y;

			if (worldX < 0)
				return default;
			if (worldX >= WIDTH_IN_CHUNKS * CHUNK_SIZE)
				return default;
			if (worldY < 0)
				return default;
			if (worldY >= HEIGHT_IN_CHUNKS * CHUNK_SIZE)
				return default;

			int chunkYpos = worldY % CHUNK_SIZE;
			int chunkYnr = (worldY - chunkYpos) / CHUNK_SIZE;
			int chunkXpos = worldX % CHUNK_SIZE;
			int chunkXnr = (worldX - chunkXpos) / CHUNK_SIZE;

			return m_Chunks[chunkXnr, chunkYnr] as T;
		}

		public ChunkCellData GetChunkCellData(int worldX, int worldY)
		{
			if (worldX < 0)
				return null;
			if (worldX >= WIDTH_IN_CHUNKS * CHUNK_SIZE)
				return null;
			if (worldY < 0)
				return null;
			if (worldY >= HEIGHT_IN_CHUNKS * CHUNK_SIZE)
				return null;

			int chunkXpos = worldX % CHUNK_SIZE;
			int chunkXnr = (worldX - chunkXpos) / CHUNK_SIZE;

			int chunkYpos = worldY % CHUNK_SIZE;
			int chunkYnr = (worldY - chunkYpos) / CHUNK_SIZE;
			var chunk = m_Chunks[chunkXnr, chunkYnr];
			if (chunk == null) return null;
			return chunk.ChunkCells[chunkXpos, chunkYpos];
		}

		public T GetWorldChunk<T>(int chunkX, int chunkY) where T : WorldChunk
		{
			return m_Chunks[chunkX, chunkY] as T;
		}

		public virtual void RemoveChunk<T>(T chunk) where T : WorldChunk
		{
			m_Chunks[chunk.ChunkX, chunk.ChunkY] = null;
		}

		public ICollection<T> GetNeighbouringChunks<T>(T chunk) where T : WorldChunk
		{
			var neighbors = new List<T>();

			for (int y = -1; y <= 1; y++)
			{
				for (int x = -1; x <= 1; x++)
				{
					byte cx = (byte)(chunk.ChunkX + x);
					byte cy = (byte)(chunk.ChunkY + y);

					if (cx < 0 || cy < 0 || cx >= WIDTH_IN_CHUNKS
						|| cy >= HEIGHT_IN_CHUNKS) continue;

					if (m_Chunks[cx, cy] == null) continue;

					neighbors.Add(m_Chunks[cx, cy] as T);
				}
			}

			return neighbors;
		}

		public int2 GetChunkIndices(float2 position)
		{
			int worldX = (int)position.x;
			int worldY = (int)position.y;
			int chunkYpos = worldY % CHUNK_SIZE;
			int chunkYnr = (worldY - chunkYpos) / CHUNK_SIZE;
			int chunkXpos = worldX % CHUNK_SIZE;
			int chunkXnr = (worldX - chunkXpos) / CHUNK_SIZE;
			return new int2(chunkXnr, chunkYnr);
		}

		public int2 GetChunkTileIndices<T>(T chunk, float2 worldPosition) where T : WorldChunk
		{
			int worldX = (int)worldPosition.x;
			int worldY = (int)worldPosition.y;

			int tileXnr = worldX - (chunk.ChunkX * CHUNK_SIZE);
			int tileYnr = worldY - (chunk.ChunkY * CHUNK_SIZE);

			return new int2(tileXnr, tileYnr);
		}

		public void AddTileObject(FNEEntity tileObject)
		{
			var chunk = GetWorldChunk<WorldChunk>(tileObject.Position);
			if (chunk == null) return;
			chunk.AddTileObject(tileObject);
		}

		public void RemoveTileObject(FNEEntity tileObject)
		{
			var chunk = GetWorldChunk<WorldChunk>(tileObject.Position);
			if (chunk == null) return;
			chunk.RemoveTileObject(tileObject);
		}

		public FNEEntity GetTileObject(int tileX, int tileY)
		{
			var chunk = m_Chunks[tileX / CHUNK_SIZE, tileY / CHUNK_SIZE];
			return chunk?.TileObjects[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
		}

		public bool? IsTileBlocking(int tileX, int tileY)
		{
			var chunk = m_Chunks[tileX / CHUNK_SIZE, tileY / CHUNK_SIZE];
			return chunk?.BlockingTiles[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
		}

		public FNEEntity GetTileObject(int2 tile)
		{
			int tileX = tile.x;
			int tileY = tile.y;
			var chunk = m_Chunks[tileX / CHUNK_SIZE, (tileY / CHUNK_SIZE)];
			if (chunk == null) return null;
			return chunk.TileObjects[tile.x % CHUNK_SIZE + ((tile.y % CHUNK_SIZE) * CHUNK_SIZE)];
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
			var chunk = GetWorldChunk<WorldChunk>(edgeObject.Position);
			
			if (chunk == null) return;

			bool isWest = edgeObject.Position.x % 1 == 0;
			bool isSouth = edgeObject.Position.y % 1 == 0;

			if (isWest)
			{
				chunk.AddWestEdgeObject(edgeObject);
			}
			else if (isSouth)
			{
				chunk.AddSouthEdgeObject(edgeObject);
			}
			else
			{
				Debug.LogError("WTF, Wall is neither West nor South!");
			}
		}

		public void RemoveEdgeObject(FNEEntity edgeObject)
		{
			var chunk = GetWorldChunk<WorldChunk>(edgeObject.Position);

			if (chunk == null) return;

			bool isWest = edgeObject.Position.x % 1 == 0;
			bool isSouth = edgeObject.Position.y % 1 == 0;

			if (isWest)
			{
				chunk.RemoveWestEdgeObject(edgeObject);
			}
			else if (isSouth)
			{
				chunk.RemoveSouthEdgeObject(edgeObject);
			}
		}

		public FNEEntity GetEdgeObject(int tileX, int tileY, EdgeObjectDirection dir)
		{
			var chunk = m_Chunks[tileX / CHUNK_SIZE, (tileY / CHUNK_SIZE)];

			if (chunk == null)
				return null;
			if (dir == EdgeObjectDirection.SOUTH)
				return chunk.SouthEdgeObjects[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
			else
				return chunk.WestEdgeObjects[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
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
			var chunk = m_Chunks[tilePos.x / CHUNK_SIZE, (tilePos.y / CHUNK_SIZE)];
			var chunkNorth = m_Chunks[tilePos.x / CHUNK_SIZE, ((tilePos.y + 1) / CHUNK_SIZE)];
			var chunkEast = m_Chunks[(tilePos.x + 1) / CHUNK_SIZE, ((tilePos.y) / CHUNK_SIZE)];

			return new List<FNEEntity>()
			{
				chunk?.SouthEdgeObjects[tilePos.x % CHUNK_SIZE + ((tilePos.y % CHUNK_SIZE) * CHUNK_SIZE)],
				chunk?.WestEdgeObjects[tilePos.x % CHUNK_SIZE + ((tilePos.y % CHUNK_SIZE) * CHUNK_SIZE)],
				chunkNorth?.SouthEdgeObjects[tilePos.x % CHUNK_SIZE + (((tilePos.y + 1) % CHUNK_SIZE) * CHUNK_SIZE)],
				chunkEast?.WestEdgeObjects[(tilePos.x + 1) % CHUNK_SIZE + ((tilePos.y % CHUNK_SIZE) * CHUNK_SIZE)]
			};
		}

		public void AddTileRoom(int2 tilePos, long roomId)
		{
			var chunk = GetWorldChunk<WorldChunk>(tilePos);
			chunk.AddTileRoom(tilePos, roomId);
		}

		public void RemoveTileRoom(int2 tilePos)
		{
			var chunk = GetWorldChunk<WorldChunk>(tilePos);
			chunk.RemoveTileRoom(tilePos);
		}

		public long GetTileRoom(float2 position)
		{
			var chunk = GetWorldChunk<WorldChunk>(position);

			if (chunk == null)
				return 0;

			int2 tileIndices = GetChunkTileIndices(chunk, position);

			return chunk.TileRooms[tileIndices.x + tileIndices.y * CHUNK_SIZE];
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
			var chunk = m_Chunks[tileX / CHUNK_SIZE, (tileY / CHUNK_SIZE)];

			if (chunk == null)
				return 0;

			return chunk.TileIdCodes[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
		}

		public bool GetTileObjectBlocking(int tileX, int tileY)
		{
			var chunk = m_Chunks[tileX / CHUNK_SIZE, (tileY / CHUNK_SIZE)];
			if (chunk == null)
				return false;
			return chunk.TileObjectBlockingList[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
		}

		public bool GetBlockedTile(int tileX, int tileY)
		{
			var chunk = m_Chunks[tileX / CHUNK_SIZE, (tileY / CHUNK_SIZE)];
			if (chunk == null)
				return false;
			return chunk.BlockingTiles[tileX % CHUNK_SIZE + ((tileY % CHUNK_SIZE) * CHUNK_SIZE)];
		}

		public bool GetTileBlocking(int2 tile)
		{
			return m_Chunks[tile.x / CHUNK_SIZE, (tile.y / CHUNK_SIZE)].TileObjectBlockingList[tile.x % CHUNK_SIZE + ((tile.y % CHUNK_SIZE) * CHUNK_SIZE)];
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
					if (GetWorldChunk<WorldChunk>(new int2(origin.x + x, origin.y + y)) == null)
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
					if (GetWorldChunk<WorldChunk>(new int2(origin.x + x, origin.y + y)) != null)
						surrounding.Add(new int2(origin.x + x, origin.y + y));
				}
			}

			return surrounding;
		}

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