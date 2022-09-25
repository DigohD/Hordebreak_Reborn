using FNZ.Client.GPUSkinning;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using FNZ.Client.Systems.Hordes.Components;

namespace FNZ.Client.Systems.Hordes
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HordeUpdateAttackingTargetSystem))]
    public class HordeMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            
            // @TODO(Anders E): Base this lerp factor on the actual speed of the agent
            const float moveLerpSpeedFactor = 4.0f;
            const float rotationLerpSpeedFactor = 2.0f;
            const float distanceThreshold = 1.5f;
            
            Dependency = Entities
                .WithName("UpdateHordePositionsAndRotationsJob")
                .WithAll<HordeEntity_Tag>()
                .WithBurst()
                .ForEach((
                    ref Translation translation,
                    ref Rotation rotation,
                    ref GPUAnimationComponent anim,
                    in HordeEntityAttackTargetPosition attackTarget,
                    in GPUAnimationState animationState,
                    in HordeEntityServerPosition targetPosition,
                    in HordeEntityStatsData stats) =>
                {
                    var currentPosition = new float2(translation.Value.x, translation.Value.z);
                    var currentRotation = rotation.Value;
                    var target = targetPosition.TargetPosition;
                    var toTarget = target - currentPosition;
                    var distance = math.length(toTarget);
                    var toTargetNormalized = math.normalize(toTarget);
                    var targetRotation = quaternion.LookRotation(
                        new float3(-toTargetNormalized.x, 0, -toTargetNormalized.y), 
                        new float3(0, 1, 0)
                    );

                    if (distance > 0.008f)
                    {
                        rotation.Value = math.slerp(
                            currentRotation,
                            targetRotation,
                            math.mul(deltaTime, rotationLerpSpeedFactor)
                        );
                    }
                    
                    var newPosition = math.select(
                        target,
                        math.lerp(
                            currentPosition,
                            target,
                            math.mul(deltaTime, moveLerpSpeedFactor)),
                        distance <= distanceThreshold);

                    if (distance >= 1.0f)
                    {
                        translation.Value = new float3(target.x, 0, target.y);
                    }
                    else
                    {
                        translation.Value = new float3(newPosition.x, 0, newPosition.y);
                    }

                    // @TODO(Anders E): Move all animation state changes to its own system
                    
                    ref var clips = ref animationState.AnimationClipSet.Value.Clips;

                    // @TODO(Anders E): Remove this temporary solution once shrubber has more animations
                    if (!clips[0].IsValid)
                    {
                        anim.CurrentAnimationId = GPUAnimationType.Run;
                    }
                    else
                    {
                        var distToTarget = math.distance(newPosition, attackTarget.TargetPosition);
                        if (distToTarget <= stats.AttackRange)
                        {
                            // var random = new Random((uint)(entityInQueryIndex) + 1);
                            // var attackAnimation = (GPUAnimationType)random.NextInt((int)GPUAnimationType.Attack, (int)GPUAnimationType.Attack3);
                            //
                            // if (!clips[(int) attackAnimation].IsValid)
                            //     attackAnimation = GPUAnimationType.Attack;
                            
                            var nd = math.normalize(attackTarget.TargetPosition - newPosition);
                            
                            var targetRotation2 = quaternion.LookRotation(
                                new float3(-nd.x, 0, -nd.y), 
                                new float3(0, 1, 0)
                            );

                            rotation.Value = math.slerp(
                                currentRotation,
                                targetRotation2,
                                math.mul(deltaTime, rotationLerpSpeedFactor*2.0f)
                            );
                            
                            anim.CurrentAnimationId = GPUAnimationType.Attack;
                            anim.Speed = anim.AttackAnimSpeed;
                        }
                        else
                        {
                            // if (distToTarget <= 5.0f && anim.CurrentAnimationId != GPUAnimationType.Run)
                            // {
                            //     anim.CurrentAnimationId = GPUAnimationType.Run;
                            // }
                            // else if (distToTarget > 5.0f && anim.CurrentAnimationId != GPUAnimationType.Walk)
                            // {
                            //     anim.CurrentAnimationId = GPUAnimationType.Walk;
                            // }
                            // if (distance >= 0.1f)
                            // {
                            //     anim.CurrentAnimationId = GPUAnimationType.Run;
                            // }
                            // else if (distance < 0.1f && distance >= 0.01f)
                            // {
                            //     anim.CurrentAnimationId = GPUAnimationType.Walk;
                            // }
                            anim.CurrentAnimationId = distance < 0.01f ? GPUAnimationType.Idle 
                                : anim.UseWalk == 1 ? GPUAnimationType.Walk : GPUAnimationType.Run;
                        }

                        var speed = 0.0f;

                        switch (anim.CurrentAnimationId)
                        {
                            case GPUAnimationType.Idle:
                                speed = anim.IdleAnimSpeed;
                                break;
                            case GPUAnimationType.Walk:
                                speed = anim.WalkAnimSpeed;
                                break;
                            case GPUAnimationType.Run:
                                speed = anim.RunAnimSpeed;
                                break;
                            case GPUAnimationType.Attack:
                                speed = anim.AttackAnimSpeed;
                                break;
                            case GPUAnimationType.Attack2:
                                speed = anim.Attack2AnimSpeed;
                                break;
                            case GPUAnimationType.Attack3:
                                speed = anim.Attack3AnimSpeed;
                                break;
                        }

                        anim.Speed = speed;
                    }
                    
                }).ScheduleParallel(Dependency);
        }
    }
}