using FNZ.Server.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace FNZ.Server.Controller
{
	public struct ChunkUnloadData
	{
		public string Path;
		public byte[] Data;
	}

	public class UnloadChunkWorker
	{
		public readonly object Lock = new object();

		private volatile int m_ThreadSafeBoolBackValue = 0;
		
		private readonly ConcurrentQueue<ChunkUnloadData> m_ChunkUnloadDataQueue;

		public UnloadChunkWorker()
		{
			m_ChunkUnloadDataQueue = new ConcurrentQueue<ChunkUnloadData>();

			var thread = new Thread(Run)
			{
				Name = "UnloadChunkWorker-Thread"
			};

			thread.Start();
		}

		public void QueueChunkForUnload(ChunkUnloadData data)
		{
			m_ChunkUnloadDataQueue.Enqueue(data);
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
	                    while (m_ChunkUnloadDataQueue.Count > 0)
						{
							if (m_ChunkUnloadDataQueue.TryDequeue(out var data))
							{
								UnloadChunk(data);
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

		private void UnloadChunk(ChunkUnloadData data)
		{
			FNEService.File.WriteFile(data.Path, data.Data);
		}
	}
}