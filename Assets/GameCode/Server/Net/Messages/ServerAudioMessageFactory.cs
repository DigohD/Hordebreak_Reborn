using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.Messages
{
	internal class ServerAudioMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerAudioMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateMusicMessage(string id, float timer)
		{
			bool hasTimer = false;
			if (timer != 0)
				hasTimer = true;

			var sendBuffer = m_NetServer.CreateMessage(1 + id.Length);
			sendBuffer.Write((byte)NetMessageType.AUDIO_MUSIC);

			sendBuffer.Write(id);
			sendBuffer.Write(hasTimer);
			if (hasTimer)
				sendBuffer.Write(timer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.AUDIO_MUSIC,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.PLAYER_STATE,
			};
		}

	}
}