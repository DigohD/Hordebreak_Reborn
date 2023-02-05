using FNZ.Server.Model.Entity.Components;
using FNZ.Server.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Controller;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.VFX;
using FNZ.Shared.Utils;
using FNZ.Shared.Utils.CollisionUtils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Controller
{

	public class RealEffectManagerServer : RealEffectManager
	{
		private ServerWorld _world;
		public RealEffectManagerServer(ServerWorld world)
		{
			_world = world;
		}
		protected struct ProjectileEffectStorage
		{
			public float Lifetime;
			public float2 Position;
			public float2 Direction;
			public float RrotationZDegrees;
			public ProjectileEffectData Data;
			public long OwnerId;

			public ItemWeaponModComponentData[] mods;

			// Final values for simulation
			public float FinalSpeed;
			public float FinalDamage;

			public string ModDeathEffect;

			public void CalculateFinalValues()
			{
				float speedMulMod = 1;
				float speedFlatMod = 0;

				float damageMulMod = 1;
				float damageFlatMod = 0;

				if(mods != null)
				{
					foreach (var mod in mods)
					{
						if (mod == null)
							continue;

						foreach (var buff in mod.modBuffList)
						{
							switch (buff.modBuff)
							{
								case ModBuff.MOD_PROJECTILE_SPEED:
									speedMulMod *= buff.mulMod;
									speedFlatMod += buff.flatMod;
									break;

								case ModBuff.MOD_DAMAGE:
									damageMulMod *= buff.mulMod;
									damageFlatMod += buff.flatMod;
									break;

								case ModBuff.MOD_DEATH_EFFECT:
									ModDeathEffect = buff.effectRef;
									break;
							}
						}
					}
				}
				

				FinalSpeed = (Data.speed * speedMulMod) + speedFlatMod;
				FinalDamage = (Data.damage * damageMulMod) + damageFlatMod;
			}
		}

		protected List<ProjectileEffectStorage> m_ProjectileEffects = new List<ProjectileEffectStorage>();
		protected List<ProjectileEffectStorage> m_ExpiredProjectiles = new List<ProjectileEffectStorage>();

		private FNEEntity m_HitEntity;
		private Action m_HitEntityAction;
		private float m_HitEntityDistance;

		private readonly byte RELEVANT_CHUNK_DESTRUCTION_PADDING = 5;

		public void Update(float deltaTime)
		{
			UpdateProjectiles(deltaTime);
		}

		protected void UpdateProjectiles(float deltaTime)
		{
			m_ExpiredProjectiles.Clear();
			
			for (int i = 0; i < m_ProjectileEffects.Count; i++)
			{
				m_HitEntity = null;
				m_HitEntityAction = null;
				m_HitEntityDistance = float.MaxValue;

				var proj = m_ProjectileEffects[i];

				if (HasProjectileExpired(proj))
					continue;

				proj.Lifetime += deltaTime;

				var velocity = proj.Direction * (deltaTime * proj.FinalSpeed);
				proj.Position += velocity;

				var hitTerrain = CheckForTerrainCollision(proj, velocity, i);
				bool hitPlayer = false;
				bool hitEnemy = false;
				if (!hitTerrain && proj.Data.targetsPlayers)
					hitPlayer = CheckForPlayerCollision(proj, velocity, i);

				if (!hitTerrain && !hitPlayer && proj.Data.targetsEnemies)
					hitEnemy = CheckForEnemyCollision(proj, velocity, i);

				if (m_HitEntityAction != null)
				{
					if (!m_HitEntity.IsDead)
					{
						m_HitEntityAction.Invoke();
						continue;
					}
				}

				

				m_ProjectileEffects[i] = proj;
				//GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug3", proj.Position, 0);
			}

			DestroyExpiredProjectiles();
		}

		private bool HasProjectileExpired(ProjectileEffectStorage proj)
		{
			// Check if the lifetime of the projectile has expired
			if (
				proj.Lifetime > proj.Data.lifetime ||
				_world.IsAnySurroundingTileInRadiusNull(
					new int2((int)proj.Position.x, (int)proj.Position.y),
					RELEVANT_CHUNK_DESTRUCTION_PADDING
				)
			)
			{
				m_ExpiredProjectiles.Add(proj);
				return true;
			}
			return false;
		}

		private bool CheckForTerrainCollision(ProjectileEffectStorage proj, float2 velocity, int projIndex)
		{
			var envHit = FNECollisionUtils.ProjectileRayCastForWallsAndTileObjectsInModel(
				proj.Position - velocity,
				proj.Position,
				_world
			);

			float dist = envHit.IsHit ? Vector2.Distance(proj.Position - velocity, envHit.HitEntity.Position) : float.MaxValue;

			if (envHit.IsHit && dist < m_HitEntityDistance)
			{
				m_HitEntityDistance = dist;
				m_HitEntity = envHit.HitEntity;

				m_HitEntityAction = new Action(() =>
				{
					if (!envHit.HitEntity.IsDead)
					{
						EffectHitEntity(proj, envHit.HitEntity);
						proj.Position = envHit.HitLocation;
						m_ProjectileEffects[projIndex] = proj;
						m_ExpiredProjectiles.Add(m_ProjectileEffects[projIndex]);
					}
				});

				return true;
			}
			return false;
		}

		private bool CheckForPlayerCollision(ProjectileEffectStorage proj, float2 velocity, int projIndex)
		{
			FNEEntity playerEntity = null;

			if (proj.OwnerId >= 0)
				return false;
			
			foreach (var playerConn in GameServer.NetConnector.GetConnectedClientConnections())
			{
				if (playerConn.RemoteUniqueIdentifier == proj.OwnerId)
					playerEntity = GameServer.NetConnector.GetPlayerFromConnection(playerConn);
			}

			var hitStruct = FNECollisionUtils.RayCastForPlayersInModel(
				proj.Position - velocity,
				proj.Position,
				_world,
				playerEntity != null ? playerEntity.NetId : -1
			);

			float dist = hitStruct.IsHit ? Vector2.Distance(proj.Position - velocity, hitStruct.HitEntity.Position) : float.MaxValue;

			if (hitStruct.IsHit && dist < m_HitEntityDistance)
			{
				m_HitEntityDistance = dist;
				m_HitEntity = hitStruct.HitEntity;

				m_HitEntityAction = new Action(() =>
				{
					if (!hitStruct.HitEntity.IsDead)
					{
						EffectHitEntity(proj, hitStruct.HitEntity);
						var tmpProj = m_ProjectileEffects[projIndex];
						tmpProj.Position = hitStruct.HitLocation;
						m_ProjectileEffects[projIndex] = tmpProj;
						m_ExpiredProjectiles.Add(m_ProjectileEffects[projIndex]);

						if (!m_HitEntity.IsDead)
							GameServer.NetAPI.Entity_UpdateComponent_BA(hitStruct.HitEntity.GetComponent<StatComponentServer>());
					}
				});

				return true;
			}
			return false;
		}

		private bool CheckForEnemyCollision(ProjectileEffectStorage proj, float2 velocity, int projIndex)
		{
			var hitStruct = FNECollisionUtils.RayCastForEnemiesInModel(
				proj.Position - velocity,
				proj.Position,
				_world
			);

			float dist = hitStruct.IsHit ? Vector2.Distance(proj.Position - velocity, hitStruct.HitEntity.Position) : float.MaxValue;

			if (hitStruct.IsHit && dist < m_HitEntityDistance)
			{
				m_HitEntityDistance = dist;
				m_HitEntity = hitStruct.HitEntity;

				m_HitEntityAction = new Action(() =>
				{
					if (!hitStruct.HitEntity.IsDead)
					{
						EffectHitEntity(proj, hitStruct.HitEntity);
						var tmpProj = m_ProjectileEffects[projIndex];
						tmpProj.Position = hitStruct.HitLocation;
						m_ProjectileEffects[projIndex] = tmpProj;
						m_ExpiredProjectiles.Add(m_ProjectileEffects[projIndex]);

						if (!m_HitEntity.IsDead)
							GameServer.NetAPI.Entity_UpdateComponent_BAR(hitStruct.HitEntity.GetComponent<EnemyStatsComponentServer>());
					}
				});
				return true;
			}
			return false;
		}

		private void DestroyExpiredProjectiles()
		{
			// Remove all dead projectiles from game, and trigger on death effects
			foreach (var expProj in m_ExpiredProjectiles)
			{
				m_ProjectileEffects.Remove(expProj);

				//GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug", expProj.Position, 0);

				EffectData deathEffectData = null;
				if (!string.IsNullOrEmpty(expProj.ModDeathEffect))
					deathEffectData = DataBank.Instance.GetData<EffectData>(expProj.ModDeathEffect);
				else if(!string.IsNullOrEmpty(expProj.Data.onDeathEffectRef))
					deathEffectData = DataBank.Instance.GetData<EffectData>(expProj.Data.onDeathEffectRef);

				if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ProjectileEffectData))
					for (int i = 0; i < deathEffectData.repetitions; i++)
						SpawnProjectileServerAuthority(
							deathEffectData,
							(ProjectileEffectData)deathEffectData.RealEffectData,
							expProj.Position,
							expProj.RrotationZDegrees,
							expProj.OwnerId
						);

				if (deathEffectData != null && deathEffectData.HasRealEffect() && deathEffectData.GetRealEffectDataType() == typeof(ExplosionEffectData))
				{
					ExecuteExplosionRealEffect(
						(ExplosionEffectData)deathEffectData.RealEffectData,
						expProj.Position
					);
				}
			}
		}

		private void EffectHitEntity(ProjectileEffectStorage proj, FNEEntity hitEntity)
		{
			var healthComp = hitEntity.GetComponent<StatComponentServer>();
			if (healthComp == null)
			{
				Debug.LogWarning("Enemy StatComponentServer null");
				return;
			}

			if (proj.Data == null)
			{
				Debug.LogWarning("proj.Data null");
				return;
			}
			
			healthComp.Server_ApplyDamage(proj.FinalDamage, proj.Data.damageTypeRef);
			healthComp.Heal(proj.Data.healing);
		}

		// This function is called when a projectile is spawned by a client
		public void SpawnProjectileClientAuthority(
			EffectData data, 
			ProjectileEffectData projData, 
			float2 position, 
			float rotationZDegrees,
			ItemWeaponModComponentData[] mods,
			NetConnection owner = null
		)
		{
			var vfxData = DataBank.Instance.GetData<VFXData>(projData.projectileVfxRef);

			var finalZ = rotationZDegrees;

			var direction = new float2(Mathf.Cos(math.radians(finalZ)), Mathf.Sin(math.radians(finalZ)));

			var owningPlayer = GameServer.NetConnector.GetPlayerFromConnection(owner); 
			
			var newProj = new ProjectileEffectStorage()
			{
				Lifetime = 0,
				Data = projData,
				RrotationZDegrees = rotationZDegrees,
				Position = position,
				Direction = direction,
				OwnerId = owner != null ? owningPlayer.NetId : -1,
				mods = mods
			};

			newProj.CalculateFinalValues();
			m_ProjectileEffects.Add(newProj);
		}

		// This function is called when a projectile is spawned by the server
		public void SpawnProjectileServerAuthority(
			EffectData data, 
			ProjectileEffectData projData, 
			float2 position, 
			float rotationZDegrees,
			long ownerId
		)
		{
			var vfxData = DataBank.Instance.GetData<VFXData>(projData.projectileVfxRef);

			for (int i = 0; i < projData.pellets; i++)
			{
				var finalZ = rotationZDegrees + FNERandom.GetRandomFloatInRange(-projData.inaccuracy, projData.inaccuracy);
				finalZ = projData.invertDirection ? finalZ + 180 : finalZ;

				if (finalZ < 0) finalZ += 360;
				finalZ = FNEUtil.UnpackShortToFloat(FNEUtil.PackFloatAsShort(finalZ));

				var direction = new float2(Mathf.Cos(math.radians(finalZ)), Mathf.Sin(math.radians(finalZ)));
				m_ProjectileEffects.Add(new ProjectileEffectStorage()
				{
					Lifetime = 0,
					Data = projData,
					RrotationZDegrees = finalZ,
					Position = position,
					Direction = direction,
					OwnerId = ownerId
				});

				GameServer.NetAPI.Effect_SpawnEffect_BAR(
					data.Id,
					position,
					finalZ
				);
			}
		}

		public void ExecuteExplosionRealEffect(ExplosionEffectData explosion, float2 position)
		{
			if (explosion.targetsPlayers)
			{
				foreach (var player in GameServer.NetConnector.GetConnectedClientEntities())
				{
					var dist = math.distance(player.Position, position);
					if (dist <= explosion.maxRadius)
					{
						float percent;
						if (explosion.minRadius >= explosion.maxRadius)
							percent = 1f;
						else
							percent = 1f - ((dist - explosion.minRadius) / (explosion.maxRadius - explosion.minRadius));

						var statComp = player.GetComponent<StatComponentServer>();
						statComp.Server_ApplyDamage(explosion.damage * percent, explosion.damageTypeRef);

						if (explosion.healing > 0)
							statComp.Heal(explosion.healing * percent);

						GameServer.NetAPI.Entity_UpdateComponent_STC(statComp, GameServer.NetConnector.GetConnectionFromPlayer(player));
					}
				}
			}

			if (explosion.targetsEnemies)
			{
				foreach (var tile in _world.GetSurroundingTilesInRadius((int2)position, (int)explosion.maxRadius + 1))
				{
					var enemies = _world.GetTileEnemies(tile).ToArray();

					for (int e = enemies.Length - 1; e >= 0; e--)
					{
						var statComp = enemies[e].GetComponent<StatComponentServer>();
						if(statComp.CurrentHealth <= 0)
							continue;;
						
						var dist = math.distance(enemies[e].Position, position);
						if (dist <= explosion.maxRadius)
						{
							float percent;
							if (explosion.minRadius >= explosion.maxRadius)
								percent = 1f;
							else
								percent = 1f - ((dist - explosion.minRadius) / (explosion.maxRadius - explosion.minRadius));

							
							statComp.Server_ApplyDamage(explosion.damage * percent, explosion.damageTypeRef);

							if (explosion.healing > 0)
								statComp.Heal(explosion.healing * percent);
						}
					}
				}
			}
		}
	}
}