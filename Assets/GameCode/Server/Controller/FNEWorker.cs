using FNZ.Server.FarNorthZMigrationStuff;
using System;
using System.Threading;
using UnityEngine;

namespace FNZ.Server.Controller
{
	public class FNEWorker
	{
		private Thread m_Thread;

		public readonly object Lock = new object();

		private volatile int m_ThreadSafeBoolBackValue = 0;

		public FNEWorker()
		{
			m_Thread = new Thread(Run)
			{
				Name = "Server-Thread"
			};

			m_Thread.Start();
		}

		public bool DoWork
		{
			get { return (Interlocked.CompareExchange(ref m_ThreadSafeBoolBackValue, 1, 1) == 1); }
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
					while (!DoWork)
						Monitor.Wait(Lock);

                    try
                    {
						GameServer.MainWorld.Tick(GameServer.DeltaTime);
						AgentSimulationSystem.Instance.Tick();
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
	}
}