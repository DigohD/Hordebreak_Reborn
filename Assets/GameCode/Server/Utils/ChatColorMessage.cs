namespace FNZ.Server.Utils
{

	public class ChatColorMessage
	{
		public enum MessageType
		{
			DEFAULT = 0,
			ERROR = 1,
			EMOTE = 2,
			COMMAND = 3,
			WHISPER = 4,
			LOCAL = 5,
			GLOBAL = 6,
			AFFECTED = 7,
			SERVER = 8,
			PLAYERNAME = 9,
			WARNING = 10,
		}

		public static string ColorMessage(string message, MessageType messageType)
		{
			switch (messageType)
			{
				case MessageType.COMMAND:
					return "<color=#00FF00FF>" + message + "</color>";

				case MessageType.ERROR:
					return "<color=#FF0000FF>" + message + "</color>";

				case MessageType.EMOTE:
					return "<color=#FFFF00FF>" + message + "</color>";

				case MessageType.WHISPER:
					return "<color=#6600FFFF>" + message + "</color>";

				case MessageType.LOCAL:
					return "<color=#FFFFFFFF>" + message + "</color>";

				case MessageType.GLOBAL:
					return "<color=#3CE13FFF>" + message + "</color>";

				case MessageType.AFFECTED:
					return "<color=#FF00FFFF>" + message + "</color>";

				case MessageType.SERVER:
					return "<color=#BABABAFF>" + message + "</color>";

				case MessageType.PLAYERNAME:
					return "<color=#00FFFFFF>" + message + "</color>";

				case MessageType.WARNING:
					return "<color=#FFAA00FF>" + message + "</color>";

				default:
					return message;

			}
		}
	}
}