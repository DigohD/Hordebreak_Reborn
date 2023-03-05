using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace FNZ.Client.Net
{
	public class ClientNetworkConnector
	{
		private NetEntityList m_NetEntities;
		private Dictionary<NetMessageType, Action<ClientNetworkConnector, NetIncomingMessage>> m_PacketListenFuncTable;

		public ClientNetworkConnector()
		{
			m_NetEntities = new NetEntityList(1000);
			m_PacketListenFuncTable = new Dictionary<NetMessageType, Action<ClientNetworkConnector, NetIncomingMessage>>();
		}

		public void Initialize()
		{
			var assemblyNames = new[] { "FNZ.Client", "FNZ.Shared" };

			var types = new List<Type>();

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var assemblyName in assemblyNames)
				{
					if (assembly.GetName().Name == assemblyName)
					{
						var allTypes = assembly.GetTypes();

						var tempTypes = allTypes.Where(
							t => typeof(INetworkManager).IsAssignableFrom(t) && !t.IsInterface);

						foreach (var type in tempTypes)
						{
							types.Add(type);
						}
					}
				}
			}

			foreach (var type in types)
			{
				Activator.CreateInstance(type);
			}

			GameClient.NetConnector.Register(NetMessageType.SERVER_PING_CHECK, OnServerPing);
		}

		public void Register(NetMessageType packetType, Action<ClientNetworkConnector, NetIncomingMessage> listenerFunc)
		{
			if (m_PacketListenFuncTable.ContainsKey(packetType))
			{
				Debug.LogError("Listener function for packet type: " + packetType.ToString() + " has already been added");
				return;
			}

			m_PacketListenFuncTable.Add(packetType, listenerFunc);
		}

		public void Dispatch(NetMessageType packetType, NetIncomingMessage incMsg)
		{
			if (m_PacketListenFuncTable.ContainsKey(packetType))
			{
				m_PacketListenFuncTable[packetType].Invoke(this, incMsg);
			}
		}

		public FNEEntity GetEntity(int netId)
		{
			return m_NetEntities.GetFneEntity(netId);
		}

		public Entity GetEcsEntity(int netId)
		{
			return m_NetEntities.GetEcsEntity(netId);
		}

		public void SyncEntity(FNEEntity entity, int netId)
		{
			m_NetEntities.Add(entity, netId);
		}

		public void SyncEntity(Entity entity, int netId)
		{
			m_NetEntities.Add(entity, netId);
		}

		public void SyncEntity(FNEEntity entity)
		{
			m_NetEntities.Add(entity, entity.NetId);
		}

		public void UnsyncEntity(FNEEntity entity)
		{
			//Debug.Log($"[CLIENT]: Unsyncing entity with NetId: {entity.NetId}");
			m_NetEntities.Remove(entity.NetId);
		}

		public void UnsyncEntity(int netId)
		{
			//Debug.Log($"[CLIENT]: Unsyncing entity with NetId: {entity.NetId}");
			m_NetEntities.Remove(netId);
		}

		private void OnServerPing(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameClient.NetAPI.CMD_Server_Ping_Response();
		}

		public List<NetEntityWrapper> GetAllEntitiesOnViewChunk(int chunkX, int chunkY)
        {
			List<NetEntityWrapper> toReturn =  new List<NetEntityWrapper>();
			foreach(var entity in m_NetEntities.list)
            {
				if (entity.fneEntity == null)
					continue;

				if (entity.fneEntity.EntityType != EntityType.TILE_OBJECT && entity.fneEntity.EntityType != EntityType.EDGE_OBJECT)
					continue;

				var isInsideX = entity.fneEntity.Position.x >= chunkX * 32 && entity.fneEntity.Position.x < (chunkX + 1) * 32;
				var isInsideY = entity.fneEntity.Position.y >= chunkY * 32 && entity.fneEntity.Position.y < (chunkY + 1) * 32;
				if(isInsideX && isInsideY)
					toReturn.Add(entity);
			}
			return toReturn;
		}
	}
}
