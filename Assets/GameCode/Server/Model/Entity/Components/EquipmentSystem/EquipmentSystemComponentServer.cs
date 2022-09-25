using FNZ.Server.Model.Entity.Components.BaseTerminal;
using FNZ.Server.Model.Entity.Components.Builder;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Constants;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.EquipmentSystem
{
	public class EquipmentSystemComponentServer : EquipmentSystemComponentShared
	{
		public override void Init()
		{
			base.Init();
		}

		public void GivePlayerStartArmorSet()
		{
			SetItemInSlot(Item.GenerateItem("armor_start_gear_jacket"), Slot.Torso);
			SetItemInSlot(Item.GenerateItem("armor_start_gear_pants"), Slot.Legs);
			SetItemInSlot(Item.GenerateItem("armor_start_gear_boots"), Slot.Feet);
		}

		public Item PlaceItemInSlot(Item itemToBePlaced, Slot slot)
		{
			if (!CanBePlaced(itemToBePlaced, slot))
			{
				return itemToBePlaced;
			}

			Item alreadyExistingItem = null;
			switch (slot)
			{
				case Slot.Head:
					alreadyExistingItem = HeadGear;
					HeadGear = itemToBePlaced;
					break;
				case Slot.Torso:
					alreadyExistingItem = TorsoGear;
					TorsoGear = itemToBePlaced;
					break;
				case Slot.Legs:
					alreadyExistingItem = LegsGear;
					LegsGear = itemToBePlaced;
					break;
				case Slot.Feet:
					alreadyExistingItem = FeetGear;
					FeetGear = itemToBePlaced;
					break;
				case Slot.Back:
					alreadyExistingItem = BackGear;
					BackGear = itemToBePlaced;
					break;
				case Slot.Waist:
					alreadyExistingItem = WaistGear;
					WaistGear = itemToBePlaced;
					break;
				case Slot.Hands:
					alreadyExistingItem = HandsGear;
					HandsGear = itemToBePlaced;
					break;

				case Slot.Weapon1:
					alreadyExistingItem = Weapon1;
					Weapon1 = itemToBePlaced;
					break;
				case Slot.Weapon2:
					alreadyExistingItem = Weapon2;
					Weapon2 = itemToBePlaced;
					break;
				
				case Slot.Consumable1:
					alreadyExistingItem = Consumable1;
					Consumable1 = itemToBePlaced;
					break;
				case Slot.Consumable2:
					alreadyExistingItem = Consumable2;
					Consumable2 = itemToBePlaced;
					break;
				case Slot.Consumable3:
					alreadyExistingItem = Consumable3;
					Consumable3 = itemToBePlaced;
					break;
			}

			GameServer.NetAPI.Entity_UpdateComponent_BA(this);
			d_OnEquipmentUpdate?.Invoke();

			return alreadyExistingItem;
		}

		public Item PopEquippedItem(Slot slot)
		{
			Item itemToFetch = null;

			switch (slot)
			{
				case Slot.Head:
					itemToFetch = HeadGear;
					HeadGear = null;
					break;

				case Slot.Torso:
					itemToFetch = TorsoGear;
					TorsoGear = null;
					break;

				case Slot.Legs:
					itemToFetch = LegsGear;
					LegsGear = null;
					break;

				case Slot.Feet:
					itemToFetch = FeetGear;
					FeetGear = null;
					break;

				case Slot.Back:
					itemToFetch = BackGear;
					BackGear = null;
					break;

				case Slot.Hands:
					itemToFetch = HandsGear;
					HandsGear = null;
					break;

				case Slot.Waist:
					itemToFetch = WaistGear;
					WaistGear = null;
					break;

				case Slot.Weapon1:
					itemToFetch = Weapon1;
					Weapon1 = null;
					break;

				case Slot.Weapon2:
					itemToFetch = Weapon2;
					Weapon2 = null;
					break;
			}

			GameServer.NetAPI.Entity_UpdateComponent_BA(this);
			d_OnEquipmentUpdate?.Invoke();

			return itemToFetch;
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((EquipmentSystemNetEvent)incMsg.ReadByte())
			{
				case EquipmentSystemNetEvent.SLOT_CLICKED:
					NE_Receive_SlotClicked(incMsg);
					break;

				case EquipmentSystemNetEvent.SLOT_SHIFT_CLICKED:
					NE_Receive_SlotShiftClicked(incMsg);
					break;

				case EquipmentSystemNetEvent.ACTIVE_ACTION_BAR_CHANGE:
					NE_Receive_ActiveActionBarChange(incMsg);
					break;

				case EquipmentSystemNetEvent.SLOT_RIGHT_CLICKED:
					NE_Receive_SlotRightClicked(incMsg);
					break;

				case EquipmentSystemNetEvent.PRIMARY_FIRE:
					NE_Receive_PrimaryFire(incMsg);
					break;
				
				case EquipmentSystemNetEvent.PRIMARY_FIRE_POSITIONED:
					NE_Receive_PrimaryFire_Positioned(incMsg);
					break;
			}
		}

		public void NE_Send_SlotUpdate(Slot slot)
		{
			var itemInSlot = GetItemInSlot(slot);
			GameServer.NetAPI.BA_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.SLOT_UPDATED,
				new EquipmentSlotUpdateData { Slot = slot, item = itemInSlot }
			);
		}

		private void NE_Receive_SlotClicked(NetIncomingMessage incMsg)
		{
			var data = new EquipmentSlotData();
			data.Deserialize(incMsg);

			var playerComp = ParentEntity.GetComponent<PlayerComponentServer>();
			Item onCursor = playerComp.GetItemOnCursor();
			Item clicked = GetItemInSlot(data.Slot);

			if (onCursor != null && clicked == null && CanBePlaced(onCursor, data.Slot))
			{
				SetItemInSlot(onCursor, data.Slot);
				playerComp.SetItemOnCursor(null);

				NE_Send_SlotUpdate(data.Slot);
			}
			else if (onCursor != null && clicked != null && CanBePlaced(onCursor, data.Slot))
			{
				SetItemInSlot(onCursor, data.Slot);
				playerComp.SetItemOnCursor(clicked);

				NE_Send_SlotUpdate(data.Slot);
			}
			else if (onCursor == null && clicked != null)
			{
				SetItemInSlot(null, data.Slot);
				playerComp.SetItemOnCursor(clicked);

				NE_Send_SlotUpdate(data.Slot);
			}
		}

		private void NE_Receive_SlotRightClicked(NetIncomingMessage incMsg)
		{
			var data = new EquipmentSlotData();
			data.Deserialize(incMsg);

			
		}

		private void NE_Receive_SlotShiftClicked(NetIncomingMessage incMsg)
		{
			var data = new EquipmentSlotData();
			data.Deserialize(incMsg);

			Item clicked = GetItemInSlot(data.Slot);
			var inventory = ParentEntity.GetComponent<InventoryComponentServer>();

			var wasPlaced = inventory.AutoPlaceIfPossible(clicked);
			if (wasPlaced)
			{
				SetItemInSlot(null, data.Slot);
				NE_Send_SlotUpdate(data.Slot);
				GameServer.NetAPI.Entity_UpdateComponent_STC(inventory, incMsg.SenderConnection);
			}
		}

		private void NE_Receive_ActiveActionBarChange(NetIncomingMessage incMsg)
		{
			var data = new EquipmentSlotData();
			data.Deserialize(incMsg);

			ActiveActionBarSlot = data.Slot;

			GameServer.NetAPI.Entity_UpdateComponent_BA(this);
		}

		public void NE_Receive_PrimaryFire(NetIncomingMessage incMsg)
		{
			ActivateConsumable();
			
			GameServer.NetAPI.BOR_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.PRIMARY_FIRE,
				null,
				incMsg.SenderConnection
			);
		}
		
		public void NE_Receive_PrimaryFire_Positioned(NetIncomingMessage incMsg)
		{
			var data = new PositionedFireData();
			data.Deserialize(incMsg);
			
			ActivateConsumable(data.Position);
			
			GameServer.NetAPI.BOR_Entity_ComponentNetEvent(
				this,
				(byte)EquipmentSystemNetEvent.PRIMARY_FIRE_POSITIONED,
				data,
				incMsg.SenderConnection
			);
		}

		public override void NetDeserialize(NetBuffer reader)
		{
			base.NetDeserialize(reader);
			d_OnEquipmentUpdate?.Invoke();
		}

		private void ActivateConsumable(float2 targetPosition = default)
        {
			var activeItem = GetActiveItem();
			if (activeItem == null)
			{
				return;
			}

			var consumableComp = activeItem.GetComponent<ItemConsumableComponent>();
			if (consumableComp == null)
			{
				return;
			}

			var connection = GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity);

			FNEEntity player = ParentEntity;

			var consumableData = consumableComp.Data;
			var statCompServer = player.GetComponent<StatComponentServer>();
			
			bool wasConsumed = true;
			
			switch (consumableData.buff)
			{
				case ConsumableBuff.HEALTH_GAIN:
					statCompServer.Heal(consumableData.amount);
					GameServer.NetAPI.Entity_UpdateComponent_STC(statCompServer, connection);
					break;

				case ConsumableBuff.HEALTH_LOSS:
					statCompServer.Server_ApplyDamage(consumableData.amount, DamageTypesConstants.TRUE_DAMAGE);
					GameServer.NetAPI.Entity_UpdateComponent_STC(statCompServer, connection);
					break;

				default:
					break;
			}

			if (activeItem.HasTargetFunctionality())
			{
				if (!string.IsNullOrEmpty(consumableComp.Data.buildingRef))
				{
					var existingTileObject = GameServer.World.GetTileObject((int)targetPosition.x, (int)targetPosition.y);
					if (existingTileObject != null && existingTileObject.Data.blocksBuilding)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("building_in_the_way_message"),
							GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity)
						);
						return;
					}
					else if (existingTileObject != null)
					{
						GameServer.EntityAPI.NetDestroyEntityImmediate(existingTileObject);
					}

					var buildData = DataBank.Instance.GetData<BuildingData>(consumableComp.Data.buildingRef);
					
					var newEntity = GameServer.EntityAPI.NetSpawnEntityImmediate(
						buildData.productRef,
						targetPosition,
						0
					);

					if (newEntity != null)
					{
						var baseTerminalComponent = newEntity.GetComponent<BaseTerminalComponentServer>();
						if (baseTerminalComponent != null)
						{
							GameServer.RoomManager.CreateNewBase((int2) newEntity.Position);
						}
						
						var tileId = GameServer.World.GetTileRoom(new float2(targetPosition));
						var room = (ServerRoom)GameServer.RoomManager.GetRoom(tileId);

						if (room != null)
						{
							GameServer.RoomManager.RecalculateBaseStatus(room.ParentBaseId, ParentEntity);
						}

						GameServer.NetAPI.World_RoomManager_BA();
						QuestManager.OnConstruction(buildData);

						var buildComp = ParentEntity.GetComponent<BuilderComponentServer>();
						
						if (buildData.unlockRefs != null && buildData.unlockRefs.Count > 0)
							buildComp.HandleUnlockRefs(
								buildData, 
								ParentEntity.GetComponent<PlayerComponentServer>(), 
								GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity)
							);
					}
					else
					{
						wasConsumed = false;
					}
				}
			}
			else
			{
				GameServer.NetAPI.Effect_SpawnEffect_BOR(consumableData.effectRef, new float2(player.Position.x, player.Position.y), player.RotationDegrees, connection);
			}

			if (wasConsumed)
			{
				if (activeItem.amount == 1)
				{
					SetItemInSlot(null, ActiveActionBarSlot);
				}
				else
				{
					activeItem.amount -= 1;
				}
				
				NE_Send_SlotUpdate(ActiveActionBarSlot);
			}
        }

	}
}
