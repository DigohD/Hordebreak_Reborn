using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		//The optional parameters "senderID" is needed for if a message is meant to be ignored or not when received by the client.	

		public void Chat_SendMessage_BA(string messageString, ChatColorMessage.MessageType channel, long senderID = 0)
		{
			messageString = ColorizeString(messageString, channel);
			var message = m_ChatMessageFactory.CreateChatMessage(messageString, senderID);
			Broadcast_All(message);
		}

		public void Chat_SendMessage_BAR(string messageString, float2 playerPosition, ChatColorMessage.MessageType channel, long senderID = 0)
		{
			messageString = ColorizeString(messageString, channel);
			var message = m_ChatMessageFactory.CreateChatMessage(messageString, senderID);
			Broadcast_All_Relevant(message, playerPosition);
		}

		public void Chat_SendMessage_BO(string messageString, NetConnection playerConnection, ChatColorMessage.MessageType channel, long senderID = 0)
		{
			messageString = ColorizeString(messageString, channel);
			var message = m_ChatMessageFactory.CreateChatMessage(messageString, senderID);
			Broadcast_Other(message, playerConnection);
		}

		public void Chat_SendMessage_BOR(string messageString, NetConnection playerConnection, ChatColorMessage.MessageType channel, long senderID = 0)
		{
			messageString = ColorizeString(messageString, channel);
			var message = m_ChatMessageFactory.CreateChatMessage(messageString, senderID);
			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(playerConnection);
			Broadcast_Other_Relevant(message, player.Position, playerConnection);
		}
		public void Chat_SendMessage_STC(string messageString, NetConnection playerConnection, ChatColorMessage.MessageType channel, long senderID = 0)
		{
			messageString = ColorizeString(messageString, channel);
			var message = m_ChatMessageFactory.CreateChatMessage(messageString, senderID);
			SendToClient(message, playerConnection);
		}

		private string ColorizeString(string message, ChatColorMessage.MessageType channel)
		{
			message = ChatColorMessage.ColorMessage(message, channel);
			return message;
		}
	}
}