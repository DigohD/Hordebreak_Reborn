using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.Messages
{
	internal class ServerNotificationMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerNotificationMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateNotificationMessage(ushort spriteId, ushort colorId, bool isPermanent, string playerMessage, float identifier)
		{
			var sendBuffer = m_NetServer.CreateMessage();
			sendBuffer.Write((byte)NetMessageType.NOTIFICATION);

			sendBuffer.Write(spriteId);
			sendBuffer.Write(colorId);
			sendBuffer.Write(isPermanent);
			sendBuffer.Write(playerMessage);
			sendBuffer.Write(identifier);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.NOTIFICATION,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.NOTIFICATION,
			};
		}
	}
}