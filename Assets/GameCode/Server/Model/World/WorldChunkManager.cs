using FNZ.Server.Controller;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using FNZ.Shared.Net.Dto.Hordes;
using UnityEngine.Profiling;

namespace FNZ.Server.Model.World
{
    public struct SyncEntitiesData
    {
        public ServerWorldChunk Chunk;
        public List<FNEEntity> Entities;
        public List<FNEEntity> MovingEntities;
    }

    public class WorldChunkManager
    {
        private const int c_ChunkRadius = 1;

        private readonly Dictionary<NetConnection, PlayerChunkState> m_PlayerChunkStates;
        private readonly HashSet<ServerWorldChunk> m_PossibleChunksToUnload;

        private readonly WorldGenWorker m_WorkerThread;
        private readonly UnloadChunkWorker m_UnloadChunkWorker;

        public WorldChunkManager()
        {
            m_WorkerThread = new WorldGenWorker();
            m_UnloadChunkWorker = new UnloadChunkWorker();
            m_PlayerChunkStates = new Dictionary<NetConnection, PlayerChunkState>();
            m_PossibleChunksToUnload = new HashSet<ServerWorldChunk>();
        }

        public PlayerChunkState GetPlayerChunkState(NetConnection conn)
        {
            return m_PlayerChunkStates.TryGetValue(conn, out var result) ? result : null;
        }

        public Dictionary<NetConnection, PlayerChunkState> GetAllPlayersChunkStates()
        {
            return m_PlayerChunkStates;
        }

        public void AddClientToChunkStreamingSystem(NetConnection clientConnection, PlayerChunkState chunkState)
        {
            m_PlayerChunkStates.Add(clientConnection, chunkState);
        }

        public void OnPlayerEnteringNewChunk(FNEEntity player)
        {
            var clientConnection = GameServer.NetConnector.GetConnectionFromPlayer(player);
            var state = m_PlayerChunkStates[clientConnection];

            var chunkSpawnData = new ChunkSpawnData
            {
                State = state,
                PlayerPosition = player.Position,
                ChunkRadius = c_ChunkRadius
            };

            lock (m_WorkerThread.Lock)
            {
                m_WorkerThread.QueueChunkForSpawn(chunkSpawnData);
                Monitor.Pulse(m_WorkerThread.Lock);
                m_WorkerThread.DoWork = true;
            }
        }

        public void ProcessChunksToLoadForClients()
        {
            foreach (var clientConnection in m_PlayerChunkStates.Keys)
            {
                var state = m_PlayerChunkStates[clientConnection];
                
                lock (state.Lock)
                {
                    if (state.ChunksAwaitingLoad.Count <= 0) continue;
                    var chunkToLoad = state.ChunksAwaitingLoad[0];

                    if (!chunkToLoad.IsInitialized ||
                        state.ChunksSentForUnloadAwaitingConfirm.Contains(chunkToLoad))
                        continue;

                    SendChunkToClient(chunkToLoad, clientConnection);
                    state.ChunksSentForLoadAwaitingConfirm.Add(chunkToLoad);
                    if (state.ChunksAwaitingLoad.Count > 0)
                        state.ChunksAwaitingLoad.RemoveAt(0);
                }
            }
        }

        private void SendChunkToClient(ServerWorldChunk chunk, NetConnection clientConnection)
        {
            var netBuffer = new NetBuffer();
            netBuffer.EnsureBufferSize(chunk.TotalBitsNetBuffer());
            chunk.NetSerialize(netBuffer);
            GameServer.NetAPI.World_LoadChunk_STC(chunk, netBuffer.Data, clientConnection);

            // var state = GetPlayerChunkState(clientConnection);
            //
            // var hordeEntitiesToSpawn = new List<HordeEntitySpawnData>();
            //
            // foreach (var e in chunk.GetAllEnemies())
            // {
            //     if (!state.MovingEntitiesSynced.Contains(e.NetId))
            //         hordeEntitiesToSpawn.Add(new HordeEntitySpawnData
            //         {
            //             Position = e.Position,
            //             Rotation = e.RotationDegrees,
            //             NetId = e.NetId,
            //             EntityIdCode = IdTranslator.Instance.GetIdCode<FNEEntityData>(e.EntityId)
            //         });
            // }
            //
            // GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_STC(hordeEntitiesToSpawn, clientConnection);
        }

        public void ProcessChunksToUnloadForClients()
        {
            Profiler.BeginSample("ProcessChunksToUnloadForClients - Part 1");
            m_PossibleChunksToUnload.Clear();

            foreach (var conn in m_PlayerChunkStates.Keys)
            {
                var state = m_PlayerChunkStates[conn];

                lock (state.Lock)
                {
                    if (state.ChunksAwaitingUnload.Count <= 0) continue;
                    var (chunkToUnload, item2) = state.ChunksAwaitingUnload[0];
                    var now = FNEUtil.NanoTime();
                    if ((now - item2) / 1000000000 < 5) continue;
                    if (state.ChunksSentForLoadAwaitingConfirm.Contains(chunkToUnload))
                        continue;

                    if (chunkToUnload != null)
                    {
                        var hordeEntitiesToDestroy = new List<HordeEntityDestroyData>();

                        foreach (var e in chunkToUnload.GetAllEnemies())
                        {
                            if (state.MovingEntitiesSynced.Contains(e.NetId))
                                hordeEntitiesToDestroy.Add(new HordeEntityDestroyData
                                {
                                    NetId = e.NetId
                                });
                        }

                        GameServer.NetAPI.Entity_DestroyHordeEntity_Batched_STC(hordeEntitiesToDestroy, conn);
                        GameServer.NetAPI.World_UnloadChunk_STC(chunkToUnload, conn);
                    
                        state.ChunksSentForUnloadAwaitingConfirm.Add(chunkToUnload);
                        m_PossibleChunksToUnload.Add(chunkToUnload);
                    }
                
                    if (state.ChunksAwaitingUnload.Count > 0)
                        state.ChunksAwaitingUnload.RemoveAt(0);
                }
            }

            Profiler.EndSample();

            Profiler.BeginSample("ProcessChunksToUnloadForClients - Part 2");

            var triggerUnloadChunkJob = false;

            foreach (var chunk in m_PossibleChunksToUnload)
            {
                var canUnloadChunk = true;

                foreach (var state in m_PlayerChunkStates.Values)
                {
                    if (state.ChunksSentForUnloadAwaitingConfirm.Contains(chunk)) continue;
                    canUnloadChunk = false;
                    break;
                }

                if (canUnloadChunk && chunk.IsActive)
                {
                    chunk.IsActive = false;

                    m_UnloadChunkWorker.QueueChunkForUnload(new ChunkUnloadData
                    {
                        Path = GameServer.FilePaths.GetOrCreateChunkFilePath(chunk),
                        Data = chunk.GetChunkData()
                    });

                    var entities = new List<FNEEntity>();

                    foreach (var edgeObj in chunk.SouthEdgeObjects)
                    {
                        if (edgeObj == null) continue;
                        entities.Add(edgeObj);
                    }

                    foreach (var edgeObj in chunk.WestEdgeObjects)
                    {
                        if (edgeObj == null) continue;
                        entities.Add(edgeObj);
                    }

                    foreach (var tileObj in chunk.TileObjects)
                    {
                        if (tileObj == null) continue;
                        entities.Add(tileObj);
                    }

                    foreach (var enemy in chunk.GetAllEnemies())
                    {
                        entities.Add(enemy);
                    }

                    GameServer.World.AddChunkToUnloadQueue(new UnloadChunkData
                    {
                        Chunk = chunk,
                        Entities = entities
                    });

                    triggerUnloadChunkJob = true;
                }
            }

            if (triggerUnloadChunkJob)
            {
                lock (m_UnloadChunkWorker.Lock)
                {
                    Monitor.Pulse(m_UnloadChunkWorker.Lock);
                    m_UnloadChunkWorker.DoWork = true;
                }
            }

            Profiler.EndSample();
        }

        public bool IsUnloadingChunks()
        {
            return m_UnloadChunkWorker.DoWork;
        }

        public void GetConnectionsWithChunkLoaded(ServerWorldChunk chunk, ref List<NetConnection> conns)
        {
            foreach (var conn in m_PlayerChunkStates.Keys)
            {
                if (m_PlayerChunkStates[conn].CurrentlyLoadedChunks.Contains(chunk))
                {
                    if (!conns.Contains(conn)) conns.Add(conn);
                }
            }
        }

        public bool ChunkCanBeUnloaded(ServerWorldChunk chunkToUnload)
        {
            foreach (var state in m_PlayerChunkStates.Values)
            {
                if (state.IsChunkInLoadedState(chunkToUnload)) return false;
            }

            return true;
        }
    }
}