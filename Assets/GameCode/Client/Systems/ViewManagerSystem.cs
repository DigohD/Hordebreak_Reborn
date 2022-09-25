using FNZ.Shared.Model.Entity;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FNZ.Client.Systems 
{
    public struct ViewData
    {
        public int NetId;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ClientWorldChunkManagerSystem))]
    public class ViewManagerSystem : SystemBase
    {
        private const int c_ViewsToQueueForSpawnPerFrame = 25;

        private NativeQueue<ViewData> m_Queue;
        
        protected override void OnCreate()
        {
            m_Queue = new NativeQueue<ViewData>(Allocator.Persistent);
        }

        public void AddViewDataToQueue(ViewData data)
        {
            m_Queue.Enqueue(data);
        }

        protected override void OnUpdate()
        {
            if (m_Queue.Count <= 0) return;
            
            var amount = (m_Queue.Count <= c_ViewsToQueueForSpawnPerFrame) ? m_Queue.Count : c_ViewsToQueueForSpawnPerFrame;

            for (var i = 0; i < amount; i++)
            {
                var data = m_Queue.Dequeue();
                var entityModel = GameClient.NetConnector.GetEntity(data.NetId);
                if (entityModel == null)
                {
                    Debug.LogError("[CLIENT, ViewManagerSystem]: entityModel null");
                    continue;
                }
                
                var viewDataRef = FNEEntity.GetEntityViewVariationId(entityModel.Data, entityModel.Position);
                GameClient.ViewAPI.QueueViewForSpawn(entityModel, viewDataRef);
            }
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
        }
    }
}