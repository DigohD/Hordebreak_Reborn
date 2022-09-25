using FNZ.Client.Net;
using FNZ.Client.Systems.Hordes.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace FNZ.Client.Systems.Hordes
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(UpdateTargetPositionSystem))]
	public class HordeUpdateAttackingTargetSystem : SystemBase
	{
		private struct HordeEntityAttackData
		{
			public int NetId;
			public float2 AttackTargetPosition;
		}
		
		private NativeQueue<HordeEntityAttackData> m_Queue;

		protected override void OnCreate()
		{
			base.OnCreate();

			m_Queue = new NativeQueue<HordeEntityAttackData>(Allocator.Persistent);

			GameClient.NetConnector.Register(NetMessageType.ATTACK_ENTITY, OnAttackEntityMessageReceived);
		}

		protected override void OnDestroy()
		{
			m_Queue.Dispose();
		}

		private void OnAttackEntityMessageReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var attackTargetBatchData = new HordeEntityAttackTargetBatchData();
			attackTargetBatchData.NetDeserialize(incMsg);

			foreach (var data in attackTargetBatchData.Entities)
			{
				if (net.GetEntity(data.AttackerNetId) == null) continue;
				if (GameClient.ViewConnector.GetEntity(data.AttackerNetId) == default) continue;
				var e = net.GetEntity(data.TargetNetId);
				if (e == null) continue;
				m_Queue.Enqueue(new HordeEntityAttackData 
				{
					NetId = data.AttackerNetId,
					AttackTargetPosition = e.Position
				});
			}
		}

		protected override void OnUpdate()
		{
			if (m_Queue.Count <= 0) return;
			var entities = new NativeArray<Entity>(m_Queue.Count, Allocator.TempJob);
			var attackTargetPositions = new NativeArray<float2>(m_Queue.Count, Allocator.TempJob);

			for (var i = 0; i < entities.Length; i++)
			{
				var data = m_Queue.Dequeue();
				var e = GameClient.ViewConnector.GetEntity(data.NetId);
				if (e == default || !EntityManager.Exists(e)) 
					continue;
				entities[i] = e;
				attackTargetPositions[i] = data.AttackTargetPosition;
			}

			var updateAnimationStateJob = new UpdateAnimationStateJob
			{
				EntitiesToUpdate = entities,
				AttackTargetPositions = attackTargetPositions,
				AllHordeEntityAttackTargetPositions = GetComponentDataFromEntity<HordeEntityAttackTargetPosition>()
			};

			Dependency = updateAnimationStateJob.Schedule(entities.Length, 128, Dependency);
		}

		[BurstCompile]
		private struct UpdateAnimationStateJob : IJobParallelFor
		{
			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<Entity> EntitiesToUpdate;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<float2> AttackTargetPositions;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<HordeEntityAttackTargetPosition> AllHordeEntityAttackTargetPositions;

			public void Execute(int index)
			{
				AllHordeEntityAttackTargetPositions[EntitiesToUpdate[index]] = new HordeEntityAttackTargetPosition
				{
					TargetPosition = AttackTargetPositions[index]
				};
			}
		}
	}
}