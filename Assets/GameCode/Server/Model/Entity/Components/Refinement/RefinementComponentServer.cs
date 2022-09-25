using FNZ.Server.Controller;
using FNZ.Server.Controller.Systems;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.Entity.Components.RoomRequirements;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components.Refinement;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Model.Entity.Components.Refinement
{
	public class RefinementComponentServer : RefinementComponentShared, ITickable
	{
		private float m_Timer;

		private bool m_NeedsFuel;

		public override void Init()
		{
			base.Init();

			m_NeedsFuel = !string.IsNullOrEmpty(Data.burnGradeRef);
			if(Data.recipes.Count > 0)
				m_ActiveRecipe = DataBank.Instance.GetData<RefinementRecipeData>(Data.recipes[0]);
		}

		public void Tick(float dt)
		{
			if (m_ActiveRecipe == null)
				return;

			var efficiency = CalculateEfficiency();

			m_Timer += dt * efficiency;

            if (m_NeedsFuel)
            {
				WithFuelTick();
            }
            else
            {
				WithoutFuelTick();
			}
		}

        private void WithoutFuelTick()
        {
			if ((!IsMaterialsPresent() || IsAnyOutputSlotFull()) && m_IsWorking)
			{
				m_IsWorking = false;
				NE_Send_IsWorking(false);
				m_ProcessTicks = 0;
			}

			if (!m_IsWorking && IsMaterialsPresent() && !IsAnyOutputSlotFull())
			{
				m_IsWorking = true;
				NE_Send_IsWorking(true);
			}

			if (m_Timer > ServerMainSystem.TARGET_SERVER_TICK_TIME)
			{
				if (m_IsWorking)
				{
					m_ProcessTicks++;

					if (m_ProcessTicks >= m_ActiveRecipe.processTime)
					{
						PerformRefinement();
					}

					NE_Send_ProgressUpdate();
				}

				m_Timer -= ServerMainSystem.TARGET_SERVER_TICK_TIME;
			}
		}

		private void WithFuelTick()
        {
	        m_BurnTicks--;
			if (m_BurnTicks <= 0)
			{
				m_BurnTicks = 0;
				if ((!HasBurnable() || !IsMaterialsPresent() || IsAnyOutputSlotFull()) && m_IsWorking)
				{
					m_IsWorking = false;
					NE_Send_IsWorking(false);
				}
				else if (m_IsWorking)
				{
					ConsumeBurnable();
				}
			}

			if (!m_IsWorking && IsMaterialsPresent() && !IsAnyOutputSlotFull() && (HasBurnable() || m_BurnTicks > 0))
			{
				m_IsWorking = true;
				NE_Send_IsWorking(true);
			}

			if (m_Timer > ServerMainSystem.TARGET_SERVER_TICK_TIME)
			{
				if (m_IsWorking && m_BurnTicks > 0)
				{
					m_ProcessTicks++;

					if (m_ProcessTicks >= m_ActiveRecipe.processTime)
					{
						PerformRefinement();
					}

					NE_Send_ProgressUpdate();
				}

				m_Timer -= ServerMainSystem.TARGET_SERVER_TICK_TIME;
			}
		}

		public RefinementRecipeData GetActiveRecipe()
		{
			return m_ActiveRecipe;
		}

		private void PerformRefinement()
		{
			m_ProcessTicks = 0;

			var required = m_ActiveRecipe.requiredMaterials;
			for (int i = 0; i < required.Count; i++)
			{
				slots[0, i].amount -= required[i].amount;
				if (slots[0, i].amount == 0)
				{
					slots[0, i] = null;
				}
				NE_Send_SlotUpdate(RefinementSlotType.INPUT, (byte)i);
			}

			var produced = m_ActiveRecipe.producedMaterials;
			for (int i = 0; i < produced.Count; i++)
			{
				if (slots[1, i] == null)
				{
					slots[1, i] = Item.GenerateItem(produced[i].itemRef, produced[i].amount, true);
				}
				else
				{
					slots[1, i].amount += produced[i].amount;
				}

				NE_Send_SlotUpdate(RefinementSlotType.OUTPUT, (byte)i);
			}

			if (!IsMaterialsPresent() || IsAnyOutputSlotFull())
			{
				m_IsWorking = false;
			}
		}

		public bool IsMaterialsPresent()
		{
			var required = m_ActiveRecipe.requiredMaterials;
			for (int i = 0; i < required.Count; i++)
			{
				if (slots[0, i] == null)
					return false;

				if (slots[0, i].amount < required[i].amount)
					return false;
			}

			return true;
		}
		
		public bool IsAnyOutputSlotFull()
		{
			var produced = m_ActiveRecipe.producedMaterials;
			for (int i = 0; i < produced.Count; i++)
			{
				var itemData = DataBank.Instance.GetData<ItemData>(produced[i].itemRef);
				if (slots[1, i] != null && slots[1, i].amount > itemData.maxStackSize - produced[i].amount)
					return true;
			}

			return false;
		}
		
		public void AddMaterialToInput(int index, Item item = null)
		{
			if (item != null && GetMaterialsInInputSlot(index) == null)
			{
				slots[0, index] = item;
			}
			else if (item != null && GetMaterialsInInputSlot(index) != null)
			{
				slots[0, index].amount += item.amount;
			}
			else
				slots[0, index].amount++;

			NE_Send_SlotUpdate(RefinementSlotType.INPUT, (byte)index);
		}
		public Item GetMaterialsInInputSlot(int index)
		{
			return slots[0, index];
		}

		public Item GetBurnableItem()
		{
			return slots[2, 0];
		}
		public string GetBurnGradeRef()
		{
			return Data.burnGradeRef;
		}
		private bool HasBurnable()
		{
			return slots[2, 0] != null;
		}
		public void AddBurnable(Item burnableItem = null)
		{
			if (slots[2, 0] == null)
				slots[2, 0] = burnableItem;
			else
				slots[2, 0].amount++;

			NE_Send_SlotUpdate(RefinementSlotType.BURNABLE, 0);
		}
		private void ConsumeBurnable()
		{
			var burnable = slots[2, 0];

			m_BurnTicks = burnable.GetComponent<ItemBurnableComponent>().Data.burnTime;
			burnable.amount--;
			if (burnable.amount == 0)
			{
				slots[2, 0] = null;
			}
			NE_Send_SlotUpdate(RefinementSlotType.BURNABLE, 0);
		}

		public Item CollectOutputItem(int index)
		{
			var item = slots[1, index];
			slots[1, index] = null;
			NE_Send_SlotUpdate(RefinementSlotType.OUTPUT, (byte)index);
			return item;
		}
		public bool HasOutputItem(int index)
		{
			return slots[1, index] != null;
		}
		public void ReturnOutputItem(Item item, int index)
		{
			if (slots[1, index] != null)
				slots[1, index].amount += item.amount;
			else
				slots[1, index] = item;

			NE_Send_SlotUpdate(RefinementSlotType.OUTPUT, (byte)index);
		}

		private float CalculateEfficiency()
		{
			var roomRequirementsComp = ParentEntity.GetComponent<RoomRequirementsComponentServer>();
			if (roomRequirementsComp != null)
			{
				var room = GameServer.RoomManager.GetRoom((int2)ParentEntity.Position);
				if (room == null)
					return roomRequirementsComp.Data.unsatisfiedMod;

				if (!GameServer.RoomManager.IsBaseOnline(room.ParentBaseId))
					return 0;

				if (room.DoesRoomFulfillRequirements(roomRequirementsComp.Data.propertyRequirements))
				{
					return 1;
				}

				return roomRequirementsComp.Data.unsatisfiedMod;
			}

			return 1;
		}

		private void NE_Send_SetActiveRecipe(ushort recipeIndex)
		{
			GameServer.NetAPI.BAR_Entity_ComponentNetEvent(
			   this,
			   (byte)RefinementNetEvent.SET_ACTIVE_RECIPE,
			   new SetRefinementRecipeData() { RecipeIndex = recipeIndex }
		   );
		}
		private void NE_Send_SlotUpdate(RefinementSlotType slotType, byte slotIndex)
		{
			var item = slots[(byte)slotType, slotIndex];
			GameServer.NetAPI.BAR_Entity_ComponentNetEvent(
			   this,
			   (byte)RefinementNetEvent.SLOT_UPDATE,
			   new RefinmentSlotUpdateData()
			   {
				   SlotType = slotType,
				   SlotIndex = slotIndex,
				   itemRef = item != null ? item.Data.Id : "",
				   itemAmount = item != null ? item.amount : 0
			   }
		   );
		}
		private void NE_Send_ProgressUpdate()
		{
			GameServer.NetAPI.BAR_Entity_ComponentNetEvent(
			   this,
			   (byte)RefinementNetEvent.PROGRESS_UPDATE,
			   new RefinementProgressUpdateData()
			   {
				   ProcessTicks = m_ProcessTicks
			   }
		   );
		}
		private void NE_Send_IsWorking(bool isworking)
		{
			GameServer.NetAPI.BAR_Entity_ComponentNetEvent(
			   this,
			   (byte)RefinementNetEvent.IS_WORKING_UPDATE,
			   new IsWorkingUpdateData()
			   {
				   IsWorking = isworking
			   }
		   );
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((RefinementNetEvent)incMsg.ReadByte())
			{
				case RefinementNetEvent.SET_ACTIVE_RECIPE:
					NE_Receive_SetActiveRecipe(incMsg);
					break;

				case RefinementNetEvent.SLOT_CLICK:
					NE_Receive_SlotClick(incMsg, RefinementNetEvent.SLOT_CLICK);
					break;

				case RefinementNetEvent.SLOT_RIGHT_CLICK:
					NE_Receive_SlotClick(incMsg, RefinementNetEvent.SLOT_RIGHT_CLICK);
					break;

				case RefinementNetEvent.SLOT_SHIFT_CLICK:
					NE_Receive_SlotClick(incMsg, RefinementNetEvent.SLOT_SHIFT_CLICK);
					break;

				case RefinementNetEvent.SLOT_SHIFT_RIGHT_CLICK:
					NE_Receive_SlotClick(incMsg, RefinementNetEvent.SLOT_SHIFT_RIGHT_CLICK);
					break;
			}
		}

		private void NE_Receive_SetActiveRecipe(NetIncomingMessage incMsg)
		{
			var data = new SetRefinementRecipeData();
			data.Deserialize(incMsg);

			for (int i = 0; i < 2; i++)
				for (int j = 0; j < 5; j++)
				{
					if (slots[i, j] != null)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
						   Localization.GetString("machine_not_empty_message"),
						   incMsg.SenderConnection
						);
						return;
					}
				}

			m_ActiveRecipe = DataBank.Instance.GetData<RefinementRecipeData>(Data.recipes[data.RecipeIndex]);

			NE_Send_SetActiveRecipe(data.RecipeIndex);
		}

		private void NE_Receive_SlotClick(NetIncomingMessage incMsg, RefinementNetEvent eventType)
		{
			var data = new RefinmentSlotClickData();
			data.Deserialize(incMsg);

			var playerComp = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection).GetComponent<PlayerComponentServer>();
			var clicked = slots[(byte)data.SlotType, data.SlotIndex];
			var cursorItem = playerComp.GetItemOnCursor();

			bool isPlaceable = false;
			if (data.SlotType == RefinementSlotType.INPUT)
			{
				isPlaceable = m_ActiveRecipe.requiredMaterials[data.SlotIndex].itemRef == cursorItem?.Data.Id;
			}
			/*else if (data.SlotType == RefinementSlotType.OUTPUT)
			{
				isPlaceable = m_ActiveRecipe.producedMaterials[data.SlotIndex].itemRef == cursorItem?.Data.Id;
			}*/
			else
			{
				var burnableComp = (ItemBurnableComponentData)cursorItem?.Data.components.Find(ic => ic is ItemBurnableComponentData);
				if (burnableComp != null)
				{
					isPlaceable = Data.burnGradeRef == burnableComp.gradeRef;
				}
			}

			if (cursorItem != null && clicked == null && isPlaceable)
			{
				slots[(byte)data.SlotType, data.SlotIndex] = cursorItem;
				playerComp.SetItemOnCursor(null);

				NE_Send_SlotUpdate(data.SlotType, data.SlotIndex);
			}
			else if (cursorItem != null && clicked != null && isPlaceable)
			{
				if (cursorItem.Data.Id == clicked.Data.Id)
				{
					clicked.amount += cursorItem.amount;
					if (clicked.amount > clicked.Data.maxStackSize)
					{
						cursorItem.amount = clicked.amount - clicked.Data.maxStackSize;
						clicked.amount = clicked.Data.maxStackSize;

						playerComp.SetItemOnCursor(cursorItem);
					}
					else
					{
						playerComp.SetItemOnCursor(null);
					}
				}
				else
				{
					var tmp = cursorItem;
					playerComp.SetItemOnCursor(slots[(byte)data.SlotType, data.SlotIndex]);
					slots[(byte)data.SlotType, data.SlotIndex] = tmp;
				}
				
				NE_Send_SlotUpdate(data.SlotType, data.SlotIndex);
			}
			else if (cursorItem == null && clicked != null)
			{
				if (eventType == RefinementNetEvent.SLOT_SHIFT_RIGHT_CLICK) 
				{
					var playerInventory = playerComp.ParentEntity.GetComponent<InventoryComponentServer>();
					if (playerInventory.AutoPlaceIfPossible(clicked))
					{
						slots[(byte)data.SlotType, data.SlotIndex] = null;
						ResetProcessForInputSlot(data.SlotType);

						GameServer.NetAPI.Entity_UpdateComponent_BA(playerInventory);
						QuestManager.OnRefinement(clicked);
					}
					else
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
						   Localization.GetString("inventory_full_message"),
						   incMsg.SenderConnection
					   );
					}
				}
				else if (eventType == RefinementNetEvent.SLOT_RIGHT_CLICK && clicked.amount > 1)
				{
					playerComp.SetItemOnCursor(Item.GenerateItem(clicked.Data.Id, clicked.amount / 2));
					clicked.amount -= clicked.amount / 2;
					QuestManager.OnRefinement(playerComp.GetItemOnCursor());
				}
				else if (eventType == RefinementNetEvent.SLOT_SHIFT_CLICK)
				{
					var playerInventory = playerComp.ParentEntity.GetComponent<InventoryComponentServer>();
					if (playerInventory.AutoPlaceIfPossible(clicked))
					{
						slots[(byte)data.SlotType, data.SlotIndex] = null;
						ResetProcessForInputSlot(data.SlotType);

						GameServer.NetAPI.Entity_UpdateComponent_BA(playerInventory);
						QuestManager.OnRefinement(clicked);
					}
					else
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
						   Localization.GetString("inventory_full_message"),
						   incMsg.SenderConnection
					   );
					}
				}
				else
				{
					slots[(byte)data.SlotType, data.SlotIndex] = null;
					playerComp.SetItemOnCursor(clicked);
					ResetProcessForInputSlot(data.SlotType);
					QuestManager.OnRefinement(playerComp.GetItemOnCursor());
				}

				NE_Send_SlotUpdate(data.SlotType, data.SlotIndex);
			}
		}

		private void ResetProcessForInputSlot(RefinementSlotType slotType)
		{
			if (slotType == RefinementSlotType.INPUT)
			{
				m_ProcessTicks = 0;
				if (m_IsWorking)
					NE_Send_IsWorking(false);

				m_IsWorking = false;

				NE_Send_ProgressUpdate();
			}
		}
	}
}
