using FNZ.Shared.Model;
using FNZ.Shared.Net;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.Net.NetworkManager 
{
	public delegate void OnMetaWorldUpdate();
	public class ClientMetaWorldNetworkManager : INetworkManager
	{
		public static OnMetaWorldUpdate d_OnMetaWorldUpdate;

		public ClientMetaWorldNetworkManager()
        {
			GameClient.NetConnector.Register(NetMessageType.META_WORLD_UPDATE, OnMetaWorldUpdateMessageReceived);
		}

		private void OnMetaWorldUpdateMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameClient.MetaWorld.DeserializeMetaWorld(incMsg);
			var testPlace = GameClient.MetaWorld.Places[0];
			Debug.LogWarning(testPlace.Coords);
			Debug.LogWarning(testPlace.Name);
			d_OnMetaWorldUpdate?.Invoke();
		}
	}
}