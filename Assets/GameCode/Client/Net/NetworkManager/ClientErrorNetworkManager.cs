using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Net;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientErrorNetworkManager : INetworkManager
	{
		public ClientErrorNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.ERROR_MESSAGE, OnErrorMessageReceived);
		}

		private void OnErrorMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var errorTitle = incMsg.ReadString();
			var errorInfo = incMsg.ReadString();

			Debug.LogError(errorTitle + " \n" + errorInfo);
		}
	}
}