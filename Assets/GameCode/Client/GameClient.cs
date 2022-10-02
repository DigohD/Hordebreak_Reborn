using FNZ.Client.GPUSkinning;
using FNZ.Client.Model;
using FNZ.Client.Model.World;
using FNZ.Client.Net;
using FNZ.Client.Net.API;
using FNZ.Client.Systems;
using FNZ.Client.View.Manager;
using FNZ.Client.View.World;
using FNZ.Shared;
using FNZ.Shared.Model.Entity;
using System.Collections.Generic;
using FNZ.Shared.Utils;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using FNZ.Client.Model.MetaWorld;

namespace FNZ.Client
{
	public class GameClient : MonoBehaviour
	{
		public static volatile bool APPLICATION_RUNNING = true;

		public static World ECS_ClientWorld;
		public static ClientWorld World;
		public static ClientMetaWorld MetaWorld;
		public static ClientWorldView WorldView;
		public static ClientNetworkAPI NetAPI;
		public static ClientNetworkConnector NetConnector;
		public static ClientEntityFactory EntityFactory;
		public static ClientRoomManager RoomManager;
		public static EnvironmentClient Environment;
		public static ClientItemsOnGroundManager ItemsOnGroundManager;

		public static ViewAPI ViewAPI;
		public static SubViewAPI SubViewAPI;
		public static GameObjectSpawner Spawner;
		public static ViewConnector ViewConnector;
		public static RealEffectManagerClient RealEffectManager;

		public static FNEEntity LocalPlayerEntity;
		public static GameObject LocalPlayerView;

		public static List<GameObject> RemotePlayerViews = new List<GameObject>();
		public static List<FNEEntity> RemotePlayerEntities = new List<FNEEntity>();

		public static FNELogger Logger;
		
		public void Start()
		{
			Logger = new FNELogger("Logs\\Client");
			NetConnector = new ClientNetworkConnector();
			ECS_ClientWorld = ECSWorldCreator.CreateWorld("ClientWorld", WorldFlags.Game, true);
			ECSWorldCreator.InitializeClientWorld(ECS_ClientWorld);
			GPUAnimationCharacterUtility.CreateConvertToGPUPrefabs();

			ViewAPI = new ViewAPI();
			SubViewAPI = new SubViewAPI();

			MetaWorld = new ClientMetaWorld();
			RoomManager = new ClientRoomManager();
			Environment = new EnvironmentClient();
			ItemsOnGroundManager = new ClientItemsOnGroundManager();

			Spawner = new GameObjectSpawner();
			ViewConnector = new ViewConnector();
			RealEffectManager = new RealEffectManagerClient();
		}

		public void Update()
		{
			Profiler.BeginSample("GameObjectSpawner");
			Spawner?.Update();
			Profiler.EndSample();
			Profiler.BeginSample("RealEffectManager");
			RealEffectManager?.Update();
			Profiler.EndSample();
		}

		public void OnApplicationQuit()
		{
			if (ECS_ClientWorld != null)
			{
				ECS_ClientWorld.QuitUpdate = true;
				ScriptBehaviourUpdateOrder.AddWorldToCurrentPlayerLoop(null);
				ECS_ClientWorld.Dispose();
			}
			else
			{
				Debug.LogError("World has already been destroyed");
			}

			APPLICATION_RUNNING = false;
		}
	}
}

