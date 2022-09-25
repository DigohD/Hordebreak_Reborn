using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World
{
	public class ChunkCellData
    {
		private List<FNEEntity> Enemies;
		private List<FNEEntity> Players;

		public int BloodSplatterCount = 0;

		public ChunkCellData()
		{
			Enemies = new List<FNEEntity>();
			Players = new List<FNEEntity>();
		}

		public void AddEnemy(FNEEntity enemy)
		{
			if (!Enemies.Contains(enemy))
				Enemies.Add(enemy);
		}

		public void RemoveEnemy(FNEEntity enemy)
		{
			Enemies.Remove(enemy);
		}

		public List<FNEEntity> GetEnemies()
		{
			return Enemies;
		}

		public void AddPlayer(FNEEntity player)
		{
			if (!Players.Contains(player))
				Players.Add(player);
			else
				Debug.LogWarning($"player with NetId: {player.NetId} already exists on cell");
		}

		public void RemovePlayer(FNEEntity player)
		{
			Players.Remove(player);
		}

		public List<FNEEntity> GetPlayers()
		{
			return Players;
		}
	};

	public abstract class WorldChunk
	{
		public byte ChunkX { get; set; }
		public byte ChunkY { get; set; }
		public byte Size { get; set; }

		public bool IsInitialized = false;

		public ushort[] TileIdCodes;
		public byte[] TileCosts;
		public byte[] TileDangerLevels;
		public bool[] BlockingTiles;
		public bool[] TileObjectBlockingList;
		public bool[] TileSeeThroughList;
		public int[] TilePositionsX;
		public int[] TilePositionsY;
		public float[] TileTemperatures;
		public long[] TileRooms;

		protected int m_SouthEdgeObjectCount;
		protected int m_WestEdgeObjectCount;
		protected int m_TileObjectCount;

		public FNEEntity[] SouthEdgeObjects;
		public FNEEntity[] WestEdgeObjects;

		public FNEEntity[] TileObjects;

		public List<FNETransform> FloatPointObjects;

		public ChunkCellData[,] ChunkCells;

		public WorldChunk(byte chunkX, byte chunkY, byte size)
		{
			ChunkX = chunkX;
			ChunkY = chunkY;
			Size = size;

			TileIdCodes = new ushort[size * size];

			BlockingTiles = new bool[size * size];

			TileCosts = new byte[size * size];
			TileDangerLevels = new byte[size * size];

			TileObjectBlockingList = new bool[size * size];
			TileSeeThroughList = new bool[size * size];

			TilePositionsX = new int[size * size];
			TilePositionsY = new int[size * size];

			TileTemperatures = new float[size * size];
			TileRooms = new long[size * size];

			SouthEdgeObjects = new FNEEntity[size * size];
			WestEdgeObjects = new FNEEntity[size * size];

			TileObjects = new FNEEntity[size * size];

			FloatPointObjects = new List<FNETransform>();

			ChunkCells = new ChunkCellData[size, size];

			for (var y = 0; y < Size; y++)
			{
				for (var x = 0; x < Size; x++)
				{
					ChunkCells[x, y] = new ChunkCellData();
				}
			}
		}

		public abstract void ClearChunk();

		public void AddTileObject(FNEEntity tileObject)
		{
			int index = ((int)tileObject.Position.x - ChunkX * Size) + ((int)tileObject.Position.y - ChunkY * Size) * Size;
			TileObjects[index] = tileObject;
			m_TileObjectCount++;
		}

		public void RemoveTileObject(FNEEntity tileObject)
		{
			int index = ((int)tileObject.Position.x - ChunkX * Size) + ((int)tileObject.Position.y - ChunkY * Size) * Size;
			TileObjects[index] = null;
			m_TileObjectCount--;
		}

		public void AddSouthEdgeObject(FNEEntity southEdgeObject)
		{
			int index = ((int)southEdgeObject.Position.x - ChunkX * Size) + (((int)southEdgeObject.Position.y) - ChunkY * Size) * Size;
			SouthEdgeObjects[index] = southEdgeObject;
			m_SouthEdgeObjectCount++;
		}

		public FNEEntity GetSouthEdgeObject(float2 position)
		{
			int index = ((int)position.x - ChunkX * Size) + (((int)position.y) - ChunkY * Size) * Size;
			return SouthEdgeObjects[index];
		}

		public void RemoveSouthEdgeObject(FNEEntity southEdgeObject)
		{
			int index = ((int)southEdgeObject.Position.x - ChunkX * Size) + (((int)southEdgeObject.Position.y) - ChunkY * Size) * Size;
			SouthEdgeObjects[index] = null;
			m_SouthEdgeObjectCount--;
		}

		public void AddWestEdgeObject(FNEEntity westEdgeObject)
		{
			int index = (((int)westEdgeObject.Position.x) - ChunkX * Size) + ((int)westEdgeObject.Position.y - ChunkY * Size) * Size;
			WestEdgeObjects[index] = westEdgeObject;
			m_WestEdgeObjectCount++;
		}

		public void AddTileRoom(int2 tilePos, long roomId)
		{
			int index = (tilePos.x - ChunkX * Size) + (tilePos.y - ChunkY * Size) * Size;
			TileRooms[index] = roomId;
		}

		public void RemoveTileRoom(int2 tilePos)
		{
			int index = (tilePos.x - ChunkX * Size) + (tilePos.y - ChunkY * Size) * Size;
			TileRooms[index] = 0;
		}

		public FNEEntity GetWestEdgeObject(float2 position)
		{
			int index = ((int)position.x - ChunkX * Size) + (((int)position.y) - ChunkY * Size) * Size;
			return WestEdgeObjects[index];
		}

		public void RemoveWestEdgeObject(FNEEntity westEdgeObject)
		{
			int index = ((int)westEdgeObject.Position.x - ChunkX * Size) + (((int)westEdgeObject.Position.y) - ChunkY * Size) * Size;
			WestEdgeObjects[index] = null;
			m_WestEdgeObjectCount--;
		}

		public virtual void FileSerialize(NetBuffer nb)
		{
			SerializeTiles(nb);
			SerializeFloatpointObjects(nb);
		}

		public virtual void FileDeserializeTilesOnly(NetBuffer nb)
		{
			DeserializeTiles(nb);
		}
		
		public virtual void FileDeserialize(NetBuffer nb)
		{
			DeserializeTiles(nb);
			DeserializeFloatpointObjects(nb);
		}

		public virtual void NetSerialize(NetBuffer nb)
		{
			SerializeTiles(nb);
			SerializeFloatpointObjects(nb);
		}

		public virtual void NetDeserialize(NetBuffer nb)
		{
			DeserializeTiles(nb);
			DeserializeFloatpointObjects(nb);
		}

		public virtual int TotalBitsNetBuffer()
		{
			return TileDataTotalBits() + EdgeObjectTotalBitsNet() +
			       TileObjectTotalBitsNet(); // + EnemiesTotalBits(true);
		}

		public virtual int TotalBitsFileBuffer()
		{
			return TileDataTotalBits() + EdgeObjectTotalBitsFile()
				+ TileObjectTotalBitsFile() + EnemiesTotalBits(false);
		}

		private void SerializeTiles(NetBuffer nb)
		{
			for (int i = 0; i < Size * Size; i++)
			{
				nb.Write(TileIdCodes[i]);
				nb.Write(TileCosts[i]);
				nb.Write(TileDangerLevels[i]);
				nb.Write(TileObjectBlockingList[i]);
				nb.Write(TileSeeThroughList[i]);
				nb.Write((ushort)(TilePositionsX[i] % Size));
				nb.Write((ushort)(TilePositionsY[i] % Size));
				nb.Write(FNEUtil.PackFloatAsShort(TileTemperatures[i]));
			}
		}

		private void DeserializeTiles(NetBuffer nb)
		{
			for (int i = 0; i < Size * Size; i++)
			{
				TileIdCodes[i] = nb.ReadUInt16();
				BlockingTiles[i] = DataBank.Instance.GetData<TileData>(IdTranslator.Instance.GetId<TileData>(TileIdCodes[i])).isBlocking;
				TileCosts[i] = nb.ReadByte();
				TileDangerLevels[i] = nb.ReadByte();
				TileObjectBlockingList[i] = nb.ReadBoolean();
				TileSeeThroughList[i] = nb.ReadBoolean();
				TilePositionsX[i] = nb.ReadUInt16() + ChunkX * Size;
				TilePositionsY[i] = nb.ReadUInt16() + ChunkY * Size;
				TileTemperatures[i] = FNEUtil.UnpackShortToFloat(nb.ReadUInt16());
			}
		}

		private void SerializeFloatpointObjects(NetBuffer bw)
		{
			bw.Write(FloatPointObjects.Count);

			foreach (var fpObject in FloatPointObjects)
			{
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.posX - ChunkX * 32));
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.posY - ChunkY * 32));
				bw.Write(FNEUtil.ConvertFloatToSignedShort(fpObject.posZ));

				bw.Write(FNEUtil.PackFloatAsShort(fpObject.rotX));
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.rotY));
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.rotZ));

				bw.Write(FNEUtil.PackFloatAsShort(fpObject.scaleX));
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.scaleY));
				bw.Write(FNEUtil.PackFloatAsShort(fpObject.scaleZ));

				bw.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(fpObject.entityId));
			}
		}

		private void DeserializeFloatpointObjects(NetBuffer br)
		{
			int size = br.ReadInt32();

			FloatPointObjects = new List<FNETransform>(size);

			for (int i = 0; i < size; i++)
			{
				FNETransform transform;

				transform.posX = FNEUtil.UnpackShortToFloat(br.ReadUInt16()) + ChunkX * 32;
				transform.posY = FNEUtil.UnpackShortToFloat(br.ReadUInt16()) + ChunkY * 32;
				transform.posZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

				transform.rotX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
				transform.rotY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
				transform.rotZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

				transform.scaleX = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
				transform.scaleY = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
				transform.scaleZ = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

				transform.entityId = IdTranslator.Instance.GetId<FNEEntityData>(br.ReadUInt16());

				FloatPointObjects.Add(transform);
			}
		}

		private int TileDataTotalBits()
		{
			int tileIdBits = sizeof(ushort) * Size * Size * 8;
			int tileCostBits = sizeof(byte) * Size * Size * 8;
			int tileDangerLevelBits = sizeof(byte) * Size * Size * 8;
			int tileBlockingBits = sizeof(byte) * Size * Size * 8;
			int tileSeeThroughBits = sizeof(byte) * Size * Size * 8;
			int tilePositionsXBits = sizeof(ushort) * Size * Size * 8;
			int tilePositionsYBits = sizeof(ushort) * Size * Size * 8;
			int tileTemperatureBits = sizeof(ushort) * Size * Size * 8;

			return tileIdBits + tileCostBits + tileDangerLevelBits + tileBlockingBits
				+ tileSeeThroughBits + tilePositionsXBits + tilePositionsYBits + tileTemperatureBits;
		}

		private int EdgeObjectTotalBitsNet()
		{
			int totalBits = 0;

			for (int i = 0; i < SouthEdgeObjects.Length; i++)
			{
				var entity = SouthEdgeObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsNetBuffer();
				}
			}

			for (int i = 0; i < WestEdgeObjects.Length; i++)
			{
				var entity = WestEdgeObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsNetBuffer();
				}
			}

			return totalBits;
		}

		private int TileObjectTotalBitsNet()
		{
			int totalBits = 0;

			for (int i = 0; i < TileObjects.Length; i++)
			{
				var entity = TileObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsNetBuffer();
				}
			}

			return totalBits;
		}

		private int EdgeObjectTotalBitsFile()
		{
			int totalBits = 0;

			for (int i = 0; i < SouthEdgeObjects.Length; i++)
			{
				var entity = SouthEdgeObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsFileBuffer();
				}
			}

			for (int i = 0; i < WestEdgeObjects.Length; i++)
			{
				var entity = WestEdgeObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsFileBuffer();
				}
			}

			return totalBits;
		}

		private int TileObjectTotalBitsFile()
		{
			int totalBits = 0;

			for (int i = 0; i < TileObjects.Length; i++)
			{
				var entity = TileObjects[i];
				if (entity != null)
				{
					totalBits += entity.TotalBitsFileBuffer();
				}
			}

			return totalBits;
		}

		public List<FNEEntity> GetAllEnemies()
		{
			var result = new List<FNEEntity>();

			for (var y = 0; y < Size; y++)
			{
				for (var x = 0; x < Size; x++)
				{
					foreach (var e in ChunkCells[x, y].GetEnemies())
						result.Add(e);
				}
			}

			return result;
		}

		public int TotalEnemies()
        {
			var count = 0;

			for (var y = 0; y < Size; y++)
				for (var x = 0; x < Size; x++)
					count += ChunkCells[x, y].GetEnemies().Count;

			return count;
		}

		private int EnemiesTotalBits(bool net)
        {
			var totalBits = 0;

			for (var y = 0; y < Size; y++)
			{
				for (var x = 0; x < Size; x++)
				{
					var enemies = ChunkCells[x, y].GetEnemies();

					foreach (var enemy in enemies)
                    {
						var data = new HordeEntitySpawnData();

						totalBits += (8*data.GetSizeInBytes(net));

						if (enemy.EntityType == EntityType.GO_ENEMY)
							totalBits += (net ? enemy.TotalBitsNetBuffer() : enemy.TotalBitsNetBuffer());
					}
				}
			}

			return totalBits;
		}
	}
}