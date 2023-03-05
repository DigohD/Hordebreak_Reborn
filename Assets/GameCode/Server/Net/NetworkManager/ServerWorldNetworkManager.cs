using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World;
using FNZ.Server.Services;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Server.Net.NetworkManager
{
	public class ServerWorldNetworkManager : INetworkManager
	{
		public ServerWorldNetworkManager()
		{
			GameServer.NetConnector.Register(NetMessageType.REQUEST_WORLD_SPAWN, OnRequestWorldSpawnPacketRecieved);
			GameServer.NetConnector.Register(NetMessageType.REQUEST_WORLD_INSTANCE, OnRequestWorldInstanceSpawnPacketRecieved);
			GameServer.NetConnector.Register(NetMessageType.CLIENT_CONFIRM_CHUNK_LOADED, OnClientConfirmChunkLoaded);
			//GameServer.NetConnector.Register(NetMessageType.CLIENT_CONFIRM_CHUNK_UNLOADED, OnClientConfirmChunkUnloaded);
			GameServer.NetConnector.Register(NetMessageType.BASE_ROOM_NAME_CHANGE, OnBaseRoomNameChange);
			GameServer.NetConnector.Register(NetMessageType.SERVER_PING_CHECK, OnServerPingResponse);
		}

		private void OnRequestWorldSpawnPacketRecieved(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			var clientConnection = incMsg.SenderConnection;
			string playerName = incMsg.ReadString();

			if (GameServer.NetConnector.IsPlayerConnected(playerName))
			{
				GameServer.NetAPI.Error_PlayerServerConnection("Error: Name", "Your name was already taken.", clientConnection);				
				clientConnection.Disconnect(string.Empty);
				return;
			}
			
			foreach (var connection in net.GetConnectedClientConnections())
			{
				GameServer.NetAPI.Player_SpawnRemote_STC(
					net.GetPlayerFromConnection(connection),
					clientConnection
				);
			}

			var playerNames = GameServer.NetConnector.GetOfflineClients();

			float2 playerPosition = float2.zero;
			FNEEntity newPlayer = null;

			var mainWorld = GameServer.GetWorldInstance(0);

			if (playerNames.Count > 0 && playerNames.Contains(playerName))
			{
				newPlayer = GameServer.EntityFactory.CreatePlayer(playerPosition, playerName);
			}
			else
			{
				// Spawn player in the middle of a chunk
				playerPosition = new float2(mainWorld.WIDTH / 2, mainWorld.HEIGHT / 2);
				newPlayer = GameServer.EntityFactory.CreatePlayer(playerPosition, playerName);
			}

			net.SyncEntity(newPlayer);
			
			if (newPlayer != null)
			{
				mainWorld.AddTickableEntity(newPlayer);
			}

			newPlayer.Enabled = true;
			
			net.AddConnectedClient(newPlayer, clientConnection);

			var playerComp = newPlayer.GetComponent<PlayerComponentServer>();

			playerComp.HomeLocation = newPlayer.Position;
			GameServer.NetAPI.Player_SpawnLocal_STC(newPlayer, clientConnection);
			GameServer.NetAPI.Player_SpawnRemote_BO(newPlayer, clientConnection);
			GameServer.NetAPI.Effect_SpawnEffect_BOR(EffectIdConstants.TELEPORT, newPlayer.Position, 0, clientConnection);
			
			var chunk = mainWorld.GetWorldChunk<ServerWorldChunk>();
			var netBuffer = new NetBuffer();
			netBuffer.EnsureBufferSize(chunk.TotalBitsNetBuffer());
			chunk.NetSerialize(netBuffer);
			GameServer.NetAPI.World_LoadChunk_STC(chunk, netBuffer.Data, clientConnection);

			playerComp.LastChunk = mainWorld.GetWorldChunk<ServerWorldChunk>();

			GameServer.NetAPI.World_Environment_STC(clientConnection, mainWorld);

			if (net.GetConnectedClientsCount() > 1)
				GameServer.NetAPI.Chat_SendMessage_BO($"{playerName} connected to server.", clientConnection, Utils.ChatColorMessage.MessageType.SERVER);

			foreach(var item in GameServer.ItemsOnGroundManager.GetItemsOnGround().Values)
			{
				GameServer.NetAPI.STC_Spawn_Item_On_Ground(item, clientConnection);
			}
			
			QuestManager.SendActiveQuestsToPlayer(incMsg.SenderConnection);
			QuestManager.InitIfNotInitialized();
			
			GameServer.NetAPI.World_RoomManager_STC(clientConnection);
			GameServer.NetAPI.MetaWorld_Update_STC(clientConnection);

			// Update new player's world map
			var chunkPaths = GameServer.FilePaths.GetAllChunkPaths();
			
			foreach (var chunkPath in chunkPaths)
			{
				var chunkReader = new ServerWorldChunk(mainWorld.WIDTH, mainWorld);
				
				var nb = new NetBuffer
				{
					Data = FileUtils.ReadFile(chunkPath.Item2)
				};
				
				chunkReader.FileDeserializeTilesOnly(nb);
				GameServer.NetAPI.World_ChunkMapUpdate_STC(clientConnection, chunkReader);
			}
			
			GameServer.NetAPI.World_SiteMapUpdate_STC(
				clientConnection,
				mainWorld.WorldMap.GetRevealedSiteMap()
			);
			
			var hostEntity = GameServer.NetConnector.GetPlayerFromConnection(GameServer.NetConnector.GetServerHostConnection());
			
			foreach (var buildingRef in hostEntity.GetComponent<PlayerComponentServer>().GetUnlockedBuildings())
				playerComp.UnlockBuilding(buildingRef);

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerComp, clientConnection);
			// End update player's world map state
		}
		
		private void OnRequestWorldInstanceSpawnPacketRecieved(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			Debug.Log("Request world instance spawn");
			var worldInstanceId = Guid.Parse(incMsg.ReadString());
			
			var seedX = FNERandom.GetRandomIntInRange(0, 1600000);
			var seedY = FNERandom.GetRandomIntInRange(0, 1600000);
			
			var newWorldInstance = new ServerWorld(512, 512)
			{
				SeedX = seedX,
				SeedY = seedY
			};
			
			var index = GameServer.WorldInstanceManager.AddWorldInstance(worldInstanceId, newWorldInstance);
			newWorldInstance.WorldInstanceIndex = index;
			
			GameServer.WorldGen.GenerateWorld(newWorldInstance, false);
			
			Debug.Log("New world instance genereated");
			
			var chunk = newWorldInstance.GetWorldChunk<ServerWorldChunk>();
			var netBuffer = new NetBuffer();
			netBuffer.EnsureBufferSize(chunk.TotalBitsNetBuffer());
			chunk.NetSerialize(netBuffer);

			var mainWorld = GameServer.GetWorldInstance(0);

			var playersToTransfer = mainWorld.GetAllPlayers();
			
			foreach (var player in playersToTransfer)
			{
				var conn = GameServer.NetConnector.GetConnectionFromPlayer(player);
				Debug.Log("Main world unload for client");
				GameServer.NetAPI.World_UnloadChunk_STC(mainWorld.GetWorldChunk<ServerWorldChunk>(), conn);
				Debug.Log("New world load for client");
				GameServer.NetAPI.World_LoadChunk_STC(chunk, netBuffer.Data, conn);
				mainWorld.RemoveTickableEntity(player);
				newWorldInstance.AddPlayerEntity(player);
				newWorldInstance.AddTickableEntity(player);
			}
			
			playersToTransfer.Clear();
		}

		private void OnClientConfirmChunkLoaded(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			Profiler.BeginSample("OnClientConfirmChunkLoaded");

			// byte chunkX = incMsg.ReadByte();
			// byte chunkY = incMsg.ReadByte();
			//
			// var chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>();
			// if (chunk == null)
			// {
			// 	return;
			// }
			
			// var state = GameServer.ChunkManager.GetPlayerChunkState(incMsg.SenderConnection);
			//
			// if (state == null)
			// {
			// 	Debug.LogError($"No player chunk state connected to: {incMsg.SenderConnection.ToString()}");
			// 	return;
			// }
			//
			// lock (state.Lock)
			// {
			// 	state.ChunksSentForLoadAwaitingConfirm.Remove(chunk);
			// 	state.CurrentlyLoadedChunks.Add(chunk);
			// }
			
			// var hordeEntitiesToSpawn = new List<HordeEntitySpawnData>();
			//
			// foreach (var e in chunk.GetAllEnemies())
			// {
			// 	hordeEntitiesToSpawn.Add(new HordeEntitySpawnData
			// 	{
			// 		Position = e.Position,
			// 		Rotation = e.RotationDegrees,
			// 		NetId = e.NetId,
			// 		EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(e.EntityId)
			// 	});
			// }
			//
			// GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_STC(hordeEntitiesToSpawn, incMsg.SenderConnection);

			Profiler.EndSample();
		}

		// private void OnClientConfirmChunkUnloaded(ServerNetworkConnector net, NetIncomingMessage incMsg)
		// {
		// 	Profiler.BeginSample("OnClientConfirmChunkUnloaded");
		//
		// 	byte chunkX = incMsg.ReadByte();
		// 	byte chunkY = incMsg.ReadByte();
		//
		// 	var chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(chunkX, chunkY);
		// 	var state = GameServer.ChunkManager.GetPlayerChunkState(incMsg.SenderConnection);
		//
		// 	if (state == null)
		// 	{
		// 		Debug.LogError($"No player chunk state connected to: {incMsg.SenderConnection.ToString()}");
		// 		return;
		// 	}
		//
		// 	lock (state.Lock)
		// 	{
		// 		state.ChunksSentForUnloadAwaitingConfirm.Remove(chunk);
		// 		state.CurrentlyLoadedChunks.Remove(chunk);
		// 	}
		// 	
		// 	Profiler.EndSample();
		// }

		private void OnBaseRoomNameChange(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			bool isBase = incMsg.ReadBoolean();
			long id = incMsg.ReadInt64();
			string newName = incMsg.ReadString();

			if (isBase)
				GameServer.RoomManager.SetBaseName(id, newName);
			else
				GameServer.RoomManager.GetRoom(id).Name = newName;

			GameServer.NetAPI.World_RoomManager_BA();
		}

		private void OnServerPingResponse(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			ServerWorld.PingPlayersReponse(incMsg.SenderConnection);
		}
	}
}