using FNZ.Server.Services;
using FNZ.Shared.Model.World;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.Messages
{
	internal class ServerItemOnGroundMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerItemOnGroundMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateSpawnItemOnGroundMessage(ItemOnGround toSpawn)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			sendBuffer.Write((byte) NetMessageType.SPAWN_ITEM_ON_GROUND);

			sendBuffer.Write(toSpawn.identifier);
			sendBuffer.Write(toSpawn.Position.x);
			sendBuffer.Write(toSpawn.Position.y);
			toSpawn.item.Serialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_ITEM_ON_GROUND,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateRemoveItemOnGroundMessage(long identifier)
		{
			var sendBuffer = m_NetServer.CreateMessage(9);
			sendBuffer.Write((byte)NetMessageType.REMOVE_ITEM_ON_GROUND);

			sendBuffer.Write(identifier);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.REMOVE_ITEM_ON_GROUND,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

	}
}