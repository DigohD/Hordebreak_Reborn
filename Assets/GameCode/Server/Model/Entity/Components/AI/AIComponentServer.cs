using FNZ.Server.Controller;
using FNZ.Server.FarNorthZMigrationStuff;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Net.API;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.AI;
using FNZ.Shared.Model.Entity.Components.Enemy;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.AI
{
	public partial class AIComponentServer : AIComponentShared, ITickable
	{
		private EnemyBehaviour m_BasicBehaviour, m_CurrentBehaviour;

		private List<FNEPoint> m_Path;
		private FNEEntity m_TargetPlayer;
		private PlayerComponentServer m_TargetPlayerComponent;
		private EnemyStatsComponentData m_EnemyStatComp;
		private EffectData m_Projectile;

		private Vector2 m_Direction;

		private float2 m_FleeDestination, m_PrevPos;
		private float2 m_Offset = new float2(0.5f, 0.5f);

		private int2 m_FleeVector;

		private float m_PrevRot, m_CurrentSpeed, m_LungeDistance, m_LungeSpeedMod, m_AttackDamage, m_AttackRange, m_AttackCooldownMax, m_AttackCooldownCurrent, m_DistanceToTarget;

		private int m_AggroRange;

		private ServerWorld _world;

		public override void Init()
		{
			base.Init();

			m_EnemyStatComp = ParentEntity.Data.GetComponentData<EnemyStatsComponentData>();
			m_CurrentSpeed = ParentEntity.Data.GetComponentData<EnemyComponentData>().baseSpeed;
			if (m_TargetPlayer != null) m_TargetPlayerComponent = m_TargetPlayer.GetComponent<PlayerComponentServer>();
			if (m_EnemyStatComp.projType != "")
			{
				m_Projectile = DataBank.Instance.GetData<EffectData>(m_EnemyStatComp.projType);
				m_Projectile.GetRealEffectData<ProjectileEffectData>().targetsEnemies = false;
			}

			m_BasicBehaviour = m_EnemyStatComp.behaviour;
			m_CurrentBehaviour = EnemyBehaviour.Chill;

			m_AttackDamage = m_EnemyStatComp.damage;
			m_AttackRange = m_EnemyStatComp.attackRange;
			m_AttackCooldownMax = m_EnemyStatComp.attackCooldown;
			m_LungeDistance = m_EnemyStatComp.lungeDistance != 0.0f ? m_EnemyStatComp.lungeDistance : m_AttackRange * 2;
			m_LungeSpeedMod = m_EnemyStatComp.lungeSpeedMod;
			m_AggroRange = m_EnemyStatComp.aggroRange;
			m_FleeDestination = ParentEntity.Position;

			_world = GameServer.GetWorldInstance(ParentEntity.WorldInstanceIndex);
		}

		public void Tick(float dt)
		{
			m_PrevPos = ParentEntity.Position;
			m_PrevRot = ParentEntity.RotationDegrees;

			m_AttackCooldownCurrent = m_AttackCooldownCurrent < 0 ? 0 : m_AttackCooldownCurrent -= GameServer.DeltaTime;
			m_DistanceToTarget = m_TargetPlayer != null ? Vector2.Distance(ParentEntity.Position, m_TargetPlayer.Position) : float.MaxValue;

			switch (m_CurrentBehaviour)
			{
				case EnemyBehaviour.Chill:
					if (ScanForEnemies())
						m_CurrentBehaviour = m_BasicBehaviour;
					break;
				case EnemyBehaviour.Aggressive:
					Aggressive();
					break;
				case EnemyBehaviour.Fleeing:
					Fleeing();
					break;
				case EnemyBehaviour.HitAndRun:
					HitAndRun();
					break;
				case EnemyBehaviour.Hiding:
					Hiding();
					break;
				case EnemyBehaviour.Investigate:
					Investigate();
					break;

				default:
					Debug.LogError("This should never happen.");
					ScanForEnemies();
					break;
			}

			if (!m_PrevPos.Equals(ParentEntity.Position) || !m_PrevRot.Equals(ParentEntity.RotationDegrees))
			{
				GameServer.NetAPI.Entity_QueuePosAndRotUpdate(BroadcastType.BAR, ParentEntity);

				if ((int)m_PrevPos.x != (int)ParentEntity.Position.x || (int)m_PrevPos.y != (int)ParentEntity.Position.y) //We have switched tile
				{
					_world.GetTileEnemies((int2)m_PrevPos).Remove(ParentEntity);
					_world.AddEnemyToTile(ParentEntity);
				}
			}

		}

		//check for visible players within aggro radius
		private bool ScanForEnemies()
		{
			m_TargetPlayer = null;
			m_TargetPlayerComponent = null;
			m_Path = null;
			var currentTile = (int2)ParentEntity.Position;

			List<int2> inVicinity = _world.GetSurroundingTilesInRadius((int2)ParentEntity.Position, m_AggroRange);

			float dist;
			var closestDistance = float.MaxValue;
			foreach (var tile in inVicinity)
			{
				if (FNEPathfinding.HasLineOfSight(_world, currentTile, tile))
				{
					var players = _world.GetTilePlayers(tile);
					if (players == null || players.Count == 0) continue;

					foreach (var player in players)
					{
						dist = Vector2.Distance(ParentEntity.Position, player.Position);
						if (dist < closestDistance)
						{
							closestDistance = dist;
							m_TargetPlayer = player;
						}
					}
				}
			}

			if (m_TargetPlayer != null)
			{
				m_TargetPlayerComponent = m_TargetPlayer.GetComponent<PlayerComponentServer>();
				return true;
			}

			return false;
		}

		public void SetNewPath(List<FNEPoint> newPath)
		{
			m_Path = newPath;
		}

		public void HeardSound(float2 location)
		{
			if (m_TargetPlayer == null)
			{
				SetNewPath(FNEPathfinding.FindPath(_world, 5000, ParentEntity.Position, location));
				m_CurrentBehaviour = EnemyBehaviour.Investigate;

				if (m_Path == null || m_Path.Count == 0)
				{
					ScanForEnemies();
					m_CurrentBehaviour = m_BasicBehaviour;
				}
			}
		}

		private void FollowPath()
		{
			m_Direction = (m_Path[0].ToFloat2() + m_Offset) - ParentEntity.Position;

			ParentEntity.Position += (float2)m_Direction.normalized * GameServer.DeltaTime * m_CurrentSpeed;
			ParentEntity.RotationDegrees = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;

			if (m_Direction.magnitude <= 0.5f)
			{
				m_Path.RemoveAt(0);

				if (m_Path.Count == 0)
				{
					m_CurrentBehaviour = m_BasicBehaviour;
					m_Path = null;
				}
			}
		}

		private void Investigate()
		{
			var oldPath = m_Path; //ScanForEnemies clears our current path so we save it here in case we don't spot any players while investigating.

			if (ScanForEnemies())
				m_CurrentBehaviour = m_BasicBehaviour;
			else
			{
				SetNewPath(oldPath);
				FollowPath();
			}
		}

		private void MoveToDestination(float2 destination, float lungeSpeedMultiplier = 1)
		{
			m_Direction = destination - ParentEntity.Position;

			ParentEntity.Position += (float2)m_Direction.normalized * GameServer.DeltaTime * (m_CurrentSpeed * lungeSpeedMultiplier);
			ParentEntity.RotationDegrees = Mathf.Atan2(m_Direction.y, m_Direction.x) * Mathf.Rad2Deg;
		}

		private void DoAttack()
		{
			if (m_Projectile != null) //Check here if we have a projectile attack
			{
				//play animation code / Send play animation message here

				_world.RealEffectManager.SpawnProjectileServerAuthority(
					m_Projectile,
					(ProjectileEffectData)m_Projectile.RealEffectData,
					ParentEntity.Position,
					ParentEntity.RotationDegrees,
					-1);
			}
			else //do melee attack
			{
				//play animation code / Send play animation message here

				var healthComp = m_TargetPlayer.GetComponent<StatComponentServer>();
				if (healthComp.CurrentHealth > 0)
				{
					healthComp.Server_ApplyDamage(m_AttackDamage, m_EnemyStatComp.damageTypeRef);
					GameServer.NetAPI.Entity_UpdateComponent_STC(healthComp, GameServer.NetConnector.GetConnectionFromPlayer(m_TargetPlayer));
				}
			}

			m_AttackCooldownCurrent = m_AttackCooldownMax;
			m_CurrentBehaviour = m_BasicBehaviour;
		}
	}
}
