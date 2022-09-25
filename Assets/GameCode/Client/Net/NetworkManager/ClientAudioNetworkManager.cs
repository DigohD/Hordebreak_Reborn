using FNZ.Client.View.Audio;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientAudioNetworkManager : INetworkManager
	{
		public ClientAudioNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.AUDIO_MUSIC, OnMusicMessage);
		}

		private void OnMusicMessage(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			string id = incMsg.ReadString();
			var hasTimer = incMsg.ReadBoolean();
			float timer;
			if (hasTimer)
			{
				timer = incMsg.ReadFloat();
				AudioManager.Instance.PlayMusic(id, timer);
			}
			else
				AudioManager.Instance.PlayMusic(id);

		}
	}
}