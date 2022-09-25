using FNZ.Shared.Model.Entity;
using System;
using System.Collections.Generic;

namespace FNZ.Shared.Utils
{
	public class FNEEntityPool
	{
		private Dictionary<string, Stack<FNEEntity>> m_PoolCache;
		private Stack<FNEEntity> m_InitialInstances;

		public FNEEntityPool(int initalCapacity)
        {
			m_PoolCache = new Dictionary<string, Stack<FNEEntity>>();

			m_InitialInstances = new Stack<FNEEntity>(initalCapacity);

			for (var i = 0; i < initalCapacity; i++)
            {
				var entity = (FNEEntity)Activator.CreateInstance(typeof(FNEEntity));
				m_InitialInstances.Push(entity);
			}
		}

		public FNEEntity GetEntity(string id)
        {
			if (!m_PoolCache.ContainsKey(id))
            {
				m_PoolCache.Add(id, new Stack<FNEEntity>());
            }

			if (m_PoolCache.TryGetValue(id, out var entityCache))
            {
				if (entityCache.Count > 0)
                {
					return entityCache.Pop();
                }
            }

			if (m_InitialInstances.Count > 0)
            {
				return m_InitialInstances.Pop();
            }

			return new FNEEntity();
        }

		public void ReturnEntity(FNEEntity entity)
        {
			entity.Reset();

			if (m_PoolCache.TryGetValue(entity.EntityId, out var entityCache))
			{
				entityCache.Push(entity);
			}
		}
	}
}