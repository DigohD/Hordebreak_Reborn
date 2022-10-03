using FNZ.Server.Model.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Server.Controller.Systems
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(NetworkServerSystem))]
	public class ServerMainSystem : SystemBase
	{
		private enum SystemStatus
		{
			Idle,
			Busy,
		}

		public static readonly float TARGET_SERVER_TICK_TIME = 0.1f;

		private SystemStatus m_Systemstatus;

		private FNEWorker m_WorkerThread;

		private float m_Timer = 0;

		private JobHandle m_JobHandle;
		private bool m_SchedulingStarted = false;

		private bool m_EverythingDone;

		protected override void OnCreate()
		{
			m_Systemstatus = SystemStatus.Idle;
			m_WorkerThread = new FNEWorker();
		}

		protected override void OnUpdate()
		{
			// if (!m_WorkerThread.DoWork && ((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L)) || Input.GetKeyDown(KeyCode.F8)))
			// {
			// 	SpawnHordeEntitiesInRadius("zombie_big", 15, 20);
			// }

			// if (!m_WorkerThread.DoWork)
   //          {
	  //           Profiler.BeginSample("ProcessChunksToloadForClients");
	  //           if (GameServer.ChunkManager != null)
			// 		GameServer.ChunkManager.ProcessChunksToLoadForClients();
	  //           Profiler.EndSample();
	  //           
	  //           Profiler.BeginSample("ProcessChunksToUnloadForClients");
	  //           if (GameServer.ChunkManager != null)
			// 		GameServer.ChunkManager.ProcessChunksToUnloadForClients();
			// 	Profiler.EndSample();
   //          }

			m_Timer += UnityEngine.Time.deltaTime;

			if (m_SchedulingStarted && !m_EverythingDone)
			{
				if (m_Systemstatus == SystemStatus.Busy && m_JobHandle.IsCompleted)
				{
					m_JobHandle.Complete();
					m_Systemstatus = SystemStatus.Idle;
					m_EverythingDone = true;

					GameServer.DeltaTime = m_Timer < TARGET_SERVER_TICK_TIME ? TARGET_SERVER_TICK_TIME : m_Timer;
				}
				else if (m_Systemstatus == SystemStatus.Idle && !m_WorkerThread.DoWork)
				{
					ScheduleJobs();
					m_Systemstatus = SystemStatus.Busy;
				}
			}
			else if (m_Timer >= TARGET_SERVER_TICK_TIME)
			{
				RunServerTick();
				m_Timer = 0;
				m_EverythingDone = false;

				if (!m_SchedulingStarted)
					m_SchedulingStarted = true;
			}
		}

		private void RunServerTick()
		{
			lock (m_WorkerThread.Lock)
			{
				m_WorkerThread.DoWork = true;
				Monitor.Pulse(m_WorkerThread.Lock);
			}
		}

		private void ScheduleJobs()
		{

		}

		void SpawnHordeEntitiesInGrid(string id, int gridSizeX, int gridSizeY)
		{
			var playerPos = GameServer.World.GetAllPlayers()[0].Position;

			var px = playerPos.x;
			var py = playerPos.y;

			var spawnDataList = new List<HordeEntitySpawnData>(gridSizeX * gridSizeY);

			for (var y = 0; y < gridSizeY; y++)
			{
				for (var x = 0; x < gridSizeX; x++)
				{
					var entityId = id;
					var rand = FNERandom.GetRandomIntInRange(0, 100);
					if (rand <= 100 && rand > 90)
						entityId = "default_zombie";
					else if (rand <= 90 && rand >= 0)
						entityId = "default_zombie";

					var spawnPosition = new float2(px + x, py + y);
					if (GameServer.World.GetWorldChunk<ServerWorldChunk>(spawnPosition) == null)
						continue;

					var spawnRotation = FNERandom.GetRandomIntInRange(0, 360);

					var modelEntity = GameServer.EntityAPI.SpawnEntityImmediate(entityId, spawnPosition, spawnRotation);
					GameServer.NetConnector.SyncEntity(modelEntity);

					var spawnData = new HordeEntitySpawnData
					{
						Position = spawnPosition,
						Rotation = spawnRotation,
						EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(entityId),
						NetId = modelEntity.NetId
					};

					spawnDataList.Add(spawnData);
				}
			}

			GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(spawnDataList);
		}
		
		void SpawnHordeEntitiesInRadius(string id, int minRadius, int maxRadius)
		{
			var playerPos = GameServer.World.GetAllPlayers()[0].Position;

			var px = playerPos.x;
			var py = playerPos.y;

			var spawnDataList = new List<HordeEntitySpawnData>(1500);
			var entitiesToSpawn = new List<FNEEntity>();

			for (var e = 0; e < 200; e++)
			{
				var entityId = id;

				var distance = FNERandom.GetRandomFloatInRange(minRadius, maxRadius);
				var v = new Vector2(distance, 0);
				
				var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
					
				var spawnPosition = new float2(px + finalOffset.x, py + finalOffset.y);
				var chunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(spawnPosition);
				if (chunk == null || !chunk.IsActive || !chunk.IsInitialized)
					continue;

				var spawnRotation = FNERandom.GetRandomIntInRange(0, 360);

				var modelEntity = GameServer.EntityAPI.CreateEntityImmediate(entityId, spawnPosition, spawnRotation);
				entitiesToSpawn.Add(modelEntity);
			}
			
			GameServer.World.AddEntityToSpawnQueue(new SpawnEntityBatchData
			{
				Entities = entitiesToSpawn
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			m_JobHandle.Complete();
		}
	}
}

