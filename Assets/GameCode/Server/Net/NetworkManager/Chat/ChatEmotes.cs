using FNZ.Server.Utils;
using FNZ.Shared.Constants;
using Lidgren.Network;

namespace FNZ.Server.Net.NetworkManager.Chat
{

	public class ChatEmotes
	{
		public bool TryExecuteEmotes(string playerName, string emote, NetConnection senderConnection)
		{
			switch (emote.ToLower())
			{
				case ChatEmoteConstants.EMOTE_CLAP:
					GameServer.NetAPI.Chat_SendMessage_STC("You clap your hands.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					GameServer.NetAPI.Chat_SendMessage_BOR(ChatColorMessage.ColorMessage($"{playerName} ", ChatColorMessage.MessageType.PLAYERNAME) + "is clapping their hands.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					return true;

				case ChatEmoteConstants.EMOTE_DANCE:
					GameServer.NetAPI.Chat_SendMessage_STC("You bust out some sick moves!", senderConnection, ChatColorMessage.MessageType.EMOTE);
					GameServer.NetAPI.Chat_SendMessage_BOR(ChatColorMessage.ColorMessage($"{playerName} ", ChatColorMessage.MessageType.PLAYERNAME) + "starts to dance.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					return true;

				case ChatEmoteConstants.EMOTE_FART:
					GameServer.NetAPI.Chat_SendMessage_STC("You let out a loud fart. It stinks to high heaven!", senderConnection, ChatColorMessage.MessageType.EMOTE);
					GameServer.NetAPI.Chat_SendMessage_BOR(ChatColorMessage.ColorMessage($"{playerName} ", ChatColorMessage.MessageType.PLAYERNAME) + "farts!", senderConnection, ChatColorMessage.MessageType.EMOTE);
					return true;

				case ChatEmoteConstants.EMOTE_GLARE:
					GameServer.NetAPI.Chat_SendMessage_STC("You glare at the people around you.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					GameServer.NetAPI.Chat_SendMessage_BOR(ChatColorMessage.ColorMessage($"{playerName} ", ChatColorMessage.MessageType.PLAYERNAME) + "glares at the people around them. Someone's in a sour mood.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					return true;

				case ChatEmoteConstants.EMOTE_WAVE:
					GameServer.NetAPI.Chat_SendMessage_STC("You wave happily to everyone around you.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					GameServer.NetAPI.Chat_SendMessage_BOR(ChatColorMessage.ColorMessage($"{playerName} ", ChatColorMessage.MessageType.PLAYERNAME) + "waves at everyone around them.", senderConnection, ChatColorMessage.MessageType.EMOTE);
					return true;

				default:
					return false;
			}
		}
	}
}