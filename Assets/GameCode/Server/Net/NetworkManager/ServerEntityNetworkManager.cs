using FNZ.Server.Model.Entity.Components;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using System.Net.Http.Headers;

namespace FNZ.Server.Net.NetworkManager
{
	public class ServerEntityNetworkManager : INetworkManager
	{
		public ServerEntityNetworkManager()
		{
			GameServer.NetConnector.Register(NetMessageType.UPDATE_ENTITY, OnComponentUpdate);
			GameServer.NetConnector.Register(NetMessageType.UPDATE_COMPONENT, OnComponentUpdate);
			GameServer.NetConnector.Register(NetMessageType.COMPONENT_NET_EVENT, OnComponentNetEvent);
			GameServer.NetConnector.Register(NetMessageType.PLAYER_HEALTH_UPDATE, OnPlayerHealthUpdate);
			GameServer.NetConnector.Register(NetMessageType.CLIENT_LEFT_SERVER, OnClientLeftServer);
			GameServer.NetConnector.Register(NetMessageType.CLIENT_CONFIRM_SPAWN_HORDE_ENTITY_BATCH, OnClientConfirmSpawnHordeEntityBatch);
			GameServer.NetConnector.Register(NetMessageType.CLIENT_CONFIRM_DESTROY_HORDE_ENTITY_BATCH, OnClientConfirmDestroyHordeEntityBatch);
		}

		private static void OnClientConfirmSpawnHordeEntityBatch(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			//var state = GameServer.ChunkManager.GetPlayerChunkState(incMsg.SenderConnection);
			
			var count = incMsg.ReadUInt16();

			for (var i = 0; i < count; i++)
			{
				var netId = incMsg.ReadInt32();
				var entity = net.GetEntity(netId);
				
				if (!entity.Enabled)
					entity.Enabled = true;

				if (entity.Agent != null && !entity.Agent.active)
				{
					entity.Agent.active = true;
				}
				
				//state.MovingEntitiesSynced.Add(netId);
			}
		}
		
		private static void OnClientConfirmDestroyHordeEntityBatch(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			//var state = GameServer.ChunkManager.GetPlayerChunkState(incMsg.SenderConnection);
			
			// var count = incMsg.ReadUInt16();
			//
			// for (var i = 0; i < count; i++)
			// {
			// 	var netId = incMsg.ReadInt32();
			// 	//state.MovingEntitiesSynced.Remove(netId);
			// }
		}

		private void OnPlayerHealthUpdate(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity = net.GetPlayerFromConnection(incMsg.SenderConnection);
			var healthComponent = entity.GetComponent<StatComponentServer>();

			float amount = incMsg.ReadFloat();
			string damageTypeRef = incMsg.ReadString();
			bool isDamage = incMsg.ReadBoolean();

			if (isDamage)
				healthComponent.Server_ApplyDamage(amount, damageTypeRef);
			else
				healthComponent.Heal(amount);

			GameServer.NetAPI.Entity_UpdateComponent_BA(healthComponent);
		}

		private void OnComponentUpdate(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity = net.GetEntity(incMsg.ReadInt32());
			Type compType = FNEComponent.ComponentIdTypeDict[incMsg.ReadUInt16()];

			foreach (var component in entity.Components)
			{
				if (component.GetType().BaseType == compType)
				{
					component.Deserialize(incMsg);

					GameServer.NetAPI.Entity_UpdateComponent_BOR(component, incMsg.SenderConnection);
					break;
				}
			}
		}

		private void OnComponentNetEvent(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity entity = net.GetEntity(incMsg.ReadInt32());
			Type compType = FNEComponent.ComponentIdTypeDict[incMsg.ReadUInt16()];

			foreach (var component in entity.Components)
			{
				if (component.GetType().BaseType == compType)
				{
					component.OnNetEvent(incMsg);
					break;
				}
			}
		}

		private void OnClientLeftServer(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameServer.EntityFactory.RemovePlayer(incMsg.SenderConnection);
			incMsg.SenderConnection.Disconnect(string.Empty);
		}

	}
}