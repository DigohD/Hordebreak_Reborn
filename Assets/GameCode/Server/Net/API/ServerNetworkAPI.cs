using FNZ.Server.Model.World;
using FNZ.Server.Net.Messages;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		private readonly NetServer m_NetServer;

		private readonly ServerWorldMessageFactory m_WorldMessageFactory;
		private readonly ServerMetaWorldMessageFactory m_MetaWorldMessageFactory;
		private readonly ServerEntityMessageFactory m_EntityMessageFactory;
		private readonly ServerHordeEntityMessageFactory m_HordeEntityMessageFactory;
		private readonly ServerPlayerMessageFactory m_PlayerMessageFactory;
		private readonly ServerChatMessageFactory m_ChatMessageFactory;
		private readonly ServerEffectMessageFactory m_EffectMessageFactory;
		private readonly ServerNotificationMessageFactory m_NotificationMessageFactory;
		private readonly ServerAudioMessageFactory m_AudioMessageFactory;
		private readonly ServerErrorMessageFactory m_ErrorMessageFactory;
		private readonly ServerItemOnGroundMessageFactory m_ItemOnGroundMessageFactory;
		private readonly ServerWorldEventMessageFactory m_WorldEventMessageFactory;
		private readonly ServerQuestMessageFactory m_QuestMessageFactory;
		
		private static Dictionary<Enum, List<Tuple<FNEComponent, NetConnection>>> m_ComponentUpdateBatch;
		private static Dictionary<Enum, List<Tuple<FNEEntity, NetConnection>>> m_EntityUpdatePosRotBatch;

		public ServerNetworkAPI(NetServer netServer)
		{
			m_NetServer = netServer;

			m_WorldMessageFactory = new ServerWorldMessageFactory(netServer);
			m_MetaWorldMessageFactory = new ServerMetaWorldMessageFactory(netServer);
			m_EntityMessageFactory = new ServerEntityMessageFactory(netServer);
			m_HordeEntityMessageFactory = new ServerHordeEntityMessageFactory(netServer);
			m_PlayerMessageFactory = new ServerPlayerMessageFactory(netServer);
			m_ChatMessageFactory = new ServerChatMessageFactory(netServer);
			m_EffectMessageFactory = new ServerEffectMessageFactory(netServer);
			m_NotificationMessageFactory = new ServerNotificationMessageFactory(netServer);
			m_AudioMessageFactory = new ServerAudioMessageFactory(netServer);
			m_ErrorMessageFactory = new ServerErrorMessageFactory(netServer);
			m_ItemOnGroundMessageFactory = new ServerItemOnGroundMessageFactory(netServer);
			m_ComponentUpdateBatch = new Dictionary<Enum, List<Tuple<FNEComponent, NetConnection>>>();
			m_EntityUpdatePosRotBatch = new Dictionary<Enum, List<Tuple<FNEEntity, NetConnection>>>();
			m_WorldEventMessageFactory = new ServerWorldEventMessageFactory(netServer);
			m_QuestMessageFactory = new ServerQuestMessageFactory(netServer);
		}

		/// <summary>
		/// Send method Abbreviation: STC
		/// </summary>
		private void SendToClient(NetMessage message, NetConnection clientConnection)
		{
			var result = clientConnection.SendMessage(
				message.Buffer,
				message.DeliveryMethod,
				(int)message.Channel
			);

			//HandleSendResult(result, message.Type);
		}

		/// <summary>
		/// Send method Abbreviation: BTC
		/// </summary>
		private void Broadcast_To_Clients(NetMessage message, List<NetConnection> clientConnections)
		{
			m_NetServer.SendMessage(
				message.Buffer,
				clientConnections,
				message.DeliveryMethod,
				(int)message.Channel
			);
		}

		/// <summary>
		/// Send method Abbreviation: BA
		/// </summary>
		private void Broadcast_All(NetMessage message)
		{
			if (m_NetServer.Connections.Count == 0)
				return;

			m_NetServer.SendMessage(
				message.Buffer,
				m_NetServer.Connections,
				message.DeliveryMethod,
				(int)message.Channel
			);
		}

		/// <summary>
		/// Send method Abbreviation: BO
		/// </summary>
		private void Broadcast_Other(NetMessage message, NetConnection connToExclude)
		{
			var connectionsToBroadcast = new List<NetConnection>();

			foreach (var nc in m_NetServer.Connections)
			{
				if (connToExclude == nc) continue;
				connectionsToBroadcast.Add(nc);
			}

			if (connectionsToBroadcast.Count > 0)
			{
				m_NetServer.SendMessage(
				   message.Buffer,
				   connectionsToBroadcast,
				   message.DeliveryMethod,
				   (int)message.Channel
				);
			}
		}

		/// <summary>
		/// Send method Abbreviation: BAR
		/// </summary>
		private void Broadcast_All_Relevant(NetMessage message, float2 impactPosition)
		{
			var world = GameServer.World;
			var chunkManager = GameServer.ChunkManager;
			var chunk = world.GetWorldChunk<ServerWorldChunk>(impactPosition);

			if (chunk != null)
			{
				var chunksToCheck = world.GetNeighbouringChunks(chunk);
				var connections = new List<NetConnection>();

				foreach (var chunkToCheck in chunksToCheck)
				{
					chunkManager.GetConnectionsWithChunkLoaded(chunkToCheck, ref connections);
				}

				if (connections.Count > 0)
					m_NetServer.SendMessage(
					   message.Buffer,
					   connections,
					   message.DeliveryMethod,
					   (int)message.Channel
					);
			}
		}

		private void Broadcast_All_InProximity(NetMessage message, float2 impactPosition, float radius)
		{
			var connectedPlayers = GameServer.World.GetAllPlayers();

			var relevantConnections = new List<NetConnection>();

			foreach (var player in connectedPlayers)
			{
				var playerPos = player.Position;
				if (math.distance(playerPos, impactPosition) <= radius)
				{
					relevantConnections.Add(GameServer.NetConnector.GetConnectionFromPlayer(player));
				}
			}
			
			if (relevantConnections.Count > 0)
				m_NetServer.SendMessage(
					message.Buffer,
					relevantConnections,
					message.DeliveryMethod,
					(int)message.Channel
				);
		}

		/// <summary>
		/// Send method Abbreviation: BARB
		/// </summary>
		private void Broadcast_All_Relevant_Batch(NetMessage message, ref NativeArray<float2> impactPositions)
		{
			var world = GameServer.World;
			var chunks = new List<ServerWorldChunk>();
			var recipients = new List<NetConnection>();

			foreach (var pos in impactPositions)
			{
				var chunk = world.GetWorldChunk<ServerWorldChunk>(pos);
				if (!chunks.Contains(chunk))
					chunks.Add(world.GetWorldChunk<ServerWorldChunk>(pos));
			}

			foreach (var chunk in chunks)
			{
				if (chunk != default)
				{
					var chunksToCheck = world.GetNeighbouringChunks(chunk);

					foreach (var chunkToCheck in chunksToCheck)
						GameServer.ChunkManager.GetConnectionsWithChunkLoaded(chunkToCheck, ref recipients);
				}
			}

			if (recipients.Count > 0)
				m_NetServer.SendMessage(
				   message.Buffer,
				   recipients,
				   message.DeliveryMethod,
				   (int)message.Channel
				);

		}

		/// <summary>
		/// Send method Abbreviation: BOR
		/// </summary>
		private void Broadcast_Other_Relevant(NetMessage message, float2 impactPosition, NetConnection connToExclude)
		{
			var world = GameServer.World;
			var chunkManager = GameServer.ChunkManager;
			var chunk = world.GetWorldChunk<ServerWorldChunk>(impactPosition);

			if (chunk != default)
			{
				var chunksToCheck = world.GetNeighbouringChunks(chunk);
				var connections = new List<NetConnection>();

				foreach (var chunkToCheck in chunksToCheck)
				{
					chunkManager.GetConnectionsWithChunkLoaded(chunkToCheck, ref connections);
				}

				connections.Remove(connToExclude);

				if (connections.Count > 0)
				{
					m_NetServer.SendMessage(
					   message.Buffer,
					   connections,
					   message.DeliveryMethod,
					   (int)message.Channel
					);
				}
			}
		}

		/// <summary>
		///     Client_Immediate_Forward is used when a packet from a client should be directly forwarded to all other clients.
		///     This allows for packets to be forwarded without recreating the messages of the packets.
		/// </summary>
		public void Client_Immediate_Forward_To_Other(byte[] messageData, NetConnection connToExclude)
		{
			var connectionsToBroadcast = new List<NetConnection>();
			foreach (var nc in GameServer.NetConnector.GetConnectedClientConnections())
			{
				if (connToExclude == nc) continue;
				connectionsToBroadcast.Add(nc);
			}

			var sendBuffer = m_NetServer.CreateMessage(messageData.Length);
			sendBuffer.Write(messageData, 0, messageData.Length);

			if (connectionsToBroadcast.Count > 0)
			{
				m_NetServer.SendMessage(
				   sendBuffer,
				   connectionsToBroadcast,
				   NetDeliveryMethod.Unreliable,
				   0
				);
			}
		}

		/// <summary>
		/// Regularly sent ping message meant to see if anyone has disconnected.
		/// </summary>
		public void Server_Ping_Clients()
		{
			var sendBuffer = m_NetServer.CreateMessage(1);
			sendBuffer.Write((byte)NetMessageType.SERVER_PING_CHECK);

			var message = new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SERVER_PING_CHECK,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.PLAYER_STATE,
			};

			Broadcast_All(message);
		}

		private void HandleSendResult(NetSendResult result, NetMessageType messageType)
		{
			string messageTypeName = messageType.ToString();

			switch (result)
			{
				case NetSendResult.FailedNotConnected:
					Debug.LogError("[Server] Message failed to enqueue because there is no connection. MessageType: " + messageTypeName);
					break;
				case NetSendResult.Sent:
					Debug.Log("[Server] Message was immediately sent. MessageType: " + messageTypeName);
					break;
				case NetSendResult.Queued:
					Debug.Log("[Server] Message was queued for delivery. MessageType: " + messageTypeName);
					break;
				case NetSendResult.Dropped:
					Debug.LogWarning("[Server] Message was dropped immediately since too many message were queued. MessageType: " + messageTypeName);
					break;
			}
		}

	}
}

