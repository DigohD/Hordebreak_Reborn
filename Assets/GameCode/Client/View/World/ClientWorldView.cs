using FNZ.Client.Model.World;
using FNZ.Client.Net.NetworkManager;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.World
{
	public class ClientWorldView
	{
		private ClientWorld m_WorldModel;
		private List<ClientWorldChunkView> m_WorldChunkViews;

		private List<Entity> m_EntityViews = new List<Entity>();
		private List<Entity> m_EntitySubViews = new List<Entity>();
		private List<GameObject> m_GameObjectViews = new List<GameObject>();
		private List<GameObject> m_GameObjectSubViews = new List<GameObject>();

		public ClientWorldView(ClientWorld worldModel)
		{
			m_WorldModel = worldModel;
			m_WorldChunkViews = new List<ClientWorldChunkView>();

			ClientWorldNetworkManager.d_TriggerUnloadWorld += ClearWorld;
			GameClient.d_TEST_WorldClear += ClearWorld;
		}

		public void AddChunkView(ClientWorldChunkView chunkView)
		{
			m_WorldChunkViews.Add(chunkView);
		}

		public void RemoveChunkView(byte chunkX, byte chunkY)
		{
			m_WorldChunkViews.RemoveAll(chunkView => chunkX == chunkView.ChunkX && chunkY == chunkView.ChunkY);
		}

		public int2 GetChunkIndices(float2 position)
		{
			return GetChunkIndices(
				(int) position.x,
				(int) position.y
			);
		}

		public int2 GetChunkIndices(int x, int y)
        {
			return new int2(
				x / 32,
				y / 32
			);
        }

		public ClientWorldChunkView GetChunkView(float2 position)
		{
			int cx = GetChunkIndices(position).x;
			int cy = GetChunkIndices(position).y;

			foreach (var chunkView in m_WorldChunkViews)
			{
				if (chunkView == null) continue;
				if (cx == chunkView.ChunkX && cy == chunkView.ChunkY)
					return chunkView;
			}

			return null;
		}

		private void OnChunkUnloaded()
		{
			if (m_GameObjectViews.Count > 0)
			{
				//GameClient.ViewAPI.QueueGameObjectsForDeactivation(m_GameObjectViews);
				foreach(var gameObject in m_GameObjectViews)
                {
					GameObject.Destroy(gameObject);
				}
				
				m_GameObjectViews.Clear();
			}

			if (m_GameObjectSubViews.Count > 0)
			{
				//GameClient.SubViewAPI.QueueSubViewGameObjectsForDeactivation(m_GameObjectSubViews);
				foreach (var gameObject in m_GameObjectSubViews)
				{
					GameObject.Destroy(gameObject);
				}
				m_GameObjectSubViews.Clear();
			}

			if (m_EntityViews.Count > 0)
			{
				GameClient.ViewAPI.DestroyViewEntities(m_EntityViews);
				m_EntityViews.Clear();
			}

			if (m_EntitySubViews.Count > 0)
			{
				GameClient.ViewAPI.DestroyViewEntities(m_EntitySubViews);
				m_EntitySubViews.Clear();
			}

			foreach(var view in m_WorldChunkViews)
				GameObject.Destroy(view.gameObject);
		}

		public void AddGameObject(GameObject go)
		{
			m_GameObjectViews.Add(go);
		}

		public void AddSubViewGameObject(GameObject go)
		{
			m_GameObjectSubViews.Add(go);
		}

		public void RemoveGameObject(GameObject go)
		{
			m_GameObjectViews.Remove(go);
		}

		public void AddEntity(Entity e)
		{
			m_EntityViews.Add(e);
		}

		public void AddSubViewEntity(Entity e)
		{
			m_EntitySubViews.Add(e);
		}

		public void RemoveEntity(Entity e)
		{
			m_EntityViews.Remove(e);
		}

		private void ClearWorld()
        {
			if (m_GameObjectViews.Count > 0)
			{
				//GameClient.ViewAPI.QueueGameObjectsForDeactivation(m_GameObjectViews);
				foreach (var gameObject in m_GameObjectViews)
				{
					GameObject.Destroy(gameObject);
				}

				m_GameObjectViews.Clear();
			}

			if (m_GameObjectSubViews.Count > 0)
			{
				//GameClient.SubViewAPI.QueueSubViewGameObjectsForDeactivation(m_GameObjectSubViews);
				foreach (var gameObject in m_GameObjectSubViews)
				{
					GameObject.Destroy(gameObject);
				}
				m_GameObjectSubViews.Clear();
			}

			if (m_EntityViews.Count > 0)
			{
				GameClient.ViewAPI.DestroyViewEntities(m_EntityViews);
				m_EntityViews.Clear();
			}

			if (m_EntitySubViews.Count > 0)
			{
				GameClient.ViewAPI.DestroyViewEntities(m_EntitySubViews);
				m_EntitySubViews.Clear();
			}

			foreach (var chunkView in m_WorldChunkViews)
            {
				GameObject.Destroy(chunkView.gameObject);
            }

		}
	}
}