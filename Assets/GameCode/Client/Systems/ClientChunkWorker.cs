using FNZ.Client.Model.World;
using FNZ.Client.View.World;
using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace FNZ.Client.Systems 
{
	public struct ChunkGenData
    {
		public byte ChunkX;
		public byte ChunkY;
		public byte[] Data;
    }

	public class ClientChunkWorker
	{
		public readonly object Lock = new object();

		private volatile int m_ThreadSafeBoolBackValue = 0;

		private readonly ConcurrentQueue<ChunkGenData> m_ChunkSpawnDataQueue;
		private readonly ClientWorldChunkManagerSystem m_ClientWorldChunkManagerSystem;

		public ClientChunkWorker()
        {
	        m_ChunkSpawnDataQueue = new ConcurrentQueue<ChunkGenData>();
			m_ClientWorldChunkManagerSystem = GameClient.ECS_ClientWorld.GetExistingSystem<ClientWorldChunkManagerSystem>();
			
			var thread = new Thread(Run)
			{
				Name = "ClientChunkCreation-Thread"
			};

			thread.Start();
		}

		public void QueueChunk(ChunkGenData data)
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
			while (GameClient.APPLICATION_RUNNING)
			{
				lock (Lock)
				{
					while (!DoWork) Monitor.Wait(Lock);

					try
					{
						while (m_ChunkSpawnDataQueue.Count > 0)
						{
							m_ChunkSpawnDataQueue.TryDequeue(out var data);
							GenerateChunk(data);
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e);
						if (GameClient.Logger != null)
						{
							GameClient.Logger.Log(e);
						}
					}

					DoWork = false;
				}
			}
		}

		private void GenerateChunk(ChunkGenData data)
        {
			var chunkX = data.ChunkX;
			var chunkY = data.ChunkY;

			var netBuffer = new NetBuffer
			{
				Data = data.Data
			};

			var world = GameClient.World;

			var chunk = new ClientWorldChunk(chunkX, chunkY, world.CHUNK_SIZE);
			world.SetChunk(chunk);

			chunk.NetDeserialize(netBuffer);

			m_ClientWorldChunkManagerSystem.QueueChunkForInitialization(new SyncEntitiesOnChunkData
			{
				Chunk = chunk,
				Entities = chunk.EntitiesToSync
			});
		}
	}
}