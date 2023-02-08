using System.Collections.Generic;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.WorldEvent;
using Unity.Mathematics;

namespace FNZ.Server.WorldEvents 
{
	public class WorldEventManager
	{
		private readonly List<IWorldEvent> m_Events;
		private readonly List<IWorldEvent> m_EventsToRemove;

		public static long EventId;

		public WorldEventManager()
		{
			m_Events = new List<IWorldEvent>();
			m_EventsToRemove = new List<IWorldEvent>();
		}

		public void SpawnWorldEvent(string id, FNEEntity entity)
		{
			var worldEventData = DataBank.Instance.GetData<WorldEventData>(id);

			IWorldEvent eventToSpawn = null;
			switch (worldEventData.EventType)
			{
				case WorldEventType.Survival:
					eventToSpawn = new SurvivalEventServer(entity, worldEventData);
					break;
				case WorldEventType.ReplaceTileObject:
					eventToSpawn = new ReplaceTileObjectEvent(entity, worldEventData);
					break;
				case WorldEventType.ConstantSpawningAmbush:
					eventToSpawn = new ConstantSpawningAmbushEventServer(entity, worldEventData);
					break;
			}

			var world = GameServer.GetWorldInstance(entity.WorldInstanceIndex);

			if (eventToSpawn != null)
			{
				m_Events.Add(eventToSpawn);
				eventToSpawn.OnTrigger();
				if(!string.IsNullOrEmpty(worldEventData.EffectRef))
					GameServer.NetAPI.Effect_SpawnEffect_BAR(world, worldEventData.EffectRef, entity.Position + new float2(0.5f, 0.5f), 0);
			}
		}

		public void RemoveEvent(IWorldEvent eventToRemove)
		{
			m_EventsToRemove.Add(eventToRemove);
		}
		
		public void Tick(float deltaTime)
		{
			foreach (var worldEvent in m_Events)
			{
				worldEvent.Tick(deltaTime);
			}

			foreach (var worldEvent in m_EventsToRemove)
			{
				m_Events.Remove(worldEvent);
				worldEvent.OnFinished();
			}
			
			m_EventsToRemove.Clear();
		}
	}
}