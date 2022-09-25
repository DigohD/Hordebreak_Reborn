using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Net.NetworkManager.Chat;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using UnityEngine;

namespace FNZ.Server.Net.NetworkManager
{
	public class ServerChatNetworkManager : INetworkManager
	{
		private ChatCommands m_Commands;
		private ChatEmotes m_Emotes;

		public ServerChatNetworkManager()
		{
			m_Commands = new ChatCommands();
			m_Emotes = new ChatEmotes();
			GameServer.NetConnector.Register(NetMessageType.CHAT_MESSAGE, OnChatMessageReceived);
		}

		private void OnChatMessageReceived(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			string messageString = incMsg.ReadString();
			FNEEntity messageSender = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);
			string playerName = messageSender.GetComponent<NameComponentServer>().entityName;

			if (messageString == null || messageString.Length == 0)
				Debug.LogError($"Received a message from {incMsg.SenderConnection} who's message string was NULL or har 0 in .Length");

			if (messageString.StartsWith("/"))
			{
				string[] playerStringParts = messageString.ToLower().Substring(1).Split(' ');
				string command = playerStringParts[0];

				if (command != string.Empty)
				{
					bool wasEmote = m_Emotes.TryExecuteEmotes(playerName, command, incMsg.SenderConnection);

					if (!wasEmote)
					{
						bool isOP = net.GetPlayerFromConnection(incMsg.SenderConnection).GetComponent<PlayerComponentServer>().IsOP;
                        try
						{
							m_Commands.ExecuteCommands(playerStringParts, isOP, incMsg);
						} catch(Exception e)
						{
							Debug.LogError($"Shit crashed in chatcommands yo, Command: {playerStringParts}, Trace: {e.StackTrace}");
						}
					}
				}
			}
			else
				GameServer.NetAPI.Chat_SendMessage_BAR(ChatColorMessage.ColorMessage($"{playerName}: ", ChatColorMessage.MessageType.PLAYERNAME) + messageString, messageSender.Position, ChatColorMessage.MessageType.LOCAL, incMsg.SenderConnection.RemoteUniqueIdentifier);
		}
	}
}