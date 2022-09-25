using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FNZ.Server.Net
{
	public class ServerNetworkConnector
	{
		private NetEntityList m_NetEntities;
		private Dictionary<NetMessageType, Action<ServerNetworkConnector, NetIncomingMessage>> m_PacketListenFuncTable;
		private Dictionary<FNEEntity, NetConnection> m_ConnectedClients;
		private NetConnection m_ServerHostConnection;

		public ServerNetworkConnector()
		{
			m_NetEntities = new NetEntityList(1000);
			m_PacketListenFuncTable = new Dictionary<NetMessageType, Action<ServerNetworkConnector, NetIncomingMessage>>();
			m_ConnectedClients = new Dictionary<FNEEntity, NetConnection>();
		}

		public void Initialize()
		{
			var assemblyNames = new[] { "FNZ.Server", "FNZ.Shared" };

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
		}

		public void Register(NetMessageType packetType, Action<ServerNetworkConnector, NetIncomingMessage> listenerFunc)
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

		public void AddConnectedClient(FNEEntity clientEntity, NetConnection clientConnection)
		{
			m_ConnectedClients.Add(clientEntity, clientConnection);

			if (m_ConnectedClients.Count == 1)
			{
				m_ServerHostConnection = clientConnection;
				clientEntity.GetComponent<PlayerComponentServer>().IsOP = true;
			}
		}

		public void RemoveDisconnectedClient(FNEEntity clientEntity)
		{
			if (m_ConnectedClients.ContainsKey(clientEntity))
				m_ConnectedClients.Remove(clientEntity);
		}

		public NetConnection GetServerHostConnection()
		{
			return m_ServerHostConnection;
		}

		public FNEEntity GetPlayerFromConnection(NetConnection conn)
		{
			foreach (var player in m_ConnectedClients.Keys)
			{
				if (m_ConnectedClients[player] == conn)
					return player;
			}

			return null;
		}

		public bool IsPlayerConnected(string name)
		{
			return GetConnectionFromName(name) != null;
		}

		public FNEEntity GetPlayerFromName(string name)
		{
			foreach (var player in m_ConnectedClients.Keys)
			{
				if (player.GetComponent<NameComponentServer>().entityName.ToLower() == name.ToLower())
					return player;
			}

			return null;
		}

		public NetConnection GetConnectionFromName(string playerName)
		{
			foreach (var player in m_ConnectedClients.Keys)
			{
				if (player.GetComponent<NameComponentServer>().entityName.ToLower() == playerName.ToLower())
					return m_ConnectedClients[player];
			}

			return null;
		}

		public NetConnection GetConnectionFromPlayer(FNEEntity player)
		{
			if (m_ConnectedClients.ContainsKey(player))
				return m_ConnectedClients[player];

			return null;
		}

		public long GetUniqueIdFromConnection(NetConnection playerConnection)
		{
			return playerConnection.RemoteUniqueIdentifier;
		}

		public IEnumerable<NetConnection> GetConnectedClientConnections() => m_ConnectedClients.Values;
		public IEnumerable<FNEEntity> GetConnectedClientEntities() => m_ConnectedClients.Keys;

		public List<string> GetOfflineClients()
		{
			var list = new List<string>();

			var path = GameServer.FilePaths.GetSavedPlayersPath();
			if (path != string.Empty)
			{
				foreach (var folderPath in Directory.GetDirectories(path))
					list.Add(Path.GetFileName(folderPath));

				return list;
			}

			return list;
		}

		public int GetConnectedClientsCount()
		{
			return m_ConnectedClients.Count;
		}

		public void SyncEntity(FNEEntity entity)
		{
			m_NetEntities.Add(entity);
		}

		public void UnsyncEntity(FNEEntity entity)
		{
			m_NetEntities.Remove(entity.NetId);
		}

		public FNEEntity GetEntity(int netId)
		{
			return m_NetEntities.GetEntity(netId);
		}

		public int GetNumberOfSyncedEntities()
		{
			return m_NetEntities.GetNumOfSyncedEntities();
		}
	}
}

