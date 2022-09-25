using FNZ.Shared.Model.Entity.Components.EnemyStats;

namespace FNZ.Server.Model.Entity.Components.AI
{
	public partial class AIComponentServer
	{
		private void HitAndRun()
		{
			if (m_AttackCooldownCurrent <= 0)
				m_CurrentBehaviour = EnemyBehaviour.Aggressive;
			else
				m_CurrentBehaviour = EnemyBehaviour.Hiding;
		}
	}
}
