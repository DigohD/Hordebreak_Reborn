using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.Messages
{
	internal class ServerErrorMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerErrorMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateServerErrorMessage(string errorTitle, string errorInfo)
		{
			var sendBuffer = m_NetServer.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.ERROR_MESSAGE);

			sendBuffer.Write(errorTitle);
			sendBuffer.Write(errorInfo);


			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.ERROR_MESSAGE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ERROR_MESSAGE,
			};
		}

	}
}