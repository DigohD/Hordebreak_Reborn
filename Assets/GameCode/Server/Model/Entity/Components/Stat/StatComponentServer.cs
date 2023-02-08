using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.EquipmentSystem;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Net.API;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Health;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items.Components;
using System;
using System.Collections.Generic;
using FNZ.Server.Model.World;
using FNZ.Server.Services;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity.Components.Enemy;
using FNZ.Shared.Net.Dto.Hordes;
using FNZ.Shared.Utils;
using Unity.Mathematics;

namespace FNZ.Server.Model.Entity.Components
{
	public class StatComponentServer : StatComponentShared, ITickable
	{
		private EquipmentSystemComponentServer m_Equipment;

		private float m_ShieldRegenStallTimer;

		private const float SHIELD_STALL_TIME = 10f;

		private PlayerComponentServer m_PlayerComp;

		private ServerWorld _world;

		public override void Init()
		{
			base.Init();
			_world = GameServer.WorldInstanceManager.GetWorldInstance(ParentEntity.WorldInstanceIndex);
		}

		public override void InitComponentLinks()
		{
			m_Equipment = ParentEntity.GetComponent<EquipmentSystemComponentServer>();
			if (m_Equipment != null)
			{
				m_Equipment.d_OnEquipmentUpdate += OnEquipmentChange;
			}
			m_PlayerComp = ParentEntity.GetComponent<PlayerComponentServer>();
		}

		private void OnEquipmentChange()
		{
			MaxHealth = Data.startHealth;
			Armor = Data.startArmor;
			MaxShields = Data.startShields;

			foreach (var slot in Enum.GetValues(typeof(Slot)))
			{
				var itemEquipmentComp = m_Equipment.GetItemInSlot((Slot)slot)?.GetComponent<ItemEquipmentComponent>();
				if (itemEquipmentComp != null && itemEquipmentComp.Data.statMods != null)
				{
					foreach (var mod in itemEquipmentComp.Data.statMods)
					{
						switch (mod.modType)
						{
							case StatModData.StatType.MaxHealth:
								MaxHealth += mod.amount;
								break;

							case StatModData.StatType.Armor:
								Armor += mod.amount;
								break;

							case StatModData.StatType.MaxShields:
								MaxShields += mod.amount;
								break;
						}
					}
				}
			}

			if (CurrentHealth > MaxHealth)
				CurrentHealth = MaxHealth;
			if (CurrentShields > MaxShields)
				CurrentShields = MaxShields;

			GameServer.NetAPI.Entity_UpdateComponent_BA(this);
		}

		public bool Server_ApplyDamage(float damageAmount, string damageTypeRef)
		{
			if (CurrentHealth <= 0)
				return true;

			if (m_PlayerComp != null)
				GameServer.NetAPI.Effect_SpawnEffect_BAR(_world, "effect_player_on_hit", m_PlayerComp.ParentEntity.Position, m_PlayerComp.ParentEntity.RotationDegrees);

			var armorMul = 100f / (100f + Armor);
			damageAmount *= armorMul;

			if (CurrentShields > 0)
			{
				CurrentShields -= damageAmount;
				if (CurrentShields < 0)
					CurrentShields = 0;

				m_ShieldRegenStallTimer = SHIELD_STALL_TIME;

				return false;
			}
			else
			{
				var damageTypeMultiplier = CalculateMultiplier(damageTypeRef);

				if (CurrentHealth - (damageAmount * damageTypeMultiplier) <= 0)
				{
					CurrentHealth = 0;
					ParentEntity.SendComponentMessage(FNEComponentMessage.ZERO_HEALTH);

					if (m_PlayerComp != null)
					{
						m_PlayerComp.KillPlayer();
					}
					else
					{
						switch (ParentEntity.EntityId)
						{
							case "shrubber":
								/*
								 CODE FOR BLINK SDHRUBBERS
								 
								 if (FNERandom.GetRandomIntInRange(0, 100) < 50)
								{
									GameServer.NetAPI.Effect_SpawnEffect_BAR("effect_kryst_berry_harvest", ParentEntity.Position, 0);

									var players = GameServer.NetConnector.GetConnectedClientEntities();

									var closestDist = float.MaxValue;
									var closetsPlayerPos = new float2(0, 0);
									foreach(var player in players)
									{
										var dist = math.distance(player.Position, ParentEntity.Position);
										if (dist < closestDist)
										{
											dist = closestDist;
											closetsPlayerPos = player.Position;
										}
									}
									
									var pos = GameServer.EntityAPI.SpawnSingleEnemyWithinRadius(
										"shrubber",
										closetsPlayerPos,
										15
									);
									
									GameServer.NetAPI.Effect_SpawnEffect_BAR("effect_kryst_berry_harvest", pos, 0);
								}
								else
								{*/
									GameServer.NetAPI.Effect_SpawnEffect_BAR(_world, "effect_shrubber_death", ParentEntity.Position, 0);
								//}
								break;

							case "default_zombie":
								GameServer.NetAPI.Effect_SpawnEffect_BAR(_world, "effect_zombie_death", ParentEntity.Position, 0);
								break;
							case "zombie_big":
								// DEMO CODE
								
								FNEService.Effect.SpawnRealEffectsAtPosition(_world, "effect_big_zombie_death", ParentEntity.Position);

								GameServer.NetAPI.Effect_SpawnEffect_BAR(_world, "effect_big_zombie_death", ParentEntity.Position, 0);
								break;

							default:
								GameServer.NetAPI.Effect_SpawnEffect_BAR(_world, "effect_zombie_death", ParentEntity.Position, 0);
								break;
						}
						//GameServer.NetAPI.Effect_SpawnEffect_BAR("effect_shrubber_death", ParentEntity.Position, 0);
						
						
						var lootTable = ParentEntity.Data.GetComponentData<EnemyComponentData>()?.lootTable;
						if (lootTable != null)
						{
							var loot = LootGenerator.GenerateLoot(lootTable);
							foreach (var item in loot)
							{
								GameServer.ItemsOnGroundManager.SpawnItemOnGround(ParentEntity.Position, item);
							}
						}
						
						GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);

						ParentEntity.IsDead = true;
						//var data = DataBank.Instance.GetData<FNEEntityViewData>(ParentEntity.Data.entityViewVariations[0]);
					}
					return true;
				}
				else
				{
					CurrentHealth -= damageAmount * damageTypeMultiplier;
					return false;
				}
			}
		}

		public void Heal(float amount)
		{
			CurrentHealth += amount;
			CurrentHealth = CurrentHealth > MaxHealth ? MaxHealth : CurrentHealth;
		}

		public void Tick(float dt)
		{
			if (m_ShieldRegenStallTimer <= 0f)
			{
				if (CurrentShields < MaxShields)
				{
					CurrentShields += (ShieldsRegeneration * dt);
					if (CurrentShields > MaxShields)
						CurrentShields = MaxShields;

					GameServer.NetAPI.Entity_UpdateComponent_BA(this);
				}
			}
			else
			{
				m_ShieldRegenStallTimer -= dt;
				if (m_ShieldRegenStallTimer < 0)
					m_ShieldRegenStallTimer = 0;
			}

		}

		private float CalculateMultiplier(string damageTypeRef)
		{
			var damagedByList = DefenseTypeData.damagedByList;

			if (defenseTypeRefOverride != string.Empty)
				damagedByList = GetDefenseTypeOverride().damagedByList;

			foreach (var damageEntry in damagedByList)
			{
				if (damageEntry.damageRef == damageTypeRef)
				{
					return damageEntry.amplification / 100.0f;
				}
			}

			return 1;
		}

	}
}
