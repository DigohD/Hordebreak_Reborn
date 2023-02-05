using System;
using System.Collections.Generic;
using System.IO;
using FNZ.Server.Model;
using FNZ.Server.Model.Entity.Components.Name;
using FNZ.Server.Model.MetaWorld;
using FNZ.Server.Model.World;
using FNZ.Server.Model.World.Blueprint;
using FNZ.Server.Net;
using FNZ.Server.Net.API;
using FNZ.Server.Services;
using FNZ.Server.Services.QuestManager;
using FNZ.Server.Utils;
using FNZ.Server.WorldEvents;
using FNZ.Shared;
using FNZ.Shared.Model;
using FNZ.Shared.Utils;
using GameCode.Server.Model.World;
using Lidgren.Network;
using Unity.Entities;
using UnityEngine;

namespace FNZ.Server
{
	public class GameServer : MonoBehaviour
	{
		public static volatile bool APPLICATION_RUNNING = true;

		public static World ECS_ServerWorld;

		public static WorldInstanceManager WorldInstanceManager;
		
		public static ServerNetworkAPI NetAPI;
		public static ServerNetworkConnector NetConnector;
		public static ServerEntityFactory EntityFactory;
		public static WorldGenerator WorldGen;
		public static ServerFilePaths FilePaths;
		public static ServerMetaWorld MetaWorld;
		public static ServerRoomManager RoomManager;
		public static ServerItemsOnGroundManager ItemsOnGroundManager;
		public static EntityAPI EntityAPI;
		public static WorldEventManager EventManager;

		public static float DeltaTime;

		public static FNELogger Logger;

		public static ServerWorld GetWorldInstance(int index)
		{
			return WorldInstanceManager.GetWorldInstance(index);
		}

		public void Start()
		{
			WorldInstanceManager = new WorldInstanceManager();
			Logger = new FNELogger("Logs\\Server");
			ECS_ServerWorld = ECSWorldCreator.CreateWorld("ServerWorld", WorldFlags.Simulation, false);
			ECSWorldCreator.InitializeServerWorld(ECS_ServerWorld);

			EntityFactory = new ServerEntityFactory();

			FilePaths = new ServerFilePaths(SharedConfigs.WorldName);

			var seedX = FNERandom.GetRandomIntInRange(0, 1600000);
			var seedY = FNERandom.GetRandomIntInRange(0, 1600000);

			MetaWorld = new ServerMetaWorld();

			if (!FilePaths.WorldFolderExists())
			{
				MetaWorld.CreateNewWorld();
			}
			else
			{
				MetaWorld.LoadFromFile();
			}
			
			WorldGen = new WorldGenerator(DataBank.Instance.GetData<WorldGenData>("default_world"));
			
			var world = new ServerWorld(512, 512)
			{
				SeedX = seedX,
				SeedY = seedY
			};
			EntityAPI = new EntityAPI();

			var baseWorld = WorldGen.GenerateWorld(
				world,
				true
			);
			
			baseWorld.WorldInstanceIndex = WorldInstanceManager.AddWorldInstance(Guid.NewGuid(), baseWorld);

			var roomManager = FNEService.File.LoadRoomManagerFromFile(FilePaths.GetBaseFilePath());
			if (roomManager == null)
				RoomManager = new ServerRoomManager();
			else
			{
				RoomManager = roomManager;
			}
			
			ItemsOnGroundManager = new ServerItemsOnGroundManager();

			EventManager = new WorldEventManager();

			if (!SharedConfigs.IsNewGame)
			{
				var path = FilePaths.GetQuestsFilePath();
				if (File.Exists(path))
				{
					var netBuffer = new NetBuffer
					{
						Data = FileUtils.ReadFile(path)
					};
			
					QuestManager.Deserialize(netBuffer);
				}
			}

			//AgentSimulationSystem.Instance.StartSimulation();
		}

		public void OnApplicationQuit()
		{
			APPLICATION_RUNNING = false;
			
			Debug.Log("Server shutdown. Saving chunks...");
			var chunk = MainWorld.GetWorldChunk<ServerWorldChunk>();
			
			var path = FilePaths.GetOrCreateChunkFilePath(chunk);
			var data = chunk.GetChunkData();
			FNEService.File.WriteFile(path, data);
			
			Debug.Log("Server shutdown. Chunks saved successfully!");
			
			Debug.Log("Server shutdown. Saving players...");
			foreach (var player in MainWorld.GetAllPlayers())
			{
				var entity = NetConnector.GetPlayerFromConnection(NetConnector.GetConnectionFromPlayer(player));
				var leavingPlayerName = entity.GetComponent<NameComponentServer>().entityName;
				var netBuffer = new NetBuffer();
				netBuffer.EnsureBufferSize(entity.TotalBitsFileBuffer());
				entity.FileSerialize(netBuffer);
				FileUtils.WriteFile(FilePaths.CreatePlayerEntityFilePath(leavingPlayerName), netBuffer.Data);
			}
			
			Debug.Log("Server shutdown. Players saved successfully!");
			
			Debug.Log("Server shutdown. Saving base...");
			
			var netBufferBase = new NetBuffer();
			RoomManager.Serialize(netBufferBase);
			FileUtils.WriteFile(FilePaths.GetOrCreateBaseFilePath(), netBufferBase.Data);
			
			Debug.Log("Server shutdown. Base saved successfully!");
			
			Debug.Log("Server shutdown. Saving quests...");
			var netBufferQuest = new NetBuffer();
			QuestManager.Serialize(netBufferQuest);
			FileUtils.WriteFile(FilePaths.GetOrCreateQuestsFilePath(), netBufferQuest.Data);
			
			Debug.Log("Server shutdown. Quests saved successfully!");

			Debug.Log("Server shutdown. Saving meta world...");

			var netBufferMetaWorld = new NetBuffer();
			MetaWorld.SerializeMetaWorld(netBufferMetaWorld);
			FileUtils.WriteFile(FilePaths.GetOrCreateMetaWorldFilePath(), netBufferMetaWorld.Data);

			Debug.Log("Server shutdown. Meta world saved successfully!");

			if (ECS_ServerWorld != null)
			{
				ECS_ServerWorld.QuitUpdate = true;
				ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(null);
				ECS_ServerWorld.Dispose();
			}
			else
			{
				Debug.LogError("World has already been destroyed");
			}
			
			Debug.Log("Server shutdown successfully!");

			
		}
	}
}
