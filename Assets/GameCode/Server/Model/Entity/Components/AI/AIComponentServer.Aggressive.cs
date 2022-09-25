using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.AI
{
	public partial class AIComponentServer
	{
		private void Aggressive()
		{
			if (m_TargetPlayer != null && !m_TargetPlayerComponent.IsDead && m_DistanceToTarget <= m_AggroRange)
			{
				if (FNEPathfinding.HasLineOfSight((int2)ParentEntity.Position, (int2)m_TargetPlayer.Position) && m_DistanceToTarget <= m_AttackRange)
				{
					m_Path = null;
					m_Direction = m_TargetPlayer.Position - ParentEntity.Position;
					ParentEntity.RotationDegrees = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;
					
					if (m_AttackCooldownCurrent <= 0) DoAttack();
				}
				else
				{
					if (FNEPathfinding.HasLineOfSight((int2)ParentEntity.Position, (int2)m_TargetPlayer.Position))
					{
						if (m_DistanceToTarget <= m_LungeDistance)
							MoveToDestination(m_TargetPlayer.Position, m_LungeSpeedMod);
						else
							MoveToDestination(m_TargetPlayer.Position);
					}
					else if (m_Path != null)
						FollowPath();
					else
						SetNewPath(FNEPathfinding.FindPath(500, ParentEntity.Position, m_TargetPlayer.Position));
				}
			}
			else
			{
				if (!ScanForEnemies())
					m_CurrentBehaviour = EnemyBehaviour.Chill;
			}
		}

	}
}
