using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Net;
using Lidgren.Network;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using FNZ.Shared.Model.World;
using Unity.Mathematics;

namespace FNZ.Server.Net.Messages
{
	internal class ServerWorldMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerWorldMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateWorldSetupMessage(int widthInTiles, int heightInTiles)
		{
			var sendBuffer = m_NetServer.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.WORLD_SETUP);

			IdTranslator.Instance.Serialize(sendBuffer);

			sendBuffer.Write(widthInTiles);
			sendBuffer.Write(heightInTiles);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.WORLD_SETUP,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_SETUP,
			};
		}

		public NetMessage CreateLoadChunkMessage(byte chunkX, byte chunkY, byte[] data)
		{
			var sendBuffer = m_NetServer.CreateMessage(data.Length + 7);

			sendBuffer.Write((byte)NetMessageType.LOAD_CHUNK);
			sendBuffer.Write(chunkX);
			sendBuffer.Write(chunkY);
			sendBuffer.Write(data.Length);
			sendBuffer.Write(data, 0, data.Length);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.LOAD_CHUNK,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateUnloadChunkMessage(byte chunkX, byte chunkY)
		{
			var sendBuffer = m_NetServer.CreateMessage(3);

			sendBuffer.Write((byte)NetMessageType.UNLOAD_CHUNK);
			sendBuffer.Write(chunkX);
			sendBuffer.Write(chunkY);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UNLOAD_CHUNK,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateChangeTileMessage(int tileX, int tileY, string id)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + 4 + 2);

			sendBuffer.Write((byte)NetMessageType.CHANGE_TILE);
			sendBuffer.Write(tileX);
			sendBuffer.Write(tileY);
			sendBuffer.Write(IdTranslator.Instance.GetIdCode<TileData>(id));

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CHANGE_TILE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateRoomManagerMessage()
		{
			var sendBuffer = m_NetServer.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.ROOM_MANAGER);

			GameServer.RoomManager.Serialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.ROOM_MANAGER,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateEnvironmentMessage()
		{
			var sendBuffer = m_NetServer.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.ENVIRONMENT);

			GameServer.MainWorld.Environment.Serialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.ENVIRONMENT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateChunkMapUpdateMessage(ServerWorldChunk chunk)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			var predictedBufferSize = 3 + (chunk.Size * chunk.Size * 2);
			
			sendBuffer.Write((byte) NetMessageType.CHUNK_MAP_UPDATE);
			
			sendBuffer.Write(chunk.ChunkX);
			sendBuffer.Write(chunk.ChunkY);
			
			var tileIdCodes = chunk.TileIdCodes;
			for (int i = 0; i < tileIdCodes.Length; i++)
			{
				sendBuffer.Write(tileIdCodes[i]);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CHUNK_MAP_UPDATE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_MAP_STATE,
			};
		}
		
		public NetMessage CreateSiteMapUpdateMessage(Dictionary<int, MapManager.RevealedSiteData> siteMap)
		{
			var sendBuffer = m_NetServer.CreateMessage();

			sendBuffer.Write((byte) NetMessageType.SITE_MAP_UPDATE);
			
			var keys = siteMap.Keys;
			
			int packetSize = 2;
			foreach (var key in keys)
			{
				packetSize += siteMap[key].GetSizeInBytes();
			}
			
			sendBuffer.EnsureBufferSize(packetSize);
			
			sendBuffer.Write((ushort) keys.Count);
			foreach (var key in keys)
			{
				siteMap[key].Serialize(sendBuffer);
			}
			
			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SITE_MAP_UPDATE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_MAP_STATE,
			};
		}

		//DEV ONLY
		public NetMessage CreateFlowFieldMessage(FNEFlowField ff)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			sendBuffer.Write((byte)NetMessageType.DEVONLY_FLOWFIELD_DEVONLY);

			var loopsize = (int)math.sqrt(ff.graph.Length);
			sendBuffer.Write(loopsize);
			for (int i = 0; i < loopsize; i++)
			{
				for (int j = 0; j < loopsize; j++)
				{
					sendBuffer.Write(ff.graph[i, j].gridX);
					sendBuffer.Write(ff.graph[i, j].gridY);
					sendBuffer.Write(ff.graph[i, j].totalCost);
				}
			}

			loopsize = (int)math.sqrt(ff.vectorField.Length);
			sendBuffer.Write(loopsize);
			for (int i = 0; i < loopsize; i++)
			{
				for (int j = 0; j < loopsize; j++)
				{
					sendBuffer.Write(ff.vectorField[i, j].vector.x);
					sendBuffer.Write(ff.vectorField[i, j].vector.y);
					//ff.vectorField[i,j].breakWall
				}
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.DEVONLY_FLOWFIELD_DEVONLY,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
		//DEVONLY
	}
}