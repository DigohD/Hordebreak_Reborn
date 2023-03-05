using FNZ.Server.Model.World;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Net.API
{
	public enum BroadcastType
	{
		BA,
		BAR,
		BOR,
		STC
	}

	public partial class ServerNetworkAPI
	{
		public void Entity_AttackTarget_BAR(List<HordeEntityAttackTargetData> attackers)
		{
			var message = m_HordeEntityMessageFactory.CreateHordeEntityAttackTargetMessage(attackers);
			var positions = new NativeArray<float2>(attackers.Count, Allocator.TempJob);

			for (int i = 0; i < attackers.Count; i++)
				positions[i] = attackers[i].AttackerPosition;

			Broadcast_All_Relevant_Batch(message, ref positions);

			positions.Dispose();
		}

		public void Entity_SpawnEntity_BAR(FNEEntity toSpawn)
		{
			var message = m_EntityMessageFactory.CreateSpawnEntityMessage(toSpawn);
			Broadcast_All_Relevant(message, toSpawn.Position);
		}

		public void Entity_SpawnEntity_BAR_Batched(FNEEntity[] toSpawn)
		{
			var positions = new NativeArray<float2>(toSpawn.Length, Allocator.TempJob);

			for (int i = 0; i < toSpawn.Length; i++)
				positions[i] = toSpawn[i].Position;

			var message = m_EntityMessageFactory.CreateSpawnEntityBatchMessage(toSpawn);
			Broadcast_All_Relevant_Batch(message, ref positions);
			positions.Dispose();
		}

		public void Entity_SpawnHordeEntity_Batched_BAR(List<HordeEntitySpawnData> hordeEntitiesToSpawn)
        {
			//Debug.Log("[SERVER]: Entity_SpawnHordeEntity_Batched_BAR");
			var message = m_HordeEntityMessageFactory.CreateSpawnHordeEntityBatchMessage(hordeEntitiesToSpawn);

			var positions = new NativeArray<float2>(hordeEntitiesToSpawn.Count, Allocator.TempJob);
			for (var i = 0; i < hordeEntitiesToSpawn.Count; i++)
				positions[i] = hordeEntitiesToSpawn[i].Position;

			Broadcast_All_Relevant_Batch(message, ref positions);
			positions.Dispose();
		}
		
		public void Entity_SpawnHordeEntity_Batched_STC(List<HordeEntitySpawnData> hordeEntitiesToSpawn, NetConnection conn)
		{
			var message = m_HordeEntityMessageFactory.CreateSpawnHordeEntityBatchMessage(hordeEntitiesToSpawn);
			SendToClient(message, conn);
		}

		/// <summary>
		///		Net API module: Entity
		///		NetMessage Type: UpdateComponents
		///		Send method: BAR (Broadcast To All Relevant Clients)
		/// 
		/// Broadcasts an update of the given Components to all relevant players. A relevant player is
		/// a player that is on the same or neighbouring chunks as the Entity on which the components were attached
		/// </summary>
		/// <param name="parent">Parent Entity for the given component(s)</param>
		/// <param name="components">The component(s) that has been updated and needs to be replicated</param>
		public void Entity_UpdateEntity_BAR(FNEEntity parent)
		{
			var message = m_EntityMessageFactory.CreateUpdateEntityMessage(parent);
			Broadcast_All_Relevant(message, parent.Position);
		}

		public void Entity_UpdateEntity_BOR(FNEEntity parent, NetConnection toExclude)
		{
			var message = m_EntityMessageFactory.CreateUpdateEntityMessage(parent);
			Broadcast_Other_Relevant(message, parent.Position, toExclude);
		}

		public void Entity_UpdateHordeEntity_Batched_STC(List<HordeEntityUpdateNetData> hordeEntitiesToUpdate, NetConnection toReceive)
		{
			var message = m_HordeEntityMessageFactory.CreateUpdateHordeEntityBatchMessage(hordeEntitiesToUpdate);
			SendToClient(message, toReceive);
		}

		public void Entity_UpdateComponent_STC(FNEComponent component, NetConnection toReceive)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			SendToClient(message, toReceive);
		}
		public void Entity_UpdateComponent_STC_Batched(FNEComponent[] component, NetConnection[] toReceive)
		{ }

		public void Entity_UpdateComponent_BA(FNEComponent component)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			Broadcast_All(message);
		}
		public void Entity_UpdateComponent_BA_Batched(FNEComponent[] components)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentBatchMessage(components);
			Broadcast_All(message);
		}

		public void Entity_UpdateComponent_BAR(FNEComponent component)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			Broadcast_All_Relevant(message, component.ParentEntity.Position);
		}

		public void Entity_UpdateComponent_BAIP(FNEComponent component, ServerWorld world)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			Broadcast_All_InProximity(world, message, component.ParentEntity.Position, 60);
		}

		public void Entity_UpdateComponent_BAR_Batched(FNEComponent[] components)
		{
			var positions = new NativeArray<float2>(components.Length, Allocator.TempJob);

			for (int i = 0; i < components.Length; i++)
				positions[i] = components[i].ParentEntity.Position;

			var message = m_EntityMessageFactory.CreateUpdateComponentBatchMessage(components);
			Broadcast_All_Relevant_Batch(message, ref positions);
			positions.Dispose();
		}

		public void Entity_UpdateComponent_BOR(FNEComponent component, NetConnection toExclude)
		{
			var message = m_EntityMessageFactory.CreateUpdateComponentMessage(component);
			Broadcast_Other_Relevant(message, component.ParentEntity.Position, toExclude);
		}
		public void Entity_UpdateComponent_BOR_Batched(FNEComponent[] components, NetConnection[] playersToExclude)
		{ }

		public void Entity_DestroyEntity_BAR(FNEEntity toDestroy)
		{
			var message = m_EntityMessageFactory.CreateDestroyEntityMessage(toDestroy);
			Broadcast_All_Relevant(message, toDestroy.Position);
		}

		public void Entity_DestroyHordeEntity_Batched_BAR(List<HordeEntityDestroyData> entitiesToDestroy)
		{
			var message = m_HordeEntityMessageFactory.CreateDestroyHordeEntityBatchMessage(entitiesToDestroy);

			var positions = new NativeArray<float2>(entitiesToDestroy.Count, Allocator.TempJob);

			for (int i = 0; i < entitiesToDestroy.Count; i++)
				positions[i] = entitiesToDestroy[i].Position;

			Broadcast_All_Relevant_Batch(message, ref positions);

			positions.Dispose();
		}
		
		public void Entity_DestroyHordeEntity_Batched_STC(List<HordeEntityDestroyData> entitiesToDestroy, NetConnection conn)
		{
			var message = m_HordeEntityMessageFactory.CreateDestroyHordeEntityBatchMessage(entitiesToDestroy);
			SendToClient(message, conn);
		}
		
		public void Entity_UpdatePosAndRot_STC(FNEEntity entity, NetConnection connection)
		{

		}
		public void Entity_UpdatePosAndRotBatch_STC(FNEEntity[] entities, NetConnection[] connections)
		{

		}

		public void Entity_UpdatePosAndRot_BA(FNEEntity entity)
		{

		}
		public void Entity_UpdatePosAndRotBatch_BA(FNEEntity[] entities)
		{

		}		

		public void Entity_UpdatePosAndRot_BAR(FNEEntity entity)
		{
			var message = m_EntityMessageFactory.CreateUpdatePosAndRotMessage(entity);
			Broadcast_All_Relevant(message, entity.Position);
		}
		public void Entity_UpdatePosAndRotBatch_BAR(FNEEntity[] entities)
		{
			var positions = new NativeArray<float2>(entities.Length, Allocator.TempJob);

			for (int i = 0; i < entities.Length; i++)
				positions[i] = entities[i].Position;

			var message = m_EntityMessageFactory.CreateUpdatePosAndRotBatchMessage(entities);
			Broadcast_All_Relevant_Batch(message, ref positions);
			positions.Dispose();
		}

		public void Entity_UpdatePosAndRot_BOR(FNEEntity entity)
		{

		}
		public void Entity_UpdatePosAndRotBatch_BOR(FNEEntity[] entities, NetConnection[] connections)
		{

		}

		public void BA_Entity_ComponentNetEvent(FNEComponent component, byte eventId, IComponentNetEventData data = null)
		{
			var message = m_EntityMessageFactory.CreateComponentNetEventMessage(component, eventId, data);
			Broadcast_All(message);
		}

		public void BAR_Entity_ComponentNetEvent(FNEComponent component, byte eventId, IComponentNetEventData data)
		{
			var message = m_EntityMessageFactory.CreateComponentNetEventMessage(component, eventId, data);
			Broadcast_All_Relevant(message, component.ParentEntity.Position);
		}

		public void BOR_Entity_ComponentNetEvent(FNEComponent component, byte eventId, IComponentNetEventData data, NetConnection toExclude)
		{
			var message = m_EntityMessageFactory.CreateComponentNetEventMessage(component, eventId, data);
			Broadcast_Other_Relevant(message, component.ParentEntity.Position, toExclude);
		}

		public void Entity_QueuePosAndRotUpdate(Enum broadcastCategory, FNEEntity entity, NetConnection connection = null)
		{
			if (!m_EntityUpdatePosRotBatch.ContainsKey(broadcastCategory))
				m_EntityUpdatePosRotBatch.Add(broadcastCategory, new List<Tuple<FNEEntity, NetConnection>>());

			if (m_EntityUpdatePosRotBatch[broadcastCategory].Find(tuple => tuple.Item1 == entity) != null)
			{
				UnityEngine.Debug.LogError("Error: Component already added!");
				return;
			}

			m_EntityUpdatePosRotBatch[broadcastCategory].Add(new Tuple<FNEEntity, NetConnection>(entity, connection));
		}

		public void Entity_RemoveFromPosAndRotUpdateBatch(FNEEntity entity)
		{
			foreach (var list in m_EntityUpdatePosRotBatch.Values)
			{
				var ent = list.Find(tuple => tuple.Item1 == entity);
				if (ent != null)
				{
					list.Remove(ent);
					break;
				}
			}
		}

		public void Entity_SendUpdatePosAndRotBatch()
		{
			foreach (var key in m_EntityUpdatePosRotBatch.Keys)
			{
				if (m_EntityUpdatePosRotBatch[key].Count > 0)
				{
					var tupleList = m_EntityUpdatePosRotBatch[key];
					FNEEntity[] entities = new FNEEntity[tupleList.Count];
					NetConnection[] connections = new NetConnection[tupleList.Count];

					for (int i = 0; i < tupleList.Count; i++)
					{
						entities[i] = tupleList[i].Item1;
						connections[i] = tupleList[i].Item2;
					}

					switch (key)
					{
						case BroadcastType.BA:
							Entity_UpdatePosAndRotBatch_BA(entities);
							break;

						case BroadcastType.BAR:
							Entity_UpdatePosAndRotBatch_BAR(entities);
							break;

						case BroadcastType.BOR:
							Entity_UpdatePosAndRotBatch_BOR(entities, connections); //Incomplete. Here for later if we need it.
							break;

						case BroadcastType.STC:
							Entity_UpdatePosAndRotBatch_STC(entities, connections); //Incomplete. Here for later if we need it.
							break;

						default:
							break;
					}

					m_EntityUpdatePosRotBatch[key].Clear();
				}
			}
		}

		public void Entity_QueueComponentUpdate(Enum broadcastCategory, FNEComponent component, NetConnection connection = null)
		{
			if (!m_ComponentUpdateBatch.ContainsKey(broadcastCategory))
				m_ComponentUpdateBatch.Add(broadcastCategory, new List<Tuple<FNEComponent, NetConnection>>());

			if (m_ComponentUpdateBatch[broadcastCategory].Find(tuple => tuple.Item1 == component) != null)
			{
				UnityEngine.Debug.LogWarning("Component already added!");
				return;
			}

			m_ComponentUpdateBatch[broadcastCategory].Add(new Tuple<FNEComponent, NetConnection>(component, connection));
		}

		public void Entity_SendUpdateComponentBatch()
		{
			foreach (var key in m_ComponentUpdateBatch.Keys)
			{
				if (m_ComponentUpdateBatch[key].Count > 0)
				{
					var tupleList = m_ComponentUpdateBatch[key];
					FNEComponent[] components = new FNEComponent[tupleList.Count];
					NetConnection[] connections = new NetConnection[tupleList.Count];

					for (int i = 0; i < tupleList.Count; i++)
					{
						components[i] = tupleList[i].Item1;
						connections[i] = tupleList[i].Item2;
					}

					switch (key)
					{
						case BroadcastType.BA:
							Entity_UpdateComponent_BA_Batched(components);
							break;

						case BroadcastType.BAR:
							Entity_UpdateComponent_BAR_Batched(components);
							break;

						case BroadcastType.BOR:
							Entity_UpdateComponent_BOR_Batched(components, connections); //Incomplete. Here for later if we need it.
							break;

						case BroadcastType.STC:
							Entity_UpdateComponent_STC_Batched(components, connections); //Incomplete. Here for later if we need it.
							break;

						default:
							break;
					}

					m_ComponentUpdateBatch[key].Clear();
				}
			}
		}

	}
}