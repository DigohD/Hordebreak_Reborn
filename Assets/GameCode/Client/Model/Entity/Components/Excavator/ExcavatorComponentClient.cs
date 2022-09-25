using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavatable;
using FNZ.Client.View.Player;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Excavator;
using FNZ.Shared.Utils.CollisionUtils;
using Lidgren.Network;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.Model.Entity.Components.Excavator
{
	public delegate void OnExcavatorStartFiring(FNEEntity hitEntity, Tuple<byte, Vector2>[] bonusSpans);
	public delegate void OnExcavatorNewBonusSpans(FNEEntity hitEntity,  Tuple<byte, Vector2>[] bonusSpans);
	public delegate void OnExcavatorStopFiring();
	public delegate void OnExcavatorProgressChange(float progress);

	public class ExcavatorComponentClient : ExcavatorComponentShared
	{
		private float m_CooldownStartTime;
		private float m_CooldownTimer;

		private float m_ExcavateProgress;

        private EquipmentSystemComponentClient m_Equipment;

		public Transform PlayerMeshStitcherMuzzle;
		
		// Used for local player animation
		private PlayerController m_PlayerController;

		// Used fore remote player animation
		private PlayerControllerRemote m_PlayerControllerRemote;

		public OnExcavatorStartFiring d_OnExcavatorStartFiring;
		public OnExcavatorStopFiring d_OnExcavatorStopFiring;
		public OnExcavatorProgressChange d_OnExcavatorProgressChange;
		public OnExcavatorNewBonusSpans d_OnExcavatorNewBonusSpans;

		private Tuple<byte, Vector2>[] m_ActiveBonusSpans;

		private ArrayList m_TmpBonusList = new ArrayList();

		public override void Init()
		{
			base.Init();

			m_CooldownStartTime = Data.BaseCooldownFueled;
			m_CooldownTimer = Data.BaseCooldownFueled;
		}

		public void Tick(float deltaTime)
		{
			if(m_LockedEntity != null)
			{
				if (GameClient.NetConnector.GetEntity(m_LockedEntity.NetId) == null)
				{
					m_LockedEntity = null;
					m_LockedEntityExcavatable = null;

					m_ExcavateProgress = 0;
					d_OnExcavatorProgressChange?.Invoke(0);
					d_OnExcavatorStopFiring?.Invoke();

					if (m_LockedEntity == null) return;
					NE_Send_UnlockEntity(m_LockedEntity.NetId);
					return;
				}
				
				m_ExcavateProgress += deltaTime / m_LockedEntityExcavatable.Data.baseExcavateTime;

				if (m_ExcavateProgress >= 1)
				{
					NE_Send_TriggerExcavate();

					CalculateBonusSpans();
					d_OnExcavatorNewBonusSpans?.Invoke(m_LockedEntity, m_ActiveBonusSpans);

					m_ExcavateProgress = 0;
					d_OnExcavatorProgressChange?.Invoke(0);
					return;
				}

				d_OnExcavatorProgressChange?.Invoke(m_ExcavateProgress);

				if (math.distance(ParentEntity.Position, m_LockedEntity.Position + new float2(0.5f, 0.5f)) > Data.BaseRange)
				{
					NE_Send_UnlockEntity(m_LockedEntity.NetId);
					m_LockedEntity = null;
					m_LockedEntityExcavatable = null;

					m_ExcavateProgress = 0;
					d_OnExcavatorProgressChange?.Invoke(0);

					d_OnExcavatorStopFiring?.Invoke();
				}
			}
		}

		public void OnFirePressed()
		{
			m_CooldownTimer = m_Fuel > 0 ? Data.BaseCooldownFueled : Data.BaseCooldownDry;
			m_CooldownStartTime = m_Fuel > 0 ? Data.BaseCooldownFueled : Data.BaseCooldownDry;

			m_ExcavateProgress = -0.5f;
			d_OnExcavatorProgressChange?.Invoke(m_ExcavateProgress);

			var aimDirection = new float2(
                Mathf.Cos(math.radians(ParentEntity.RotationDegrees)),
                Mathf.Sin(math.radians(ParentEntity.RotationDegrees))
            );

            float2 endPoint = ParentEntity.Position + (aimDirection * Data.BaseRange * 0.75f);
            var hit = FNECollisionUtils.ExcavatorRayCastForWallsAndTileObjectsInModel(ParentEntity.Position, endPoint, GameClient.World);

            if (hit.IsHit && hit.HitEntity.HasComponent<ExcavatableComponentClient>())
            {
				m_LockedEntity = hit.HitEntity;
				m_LockedEntityExcavatable = hit.HitEntity.GetComponent<ExcavatableComponentClient>();

				CalculateBonusSpans();

				d_OnExcavatorStartFiring?.Invoke(hit.HitEntity, m_ActiveBonusSpans);

				NE_Send_LockEntity(m_LockedEntity.NetId);
			}
		}

		private void CalculateBonusSpans()
		{
			if (m_LockedEntityExcavatable.Data.excavatableBonuses != null)
			{
				var bonuses = m_LockedEntityExcavatable.Data.excavatableBonuses;

				m_TmpBonusList.Clear();

				float bonusTotal = 0;
				for (int i = 0; i < bonuses.Count; i++)
				{
					var bonusData = bonuses[i];
					if(UnityEngine.Random.Range(0f, 100f) < bonusData.chance)
					{
						m_TmpBonusList.Add(i);
					}
					bonusTotal += bonusData.bonusTime;
				}

				m_ActiveBonusSpans = new Tuple<byte, Vector2>[m_TmpBonusList.Count];


				float nextMinimum = 0.3f;
				float nextMaximum = m_LockedEntityExcavatable.Data.baseExcavateTime - bonusTotal;
				for (int i = 0; i < m_TmpBonusList.Count; i++)
				{
					var bonusData = m_LockedEntityExcavatable.Data.excavatableBonuses[(int) m_TmpBonusList[i]];

					var startTime = UnityEngine.Random.Range(nextMinimum, nextMaximum);
					var endTime = startTime + bonusData.bonusTime;

					m_ActiveBonusSpans[i] = new Tuple<byte, Vector2>((byte)i, new Vector2(startTime, endTime));

					nextMinimum = endTime;
					nextMaximum += bonusData.bonusTime;
				}

				for (int i = 0; i < m_TmpBonusList.Count; i++)
				{
					// Debug.LogWarning("SPAN " + i + " = " + m_ActiveBonusSpans[(int) m_TmpBonusList[i]].Item2.ToString());
				}
			}
		}

		public void OnSecondaryFirePressed()
		{
			return; 
			
			if (m_LockedEntity != null)
			{
				d_OnExcavatorProgressChange?.Invoke(0);
				NE_Send_TriggerExcavate();
				m_ExcavateProgress = 0;

				CalculateBonusSpans();
				d_OnExcavatorNewBonusSpans?.Invoke(m_LockedEntity, m_ActiveBonusSpans);
			}
		}

		public void OnFireReleased()
		{
            if(m_LockedEntity != null)
            {
                NE_Send_UnlockEntity(m_LockedEntity.NetId);
                m_LockedEntity = null;
				m_LockedEntityExcavatable = null;
			}

            d_OnExcavatorStopFiring?.Invoke();
		}

		public void OnExcavatorDeactivation()
        {
            OnFireReleased();
        }

		public void NE_Send_LockEntity(int netId)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)ExcavatorNetEvent.LOCK_ENTITY,
                new LockEntityData(netId)
			);
		}

        public void NE_Send_UnlockEntity(int netId)
        {
            GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
                this,
                (byte)ExcavatorNetEvent.UNLOCK_ENTITY,
                new LockEntityData(netId)
            );
        }

        public void NE_Send_RefuelSlotClick()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)ExcavatorNetEvent.REFUEL_SLOT_CLICK
			);
		}

		public void NE_Send_RefuelMaxClick()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)ExcavatorNetEvent.REFUEL_MAX_CLICK
			);
		}

		public void NE_Send_RefuelOneClick()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)ExcavatorNetEvent.REFUEL_ONE_CLICK
			);
		}

		public void NE_Send_TriggerExcavate()
		{
			for (int i = 0; i < m_ActiveBonusSpans.Length; i++)
			{
				var span = m_ActiveBonusSpans[i];
				var bonusData = m_LockedEntityExcavatable.Data.excavatableBonuses[span.Item1];

				var baseTime = m_LockedEntityExcavatable.Data.baseExcavateTime;
				var spanStartPercent = span.Item2.x / baseTime;
				var spanEndPercent = span.Item2.y / baseTime;

				if (m_ExcavateProgress > spanStartPercent && m_ExcavateProgress < spanEndPercent)
				{
					GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
						this,
						(byte)ExcavatorNetEvent.TRIGGER_EXCAVATE,
						new TriggerExcavateData(span.Item1)
					);
					return;
				}
			}

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)ExcavatorNetEvent.TRIGGER_EXCAVATE,
				new TriggerExcavateData(255)
			);
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((ExcavatorNetEvent)incMsg.ReadByte())
			{
                case ExcavatorNetEvent.LOCK_ENTITY:
                    NE_Receive_LockEntity(incMsg);
                    break;

                case ExcavatorNetEvent.UNLOCK_ENTITY:
                    NE_Receive_UnlockEntity(incMsg);
                    break;

                case ExcavatorNetEvent.FUEL_UPDATE:
					NE_Receive_FuelUpdate(incMsg);
					break;

				case ExcavatorNetEvent.REFUEL_SLOT_UPDATE:
					NE_Receive_RefuelSlotUpdate(incMsg);
					break;
			}
		}

        private void NE_Receive_LockEntity(NetIncomingMessage incMsg)
        {
            var data = new LockEntityData();
            data.Deserialize(incMsg);

            m_LockedEntity = GameClient.NetConnector.GetEntity(data.NetId);
            if (m_LockedEntity != null)
                d_OnExcavatorStartFiring?.Invoke(m_LockedEntity, m_ActiveBonusSpans);
        }

        private void NE_Receive_UnlockEntity(NetIncomingMessage incMsg)
        {
            var data = new LockEntityData();
            data.Deserialize(incMsg);

            m_LockedEntity = null;
			m_LockedEntityExcavatable = null;

			d_OnExcavatorStopFiring?.Invoke();
        }

        private void NE_Receive_FuelUpdate(NetIncomingMessage incMsg)
		{
			var data = new FuelUpdateData();
			data.Deserialize(incMsg);

			d_FuelChange?.Invoke(data.fuel, m_Fuel, Data.BaseFuel);
			m_Fuel = data.fuel;
		}

		private void NE_Receive_RefuelSlotUpdate(NetIncomingMessage incMsg)
		{
			var data = new RefuelSlotUpdateData();
			data.Deserialize(incMsg);

			RefuelSlot = data.slotItem;
			d_RefuelSlotChange?.Invoke(data.slotItem);
		}

		private void NE_Receive_Fire(NetIncomingMessage incMsg)
		{
			if (m_PlayerControllerRemote == null)
			{
				m_PlayerControllerRemote = GameClient.ViewConnector.GetGameObject(ParentEntity).GetComponentInChildren<PlayerControllerRemote>();
			}

			/*
			  
			OLD EXCAVATOR CODE
	
			m_PlayerControllerRemote.PlayOneShotAnimation(OneShotAnimationType.Fire);

			if (m_Fuel <= 0)
			{
				GameClient.ViewAPI.SpawnEffect(
					"excavator_dry",
					ParentEntity.Position,
					ParentEntity.RotationZdegrees,
					false,
					false
				);
			}
			else
			{
				GameClient.ViewAPI.SpawnEffect(
					"excavator_loadup",
					ParentEntity.Position,
					ParentEntity.RotationZdegrees,
					false,
					false
				);
			}

			EffectData effectData = null;

			if (FNERandom.GetRandomIntInRange(0, 2) == 0)
			{
				effectData = DataBank.Instance.GetData<EffectData>("excavator_fire");
			}
			else
			{
				effectData = DataBank.Instance.GetData<EffectData>("excavator_fire2");
			}

			string vfxId = effectData.vfxRef != string.Empty ? effectData.vfxRef : string.Empty;
			string sfxId = effectData.sfxRef != string.Empty ? effectData.sfxRef : string.Empty;

			var muzzle = PlayerMeshStitcherMuzzle;
			var pos = new float2(muzzle.position.x, muzzle.position.y);

			if (vfxId != null)
				GameClient.ViewAPI.SpawnVFX(vfxId, pos, ParentEntity.RotationZdegrees, muzzle.position.z);

			if (sfxId != string.Empty)
				AudioManager.Instance.PlaySfx3dClip(sfxId, pos);

			*/
		}

		public float GetCooldownTimer()
		{
			return m_CooldownTimer;
		}

		public float GetCooldownStartTime()
		{
			return m_CooldownStartTime;
		}
	}
}
