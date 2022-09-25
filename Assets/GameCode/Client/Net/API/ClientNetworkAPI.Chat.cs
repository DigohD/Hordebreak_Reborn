namespace FNZ.Client.Net.API
{
	public partial class ClientNetworkAPI
	{
		public void CMD_Chat_ChatMessage(string messageString)
		{
			var message = m_ChatMessageFactory.CreateClientChatMessage(messageString);
			Command(message);
		}
	}
}