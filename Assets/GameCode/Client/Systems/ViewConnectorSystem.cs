using FNZ.Client.Systems.Hordes;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace FNZ.Client.Systems
{
    public struct ViewConnectorData
    {
        public Entity EntityToSync;
        public int NetId;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HordeSpawnerSystem))]
    public class ViewConnectorSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EndInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem =
                GameClient.ECS_ClientWorld.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            // @TODO(Anders E): Figure out a better solution for connecting NetId to Entity that doesnt cause a sync point
            Profiler.BeginSample("UpdateViewConnector");

            var viewConnector = GameClient.ViewConnector;

            var entitiesSpawned = m_Query.CalculateEntityCount();

            if (entitiesSpawned > 0)
            {
                var commandBuffer = m_EntityCommandBufferSystem
                    .CreateCommandBuffer()
                    .AsParallelWriter();

                var result = new NativeArray<ViewConnectorData>(entitiesSpawned, Allocator.TempJob);

                Entities
                    .WithName("UpdateViewConnectorJob")
                    .WithAll<NetEntitySpawnedEventData>()
                    .WithStoreEntityQueryInField(ref m_Query)
                    .WithBurst()
                    .ForEach((Entity e, int entityInQueryIndex, ref NetEntitySpawnedEventData netEventData) =>
                    {
                        if (netEventData.MarkedForDestroy == 1)
                            return;

                        result[entityInQueryIndex] = new ViewConnectorData
                        {
                            NetId = netEventData.NetId,
                            EntityToSync = netEventData.SpawnedEntity
                        };

                        var blobShadowEntity = netEventData.BlobShadowEntity;

                        if (blobShadowEntity != default)
                        {
                            commandBuffer.SetComponent(entityInQueryIndex, blobShadowEntity, new Parent
                            {
                                Value = netEventData.SpawnedEntity
                            });

                            var buffer = commandBuffer.AddBuffer<LinkedEntityGroup>(entityInQueryIndex, netEventData.SpawnedEntity);
                            buffer.Add(netEventData.SpawnedEntity);
                            buffer.Add(netEventData.BlobShadowEntity);
                        }
                        
                        netEventData.MarkedForDestroy = 1;

                        commandBuffer.DestroyEntity(entityInQueryIndex, e);
                    }).ScheduleParallel();

                CompleteDependency();

                for (var i = 0; i < entitiesSpawned; i++)
                {
                    var data = result[i];
                    if (viewConnector.GetEntity(data.NetId) == default)
                        viewConnector.AddEntity(data.EntityToSync, data.NetId);
                }

                result.Dispose();
                m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
            }

            Profiler.EndSample();
        }
    }
}