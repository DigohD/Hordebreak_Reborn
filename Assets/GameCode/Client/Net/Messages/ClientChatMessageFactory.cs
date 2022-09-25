using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.Messages
{
	internal class ClientChatMessageFactory
	{
		private readonly NetClient m_NetClient;

		public ClientChatMessageFactory(NetClient netClient)
		{
			m_NetClient = netClient;
		}

		public NetMessage CreateClientChatMessage(string message)
		{
			var sendBuffer = m_NetClient.CreateMessage(1 + message.Length);

			sendBuffer.Write((byte)NetMessageType.CHAT_MESSAGE);
			sendBuffer.Write(message);

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

