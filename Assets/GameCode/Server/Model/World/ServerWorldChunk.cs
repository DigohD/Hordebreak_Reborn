using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.World
{
	public class ServerWorldChunk : WorldChunk
	{
		public bool IsActive = true;
		public List<FNEEntity> EntitiesToSync = new List<FNEEntity>();
		public List<FNEEntity> MovingEntitiesToSync = new List<FNEEntity>();

		public ServerWorldChunk(byte chunkX, byte chunkY, byte size)
			: base(chunkX, chunkY, size)
		{

		}

		public byte[] GetChunkData()
		{
			var netBuffer = new NetBuffer();
			var predictedBufferSize = TotalBitsFileBuffer();
			netBuffer.EnsureBufferSize(predictedBufferSize);
			//Debug.Log($"[SERVER, SaveWorldChunkToFile]: ChunkX: {chunk.ChunkX} , ChunkY: {chunk.ChunkY}");
			FileSerialize(netBuffer);
			var actualBufferSizeAfterSerialization = netBuffer.LengthBits;
			if (actualBufferSizeAfterSerialization > predictedBufferSize)
			{
				Debug.Log($"The BufferSize after serialization of chunk is: {actualBufferSizeAfterSerialization} bits which is larger then the predicted size of: {predictedBufferSize} bits.");
			}

			return netBuffer.Data;
		}

		public override void ClearChunk()
		{
			// foreach (var edgeObj in SouthEdgeObjects)
			// {
			// 	if (edgeObj == null) continue;
			// 	GameServer.EntityAPI.DestroyEntityImmediate(edgeObj, true);
			// }
			//
			// foreach (var edgeObj in WestEdgeObjects)
			// {
			// 	if (edgeObj == null) continue;
			// 	GameServer.EntityAPI.DestroyEntityImmediate(edgeObj, true);
			// }
			//
			// foreach (var tileObj in TileObjects)
			// {
			// 	if (tileObj == null) continue;
			// 	GameServer.EntityAPI.DestroyEntityImmediate(tileObj, true);
			// }
			//
			// for (var y = 0; y < Size; y++)
			// {
			// 	for (var x = 0; x < Size; x++)
			// 	{
			// 		var enemies = ChunkCells[x, y].GetEnemies();
			//
			// 		foreach (var enemy in enemies)
			// 		{
			// 			if (enemy == null) continue;
			// 			GameServer.EntityAPI.DestroyEntityImmediate(enemy, true);
			// 		}
			// 		
			// 		enemies.Clear();
			// 	}
			// }
		}

		public override void FileSerialize(NetBuffer nb)
		{
			base.FileSerialize(nb);
			FileSerializeEdgeObjects(nb);
			FileSerializeTileObjects(nb);
			SerializeEnemies(nb, false);
		}

		public override void FileDeserialize(NetBuffer nb)
		{
			base.FileDeserialize(nb);
			FileDeserializeEdgeObjects(nb);
			FileDeserializeTileObjects(nb);
			FileDeserializeEnemies(nb);
			SetTileRooms();
		}

		public override void NetSerialize(NetBuffer nb)
		{
			base.NetSerialize(nb);
			NetSerializeEdgeObjects(nb);
			NetSerializeTileObjects(nb);
			//SerializeEnemies(nb, true);
		}

		private void FileSerializeEdgeObjects(NetBuffer nb)
		{
			nb.Write(m_SouthEdgeObjectCount);

			foreach (var edgeObject in SouthEdgeObjects)
			{
				if (edgeObject == null) continue;

				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(edgeObject.EntityId));
				edgeObject.FileSerialize(nb);
			}

			nb.Write(m_WestEdgeObjectCount);

			foreach (var edgeObject in WestEdgeObjects)
			{
				if (edgeObject == null) continue;

				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(edgeObject.EntityId));
				edgeObject.FileSerialize(nb);
			}
		}

		private void FileSerializeTileObjects(NetBuffer nb)
		{
			nb.Write(m_TileObjectCount);

			foreach (var tileObject in TileObjects)
			{
				if (tileObject == null) continue;
				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(tileObject.EntityId));
				tileObject.FileSerialize(nb);
			}
		}

		private void FileDeserializeEdgeObjects(NetBuffer nb)
		{
			int southCount = nb.ReadInt32();

			for (int i = 0; i < southCount; i++)
			{
				string id = IdTranslator.Instance.GetId<FNEEntityData>(nb.ReadUInt16());

				var edgeObj = GameServer.EntityAPI.CreateEntityImmediate(id, new float2(), 0);
				edgeObj.FileDeserialize(nb);

				EntitiesToSync.Add(edgeObj);

				//GameServer.EntityAPI.AddEntityToWorldStateImmediate(edgeObj);
				//GameServer.NetConnector.SyncEntity(edgeObj);
			}

			int westCount = nb.ReadInt32();

			for (int i = 0; i < westCount; i++)
			{
				string id = IdTranslator.Instance.GetId<FNEEntityData>(nb.ReadUInt16());

				var edgeObj = GameServer.EntityAPI.CreateEntityImmediate(id, new float2(), 0);
				edgeObj.FileDeserialize(nb);

				EntitiesToSync.Add(edgeObj);

				//GameServer.EntityAPI.AddEntityToWorldStateImmediate(edgeObj);
				//GameServer.NetConnector.SyncEntity(edgeObj);
			}
		}

		private void FileDeserializeTileObjects(NetBuffer nb)
		{
			int count = nb.ReadInt32();

			for (int i = 0; i < count; i++)
			{
				string entityId = IdTranslator.Instance.GetId<FNEEntityData>(nb.ReadUInt16());

				var tileObject = GameServer.EntityAPI.CreateEntityImmediate(entityId, new float2());
				tileObject.FileDeserialize(nb);

				var chunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(tileObject.Position);
				var chunkTileIndices = GameServer.World.GetChunkTileIndices(chunk, tileObject.Position);
				var index = chunkTileIndices.x + chunkTileIndices.y * chunk.Size;
				chunk.TileObjectBlockingList[index] = tileObject.Data.blocking;

				EntitiesToSync.Add(tileObject);

				//GameServer.EntityAPI.AddEntityToWorldStateImmediate(tileObject);
				//GameServer.NetConnector.SyncEntity(tileObject);
			}
		}

		private void FileDeserializeEnemies(NetBuffer nb)
		{
			var amount = nb.ReadInt32();

			for (int i = 0; i < amount; i++)
			{
				var data = new HordeEntitySpawnData();
				data.FileDeserialize(nb);

				var entityId = IdTranslator.Instance.GetId<FNEEntityData>(data.EntityIdCode);
				var entity = GameServer.EntityAPI.CreateEntityImmediate(entityId, data.Position, data.Rotation);

				if (entity.EntityType == EntityType.GO_ENEMY)
				{
					entity.FileDeserialize(nb);
				}

				MovingEntitiesToSync.Add(entity);

				//GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity);
				//GameServer.NetConnector.SyncEntity(entity);
			}
		}

		private void NetSerializeEdgeObjects(NetBuffer nb)
		{
			nb.Write(m_SouthEdgeObjectCount);

			foreach (var edgeObject in SouthEdgeObjects)
			{
				if (edgeObject == null) continue;

				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(edgeObject.EntityId));
				edgeObject.NetSerialize(nb);
			}

			nb.Write(m_WestEdgeObjectCount);

            foreach (var edgeObject in WestEdgeObjects)
			{
				if (edgeObject == null) continue;

				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(edgeObject.EntityId));
				edgeObject.NetSerialize(nb);
			}
		}

		private void NetSerializeTileObjects(NetBuffer nb)
		{
			nb.Write(m_TileObjectCount);

			for (var i = 0; i < TileObjects.Length; i++)
            {
				var tileObject = TileObjects[i];

				if (tileObject == null)
				{
					continue;
				}

				nb.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(tileObject.EntityId));
				tileObject.NetSerialize(nb);
			}
		}

		private void SerializeEnemies(NetBuffer nb, bool netSerialize)
		{
			var c = TotalEnemies();
			nb.Write(c);

			for (var y = 0; y < Size; y++)
			{
				for (var x = 0; x < Size; x++)
				{
					var enemies = ChunkCells[x, y].GetEnemies();

					foreach (var enemy in enemies)
					{
						var data = new HordeEntitySpawnData
						{
							EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(enemy.EntityId),
							NetId = enemy.NetId,
							Position = enemy.Position,
							Rotation = enemy.RotationDegrees
						};

						if (netSerialize)
							data.NetSerialize(nb);
						else
							data.FileSerialize(nb);

						if (enemy.EntityType == EntityType.GO_ENEMY)
						{
							if (netSerialize)
								enemy.NetSerialize(nb);
							else
								enemy.FileSerialize(nb);
						}
					}
				}
			}
		}

		public void ChangeTile(int x, int y, string tileId)
		{
			int index = ((int)x - ChunkX * Size) + (y - ChunkY * Size) * Size;
			TileIdCodes[index] = IdTranslator.Instance.GetIdCode<TileData>(tileId);
			BlockingTiles[index] = DataBank.Instance.GetData<TileData>(tileId).isBlocking;
		}

		public void SetTileRooms()
		{
			var worldX = ChunkX * Size;
			var worldY = ChunkY * Size;
			for (int i = 0; i < TileRooms.Length; i++)
			{
				var worldEquivalent = new int2(worldX + (i % Size), worldY + (i / Size));
				var tileRoom = GameServer.RoomManager.GetTileRoomWithoutWorldData(worldEquivalent);
				if (tileRoom != null)
					TileRooms[i] = tileRoom.Id;
			}
		}
	}
}