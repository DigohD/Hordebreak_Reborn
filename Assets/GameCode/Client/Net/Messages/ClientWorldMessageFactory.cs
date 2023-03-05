using System;
using System.Collections.Generic;
using FNZ.Client.StaticData;
using FNZ.Shared.Model.World;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.Messages
{
	internal class ClientWorldMessageFactory
	{
		private readonly NetClient m_NetClient;

		public ClientWorldMessageFactory(NetClient netClient)
		{
			m_NetClient = netClient;
		}

		public NetMessage CreateRequestWorldSpawnMessage()
		{
			var sendBuffer = m_NetClient.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.REQUEST_WORLD_SPAWN);
			sendBuffer.Write(NetData.LOCAL_PLAYER_NAME);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.REQUEST_WORLD_SPAWN,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_SETUP,
			};
		}

		public NetMessage CreateRequestWorldInstanceSpawnMessage(Guid id, string siteId)
		{
			var sendBuffer = m_NetClient.CreateMessage();
			
			sendBuffer.Write((byte)NetMessageType.REQUEST_WORLD_INSTANCE);
			sendBuffer.Write(id.ToString());
			sendBuffer.Write(siteId);

			return new NetMessage
			{	
				Buffer = sendBuffer,
				Type = NetMessageType.REQUEST_WORLD_INSTANCE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_SETUP
			};
		}

		public NetMessage CreateConfirmChunkLoadedMessage(WorldChunk chunk)
		{
			var sendBuffer = m_NetClient.CreateMessage(3);

			sendBuffer.Write((byte)NetMessageType.CLIENT_CONFIRM_CHUNK_LOADED);
			sendBuffer.Write(chunk.ChunkX);
			sendBuffer.Write(chunk.ChunkY);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CLIENT_CONFIRM_CHUNK_LOADED,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		// public NetMessage CreateConfirmChunkUnloadedMessage(WorldChunk chunk)
		// {
		// 	var sendBuffer = m_NetClient.CreateMessage(3);
		//
		// 	sendBuffer.Write((byte)NetMessageType.CLIENT_CONFIRM_CHUNK_UNLOADED);
		// 	sendBuffer.Write(chunk.ChunkX);
		// 	sendBuffer.Write(chunk.ChunkY);
		//
		// 	return new NetMessage
		// 	{
		// 		Buffer = sendBuffer,
		// 		Type = NetMessageType.CLIENT_CONFIRM_CHUNK_UNLOADED,
		// 		DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
		// 		Channel = SequenceChannel.WORLD_STATE,
		// 	};
		// }

		public NetMessage CreateBaseRoomNameChangeMessage(bool isBase, long id, string newName)
		{
			var sendBuffer = m_NetClient.CreateMessage(10 + (newName.Length * 4));

			sendBuffer.Write((byte)NetMessageType.BASE_ROOM_NAME_CHANGE);
			sendBuffer.Write(isBase);
			sendBuffer.Write(id);
			sendBuffer.Write(newName);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.BASE_ROOM_NAME_CHANGE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
	}
}

