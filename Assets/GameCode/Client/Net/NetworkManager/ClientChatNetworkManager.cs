using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.StaticData;
using FNZ.Client.View.UI.Chat;
using FNZ.Shared.Constants;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;

namespace FNZ.Client.Net.NetworkManager
{

	public class ClientChatNetworkManager : INetworkManager
	{
		public ClientChatNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.CHAT_MESSAGE, OnChatMessageReceived);
		}

		private void OnChatMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			string messageString = incMsg.ReadString();
			long senderID = incMsg.ReadInt64();

			var self = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();

			if (messageString == ChatCommandConstants.COMMAND_PING)
			{
				TimeSpan ping = DateTime.Now - NetData.PING_TIMESTAMP;
				UI_Chat.NewMessage(ping.Milliseconds.ToString());
				return;
			}

			if (!self.IsPlayerMuted(senderID))
				UI_Chat.NewMessage(messageString);
		}
	}
}