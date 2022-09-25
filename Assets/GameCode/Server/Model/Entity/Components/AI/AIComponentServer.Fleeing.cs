using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.AI
{
	public partial class AIComponentServer
	{
		private void Fleeing()
		{
			if (m_Path != null)
				FollowPath();
			else
			{
				if (!ScanForEnemies())
					m_CurrentBehaviour = EnemyBehaviour.Chill;
				else
				{
					if (m_DistanceToTarget <= m_AggroRange * 0.75f)
					{
						m_FleeDestination.x = m_TargetPlayer.Position.x >= ParentEntity.Position.x ? ParentEntity.Position.x - (m_AggroRange / 4) : ParentEntity.Position.x + (m_AggroRange / 4);
						m_FleeDestination.y = m_TargetPlayer.Position.y >= ParentEntity.Position.y ? ParentEntity.Position.y - (m_AggroRange / 4) : ParentEntity.Position.y + (m_AggroRange / 4);

						float dist;
						float longestDist = 0;
						foreach (var tile in GameServer.World.GetSurroundingTilesInRadius((int2)m_FleeDestination, 1))
						{
							dist = Vector2.Distance(ParentEntity.Position, (float2)tile);
							if (dist > longestDist)
							{
								longestDist = dist;
								m_FleeDestination = tile;
							}
						}

						SetNewPath(FNEPathfinding.FindPath(500, ParentEntity.Position, m_FleeDestination));
					}
					else
					{
						m_CurrentBehaviour = EnemyBehaviour.Chill;
					}
				}
			}
		}
	}

}
