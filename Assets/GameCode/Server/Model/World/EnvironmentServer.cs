using FNZ.Server.Utils;
using FNZ.Shared.Model.World;

namespace FNZ.Server.Model.World
{
	public class EnvironmentServer : EnvironmentShared
	{
		private float m_Timer = 0;

		public EnvironmentServer() : base() { }

		public void Tick(float deltaTime)
		{
			m_Timer += deltaTime;
			if (m_Timer >= SECONDS_PER_HOUR)
			{
				m_Timer = 0;

				if (!freezeTime)
					m_Hour++;

				if (m_Hour % 24 == 0)
				{
					m_Hour = 0;
				}

				GameServer.NetAPI.World_Environment_BA();
			}

			foreach (var connection in GameServer.NetConnector.GetConnectedClientConnections())
			{
				var playerEntity = GameServer.NetConnector.GetPlayerFromConnection(connection);

				TileTimeEffectGen.GenerateTileTimeEffects(playerEntity);
			}
		}

		public byte Hour
		{
			get
			{
				return m_Hour;
			}
			set
			{
				m_Hour = value;
			}
		}

		public bool FreezeTime
		{
			get
			{
				return freezeTime;
			}
			set
			{
				freezeTime = value;
			}
		}
	}
}