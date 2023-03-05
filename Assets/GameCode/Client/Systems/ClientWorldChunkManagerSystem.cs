using FNZ.Client.Model.World;
using FNZ.Client.Systems.Hordes;
using FNZ.Client.View.World;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net.Dto.Hordes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
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

        private List<int2> m_LoadedChunks = new List<int2>();
        private List<int2> m_ChunksToBeLoaded = new List<int2>();
        private float2 lastPlayerPos = new float2(-1, -1);

        protected override void OnCreate()
        {
            m_Queue = new ConcurrentQueue<SyncEntitiesOnChunkData>();
        }

        protected override void OnUpdate()
        {
            var chunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();

            m_HordeSpawnerSystem ??= GameClient.ECS_ClientWorld.GetExistingSystem<HordeSpawnerSystem>();
            m_ViewManagerSystem ??= GameClient.ECS_ClientWorld.GetExistingSystem<ViewManagerSystem>();

            while (m_Queue.Count > 0)
            {
                m_Queue.TryDequeue(out var data);
                InitChunk(data);
            }


            var player = GameClient.LocalPlayerEntity;
            if (chunk != null && chunk.IsInitialized && player != null)
            {
                if ((int)lastPlayerPos.x / 32 != (int)player.Position.x / 32 || (int)lastPlayerPos.y / 32 != (int)player.Position.y / 32)
                {
                    OnPlayerChangeChunk();
                    lastPlayerPos = player.Position;
                }
            }
        }

        private void OnPlayerChangeChunk()
        {
            var worldChunk = GameClient.World.GetWorldChunk<ClientWorldChunk>();
            var player = GameClient.LocalPlayerEntity;

            var pChunkX = (int) player.Position.x / 32;
            var pChunkY = (int) player.Position.y / 32;

            m_ChunksToBeLoaded.Clear();

            for(int i = pChunkX - 1; i <= pChunkX + 1; i++)
            {
                for (int j = pChunkY - 1; j <= pChunkY + 1; j++)
                {
                    if (pChunkX < 0 || pChunkY < 0 || pChunkX >= worldChunk.SideSize || pChunkY >= worldChunk.SideSize)
                        continue;

                    m_ChunksToBeLoaded.Add(new int2(i, j));
                }
            }

            foreach(var chunkPos in m_ChunksToBeLoaded)
            {
                if (!m_LoadedChunks.Contains(chunkPos))
                {
                    var chunkViewGO = Object.Instantiate((GameObject)Resources.Load("Prefab/Chunk/Chunk"));
                    chunkViewGO.transform.position = new Vector3(chunkPos.x * 32, 0f, chunkPos.y * 32);
                    chunkViewGO.transform.rotation = Quaternion.Euler(270, 90, 90);

                    chunkViewGO.name = "world_chunk-" + chunkPos.x + "-" + chunkPos.y;
                    var chunkView = chunkViewGO.AddComponent<ClientWorldChunkView>();

                    GameClient.WorldView.AddChunkView(new int2(chunkPos.x, chunkPos.y), chunkView);

                    chunkView.Init(worldChunk, (byte) chunkPos.x, (byte) chunkPos.y);
                    m_LoadedChunks.Add(chunkPos);

                    var netConn = GameClient.NetConnector;

                    foreach (var entity in netConn.GetAllEntitiesOnViewChunk(chunkPos.x, chunkPos.y))
                    {
                        m_ViewManagerSystem.AddViewDataToQueue(new ViewData
                        {
                            NetId = entity.fneEntity.NetId
                        });
                    }
                }
            }

            foreach (var chunkPos in m_LoadedChunks)
            {
                if (!m_ChunksToBeLoaded.Contains(chunkPos))
                {
                    // Unload chunk
                }
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
                        break;
                    case EntityType.TILE_OBJECT:
                        GameClient.World.AddTileObject(entity);
                        GameClient.NetConnector.SyncEntity(entity);
                        
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

            for(byte x = 0; x < chunk.SideSize / 32; x++)
            {
                for (byte y = 0; y < chunk.SideSize / 32; y++)
                {
                    // Old chunk instantiation
                }
            }

            chunk.EntitiesToSync.Clear();

            GameClient.NetAPI.CMD_World_ConfirmChunkLoaded(chunk);
        }
    }
}