using FNZ.Server.Model.World;
using FNZ.Server.Services;
using FNZ.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Controller
{
	public struct ChunkSpawnData
	{
		public float2 PlayerPosition;
		public PlayerChunkState State;
		public int ChunkRadius;
	}

	public class WorldGenWorker
	{
		public readonly object Lock = new object();

		private volatile int m_ThreadSafeBoolBackValue = 0;

		private readonly ConcurrentQueue<ChunkSpawnData> m_ChunkSpawnDataQueue;

		public WorldGenWorker()
		{
			m_ChunkSpawnDataQueue = new ConcurrentQueue<ChunkSpawnData>();

			var thread = new Thread(Run)
			{
				Name = "WorldGen-Thread"
			};

			thread.Start();
		}

		public void QueueChunkForSpawn(ChunkSpawnData data)
		{
			m_ChunkSpawnDataQueue.Enqueue(data);
		}

		public bool DoWork
		{
			get => (Interlocked.CompareExchange(ref m_ThreadSafeBoolBackValue, 1, 1) == 1);
			set
			{
				if (value) Interlocked.CompareExchange(ref m_ThreadSafeBoolBackValue, 1, 0);
				else Interlocked.CompareExchange(ref m_ThreadSafeBoolBackValue, 0, 1);
			}
		}

		private void Run()
		{
			while (GameServer.APPLICATION_RUNNING)
			{
				lock (Lock)
				{
					while (!DoWork) Monitor.Wait(Lock);

                    try
                    {
						while (m_ChunkSpawnDataQueue.Count > 0)
						{
							if (m_ChunkSpawnDataQueue.TryDequeue(out var data))
							{
								OnPlayerEnteringNewChunk(data.State, data.PlayerPosition, data.ChunkRadius);
							}
						}
                    }
					catch(Exception e)
                    {
						Debug.LogError(e);
						if (GameServer.Logger != null)
						{
							GameServer.Logger.Log(e);
						}
                    }

					DoWork = false;
				}
			}
		}
		
		private void OnPlayerEnteringNewChunk(PlayerChunkState state, float2 playerPos, int radius)
		{
			var world = GameServer.World;

			var newChunk = world.GetWorldChunk<ServerWorldChunk>(playerPos);

			var chunkPos = world.GetChunkIndices(playerPos);

			newChunk ??= LoadWorldChunk((byte) chunkPos.x, (byte) chunkPos.y);

			var newChunkX = newChunk.ChunkX;
			var newChunkY = newChunk.ChunkY;

			var newLoadedChunks = new List<ServerWorldChunk>();

			lock (state.Lock)
			{
				for (var y = -radius; y <= radius; y++)
				{
					for (var x = -radius; x <= radius; x++)
					{
						var cx = (byte)(newChunkX + x);
						var cy = (byte)(newChunkY + y);

						if (cx < 0 || cy < 0 || cx >= world.WIDTH_IN_CHUNKS || cy >= world.HEIGHT_IN_CHUNKS) continue;

						var nChunk = world.GetWorldChunk<ServerWorldChunk>(cx, cy) ?? LoadWorldChunk(cx, cy);

						GameServer.World.WorldMap.VisitChunk(nChunk);
						
						if (!state.CurrentlyLoadedChunks.Contains(nChunk) &&
						    !state.ChunksSentForLoadAwaitingConfirm.Contains(nChunk) &&
						    !state.ChunksAwaitingLoad.Contains(nChunk))
						{
							state.ChunksAwaitingLoad.Add(nChunk);
						}

						state.ChunksAwaitingUnload.RemoveAll(t => t.Item1 == nChunk);

						newLoadedChunks.Add(nChunk);
					}
				}

				var i = 0;

				while (i < state.CurrentlyLoadedChunks.Count)
				{
					var chunk = state.CurrentlyLoadedChunks[i];

					if (!newLoadedChunks.Contains(chunk) &&
						state.ChunksAwaitingUnload.FindAll(c => c.Item1 == chunk).Count == 0 &&
						!state.ChunksSentForUnloadAwaitingConfirm.Contains(chunk))
					{
						state.ChunksAwaitingUnload.Add(new Tuple<ServerWorldChunk, long>(chunk, FNETime.NanoTime()));
					}

					i++;
				}

				i = 0;
				while (i < state.ChunksAwaitingLoad.Count)
				{
					var chunk = state.ChunksAwaitingLoad[i];

					if (!newLoadedChunks.Contains(chunk))
					{
						state.ChunksAwaitingLoad.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}

				foreach (var chunk in state.ChunksSentForLoadAwaitingConfirm)
				{
					if (!newLoadedChunks.Contains(chunk))
					{
						state.ChunksAwaitingUnload.Add(new Tuple<ServerWorldChunk, long>(chunk, FNETime.NanoTime()));
					}
				}
			}
		}

		private ServerWorldChunk LoadWorldChunk(byte chunkX, byte chunkY)
		{
			var chunk = new ServerWorldChunk(chunkX, chunkY, GameServer.World.CHUNK_SIZE);
			GameServer.World.AddWorldChunk(chunk);

			var chunkFilePath = GameServer.FilePaths.GetChunkFilePath(chunkX, chunkY);

			if (File.Exists(chunkFilePath))
			{
				FNEService.File.LoadWorldChunkFromFile(chunkFilePath, chunk);
			}
			else
			{
				//GameServer.WorldGen.GenerateChunk(chunk);
			}

			GameServer.World.SyncEntities(new SyncEntitiesData
			{
				Chunk = chunk,
				Entities = chunk.EntitiesToSync,
				MovingEntities = chunk.MovingEntitiesToSync
			});
			
			return chunk;
		}
	}
}