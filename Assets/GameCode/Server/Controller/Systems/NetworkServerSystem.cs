using FNZ.Server.Model;
using FNZ.Server.Model.World;
using FNZ.Server.Net;
using FNZ.Server.Net.API;
using FNZ.Server.Services;
using FNZ.Shared;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Server.Controller.Systems
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class NetworkServerSystem : SystemBase
	{
		private NetServer m_Server;
		private ServerNetworkConnector m_NetConnector;

		public void InitializeServer(string appIdentifier, int port, int maxConnections)
		{
			var config = new NetPeerConfiguration(appIdentifier)
			{
				Port = port,
				MaximumConnections = maxConnections
			};

			m_Server = new NetServer(config);
			m_Server.Start();

			GameServer.NetAPI = new ServerNetworkAPI(m_Server);
			GameServer.NetConnector = new ServerNetworkConnector();
			m_NetConnector = GameServer.NetConnector;
			m_NetConnector.Initialize();
			m_Server.Start();
		}

		protected override void OnCreate()
		{
			InitializeServer(SharedConfigs.AppIdentifier, 7676, 5);
		}

		protected override void OnUpdate()
		{
			NetIncomingMessage msg;
			Profiler.BeginSample("Server NetLoop");
			while ((msg = m_Server.ReadMessage()) != null)
			{
				msg.m_readPosition = 0;

				switch (msg.MessageType)
				{
					case NetIncomingMessageType.Data:
						Profiler.BeginSample("ParsePacket");
						ParsePacket(msg);
						Profiler.EndSample();
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						break;
					case NetIncomingMessageType.StatusChanged:
						switch (msg.SenderConnection.Status)
						{
							case NetConnectionStatus.Connected:
								OnClientConnected(msg.SenderConnection);
								break;

							case NetConnectionStatus.Disconnected:

								OnClientDisconnected(msg.SenderConnection);
								break;
						}
						break;
					default:
						ParsePacket(msg);
						break;
				}
			}
			Profiler.EndSample();
		}

		private void ParsePacket(NetIncomingMessage incMsg)
		{
			m_NetConnector.Dispatch((NetMessageType)incMsg.ReadByte(), incMsg);
			m_Server.Recycle(incMsg);
		}

		//"Ooh, when least expected..."
		private void OnClientConnected(NetConnection clientConnection)
		{
			GameServer.NetAPI.World_WorldSetup_STC(
				GameServer.MainWorld.WIDTH,
				GameServer.MainWorld.HEIGHT,
				GameServer.MainWorld.CHUNK_SIZE,
				clientConnection
			);

		}

		private void OnClientDisconnected(NetConnection clientConnection)
		{

		}

		protected override void OnDestroy()
		{
			m_Server.Shutdown("Exit");
		}
	}
}
