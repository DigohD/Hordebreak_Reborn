using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using FNZ.Client.Systems.Hordes.Components;
using FNZ.Shared.Net;
using FNZ.Client.Net;
using Lidgren.Network;
using FNZ.Shared.Net.Dto.Hordes;

namespace FNZ.Client.Systems.Hordes 
{
    public struct UpdateTargetPositionData
    {
        public Entity EntityToUpdate;
        public float2 TargetPosition;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ViewConnectorSystem))]
    public class UpdateTargetPositionSystem : SystemBase
    {
        private NativeQueue<UpdateTargetPositionData> m_UpdateTargetPositionsQueue;

        protected override void OnCreate()
        {
            m_UpdateTargetPositionsQueue = new NativeQueue<UpdateTargetPositionData>(Allocator.Persistent);

            GameClient.NetConnector.Register(NetMessageType.UPDATE_HORDE_ENTITY_BATCH, OnUpdateHordeEntityBatchMessage);
        }

        protected override void OnDestroy()
        {
            m_UpdateTargetPositionsQueue.Dispose();
        }

        private void OnUpdateHordeEntityBatchMessage(ClientNetworkConnector net, NetIncomingMessage incMsg)
        {
            var updateBatch = new HordeEntityUpdateBatchNetData();
            updateBatch.NetDeserialize(incMsg);

            foreach (var updateData in updateBatch.Entities)
            {
                var netId = updateData.NetId;
                var position = updateData.Position;

                var entityToUpdate = GameClient.ViewConnector.GetEntity(netId);

                if (entityToUpdate == default) 
                    continue;
                
                var entityModel = GameClient.NetConnector.GetEntity(netId);
                if (entityModel == null) continue;

                var currentPosition = entityModel.Position;
                var currentTilePos = new int2((int)currentPosition.x, (int)currentPosition.y);
                var newTilePosition = new int2((int)position.x, (int)position.y);
                
                if (currentTilePos.x != newTilePosition.x || currentTilePos.y != newTilePosition.y)
                {
                    var currentChunkCell = GameClient.World.GetChunkCellData(currentTilePos.x, currentTilePos.y);
                    var newCell = GameClient.World.GetChunkCellData(newTilePosition.x, newTilePosition.y);

                    if (currentChunkCell != null && newCell != null)
                    {
                        currentChunkCell.RemoveEnemy(entityModel);
                        newCell.AddEnemy(entityModel);
                    }
                }

                entityModel.Position = position;

                m_UpdateTargetPositionsQueue.Enqueue(new UpdateTargetPositionData
                {
                    EntityToUpdate = entityToUpdate,
                    TargetPosition = position
                });
            }
        }

        protected override void OnUpdate()
        {
            if (m_UpdateTargetPositionsQueue.Count <= 0) return;
            
            var entities = new NativeArray<Entity>(m_UpdateTargetPositionsQueue.Count, Allocator.TempJob);
            var targetPositions = new NativeArray<HordeEntityServerPosition>(m_UpdateTargetPositionsQueue.Count, Allocator.TempJob);

            for (var i = 0; i < entities.Length; i++)
            {
                var updateTargetPositionData = m_UpdateTargetPositionsQueue.Dequeue();

                entities[i] = updateTargetPositionData.EntityToUpdate;

                targetPositions[i] = new HordeEntityServerPosition
                {
                    TargetPosition = updateTargetPositionData.TargetPosition
                };
            }

            Dependency = new UpdateTargetPositionJob
            {
                Entities = entities,
                TargetPositions = targetPositions,
                AllTargetPositions = GetComponentDataFromEntity<HordeEntityServerPosition>()
            }.Schedule(targetPositions.Length, 128, Dependency);
        }
    }

    [BurstCompile]
    public struct UpdateTargetPositionJob : IJobParallelFor
    {
        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<Entity> Entities;

        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<HordeEntityServerPosition> TargetPositions;

        [WriteOnly, NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<HordeEntityServerPosition> AllTargetPositions;

        public void Execute(int index)
        {
            AllTargetPositions[Entities[index]] = TargetPositions[index];
        }
    }
}