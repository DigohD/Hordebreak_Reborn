using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace FNZ.Client.Systems
{
    public struct BloodSplatterSpawnData
    {
        public float2 Position;
        public float Rotation;
        public float Scale;
        public float LifeTime;
    }
    
    public struct BloodSplatter_Tag : IComponentData { }

    public struct BloodSplatterLifeTimeData : IComponentData
    {
        public bool Spawned;
        public float LifeTime;
    }
    
    public struct BloodSplatterTargetScale : IComponentData
    {
        public float Scale;
    }
    
    public struct BloodSplatterDestroyedData : IComponentData
    {
        public int2 TilePosition;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(RenderMeshSpawnerSystem))]
    public class BloodSplatterSystem : SystemBase
    {
        private Entity m_EntityPrefab;
        private BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

        private NativeQueue<BloodSplatterSpawnData> m_Queue;

        private const int c_MaxBloodSplatterCountPerTile = 2;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = GameClient.ECS_ClientWorld
                .GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            m_Queue = new NativeQueue<BloodSplatterSpawnData>(Allocator.Persistent);

            var prefab = (GameObject) Resources.Load("Prefab/Effects/BloodSplatterEffect");
            var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
            var material = meshRenderer.sharedMaterial;

            var desc = new RenderMeshDescription(
                meshFilter.sharedMesh,
                material,
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: true
            );

            var entityManager = GameClient.ECS_ClientWorld.EntityManager;
            m_EntityPrefab = entityManager.CreateEntity();

            RenderMeshUtility.AddComponents(
                m_EntityPrefab,
                entityManager,
                desc
            );

            entityManager.AddComponentData(m_EntityPrefab, new BloodSplatter_Tag());
            entityManager.AddComponentData(m_EntityPrefab, new BloodSplatterTargetScale());
            entityManager.AddComponentData(m_EntityPrefab, new LocalToWorld());
            entityManager.AddComponentData(m_EntityPrefab, new Translation());
            entityManager.AddComponentData(m_EntityPrefab, new Rotation());
            entityManager.AddComponentData(m_EntityPrefab, new Scale());
            entityManager.AddComponentData(m_EntityPrefab, new BloodSplatterLifeTimeData());
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            const float lerpSpeed = 5.6f;

            Entities
                .WithName("ScaleBloodSplatterJob")
                .WithAll<BloodSplatter_Tag>()
                .WithBurst()
                .ForEach((ref Scale scale, in BloodSplatterTargetScale targetScale) =>
                {
                    if (!(scale.Value < targetScale.Scale)) return;
                    
                    var newScale = math.lerp(
                        scale.Value,
                        targetScale.Scale,
                        math.mul(deltaTime, lerpSpeed));

                    scale.Value = newScale > targetScale.Scale ? targetScale.Scale : newScale;
                }).ScheduleParallel();
            
            if (m_Queue.Count <= 0) return;

            var bloodSplatterToSpawn = m_Queue.ToArray(Allocator.TempJob);
            m_Queue.Clear();

            Dependency = new BloodSplatterSpawnJob
            {
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                SpawnDataArray = bloodSplatterToSpawn,
                Prefab = m_EntityPrefab
            }.Schedule(bloodSplatterToSpawn.Length, 128, Dependency);

            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        public void AddBloodSplatterToSpawnQueue(BloodSplatterSpawnData spawnData)
        {
            var chunkCellData =
                GameClient.World.GetChunkCellData((int) spawnData.Position.x, (int) spawnData.Position.y);
            if (chunkCellData == null) return;
            if (chunkCellData.BloodSplatterCount > c_MaxBloodSplatterCountPerTile) return;
            m_Queue.Enqueue(spawnData);
            chunkCellData.BloodSplatterCount++;
        }

        [UpdateInGroup(typeof(SimulationSystemGroup))]
        [UpdateAfter(typeof(ViewConnectorSystem))]
        public class BloodSplatterDestroySystem : SystemBase
        {
            private EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;
            private EntityQuery m_BloodSplatterDestroyedQuery;

            protected override void OnCreate()
            {
                m_EntityCommandBufferSystem =
                    GameClient.ECS_ClientWorld.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

                m_BloodSplatterDestroyedQuery = GetEntityQuery(ComponentType.ReadOnly<BloodSplatterDestroyedData>());
            }

            protected override void OnUpdate()
            {
                var destroyedBloodSplatterCount = m_BloodSplatterDestroyedQuery.CalculateEntityCount();

                if (destroyedBloodSplatterCount > 0)
                {
                    var destroyedBloodSplatterCommandBuffer = m_EntityCommandBufferSystem
                        .CreateCommandBuffer()
                        .AsParallelWriter();

                    var result = new NativeArray<int2>(destroyedBloodSplatterCount, Allocator.TempJob);
                    
                    Entities
                        .WithName("UpdateBloodSplatterCount")
                        .WithAll<BloodSplatterDestroyedData>()
                        .WithStoreEntityQueryInField(ref m_BloodSplatterDestroyedQuery)
                        .WithBurst()
                        .ForEach((Entity e, int entityInQueryIndex, ref BloodSplatterDestroyedData netEventData) =>
                        {
                            result[entityInQueryIndex] = new int2(netEventData.TilePosition.x, netEventData.TilePosition.y);
                            destroyedBloodSplatterCommandBuffer.DestroyEntity(entityInQueryIndex, e);
                        }).ScheduleParallel();
                    
                    CompleteDependency();

                    for (var i = 0; i < destroyedBloodSplatterCount; i++)
                    {
                        var pos = result[i];
                        var cell = GameClient.World.GetChunkCellData(pos.x, pos.y);
                        cell.BloodSplatterCount--;
                        if (cell.BloodSplatterCount < 0)
                            cell.BloodSplatterCount = 0;
                    }

                    result.Dispose();
                }
                
                var commandBuffer = m_EntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

                var deltaTime = Time.DeltaTime;
                
                Entities
                    .WithName("DestroyBloodSplatterJob")
                    .WithAll<BloodSplatterLifeTimeData>()
                    .WithBurst()
                    .ForEach(
                        (Entity entity, int entityInQueryIndex, 
                            ref BloodSplatterLifeTimeData lifeTime, 
                            ref Scale scale,
                            in Translation translation) =>
                        {
                            if (!lifeTime.Spawned) return;

                            if (lifeTime.LifeTime <= 0.0f)
                            {
                                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
                                var bloodSplatterDestroyedEventEntity = commandBuffer.CreateEntity(entityInQueryIndex);
                                commandBuffer.AddComponent(entityInQueryIndex, bloodSplatterDestroyedEventEntity, new BloodSplatterDestroyedData
                                {
                                    TilePosition = new int2((int)translation.Value.x, (int)translation.Value.z)
                                });
                            }
                            else
                            {
                                lifeTime.LifeTime -= deltaTime;
                            }
                        }).ScheduleParallel();

                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            }
        }

        [BurstCompile(FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard,
            CompileSynchronously = false)]
        private struct BloodSplatterSpawnJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            [ReadOnly] 
            public Entity Prefab;

            [ReadOnly, DeallocateOnJobCompletion] 
            public NativeArray<BloodSplatterSpawnData> SpawnDataArray;

            public void Execute(int index)
            {
                var spawnData = SpawnDataArray[index];
                var position = spawnData.Position;
                var scale = spawnData.Scale;
                var lifeTime = spawnData.LifeTime;
                var spawnPosition = new float3(position.x, 0.02f, position.y);
                var rotation =  quaternion.AxisAngle(new float3(0, 1, 0), spawnData.Rotation);
                
                var spawnedEntity = CommandBuffer.Instantiate(index, Prefab);

                CommandBuffer.SetComponent(index, spawnedEntity, new LocalToWorld
                {
                    Value = float4x4.TRS(
                        spawnPosition,
                        rotation,
                        new float3(0, 0, 0)
                    )
                });

                CommandBuffer.SetComponent(index, spawnedEntity, new Translation
                {
                    Value = spawnPosition
                });
                
                CommandBuffer.SetComponent(index, spawnedEntity, new Rotation
                {
                    Value = rotation
                });

                CommandBuffer.SetComponent(index, spawnedEntity, new Scale
                {
                    Value = 0
                });

                CommandBuffer.SetComponent(index, spawnedEntity, new BloodSplatterTargetScale
                {
                    Scale = scale
                });

                CommandBuffer.SetComponent(index, spawnedEntity, new BloodSplatterLifeTimeData
                {
                    LifeTime = lifeTime,
                    Spawned = true
                });
            }
        }
    }
}