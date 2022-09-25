using FNZ.Shared.Model.Entity;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{
	public class PlayerNetworkSystem
	{
		private float m_UpdatesPerSecond;
		private float m_UpdateTickTimer;
		private float m_SecondsPerUpdate;

		private float2 lastPos;
		private float lastRot;

		private PlayerController m_PlayerController;
		public FNEEntity m_PlayerEntity;

		public PlayerNetworkSystem(PlayerController playerController)
		{
			m_PlayerController = playerController;
			m_PlayerEntity = playerController.GetPlayerEntity();
			m_UpdatesPerSecond = 30.0f;
			m_SecondsPerUpdate = 1.0f / m_UpdatesPerSecond;
		}

		public void Update()
		{
			m_UpdateTickTimer += Time.deltaTime;

			if (m_UpdateTickTimer >= m_SecondsPerUpdate)
			{
				if (m_PlayerEntity.Position.x != lastPos.x
					|| m_PlayerEntity.Position.y != lastPos.y
					|| m_PlayerEntity.RotationDegrees != lastRot)
				{
					lastPos = m_PlayerEntity.Position;
					lastRot = m_PlayerEntity.RotationDegrees;

					GameClient.NetAPI.CMD_Player_UpdatePosAndRot(m_PlayerEntity);
				}

				m_UpdateTickTimer -= m_SecondsPerUpdate;
			}
		}
	}
}