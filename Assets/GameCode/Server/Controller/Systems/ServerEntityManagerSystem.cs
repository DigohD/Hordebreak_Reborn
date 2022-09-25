using FNZ.Shared.Model.Entity;
using System.Collections.Concurrent;
using Unity.Entities;
using Unity.Mathematics;

namespace FNZ.Server.Controller.Systems 
{
	public struct FNEEntitySpawnData
	{
		public string EntityId;
		public bool NetSync;
		public float2 Position;
		public float Rotation;
	};

	[DisableAutoCreation]
	public class ServerEntityManagerSystem : SystemBase
	{
		private ConcurrentQueue<FNEEntitySpawnData> m_SpawnEntityQueue;
		private ConcurrentQueue<FNEEntity> m_SyncEntityQueue;
		private ConcurrentQueue<FNEEntity> m_WorldStateEntityQueue;
		private ConcurrentQueue<FNEEntity> m_DestroyEntityQueue;

		protected override void OnCreate()
		{
			m_SpawnEntityQueue = new ConcurrentQueue<FNEEntitySpawnData>();
			m_SyncEntityQueue = new ConcurrentQueue<FNEEntity>();
			m_WorldStateEntityQueue = new ConcurrentQueue<FNEEntity>();
			m_DestroyEntityQueue = new ConcurrentQueue<FNEEntity>();
		}

		protected override void OnUpdate()
	    {
			// @TODO(Anders E): 
			// - limit the amount of elements to be processed of each queue each frame
			// - Fix so we send batched packets

			if (m_SpawnEntityQueue.Count > 0)
			{
				var count = m_SpawnEntityQueue.Count;
				for (var i = 0; i < count; i++)
				{
					m_SpawnEntityQueue.TryDequeue(out var spawnData);

					if (spawnData.NetSync)
					{
						GameServer.EntityAPI.NetSpawnEntityImmediate(spawnData.EntityId, spawnData.Position, spawnData.Rotation);
					}
					else
					{
						GameServer.EntityAPI.SpawnEntityImmediate(spawnData.EntityId, spawnData.Position, spawnData.Rotation);
					}
				}
			}

			if (m_WorldStateEntityQueue.Count > 0)
			{
				var count = m_WorldStateEntityQueue.Count;
				for (var i = 0; i < count; i++)
				{
					m_WorldStateEntityQueue.TryDequeue(out var entity);
					GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity);
				}
			}

			if (m_SyncEntityQueue.Count > 0)
			{
				var count = m_SyncEntityQueue.Count;
				for (var i = 0; i < count; i++)
				{
					m_SyncEntityQueue.TryDequeue(out var entity);
					GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity);
					GameServer.EntityAPI.NetSyncEntityToRelevantClients(entity);
				}
			}

			if (m_DestroyEntityQueue.Count > 0)
			{
				var count = m_DestroyEntityQueue.Count;
				for (var i = 0; i < count; i++)
				{
					m_DestroyEntityQueue.TryDequeue(out var entity);
					GameServer.EntityAPI.NetDestroyEntityImmediate(entity);
				}
			}
	    }

		public void AddEntityToSpawnQueue(FNEEntitySpawnData spawnData)
		{
			m_SpawnEntityQueue.Enqueue(spawnData);
		}

		public void AddEntityToWorldStateAndNetSyncQueue(FNEEntity entity)
		{
			m_SyncEntityQueue.Enqueue(entity);
		}

		public void AddEntityToWorldStateQueue(FNEEntity entity)
		{
			m_WorldStateEntityQueue.Enqueue(entity);
		}

		public void AddEntityToDestroyQueue(FNEEntity entity)
		{
			m_DestroyEntityQueue.Enqueue(entity);
		}
	}
}