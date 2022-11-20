using FNZ.Server.Controller;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Services;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Net.Dto.Hordes;
using Unity.Mathematics;
using UnityEngine;
using static FNZ.Server.Model.World.Blueprint.WorldBlueprintGen;

namespace FNZ.Server.Model.World
{
	public struct SpawnEntityBatchData
	{
		public List<FNEEntity> Entities;
	}
	
	public struct UnloadChunkData
	{
		public ServerWorldChunk Chunk;
		public List<FNEEntity> Entities;
	}

	public struct UpdateObstacleData
	{
		public FNEEntity Entity;
		public bool ShouldRemove;
	}
	
	public class ServerWorld : GameWorld
	{
		public int SeedX;
		public int SeedY;

		private readonly List<ServerWorldChunk> m_CurrentlyLoadedChunks;

		private readonly List<FNEEntity> m_Players;
		private readonly List<FNEEntity> m_TickableEntities;

		private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToRemove;
		private readonly ConcurrentQueue<FNEEntity> m_TickableEntitiesToAdd;

		private readonly ConcurrentQueue<SyncEntitiesData> m_SyncEntitiesQueue = new ConcurrentQueue<SyncEntitiesData>();
		private readonly ConcurrentQueue<UnloadChunkData> m_ChunksToUnload = new ConcurrentQueue<UnloadChunkData>();
		private readonly ConcurrentQueue<SpawnEntityBatchData> m_EntitiesToSpawn = new ConcurrentQueue<SpawnEntityBatchData>();
		private readonly ConcurrentQueue<FlowFieldGenData> m_FlowFieldsToSpawn = new ConcurrentQueue<FlowFieldGenData>();
		private readonly ConcurrentQueue<FNEEntity> m_EntitiesToDestroy = new ConcurrentQueue<FNEEntity>();
		private readonly ConcurrentQueue<UpdateObstacleData> m_ObstacleEntitiesToUpdate = new ConcurrentQueue<UpdateObstacleData>();

		private readonly Stack<ILateTickable> m_LateTickableEntities = new Stack<ILateTickable>();

		private readonly object m_ChunksLock = new object();

		public readonly EnvironmentServer Environment;
		public readonly RealEffectManagerServer RealEffectManager;

		private static Dictionary<NetConnection, byte> s_PlayerConnectionWarningSystem;
		private static List<NetConnection> s_NonResponsiveConnections;

		private const byte PING_MAX_WARNINGS = 3;
		private byte m_ServerPingCounter = 1;

		private Dictionary<int, SiteMetaData> m_SiteMetaData;

		public ServerMapManager WorldMap;
		
		public ServerWorld(int widthInTiles, int heightInTiles)
		{
			WIDTH = widthInTiles;
			HEIGHT = heightInTiles;

			WIDTH_IN_CHUNKS = WIDTH / CHUNK_SIZE;
			HEIGHT_IN_CHUNKS = HEIGHT / CHUNK_SIZE;

			m_Chunk = new ServerWorldChunk(0, 0, widthInTiles);

			m_CurrentlyLoadedChunks = new List<ServerWorldChunk>();
			m_Players = new List<FNEEntity>();
			m_TickableEntities = new List<FNEEntity>();
			m_TickableEntitiesToRemove = new ConcurrentQueue<FNEEntity>();
			m_TickableEntitiesToAdd = new ConcurrentQueue<FNEEntity>();

			Environment = new EnvironmentServer();
			RealEffectManager = new RealEffectManagerServer();
			WorldMap = new ServerMapManager(WIDTH_IN_CHUNKS, HEIGHT_IN_CHUNKS);
			
			s_NonResponsiveConnections = new List<NetConnection>();
			s_PlayerConnectionWarningSystem = new Dictionary<NetConnection, byte>();
		}

		public void QueueFlowField(FlowFieldGenData data)
		{
			m_FlowFieldsToSpawn.Enqueue(data);
		}

		public void SyncEntities(SyncEntitiesData entitiesData)
		{
			m_SyncEntitiesQueue.Enqueue(entitiesData);
		}

		public void QueueObstacleForUpdate(UpdateObstacleData data)
		{
			m_ObstacleEntitiesToUpdate.Enqueue(data);
		}

		public void AddChunkToUnloadQueue(UnloadChunkData chunk)
		{
			m_ChunksToUnload.Enqueue(chunk);
		}

		public void AddEntityToSpawnQueue(SpawnEntityBatchData entity)
		{
			m_EntitiesToSpawn.Enqueue(entity);
		}

		public void AddEntityToDestroyQueue(FNEEntity entity)
		{
			m_EntitiesToDestroy.Enqueue(entity);			
		}

		private void InitialiazeGeneratedChunks()
		{
			while (m_SyncEntitiesQueue.Count > 0)
			{
				if (!m_SyncEntitiesQueue.TryDequeue(out var entitiesData)) continue;
				var chunk = entitiesData.Chunk;

				foreach (var entity in entitiesData.Entities)
				{
					GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity, true);
					GameServer.NetConnector.SyncEntity(entity);
					entity.Enabled = true;
				}

				foreach (var entity in entitiesData.MovingEntities)
				{
					GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity, true);
					GameServer.NetConnector.SyncEntity(entity);
					entity.Enabled = false;
					if (entity.Agent != null)
						entity.Agent.active = false;
				}
				
				chunk.EntitiesToSync.Clear();
				chunk.MovingEntitiesToSync.Clear();

				chunk.IsInitialized = true;
			}
        }

		public List<ServerWorldChunk> GetCurrentlyLoadedChunks()
		{
			return m_CurrentlyLoadedChunks;
		}

		private void SpawnEntities()
		{
			while (m_EntitiesToSpawn.Count > 0)
			{
				if (!m_EntitiesToSpawn.TryDequeue(out var data)) continue;
				
				var spawnDataList = new List<HordeEntitySpawnData>(data.Entities.Count);
				
				foreach (var e in data.Entities)
				{
					GameServer.EntityAPI.AddEntityToWorldStateImmediate(e, true);
					GameServer.NetConnector.SyncEntity(e);
					e.Enabled = true;
					
					spawnDataList.Add(new HordeEntitySpawnData
					{
						Position = e.Position,
						Rotation = e.RotationDegrees,
						NetId = e.NetId,
						EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(e.EntityId)
					});
				}
				
				GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(spawnDataList);
			}
		}

		private void DestroyEntities()
		{
			while (m_EntitiesToDestroy.Count > 0)
			{
				if (!m_EntitiesToDestroy.TryDequeue(out var entity)) continue;

				GameServer.EntityAPI.DestroyEntityImmediate(entity, false);
				GameServer.NetConnector.UnsyncEntity(entity);
				entity.Enabled = true;
			}
		}

		private void UnloadChunks()
		{
			while (m_ChunksToUnload.Count > 0)
			{
				if (!m_ChunksToUnload.TryDequeue(out var data)) continue;
				foreach (var e in data.Entities)
				{
					GameServer.EntityAPI.DestroyEntityImmediate(e, true);
				}
				
				GameServer.MainWorld.RemoveChunk(data.Chunk);
			}
		}

		private void SpawnFlowFields()
		{
			if (!m_FlowFieldsToSpawn.TryDequeue(out var data)) return;

			FlowFieldUtility.GenerateFlowFieldAndUpdateEnemies(data.SourcePosition, data.Range, data.FlowFieldType);
		}

		private void UpdateObstacles()
		{
			while (m_ObstacleEntitiesToUpdate.Count > 0)
			{
				if (!m_ObstacleEntitiesToUpdate.TryDequeue(out var data)) continue;

				if (data.ShouldRemove)
				{
					AgentSimulationSystem.Instance.RemoveObstacle(data.Entity);
				}
				else
				{
					AgentSimulationSystem.Instance.AddObstacle(data.Entity.GetComponent<ObstacleComponentServer>());
				}
			}
		}
		public void Tick(float deltaTime)
		{
			//UnloadChunks();
			//InitialiazeGeneratedChunks();
			SpawnFlowFields();
			SpawnEntities();
			UpdateObstacles();
			AddTickablesFromQueue();
			RemoveTickablesFromQueue();
			WorldMap.Tick();
			
			// @TODO(Anders E): Let this try catch stay for awhile to make sure the concurrent modification issue is fixed. But remove it eventually
			try
			{
				foreach (var entity in m_TickableEntities)
				{
					var chunk = GetWorldChunk<ServerWorldChunk>(entity.Position);
					if (chunk == null) continue;
					if (!chunk.IsActive)
					{
						continue;
					}
					if (!entity.Enabled) continue;

					var tickableAndActiveComps = entity.Components
						.Where(comp => comp is ITickable && comp.Enabled);

					foreach (var fneComponent in tickableAndActiveComps)
					{
						var comp = (ITickable) fneComponent;
						comp.Tick(deltaTime);
						if (comp is ILateTickable && !m_LateTickableEntities.Contains(comp))
							m_LateTickableEntities.Push((ILateTickable) comp);
					}
				}
			}
			catch (InvalidOperationException e)
			{
				GameServer.NetAPI.Error_SendErrorMessage_BA("TickableEntities Concurrent Modification!", "");
				Debug.LogError(e);
			}

			try
			{
				for(int i = 0; i < m_LateTickableEntities.Count; i++)
				{
					m_LateTickableEntities.Pop().LateTick(deltaTime);
				}
			}
			catch (InvalidOperationException e)
			{
				GameServer.NetAPI.Error_SendErrorMessage_BA("LateTickableEntities Concurrent Modification!", "");
				m_LateTickableEntities.Clear();
				Debug.LogError(e);
			}

			GameServer.EntityFactory.ExecuteEntityReplacementQueue();

			RealEffectManager.Update(deltaTime);
			Environment.Tick(deltaTime);
			GameServer.EventManager.Tick(deltaTime);

			GameServer.NetAPI.Entity_SendUpdateComponentBatch();
			GameServer.NetAPI.Entity_SendUpdatePosAndRotBatch();

			if (m_ServerPingCounter == 50)
				PingPlayers();
			m_ServerPingCounter = m_ServerPingCounter >= 50 ? (byte)1 : m_ServerPingCounter += 1;
		}

		public void AddWorldChunk(ServerWorldChunk chunk)
		{
			m_Chunk = chunk;
		}

		public override void RemoveChunk<T>(T chunk)
		{
			
		}

		public void AddPlayerEntity(FNEEntity playerEntity)
		{
			m_Players.Add(playerEntity);
		}

		public void AddTickableEntity(FNEEntity entity)
		{
			m_TickableEntitiesToAdd.Enqueue(entity);
		}

		public void AddTickableEntityImmediate(FNEEntity entity)
		{
			m_TickableEntities.Add(entity);
			if (entity.EntityType == EntityType.ECS_ENEMY)
			{
				var stats = entity.Data.GetComponentData<EnemyStatsComponentData>();

				AgentSimulationSystem.Instance.AddAgent(
					entity,
					new Vector2(entity.Position.x, entity.Position.y),
					stats.agentRadius,
					FNERandom.GetRandomFloatInRange(stats.minSpeed, stats.maxSpeed)
				);
			}
		}

		public void RemoveTickableEntity(FNEEntity entity)
		{
			entity.Enabled = false;
			m_TickableEntitiesToRemove.Enqueue(entity);
		}

		private void AddTickablesFromQueue()
		{
			while (m_TickableEntitiesToAdd.Count > 0)
			{
				if (!m_TickableEntitiesToAdd.TryDequeue(out var entity)) continue;
				m_TickableEntities.Add(entity);

				if (entity.EntityType == EntityType.ECS_ENEMY)
				{
					var stats = entity.Data.GetComponentData<EnemyStatsComponentData>();

					AgentSimulationSystem.Instance.AddAgent(
						entity,
						new Vector2(entity.Position.x, entity.Position.y),
						stats.agentRadius,
						FNERandom.GetRandomFloatInRange(stats.minSpeed, stats.maxSpeed)
					);
				}
			}
		}

		public void RemoveTickableImmediate(FNEEntity entity)
		{
			if (m_TickableEntities.Contains(entity))
				m_TickableEntities.Remove(entity);

			if (entity.EntityType == EntityType.ECS_ENEMY)
			{
				AgentSimulationSystem.Instance.RemoveAgent(entity);
			}
		}

		private void RemoveTickablesFromQueue()
		{
			while (m_TickableEntitiesToRemove.Count > 0)
			{
				if (!m_TickableEntitiesToRemove.TryDequeue(out var entityToRemove)) continue;
				// NOTE(Anders E): First time chunks are loaded/unloaded there seem to be a mismatch between m_TickableEntitiesToRemove and m_TickableEntities
				// m_TickableEntities does not contain one of the entities that has been added to m_TickableEntitiesToRemove and im not sure why.
				if (m_TickableEntities.Contains(entityToRemove))
					m_TickableEntities.RemoveBySwap(entityToRemove);

				if (entityToRemove.EntityType == EntityType.ECS_ENEMY)
				{
					AgentSimulationSystem.Instance.RemoveAgent(entityToRemove);
				}
			}
		}

		public void RemovePlayerEntity(FNEEntity playerToRemove)
		{
			m_Players.Remove(playerToRemove);
			RemoveTickableEntity(playerToRemove);
		}

		public void ChangeTile(int x, int y, string id)
		{
			var chunk = GetWorldChunk<ServerWorldChunk>(new int2(x, y));
			chunk.ChangeTile(x, y, id);
		}

		public List<FNEEntity> GetAllPlayers()
		{
			return m_Players;
		}

		public float GetTileTemperature(float2 position)
		{
			var chunk = GetWorldChunk<ServerWorldChunk>(position);
			int2 tileIndices = GetChunkTileIndices(chunk, position);

			return chunk.TileTemperatures[tileIndices.x + tileIndices.y * CHUNK_SIZE];
		}

		private void PingPlayers()
		{
			if (s_NonResponsiveConnections.Count > 0)
				PurgePlayers();
			s_NonResponsiveConnections.Clear();

			GameServer.NetAPI.Server_Ping_Clients();
			s_NonResponsiveConnections = GameServer.NetConnector.GetConnectedClientConnections().ToList();

			void PurgePlayers()
			{
				foreach (var conn in s_NonResponsiveConnections)
				{
					if (!s_PlayerConnectionWarningSystem.ContainsKey(conn))
						s_PlayerConnectionWarningSystem.Add(conn, 0);

					s_PlayerConnectionWarningSystem[conn]++;

					if (s_PlayerConnectionWarningSystem[conn] >= PING_MAX_WARNINGS)
					{
						s_PlayerConnectionWarningSystem.Remove(conn);
						conn.Disconnect(string.Empty);
						GameServer.EntityFactory.RemovePlayer(conn);
					}
				}
			}
		}

		public static void PingPlayersReponse(NetConnection senderConnection)
		{
			s_NonResponsiveConnections.Remove(senderConnection);
			s_PlayerConnectionWarningSystem.Remove(senderConnection);
		}

		public void LoadSiteMetaData()
        {
			var filePath = GameServer.FilePaths.GetOrCreateChunkSiteMetaFilePath();

			if (File.Exists(filePath))
			{
				m_SiteMetaData = FNEService.File.LoadSiteMetaDataFromFile(filePath);
			}
		}

		public Dictionary<int, SiteMetaData> GetSiteMetaData()
        {
			return m_SiteMetaData;
        }

	}
}

