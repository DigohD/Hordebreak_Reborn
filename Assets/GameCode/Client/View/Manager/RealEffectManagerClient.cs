using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Model.World;
using FNZ.Shared.Controller;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using FNZ.Shared.Utils.CollisionUtils;
using System;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.Enemy;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using static FNZ.Shared.Utils.CollisionUtils.FNECollisionUtils;

namespace FNZ.Client.View.Manager
{
	public class RealEffectManagerClient : RealEffectManager
	{
		protected struct ProjectileEffectStorage
		{
			public float SpawnTimeStamp;
			public ProjectileEffectData Data;
			public GameObject ViewRef;
			public bool IsLocal;
            public int OwnerNetId;

			public ItemWeaponModComponentData[] mods;

			public string ModDeathEffect;

			// Final values for simulation
			public float FinalSpeed;
			public float FinalDamage;
			public float FinalPellets;

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
			}
		}

		protected List<ProjectileEffectStorage> m_ProjectileEffects = new List<ProjectileEffectStorage>();
		protected List<ProjectileEffectStorage> m_ExpiredProjectiles = new List<ProjectileEffectStorage>();

		private List<float2> m_Positions = new List<float2>();
		private List<float> m_FinalRots = new List<float>();

		private readonly byte RELEVANT_CHUNK_DESTRUCTION_PADDING = 5;

		public void Update()
		{
			UpdateProjectiles();
		}

		protected void UpdateProjectiles()
		{
			m_ExpiredProjectiles.Clear();

			foreach (var proj in m_ProjectileEffects)
			{
				var projTransform = proj.ViewRef.transform;
				var projPos = new float2(projTransform.position.x, projTransform.position.z);

				if (HasProjectileExpired(proj, projPos))
					continue;

				var velocity = proj.ViewRef.transform.right.normalized * Time.deltaTime * proj.FinalSpeed;

				if (HasProjectileHitTerrain(proj, projPos, velocity))
					continue;

				if (proj.Data.targetsPlayers && HasProjectileHitPlayer(proj, projPos, velocity))
					continue;

				if (proj.Data.targetsEnemies && HasProjectileHitEnemy(proj, projPos, velocity))
					continue;

				// Projectile has not hit anything; move it forward
				proj.ViewRef.transform.Translate(velocity, Space.World);
			}

			DeactivateExpiredProjectiles();
		}

		private bool HasProjectileExpired(ProjectileEffectStorage proj, float2 projPos)
		{
			// Check if projectile lifetime has run out, or has left relevant chunks
			if (
				Time.time - proj.SpawnTimeStamp > proj.Data.lifetime ||
				GameClient.World.IsAnySurroundingTileInRadiusNull(
					new int2((int)projPos.x, (int)projPos.y),
					RELEVANT_CHUNK_DESTRUCTION_PADDING
				)
			)
			{
				m_ExpiredProjectiles.Add(proj);
				return true;
			}
			return false;
		}

		private bool HasProjectileHitTerrain(ProjectileEffectStorage proj, float2 projPos, Vector3 velocity)
		{
			// Check if projectile has hit terrain
			var hit = FNECollisionUtils.ProjectileRayCastForWallsAndTileObjectsInModel(
				projPos - new float2(velocity.x, velocity.z),
				projPos,
				GameClient.World
			);

			if (hit.IsHit)
			{
				m_ExpiredProjectiles.Add(proj);

				var viewId = FNEEntity.GetEntityViewVariationId(hit.HitEntity.Data, hit.HitEntity.Position);
				var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewId);

                if (!string.IsNullOrEmpty(viewData.onHitEffectRef))
                {
					float angle = Mathf.Atan2(velocity.z, velocity.x) * Mathf.Rad2Deg;

					GameClient.ViewAPI.SpawnEffect(
						viewData.onHitEffectRef,
						hit.HitLocation,
						angle,
						false,
						false,
						null,
						null,
						proj.ViewRef.transform.position.y
					);
				}

				return true;
			}

			return false;
		}

		private bool HasProjectileHitPlayer(ProjectileEffectStorage proj, float2 projPos, Vector3 velocity)
		{
			if (proj.IsLocal || proj.OwnerNetId > 0)
				return false;
			
			foreach (var player in GameClient.RemotePlayerEntities)
			{
                if(proj.OwnerNetId != player.NetId && !player.GetComponent<PlayerComponentClient>().IsDead)
				    if (RayTraceProjectilePlayer(
					    proj, projPos, velocity, (Vector2) player.Position
                    ))
				    {
					    return true;
				    }
			}

			if (!proj.IsLocal && !GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().IsDead)
			{
				if (RayTraceProjectilePlayer(
					proj, projPos, velocity, (Vector2) GameClient.LocalPlayerEntity.Position
                ))
				{
					return true;
				}
			}
			return false;
		}

		private bool HasProjectileHitEnemy(ProjectileEffectStorage proj, float2 projPos, Vector3 velocity)
		{
			var hit = FNECollisionUtils.RayCastForEnemiesInModel(projPos - new float2(velocity.x, velocity.z), projPos, GameClient.World);

			if (hit.IsHit)
			{

				float angle = Mathf.Atan2(velocity.z, velocity.x) * Mathf.Rad2Deg;

				var view = DataBank.Instance.GetData<FNEEntityViewData>(hit.HitEntity.Data.ViewRef);

				GameClient.ViewAPI.SpawnEffect(
					view.onHitEffectRef,
					hit.HitLocation,
					angle,
					false,
					false,
					null,
					null,
					proj.ViewRef.transform.position.y
				);

				m_ExpiredProjectiles.Add(proj);
				return true;
			}

			return false;
		}

		private bool RayTraceProjectilePlayer(ProjectileEffectStorage proj, float2 projPos, Vector3 velocity, Vector2 playerPos)
		{
			var lineColRes = new LineCircleCollisionResult();
			FNECollisionUtils.LineCircleCollision(
				projPos - new float2(velocity.x, velocity.z),
				projPos,
				new Vector3(playerPos.x, playerPos.y),
				DefaultValues.PLAYER_RADIUS,
				ref lineColRes
			);

			if (lineColRes.hit)
			{
				m_ExpiredProjectiles.Add(proj);
				return true;
			}

			return false;
		}

		private void DeactivateExpiredProjectiles()
		{
			// Remove projectiles who have hit something or expired, spawn death effect
			foreach (var expProj in m_ExpiredProjectiles)
			{
				m_ProjectileEffects.Remove(expProj);
				GameObjectPoolManager.RecycleObject(expProj.ViewRef.name, expProj.ViewRef);
                if (string.IsNullOrEmpty(expProj.ModDeathEffect))
                {
	                if (!string.IsNullOrEmpty(expProj.Data.onDeathEffectRef))
	                {
		                GameClient.ViewAPI.SpawnEffect(
			                expProj.Data.onDeathEffectRef,
			                new Vector2(expProj.ViewRef.transform.position.x, expProj.ViewRef.transform.position.z),
			                -expProj.ViewRef.transform.rotation.eulerAngles.y,
			                false,
			                false,
			                null,
			                null,
			                expProj.ViewRef.transform.position.y
		                );
	                }
                }
                else
                {
					GameClient.ViewAPI.SpawnEffect(
						expProj.ModDeathEffect,
						new Vector2(expProj.ViewRef.transform.position.x, expProj.ViewRef.transform.position.z),
						-expProj.ViewRef.transform.rotation.eulerAngles.y,
						false,
						false,
						null,
						null,
						expProj.ViewRef.transform.position.y
					);
				}
				
			}
		}

		// This function is used when the player spawns projectiles with some kind of input
		public void SpawnProjectileLocalAuthority(
			EffectData data, 
			ProjectileEffectData projData, 
			float2 position, 
			float rotationDegrees,
			string[] modItemIds,
			ItemWeaponModComponentData[] mods,
			float effectHeight = -1
		)
		{
			Profiler.BeginSample("Discus mannen");

			int additionalPellets = 0;
			int spreadMulMod = 1;
			if (mods != null)
			{
				foreach (var mod in mods)
				{
					if (mod == null)
						continue;

					foreach (var buff in mod.modBuffList)
					{
						if (buff.modBuff == ModBuff.MOD_ADDITIONAL_PELLETS)
						{
							additionalPellets += (int)buff.flatMod;
						}
						else if (buff.modBuff == ModBuff.MOD_SPREAD)
						{
							spreadMulMod *= (int)buff.mulMod;
						}
					}
				}
			}
			

			for (int i = 0; i < projData.pellets + additionalPellets; i++)
			{
				var finalRot = rotationDegrees + UnityEngine.Random.Range(-projData.inaccuracy * spreadMulMod, projData.inaccuracy * spreadMulMod);
				finalRot = projData.invertDirection ? finalRot + 180 : finalRot;

				if (finalRot < 0) finalRot += 360;
				// Turn rotation into its final value after conversion precision loss. Necessary for server sync.
				finalRot = FNEUtil.UnpackShortToFloat(FNEUtil.PackFloatAsShort(finalRot));

				var projectile = GameObjectPoolManager.GetObjectInstance(
					projData.projectileVfxRef, 
					PrefabType.VFX, 
					position
				);
				PrepareProjectile(projectile, position, -finalRot, effectHeight);

				var newProj = new ProjectileEffectStorage()
				{
					SpawnTimeStamp = Time.time + 0.1f,
					ViewRef = projectile,
					Data = projData,
					IsLocal = true,
					mods = mods
				};
				
				newProj.CalculateFinalValues();

				m_ProjectileEffects.Add(newProj);

				if (projData.pellets == 1)
					GameClient.NetAPI.CMD_Effect_Spawn_Projectile(data.Id, position, finalRot, modItemIds);
				else
				{
					m_Positions.Add(position);
					m_FinalRots.Add(finalRot);
				}
			}

			if (projData.pellets > 1)
			{
				GameClient.NetAPI.CMD_Effect_Spawn_ProjectileBatch(data.Id, m_Positions, m_FinalRots, modItemIds);
				m_Positions.Clear();
				m_FinalRots.Clear();
			}

			Profiler.EndSample();
		}

		// This function spawns projectiles as requested by the server
		public void SpawnProjectileServerAuthority(EffectData data, ProjectileEffectData projData, float2 position, float rotationDegrees, int ownerNetId = 0)
		{
			var projectile = GameObjectPoolManager.GetObjectInstance(projData.projectileVfxRef, PrefabType.VFX, position);
			PrepareProjectile(projectile, position, -rotationDegrees);

			var newProj = new ProjectileEffectStorage()
			{
				SpawnTimeStamp = Time.time,
				ViewRef = projectile,
				Data = projData,
				IsLocal = false,
				OwnerNetId = ownerNetId
			};
			
			newProj.CalculateFinalValues();
			m_ProjectileEffects.Add(newProj);
		}

		private void PrepareProjectile(GameObject projectile, float2 position, float finalRot, float effectHeight = 1.2f)
		{
			projectile.transform.position = new Vector3(position.x, effectHeight, position.y);
			projectile.transform.rotation = Quaternion.Euler(0, finalRot, 0);

            projectile.GetComponent<EffectTimer>().BlockRecyclingWhenDead();
        }
	}
}