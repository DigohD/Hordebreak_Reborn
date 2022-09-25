using Lidgren.Network;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		public void Audio_ChangeMusic_STC(NetConnection clientConnection, string id, float timer = 0)
		{
			var message = m_AudioMessageFactory.CreateMusicMessage(id, timer);
			SendToClient(message, clientConnection);
		}

	}
}