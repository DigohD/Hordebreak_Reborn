using FNZ.Client.GPUSkinning;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace FNZ.Client.Systems.Hordes 
{
	public enum GPUAnimationType : byte
	{
		Idle = 0,
		Walk = 1,
		Run = 2,
		Attack = 3,
		Attack2 = 4,
		Attack3 = 5,
	}

	public struct GPUAnimationComponent : IComponentData
	{
		public GPUAnimationType CurrentAnimationId;
		public float Speed;
		public float IdleAnimSpeed;
		public float WalkAnimSpeed;
		public float RunAnimSpeed;
		public float AttackAnimSpeed;
		public float Attack2AnimSpeed;
		public float Attack3AnimSpeed;
		public bool IsFirstFrame;
		public bool RandomizeStartTime;
		public float2 RandomizeMinMaxSpeed;
		public byte UseWalk;
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(HordeMovementSystem))]
	public class GPUAnimatorSystem : SystemBase
	{
		private EntityQuery m_Query;

        protected override void OnCreate()
        {
			var queryDescription = new EntityQueryDesc
			{
				All = new ComponentType[] 
				{
					ComponentType.ReadWrite<GPUAnimationComponent>(),
					ComponentType.ReadWrite<GPUAnimationState>()
				}
			};

			m_Query = GetEntityQuery(queryDescription);
		}

        protected override void OnUpdate()
		{
			var animJob = new AnimatorJob
			{
				DeltaTime = Time.DeltaTime,
				GPUAnimationComponentTypeHandle = GetComponentTypeHandle<GPUAnimationComponent>(),
				GPUAnimationStateTypeHandle = GetComponentTypeHandle<GPUAnimationState>()
			};

			Dependency = animJob.ScheduleParallel(m_Query, 1, Dependency);
		}
	}

	[BurstCompile]
	public struct AnimatorJob : IJobEntityBatch
	{
		[ReadOnly] public float DeltaTime;

		public ComponentTypeHandle<GPUAnimationComponent> GPUAnimationComponentTypeHandle;
		public ComponentTypeHandle<GPUAnimationState> GPUAnimationStateTypeHandle;

		public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
		{
			var chunkAnimComps = batchInChunk.GetNativeArray(GPUAnimationComponentTypeHandle);
			var chunkAnimStates = batchInChunk.GetNativeArray(GPUAnimationStateTypeHandle);

			for (var i = 0; i < batchInChunk.Count; i++)
			{
				var animComp = chunkAnimComps[i];
				var animState = chunkAnimStates[i];

				animState.AnimationClipIndex = (int)animComp.CurrentAnimationId;
				ref var clips = ref animState.AnimationClipSet.Value.Clips;

				if (!clips[animState.AnimationClipIndex].IsValid) return;

				if ((uint)animState.AnimationClipIndex < (uint)clips.Length)
				{
					if (!animComp.IsFirstFrame)
					{
						animState.Time += DeltaTime * animComp.Speed;
					}
					else
					{
						var length = 20.0F;
						var random = new Random((uint)(i + batchIndex) + 1);

						random.NextInt();

						if (animComp.RandomizeStartTime && (animState.AnimationClipIndex == (int)GPUAnimationType.Run 
						                                    || animState.AnimationClipIndex == (int)GPUAnimationType.Walk 
						                                    || animState.AnimationClipIndex == (int)GPUAnimationType.Idle))
							animState.Time = random.NextFloat(0, length);

						//animComp.Speed = random.NextFloat(animComp.RandomizeMinMaxSpeed.x, animComp.RandomizeMinMaxSpeed.y);

						animComp.IsFirstFrame = false;	
					}
				}

				chunkAnimComps[i] = animComp;
				chunkAnimStates[i] = animState;
			}
		}
	}
}