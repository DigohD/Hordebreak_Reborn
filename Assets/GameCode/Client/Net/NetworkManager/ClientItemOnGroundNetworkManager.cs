using FNZ.Client.View.Audio;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using FNZ.Shared.Net;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientItemOnGroundNetworkManager : INetworkManager
	{
		public ClientItemOnGroundNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.SPAWN_ITEM_ON_GROUND, OnSpawnItemOnGroundMessage);
			GameClient.NetConnector.Register(NetMessageType.REMOVE_ITEM_ON_GROUND, OnRemoveItemOnGroundMessage);
		}

		private void OnSpawnItemOnGroundMessage(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var identifier = incMsg.ReadInt64();
			var position = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
			var item = new Item();
			item.Deserialize(incMsg);

			GameClient.ItemsOnGroundManager.AddItemOnGround(new ItemOnGround{
				identifier = identifier,
				item = item,
				Position = position,
				FlingDirectionZ = 0
			});
		}

		private void OnRemoveItemOnGroundMessage(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var identifier = incMsg.ReadInt64();

			GameClient.ItemsOnGroundManager.RemoveItemOnGround(identifier);
		}
	}
}