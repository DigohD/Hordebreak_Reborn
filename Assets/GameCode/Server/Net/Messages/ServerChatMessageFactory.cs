using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.Messages
{
	internal class ServerChatMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerChatMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateChatMessage(string messageString, long senderID)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + messageString.Length + 8);

			sendBuffer.Write((byte)NetMessageType.CHAT_MESSAGE);
			sendBuffer.Write(messageString);
			sendBuffer.Write(senderID);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CHAT_MESSAGE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.CHAT,
			};
		}
	}
}

