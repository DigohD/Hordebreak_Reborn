using Lidgren.Network;
using System.Collections.Generic;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		public void Error_SendErrorMessage_BA(string errorName, string errorInfo)
		{
			var message = m_ErrorMessageFactory.CreateServerErrorMessage(errorName, errorInfo);
			Broadcast_All(message);
		}

		public void Error_SendErrorMessage_BTC(List<NetConnection> connections, string errorName, string errorInfo)
		{
			var message = m_ErrorMessageFactory.CreateServerErrorMessage(errorName, errorInfo);
			Broadcast_To_Clients(message, connections);
		}

		public void Error_SendErrorMessage_STC(NetConnection player, string errorName, string errorInfo)
		{
			var message = m_ErrorMessageFactory.CreateServerErrorMessage(errorName, errorInfo);
			SendToClient(message, player);
		}

		public void Error_PlayerServerConnection(string errorName, string errorMessage, NetConnection senderConnection)
		{
			var message = m_EntityMessageFactory.CreatePlayerServerConnectionErrorMessage(errorName, errorMessage);
			SendToClient(message, senderConnection);
		}

	}
}