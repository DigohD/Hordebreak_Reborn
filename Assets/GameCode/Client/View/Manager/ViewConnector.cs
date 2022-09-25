using FNZ.Shared.Model.Entity;
using FNZ.Shared.Utils;
using Unity.Entities;
using UnityEngine;

namespace FNZ.Client.View.Manager
{

	public class ViewConnector
	{
		private FNEList<Entity> m_Entities;
		private FNEList<GameObject> m_GameObjects;

		private FNEList<Entity> m_SubViewEntities;
		private FNEList<GameObject> m_SubViewGameObjects;
		
		public ViewConnector()
		{
			m_Entities = new FNEList<Entity>(1000);
			m_GameObjects = new FNEList<GameObject>(1000);
			m_SubViewEntities = new FNEList<Entity>(1000);
			m_SubViewGameObjects = new FNEList<GameObject>(1000);
		}

		public void AddEntity(Entity e, int netID)
		{
			m_Entities.Add(e, netID);
		}
		
		public void AddSubViewEntity(Entity e, int netID)
		{
			m_SubViewEntities.Add(e, netID);
		}

		public void AddGameObject(GameObject go, int netID)
		{
			m_GameObjects.Add(go, netID);
		}
		
		public void AddSubViewGameObject(GameObject go, int netID)
		{
			m_SubViewGameObjects.Add(go, netID);
		}

		public void RemoveView(int netID)
		{
			if (m_Entities.GetEntity(netID) != default)
				m_Entities.Remove(netID);
			if (m_GameObjects.GetEntity(netID) != null)
				m_GameObjects.Remove(netID);
			
			if (m_SubViewGameObjects.GetEntity(netID) != null)
				m_SubViewGameObjects.Remove(netID);
		}

		public GameObject PopGameObjectView(int netID)
		{
			var go = m_GameObjects.GetEntity(netID);
			if (go != null)
				m_GameObjects.Remove(netID);
			return go;
		}
		
		public GameObject PopSubViewGameObjectView(int netID)
		{
			var go = m_SubViewGameObjects.GetEntity(netID);
			if (go != null)
				m_SubViewGameObjects.Remove(netID);
			return go;
		}

		public Entity PopEntityView(FNEEntity model)
		{
			var entity = m_Entities.GetEntity(model.NetId);
			if (entity != null)
				m_Entities.Remove(model.NetId);
			return entity;
		}

		public Entity GetEntity(FNEEntity model)
		{
			return m_Entities.GetEntity(model.NetId);
		}

		public GameObject GetGameObject(FNEEntity model)
		{
			return m_GameObjects.GetEntity(model.NetId);
		}
		
		public GameObject GetSubViewGameObject(FNEEntity model)
		{
			return m_SubViewGameObjects.GetEntity(model.NetId);
		}

		public Entity GetEntity(int netId)
		{
			return m_Entities.GetEntity(netId);
		}

		public GameObject GetGameObject(int netId)
		{
			return m_GameObjects.GetEntity(netId);
		}
		
		public GameObject GetSubViewGameObject(int netId)
		{
			return m_SubViewGameObjects.GetEntity(netId);
		}
	}
}