using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.Messages
{
	internal class ServerPlayerMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerPlayerMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateSpawnLocalPlayerMessage(FNEEntity playerEntity)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			sendBuffer.Write((byte)NetMessageType.SPAWN_LOCAL_PLAYER);

			sendBuffer.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(playerEntity.EntityId));
			playerEntity.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_LOCAL_PLAYER,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_SETUP,
			};
		}

		public NetMessage CreateSpawnRemotePlayerMessage(FNEEntity playerEntity)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			sendBuffer.Write((byte)NetMessageType.SPAWN_REMOTE_PLAYER);

			sendBuffer.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(playerEntity.EntityId));
			playerEntity.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_REMOTE_PLAYER,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_SETUP,
			};
		}

		public NetMessage CreateRemoveRemotePlayerMessage(FNEEntity playerEntity)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4);

			sendBuffer.Write((byte)NetMessageType.REMOVE_REMOTE_PLAYER);
			sendBuffer.Write(playerEntity.NetId);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.REMOVE_REMOTE_PLAYER,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateTeleportPlayerMessage(float2 destination)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + 4);
			sendBuffer.Write((byte)NetMessageType.PLAYER_TELEPORT);

			sendBuffer.Write(destination.x);
			sendBuffer.Write(destination.y);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.PLAYER_TELEPORT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.PLAYER_STATE,
			};
		}

	}
}