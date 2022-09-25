using System.Collections.Generic;
using System.Linq;
using FNZ.Client.Net;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace FNZ.Client.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BloodSplatterSystem))]
    public class DestroyEntitySystem : SystemBase
    {
        private BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
        private NativeQueue<Entity> m_Queue;
        
        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = GameClient.ECS_ClientWorld
                .GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            m_Queue = new NativeQueue<Entity>(Allocator.Persistent);
            
            GameClient.NetConnector.Register(NetMessageType.DESTROY_HORDE_ENTITY_BATCH, OnDestroyHordeEntityBatch);
        }
        
        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count <= 0) return;

            var entitiesToDestroy = m_Queue.ToArray(Allocator.TempJob);
            m_Queue.Clear();

            Dependency = new DestroyEntitiesJob
            {
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntitiesToDestroy = entitiesToDestroy
            }.Schedule(entitiesToDestroy.Length, 128, Dependency);
                
            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
        }
        
        public void QueueEntityForDestroy(int netId)
        {
            var entityToDestroy = UnsyncModelAndReturnEntityViewToDestroy(netId);
            if (entityToDestroy != default)
                m_Queue.Enqueue(entityToDestroy);
            GameClient.NetAPI.CMD_Entity_ConfirmHordeEntityBatchDestroyed(
                new List<int> { netId }
            );
        }
        
        private void OnDestroyHordeEntityBatch(ClientNetworkConnector net, NetIncomingMessage incMsg)
        {
            var destroyBatch = new HordeEntityDestroyBatchNetData();
            destroyBatch.NetDeserialize(incMsg);
            
            for (var i = 0; i < destroyBatch.Count; i++)
            {
                var netId = destroyBatch.Entities[i].NetId;
                var entityToDestroy = UnsyncModelAndReturnEntityViewToDestroy(netId);
                if (entityToDestroy != default)
                    m_Queue.Enqueue(entityToDestroy);
            }
            
            GameClient.NetAPI.CMD_Entity_ConfirmHordeEntityBatchDestroyed(
                destroyBatch.Entities
                .Select(e => e.NetId)
                .ToList()
            );
        }
        
        private static Entity UnsyncModelAndReturnEntityViewToDestroy(int netId)
        {
            var entityToDestroy = GameClient.ViewConnector.GetEntity(netId);

            if (entityToDestroy == default)
            {
                Debug.LogError(
                    $"[CLIENT, UnsyncEntity]: Could not find Entity in ViewConnector with NetId: {netId}. This should never happen.");
                //return default;
            }

            var entityModel = GameClient.NetConnector.GetEntity(netId);

            if (entityModel == null)
            {
                Debug.LogError(
                    $"[CLIENT, UnsyncEntity]: Could not find Entity in NetEntity list with NetId: {netId}. This should never happen.");
                return default;
            }

            GameClient.World.RemoveEnemyFromTile(entityModel);
            GameClient.ViewConnector.RemoveView(netId);
            GameClient.NetConnector.UnsyncEntity(entityModel);

            return entityToDestroy;
        }
    }
    
    [BurstCompile(FloatMode = FloatMode.Default, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = false)]
    public struct DestroyEntitiesJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<Entity> EntitiesToDestroy;
        
        public void Execute(int index)
        {
            CommandBuffer.DestroyEntity(index, EntitiesToDestroy[index]);
        }
    }
}