using FNZ.Client.View.Audio;
using FNZ.Client.View.Camera;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Systems;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Lidgren.Network;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace FNZ.Client.Model.Entity.Components.EquipmentSystem
{
	public delegate void d_OnEquipmentVisualChange(Item item, Slot slot);
	public delegate void OnActiveItemChange(Item item, Slot slot);

	public class EquipmentSystemComponentClient : EquipmentSystemComponentShared
	{
		public d_OnEquipmentVisualChange d_OnEquipmentVisualChange;
		public OnActiveItemChange d_OnActiveItemChange;

		public Transform PlayerMeshStitcherMuzzle;
		
		// For local player animations
		public PlayerController PlayerController;

		// For remote player animations
		public PlayerControllerRemote PlayerControllerRemote;

		// Used by consumables and weapons with mouse target functionality
		public float2 CurrentTargetPosition;
		
		public override void Init()
		{
			base.Init();
		}

		public void Tick(float deltaTime)
		{
			GetActiveItem()?.GetComponent<ItemEquipmentComponent>()?.Tick(deltaTime);
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((EquipmentSystemNetEvent)incMsg.ReadByte())
			{
				case EquipmentSystemNetEvent.SLOT_UPDATED:
					NE_Receive_SlotUpdated(incMsg);
					break;

				case EquipmentSystemNetEvent.PRIMARY_FIRE:
					NE_Receive_PrimaryFire(incMsg);
					break;
			}
		}

		private void NE_Receive_SlotUpdated(NetIncomingMessage incMsg)
		{
			var data = new EquipmentSlotUpdateData();
			data.Deserialize(incMsg);

			var prevItem = GetItemInSlot(data.Slot);

			var item = data.item;
			SetItemInSlot(item, data.Slot);

			if (data.Slot <= Slot.Weapon2 || (data.Slot >= Slot.Consumable1 && data.Slot <= Slot.Consumable4))
			{
				// TODO: Causes major lag for consumables with mesh data
				d_OnEquipmentVisualChange?.Invoke(item, data.Slot);
				if (data.Slot == ActiveActionBarSlot)
					d_OnActiveItemChange?.Invoke(item, data.Slot);
			}
				
			ResetActiveItemDelegate();

			d_OnEquipmentUpdate?.Invoke();
		}

		public void NE_Receive_PrimaryFire(NetIncomingMessage incMsg)
		{
			var activeItem = GetActiveItem();
			var weaponComp = activeItem.GetComponent<ItemWeaponComponent>();
			if (weaponComp == null)
			{
				return;
			}

			if (PlayerControllerRemote == null)
			{
				PlayerControllerRemote = GameClient.ViewConnector.GetGameObject(ParentEntity).GetComponentInChildren<PlayerControllerRemote>();
			}

			PlayerControllerRemote.PlayOneShotAnimation(OneShotAnimationType.Fire);

			var effectData = DataBank.Instance.GetData<EffectData>(weaponComp.Data.effectRef);

            string vfxId = effectData.vfxRef != string.Empty ? effectData.vfxRef : string.Empty;
            string sfxId = effectData.sfxRef != string.Empty ? effectData.sfxRef : string.Empty;

			var muzzle = PlayerMeshStitcherMuzzle.position;
			var pos = new float2(muzzle.x, muzzle.y);

            if (vfxId != null)
                GameClient.ViewAPI.SpawnVFX(vfxId, pos, ParentEntity.RotationDegrees, muzzle.z);

            if (sfxId != string.Empty)
                AudioManager.Instance.PlaySfx3dClip(sfxId, pos);
        }

		public void NE_Send_SlotClicked(Slot slot)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.SLOT_CLICKED,
				new EquipmentSlotData { Slot = slot }
			);
		}

		public void NE_Send_SlotRightClicked(Slot slot)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.SLOT_RIGHT_CLICKED,
				new EquipmentSlotData { Slot = slot }
			);
		}

		public void NE_Send_SlotShiftClicked(Slot slot)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.SLOT_SHIFT_CLICKED,
				new EquipmentSlotData { Slot = slot }
			);
		}

		public void NE_Send_ActiveActionBarChange(Slot newActiveSlot)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.ACTIVE_ACTION_BAR_CHANGE,
				new EquipmentSlotData { Slot = newActiveSlot }
			);
		}

		public void NE_Send_PrimaryFire()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.PRIMARY_FIRE
			);
		}
		
		public void NE_Send_PrimaryFirePositioned(float2 pos)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.PRIMARY_FIRE_POSITIONED,
				new PositionedFireData {Position = pos}
			);
		}

		private void ResetItemTriggerDelegates()
		{
			var weapon1ItemComp = GetItemInSlot(Slot.Weapon1)?.GetComponent<ItemWeaponComponent>();
			var weapon2ItemComp = GetItemInSlot(Slot.Weapon2)?.GetComponent<ItemWeaponComponent>();

			if (weapon1ItemComp != null)
				weapon1ItemComp.d_OnTrigger = null;
			if (weapon2ItemComp != null)
				weapon2ItemComp.d_OnTrigger = null;
		}

		public override void Deserialize(NetBuffer br)
		{
			var tmpSlot = ActiveActionBarSlot;

			base.Deserialize(br);

			if (tmpSlot != ActiveActionBarSlot)
			{
				d_OnActiveItemChange?.Invoke(GetActiveItem(), ActiveActionBarSlot);
			}

			ResetActiveItemDelegate();

			d_OnEquipmentUpdate?.Invoke();
		}

		private void TriggerItem(Item triggeringItem, string effectRef)
		{
			if (triggeringItem.HasTargetFunctionality())
			{
				NE_Send_PrimaryFirePositioned(CurrentTargetPosition);
			}
			else
			{
				NE_Send_PrimaryFire();
			}

			if (PlayerController == null)
			{
				PlayerController = GameClient.LocalPlayerView.GetComponentInChildren<PlayerController>();
			}

			PlayerController.GetPlayerAnimationSystem().PlayOneShotAnimation(OneShotAnimationType.Fire);

			if (!string.IsNullOrEmpty(effectRef))
			{
				var effectData = DataBank.Instance.GetData<EffectData>(effectRef);

				string vfxId = effectData.vfxRef != string.Empty ? effectData.vfxRef : string.Empty;
				string sfxId = effectData.sfxRef != string.Empty ? effectData.sfxRef : string.Empty;

				var muzzle = PlayerMeshStitcherMuzzle;
				var pos = new float2(muzzle.position.x, muzzle.position.z);

				if (!string.IsNullOrEmpty(vfxId))
					GameClient.ViewAPI.SpawnVFX(vfxId, pos, ParentEntity.RotationDegrees, muzzle.position.y);

				if (!string.IsNullOrEmpty(sfxId))
					AudioManager.Instance.PlaySfx3dClip(sfxId, pos);

				var weaponComp = triggeringItem.GetComponent<ItemWeaponComponent>();
				GameClient.ViewAPI.SpawnRealEffects(
					effectData,
					true,
					pos,
					ParentEntity.RotationDegrees,
					weaponComp?.GetModItemIdsArray(),
					weaponComp?.GetModComponentArray(),
					muzzle.position.y
				);

				if (effectData.screenShake >= 0)
					CameraScript.shakeCamera(effectData.screenShake, effectData.screenShake);
			}
			
			if(GetActiveItem() == null)
            {
				SetItemInSlot(null, ActiveActionBarSlot);
			}
        }

		private void ReloadItem(Item reloadingItem, string effectRef)
		{
			var weaponComponent = reloadingItem.GetComponent<ItemWeaponComponent>();

			GameClient.ViewAPI.SpawnEffect(
				effectRef, 
				ParentEntity.Position, 
				ParentEntity.RotationDegrees, 
				true, 
				true,
				weaponComponent.GetModItemIdsArray(),
				weaponComponent.GetModComponentArray()
			);
		}

		private void ResetActiveItemDelegate()
		{
			ResetItemTriggerDelegates();

			var itemEqComp = GetActiveItem()?.GetComponent<ItemEquipmentComponent>();

			if (itemEqComp != null)
			{
				itemEqComp.d_OnTrigger += TriggerItem;
				itemEqComp.d_OnReload += ReloadItem;
			}
		}
	}
}
