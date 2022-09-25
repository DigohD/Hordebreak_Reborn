using FNZ.Server.Controller;
using FNZ.Server.Controller.Systems;
using FNZ.Shared.Model.Entity.Components;

namespace FNZ.Server.FarNorthZMigrationStuff
{
	public class NPCPlayerAwareComponentServer : FNEComponent, ITickable
	{
		FlowFieldComponentServer m_FlowFieldComponent;
		private bool seenPlayer = false;
		private bool heardSound = false;
		public bool isSeeingPlayer = false;

		private int seenAlertTimer = 0;
		private int seenAlertTimeInSeconds = 0;

		private int soundAlertTimer = 0;
		private int soundAlertTimeInSeconds = 0;

		public override void Init()
		{
			base.Init();
			m_FlowFieldComponent = ParentEntity.GetComponent<FlowFieldComponentServer>();
		}

		public void Tick(float dt)
		{
			if (seenPlayer)
			{
				if (seenAlertTimer >= seenAlertTimeInSeconds)
				{
					seenAlertTimer = 0;
					seenPlayer = false;
					m_FlowFieldComponent.sightFlowField = null;
				}
				else
				{
					seenAlertTimer++;
				}
			}
			else if (heardSound)
			{
				if (soundAlertTimer >= soundAlertTimeInSeconds)
				{
					soundAlertTimer = 0;
					heardSound = false;
					m_FlowFieldComponent.soundFlowField = null;
				}
				else
				{
					soundAlertTimer++;
				}
			}
		}

		public void SeenAlert(int seconds)
		{
			if (heardSound)
			{
				soundAlertTimer = 0;
				heardSound = false;
				m_FlowFieldComponent.soundFlowField = null;
			}

			seenAlertTimer = 0;
			seenAlertTimeInSeconds = (int)(1 / ServerMainSystem.TARGET_SERVER_TICK_TIME) * seconds;
			seenPlayer = true;
		}

		public void SoundAlert(int seconds)
		{
			soundAlertTimer = 0;
			soundAlertTimeInSeconds = (int)(1 / ServerMainSystem.TARGET_SERVER_TICK_TIME) * seconds;
			heardSound = true;

			if (!isSeeingPlayer)
			{
				seenAlertTimer = 0;
				seenPlayer = false;
				m_FlowFieldComponent.sightFlowField = null;
			}
		}

		public bool HasSeenPlayer()
		{
			return seenPlayer;
		}

		public bool HasHeardSound()
		{
			return heardSound;
		}

		public override ushort GetSizeInBytes() { return 0; }

	}
}