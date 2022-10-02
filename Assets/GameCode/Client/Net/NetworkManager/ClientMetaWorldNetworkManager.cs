using FNZ.Shared.Model;
using FNZ.Shared.Net;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.Net.NetworkManager 
{

	public class ClientMetaWorldNetworkManager : INetworkManager
	{
		private void OnMetaWorldUpdateMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameClient.MetaWorld.DeserializeMetaWorld(incMsg);
			var testPlace = GameClient.MetaWorld.Places[0];
			Debug.LogWarning(testPlace.Coords);
			Debug.LogWarning(testPlace.Name);
		}
	}
}