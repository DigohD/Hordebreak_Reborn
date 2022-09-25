using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.Manager
{
	public struct GameObjectSpawnData
	{
		public string type;
		public string id;
		public int netID;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
	}

	public struct GameObjectActivationData
	{
		public int NetId;
		public GameObject View;
	}

	public struct GameObjectDeactivationData
	{
		public string id;
		public GameObject go;
	}

	public class GameObjectSpawner
	{
		public int SpawnsPerFrame;
		public int ActivatesPerFrame;
		public int DeactivatesPerFrame;

		private Queue<GameObjectSpawnData> m_GameObjectSpawnQueue;
		private Queue<GameObjectDeactivationData> m_DeactivateQueue;
		private Queue<GameObjectActivationData> m_ActivateQueue;

		private Queue<GameObjectSpawnData> m_SubViewGameObjectSpawnQueue;
		private Queue<GameObjectDeactivationData> m_SubViewDeactivateQueue;
		private Queue<GameObjectActivationData> m_SubViewActivateQueue;
		
		public GameObjectSpawner()
		{
			SpawnsPerFrame = 5;
			ActivatesPerFrame = 50;
			DeactivatesPerFrame = 50;
			m_GameObjectSpawnQueue = new Queue<GameObjectSpawnData>();
			m_DeactivateQueue = new Queue<GameObjectDeactivationData>();
			m_ActivateQueue = new Queue<GameObjectActivationData>();
			
			m_SubViewActivateQueue = new Queue<GameObjectActivationData>();
			m_SubViewGameObjectSpawnQueue = new Queue<GameObjectSpawnData>();
			m_SubViewDeactivateQueue = new Queue<GameObjectDeactivationData>();
		}

		public void Update()
		{
			int objectsToSpawn = m_GameObjectSpawnQueue.Count;
			int objectsToActivate = m_ActivateQueue.Count;
			int objectsToDeactivate = m_DeactivateQueue.Count;

			int subViewsToActivate = m_SubViewActivateQueue.Count;
			int subViewsToSpawn = m_SubViewGameObjectSpawnQueue.Count;
			int subViewsToDeactivate = m_SubViewDeactivateQueue.Count;
			
			if (objectsToActivate > 0)
			{
				int amount = (objectsToActivate <= ActivatesPerFrame) ? objectsToActivate : ActivatesPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var go = m_ActivateQueue.Dequeue();
					GameClient.ViewAPI.ActivateGameObject(go);
				}
			}
			else if (objectsToSpawn > 0)
			{
				int amount = (objectsToSpawn <= SpawnsPerFrame) ? objectsToSpawn : SpawnsPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var spawnData = m_GameObjectSpawnQueue.Dequeue();
					GameClient.ViewAPI.SpawnGameObject(spawnData);
				}
			}
			else if (subViewsToActivate > 0)
			{
				int amount = (subViewsToActivate <= ActivatesPerFrame) ? subViewsToActivate : ActivatesPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var go = m_SubViewActivateQueue.Dequeue();
					GameClient.SubViewAPI.ActivateSubViewGameObject(go);
				}
			}
			else if (subViewsToSpawn > 0)
			{
				int amount = (subViewsToSpawn <= ActivatesPerFrame) ? subViewsToSpawn : ActivatesPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var go = m_SubViewGameObjectSpawnQueue.Dequeue();
					GameClient.SubViewAPI.SpawnSubViewGameObject(go);
				}
			}
			else if (objectsToDeactivate > 0)
			{
				int amount = (objectsToDeactivate <= DeactivatesPerFrame) ? objectsToDeactivate : DeactivatesPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var data = m_DeactivateQueue.Dequeue();
					GameObjectPoolManager.DoRecycle(data.id, data.go);
				}
			}else if (subViewsToDeactivate > 0)
			{
				int amount = (subViewsToDeactivate <= DeactivatesPerFrame) ? subViewsToDeactivate : DeactivatesPerFrame;

				for (int i = 0; i < amount; i++)
				{
					var data = m_SubViewDeactivateQueue.Dequeue();
					GameObjectPoolManager.DoRecycle(data.id, data.go);
				}
			}
		}

		public void QueueForSpawning(GameObjectSpawnData data)
		{
			m_GameObjectSpawnQueue.Enqueue(data);
		}
		
		public void QueueSubViewForSpawning(GameObjectSpawnData data)
		{
			m_SubViewGameObjectSpawnQueue.Enqueue(data);
		}

		public void QueueForActivation(GameObjectActivationData data)
		{
			m_ActivateQueue.Enqueue(data);
		}
		
		public void QueueSubViewForActivation(GameObjectActivationData data)
		{
			m_SubViewActivateQueue.Enqueue(data);
		}

		public void QueueForDeactivation(GameObjectDeactivationData data)
		{
			m_DeactivateQueue.Enqueue(data);
		}
		
		public void QueueSubViewForDeactivation(GameObjectDeactivationData data)
		{
			m_SubViewDeactivateQueue.Enqueue(data);
		}
	}
}