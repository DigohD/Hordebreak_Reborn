using FNZ.Client.Model.World;
using FNZ.Client.Systems.Hordes;
using FNZ.Client.View.World;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net.Dto.Hordes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.Systems 
{
    public struct SyncEntitiesOnChunkData
    {
        public ClientWorldChunk Chunk;
        public List<FNEEntity> Entities;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(NetworkClientSystem))]
    public class ClientWorldChunkManagerSystem : SystemBase
    {
        private ConcurrentQueue<SyncEntitiesOnChunkData> m_Queue;
        
        private HordeSpawnerSystem m_HordeSpawnerSystem;
        private ViewManagerSystem m_ViewManagerSystem;

        protected override void OnCreate()
        {
            m_Queue = new ConcurrentQueue<SyncEntitiesOnChunkData>();
        }

        protected override void OnUpdate()
        {
            m_HordeSpawnerSystem ??= GameClient.ECS_ClientWorld.GetExistingSystem<HordeSpawnerSystem>();
            m_ViewManagerSystem ??= GameClient.ECS_ClientWorld.GetExistingSystem<ViewManagerSystem>();

            while (m_Queue.Count > 0)
            {
                m_Queue.TryDequeue(out var data);
                InitChunk(data);
            }
        }

        public void QueueChunkForInitialization(SyncEntitiesOnChunkData data)
        {
            m_Queue.Enqueue(data);
        }

        private void InitChunk(SyncEntitiesOnChunkData data)
        {
            var chunk = data.Chunk;
            
            Profiler.BeginSample("Process Entities to sync");
            
            foreach (var entity in data.Entities)
            {
                switch (entity.EntityType)
                {
                    case EntityType.EDGE_OBJECT:
                        GameClient.World.AddEdgeObject(entity);
                        GameClient.NetConnector.SyncEntity(entity);
                        m_ViewManagerSystem.AddViewDataToQueue(new ViewData
                        {
                            NetId = entity.NetId
                        });
                        break;
                    case EntityType.TILE_OBJECT:
                        GameClient.World.AddTileObject(entity);
                        GameClient.NetConnector.SyncEntity(entity);
                        m_ViewManagerSystem.AddViewDataToQueue(new ViewData
                        {
                            NetId = entity.NetId
                        });
                        break;
                    // case EntityType.ECS_ENEMY:
                    //     GameClient.World.AddEnemyToTile(entity);
                    //     GameClient.NetConnector.SyncEntity(entity);
                    //     m_HordeSpawnerSystem.QueueHordeEntityForSpawn(entity.EntityId, new HordeEntitySpawnData
                    //     {
                    //         NetId = entity.NetId,
                    //         Position = entity.Position,
                    //         Rotation = entity.RotationDegrees
                    //     });
                    //     break;
                    // case EntityType.GO_ENEMY:
                    //     break;
                }
            }
            
            Profiler.EndSample();

            chunk.IsInitialized = true;

            var chunkX = chunk.ChunkX;
            var chunkY = chunk.ChunkY;

            Profiler.BeginSample("Tilemap view creation");

            Profiler.BeginSample("Instantiate ChunkView");
            var chunkViewGO = Object.Instantiate((GameObject)Resources.Load("Prefab/Chunk/Chunk"));
            chunkViewGO.transform.position = new Vector3(0, 0f, 0);
            chunkViewGO.transform.rotation = Quaternion.Euler(270, 90, 90);

            chunkViewGO.name = "world_chunk-" + chunkX + "-" + chunkY;
            var chunkView = chunkViewGO.AddComponent<ClientWorldChunkView>();
            
            GameClient.WorldView.AddChunkView(chunkView);
            Profiler.EndSample();
            
            Profiler.BeginSample("Init ChunkView");
            chunkView.Init(chunk);
            Profiler.EndSample();

            chunk.view = chunkView;

            chunk.EntitiesToSync.Clear();

            GameClient.NetAPI.CMD_World_ConfirmChunkLoaded(chunk);

            Profiler.EndSample();
        }
    }
}