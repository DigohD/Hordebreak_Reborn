using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using FNZ.Client.GPUSkinning;
using FNZ.Client.Net;
using FNZ.Client.Systems.Hordes.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.GPUAnimationData;
using Lidgren.Network;

namespace FNZ.Client.Systems.Hordes
{
    public struct HordeSpawnData
    {
        public int NetId;
        public Entity Prefab;
        public Entity BlobShadowPrefab;
        public float2 Position;
        public float Rotation;
        public float IdleAnimSpeed;
        public float WalkAnimSpeed;
        public float RunAnimSpeed;
        public float AttackAnimSpeed;
        public float Attack2AnimSpeed;
        public float Attack3AnimSpeed;
        public byte UseWalk;
    }

    public struct BlobShadow_Tag : IComponentData { }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(DestroyEntitySystem))]
    public class HordeSpawnerSystem : SystemBase
    {
        private BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
        private NativeQueue<HordeSpawnData> m_Queue;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = GameClient.ECS_ClientWorld
                .GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            m_Queue = new NativeQueue<HordeSpawnData>(Allocator.Persistent);
            
            GameClient.NetConnector.Register(NetMessageType.SPAWN_HORDE_ENTITY_BATCH, OnSpawnHordeEntityBatch);
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count <= 0) return;

            var entitySpawnDataArray = new NativeArray<HordeSpawnData>(m_Queue.Count, Allocator.TempJob);

            for (var i = 0; i < entitySpawnDataArray.Length; i++)
            {
                if (!m_Queue.TryDequeue(out var data)) continue;
                entitySpawnDataArray[i] = data;
            }

            Dependency = new HordeSpawnJob
            {
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                SpawnDataArray = entitySpawnDataArray
            }.Schedule(entitySpawnDataArray.Length, 128, Dependency);
            
            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        public void QueueHordeEntityForSpawn(string id, HordeEntitySpawnData spawnData)
        {
            var entityPrefab = GPUAnimationCharacterUtility.GetEntityPrefab(id);

            if (entityPrefab == null)
                return;

            var blobShadowPrefab = GPUAnimationCharacterUtility.GetBlobShadowEntityPrefab(id);

            var entityData = DataBank.Instance.GetData<FNEEntityData>(id);
            var animCompData = entityData.GetComponentData<GPUAnimationComponentData>();
            
            var idleAnimData = animCompData.Animations[(int)GPUAnimationType.Idle];
            var walkAnimData = animCompData.Animations[(int)GPUAnimationType.Walk];
            var runAnimData = animCompData.Animations[(int)GPUAnimationType.Run];
            var attackAnimData = animCompData.Animations[(int)GPUAnimationType.Attack];
            var attack2AnimData = animCompData.Animations[(int)GPUAnimationType.Attack2];
            var attack3AnimData = animCompData.Animations[(int)GPUAnimationType.Attack3];
            
            m_Queue.Enqueue(new HordeSpawnData
            {
                Prefab = entityPrefab.Value,
                BlobShadowPrefab = blobShadowPrefab ?? default,
                Position = spawnData.Position,
                Rotation = spawnData.Rotation,
                NetId = spawnData.NetId,
                IdleAnimSpeed = idleAnimData.IsUsed ? idleAnimData.Speed : 0,
                WalkAnimSpeed = walkAnimData.IsUsed ? walkAnimData.Speed : 0,
                RunAnimSpeed = runAnimData.IsUsed ? runAnimData.Speed : 0,
                AttackAnimSpeed = attackAnimData.IsUsed ? attackAnimData.Speed : 0,
                Attack2AnimSpeed = attack2AnimData.IsUsed ? attack2AnimData.Speed : 0,
                Attack3AnimSpeed = attack3AnimData.IsUsed ? attack3AnimData.Speed : 0,
                UseWalk = (byte)(!runAnimData.IsUsed ? 1 : 0)
            });
        }

        private void OnSpawnHordeEntityBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
        {
            var spawnDataBatch = new HordeEntitySpawnBatchData();
            spawnDataBatch.NetDeserialize(incMsg);

            foreach (var spawnData in spawnDataBatch.Entities)
            {
                var entityId = IdTranslator.Instance.GetId<FNEEntityData>(spawnData.EntityIdCode);
                var enemy = GameClient.EntityFactory.CreateEnemy(entityId, spawnData.NetId, spawnData.Position,
                    spawnData.Rotation);
                GameClient.World.AddEnemyToTile(enemy);
                GameClient.NetConnector.SyncEntity(enemy);
                QueueHordeEntityForSpawn(entityId, spawnData);
            }
            
            GameClient.NetAPI.CMD_Entity_ConfirmHordeEntityBatchSpawned(
                spawnDataBatch.Entities
                .Select(e => e.NetId)
                .ToList()
            );
        }
    }

    public struct NetEntitySpawnedEventData : IComponentData
    {
        public int NetId;
        public Entity SpawnedEntity;
        public Entity BlobShadowEntity;
        public byte MarkedForDestroy;
    }

    [BurstCompile(FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
    public struct HordeSpawnJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [ReadOnly, DeallocateOnJobCompletion] 
        public NativeArray<HordeSpawnData> SpawnDataArray;

        public void Execute(int index)
        {
            var spawnData = SpawnDataArray[index];

            var netId = spawnData.NetId;
            var position = spawnData.Position;
            var rotation = spawnData.Rotation;

            var spawnedEntity = CommandBuffer.Instantiate(index, spawnData.Prefab);
            Entity blobShadow = default;
            
            if (spawnData.BlobShadowPrefab != default)
            {
                blobShadow = CommandBuffer.Instantiate(index, spawnData.BlobShadowPrefab);
            }
           
            var netSyncEventEntity = CommandBuffer.CreateEntity(index); 
            
            CommandBuffer.AddComponent(index, netSyncEventEntity, new NetEntitySpawnedEventData
            {
                NetId = netId,
                SpawnedEntity = spawnedEntity,
                BlobShadowEntity = blobShadow,
                MarkedForDestroy = 0
            });
            
            CommandBuffer.SetComponent(index, spawnedEntity, new Translation
            {
                Value = new float3(position.x, 0, position.y)
            });

            CommandBuffer.SetComponent(index, spawnedEntity, new Rotation
            {
                Value = quaternion.Euler(0, rotation, 0)
            });
            
            CommandBuffer.SetComponent(index, spawnedEntity, new GPUAnimationComponent
            {
                Speed = 1.0f,
                IdleAnimSpeed = spawnData.IdleAnimSpeed,
                WalkAnimSpeed = spawnData.WalkAnimSpeed,
                RunAnimSpeed = spawnData.RunAnimSpeed,
                AttackAnimSpeed = spawnData.AttackAnimSpeed,
                Attack2AnimSpeed = spawnData.Attack2AnimSpeed,
                Attack3AnimSpeed = spawnData.Attack3AnimSpeed,
                CurrentAnimationId = GPUAnimationType.Idle,
                IsFirstFrame = true,
                UseWalk = spawnData.UseWalk
            });
            
            CommandBuffer.SetComponent(index, spawnedEntity, new HordeEntityServerPosition
            {
                TargetPosition = position
            });

            CommandBuffer.SetComponent(index, spawnedEntity, new NetworkIdComponent
            {
                NetId = netId
            });
        }
    }
}