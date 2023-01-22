using FNZ.Server.Model.Entity.Components.EquipmentSystem;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.Entity.Components.Refinement;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Inventory;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.Entity.Components.Inventory
{
	public class InventoryComponentServer : InventoryComponentShared
	{
		public override void Init()
		{
			base.Init();

			if (m_Data.produceLootTable != null)
				GenerateLoot();
		}

		public Item PlaceItem(Item item, int2? topLeftCell)
		{
			if (!CanPlaceItem(item, topLeftCell, true))
			{
				return item;
			}

			Item swapItem = null;
			bool doBreak = false;
			for (int x = topLeftCell.Value.x; x < topLeftCell.Value.x + item.Data.width && !doBreak; x++)
			{
				for (int y = topLeftCell.Value.y; y < topLeftCell.Value.y + item.Data.height && !doBreak; y++)
				{
					if (GetItemAtCell(x, y) != null)
					{
						swapItem = GetItemAtCell(x, y);
						doBreak = true;
					}
				}
			}

			//stack items
			if (swapItem != null && swapItem.Data.Id == item.Data.Id)
			{
				if (swapItem.amount + item.amount <= swapItem.Data.maxStackSize)
				{
					swapItem.amount += item.amount;

					return null;
				}
				else if (swapItem.amount == swapItem.Data.maxStackSize)
				{
					RemoveItem(swapItem);
					PlaceItem(item, topLeftCell);

					return swapItem;
				}
				else
				{
					item.amount -= swapItem.Data.maxStackSize - swapItem.amount;
					swapItem.amount = swapItem.Data.maxStackSize;

					return item;
				}
			}

			for (int x = topLeftCell.Value.x; x < topLeftCell.Value.x + item.Data.width; x++)
			{
				for (int y = topLeftCell.Value.y; y < topLeftCell.Value.y + item.Data.height; y++)
				{
					if (swapItem != null)
					{
						RemoveItem(swapItem);
						PlaceItem(item, topLeftCell);
						return swapItem;
					}
					else if (item.amount > item.Data.maxStackSize)
					{
						item.amount -= item.Data.maxStackSize;
						cells[x + y * m_Data.width] = Item.GenerateItem(item.Data.Id, item.Data.maxStackSize);
						return item;
					}
					else
					{
						cells[x + y * m_Data.width] = item;
					}
				}
			}

			return swapItem;
		}

		public void RemoveItem(Item item)
		{
			if (item == null) return;

			for (int x = 0; x < m_Data.width; x++)
			{
				for (int y = 0; y < m_Data.height; y++)
				{
					if (GetItemAtCell(x, y) == item)
					{
						cells[x + y * m_Data.width] = null;
					}
				}
			}
		}

		public bool RemoveItem(Item item, int amount)
		{
			if (item == null) return false;

			for (int x = 0; x < m_Data.width; x++)
			{
				for (int y = 0; y < m_Data.height; y++)
				{
					if (GetItemAtCell(x, y) == item)
					{
						if (item.amount <= amount)
						{
							RemoveItem(item);
							return true;
						}
						else
						{
							item.amount -= amount;
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool RemoveItemOfId(string id, int amount)
		{
			if (GetItemCount(id) < amount || amount == 0)
				return false;

			foreach (Item item in GetItemsAndTheirPositions().Keys)
			{
				if (item.Data.Id != id)
					continue;

				if (item.amount <= amount)
				{
					RemoveItem(item);
					amount -= item.amount;

					if (amount == 0)
					{
						// RequestInvokeInventoryUpdate();
						return true;
					}
				}
				else
				{
					item.amount -= amount;
					// RequestInvokeInventoryUpdate();
					return true;
				}
			}


			
			Debug.LogWarning("RemoveItemOfId in GridInventoryComponent reached point it should logically not be able to reach. Something is wrong with the function.");
			return false;
		}

		public bool AutoPlaceIfPossible(Item item)
		{
			//check for stack possiblity
			for (int x = 0; x < m_Data.width; x++)
			{
				for (int y = 0; y < m_Data.height; y++)
				{
					Item otherItem = GetItemAtCell(x, y);
					if (otherItem != null && otherItem.Data.Id == item.Data.Id)
					{
						if (otherItem.amount + item.amount <= otherItem.Data.maxStackSize)
						{
							otherItem.amount += item.amount;
							return true;
						}
						else if (GetFirstAvailableSlot(item) != null)
						{
							item.amount -= otherItem.Data.maxStackSize - otherItem.amount;
							otherItem.amount = otherItem.Data.maxStackSize;
						}
					}
				}
			}

			//no more stacking possible. Find empty possible slot.
			int2? pos = GetFirstAvailableSlot(item);

			while (item != null && item.amount > 0)
			{
				if (pos == null)
					return false;

				item = PlaceItem(item, pos);

				if (item != null)
					pos = GetFirstAvailableSlot(item);
			}

			return true;
		}

		public bool PlaceItemInRandomSlotIfPossible(Item item)
		{
			List<int2> availableSlots = new List<int2>();
			for (int x = 0; x <= m_Data.width - item.Data.width; x++)
			{
				for (int y = 0; y <= m_Data.height - item.Data.height; y++)
				{
					int2 point = new int2(x, y);
					if (CanPlaceItem(item, point, false))
					{
						availableSlots.Add(point);
					}
				}
			}

			if (availableSlots.Count == 0)
			{
				return false;
			}

			int2? pos = availableSlots[FNERandom.GetRandomIntInRange(0, availableSlots.Count)];


			if (pos != null)
			{
				PlaceItem(item, pos);
				return true;
			}

			return false;
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((InventoryNetEvent)incMsg.ReadByte())
			{
				case InventoryNetEvent.CELL_CLICKED:
					NE_Receive_CellClicked(incMsg);
					break;

				case InventoryNetEvent.CELL_SHIFT_CLICKED:
					NE_Receive_CellShiftClicked(incMsg);
					break;

				case InventoryNetEvent.CELL_RIGHT_CLICKED:
					NE_Receive_CellRightClicked(incMsg);
					break;

				case InventoryNetEvent.CELL_SHIFT_RIGHT_CLICKED:
					NE_Receive_CellShiftRightClicked(incMsg);
					break;

				case InventoryNetEvent.MOD_CLICKED:
					NE_Receive_ModClicked(incMsg);
					break;

				case InventoryNetEvent.MOD_RIGHT_CLICKED:
					NE_Receive_ModRightClicked(incMsg);
					break;
			}
		}

		private void NE_Receive_CellClicked(NetIncomingMessage incMsg)
		{
			var data = new InventoryCellClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);

			Item ItemOnCursor = player.GetComponent<PlayerComponentServer>().GetItemOnCursor();
			Item clickedItem = GetItemAtCell(data.x, data.y);

			if (ItemOnCursor == null)
			{
				RemoveItem(clickedItem);
				
				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clickedItem);
				
				if (GetItemsAndTheirPositions().Count == 0 && m_Data.destroyWhenEmpty)
				{
					GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
				}
				else
				{
					GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
				}
			}
			else if (ItemOnCursor != null)
			{
				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(PlaceItem(ItemOnCursor, new int2(data.x, data.y)));
				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
		}

		private void NE_Receive_CellShiftClicked(NetIncomingMessage incMsg)
		{
			var data = new InventoryCellClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);

			Item onCursor = player.GetComponent<PlayerComponentServer>().GetItemOnCursor();
			Item clicked = GetItemAtCell(data.x, data.y);

			var playerInventory = player.GetComponent<InventoryComponentServer>();

			if (playerInventory != this && clicked != null)
			{
				PlaceItemInInventory(clicked, playerInventory, incMsg);

				if (GetItemsAndTheirPositions().Count == 0 && m_Data.destroyWhenEmpty)
				{
					GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
				}
			}
			else if (onCursor != null)
			{
				if (clicked != null)
				{
					if (clicked.Data.Id == onCursor.Data.Id && clicked.amount < clicked.Data.maxStackSize)
					{
						onCursor.amount--;
						PlaceItem(Item.GenerateItem(onCursor.Data.Id, 1), new int2(data.x, data.y));

						if (onCursor.amount == 0)
							player.GetComponent<PlayerComponentServer>().SetItemOnCursor(null);
					}
					else
						player.GetComponent<PlayerComponentServer>().SetItemOnCursor(PlaceItem(onCursor, new int2(data.x, data.y)));
				}
				else
				{
					onCursor.amount--;
					PlaceItem(Item.GenerateItem(onCursor.Data.Id, 1), new int2(data.x, data.y));

					if (onCursor.amount == 0)
						player.GetComponent<PlayerComponentServer>().SetItemOnCursor(null);
				}

				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
			else if (clicked != null)
			{
				if (data.InteractedEntityId > 0)
				{
					var entity = GameServer.NetConnector.GetFneEntity(data.InteractedEntityId);
					FNEComponent entityComponent;

					entityComponent = entity.GetComponent<InventoryComponentServer>();
					if (entityComponent != null)
					{
						PlaceItemInInventory(clicked, (InventoryComponentServer)entityComponent, incMsg);
						return;
					}

					entityComponent = entity.GetComponent<RefinementComponentServer>();
					if (entityComponent != null)
					{
						PlaceItemInRefinement(clicked, (RefinementComponentServer)entityComponent, incMsg);
						return;
					}
				}
				else
				{
					var equipmentComp = clicked.GetComponent<ItemEquipmentComponent>();

					if (equipmentComp != null)
					{
						var equipmentSystem = player.GetComponent<EquipmentSystemComponentServer>();
						foreach (var s in (Slot[])Enum.GetValues(typeof(Slot)))
						{
							if (player.GetComponent<EquipmentSystemComponentServer>().CanBePlaced(clicked, s))
							{
								var itemInSlot = equipmentSystem.GetItemInSlot(s);

								if (s == Slot.Weapon1 && itemInSlot != null)
								{
									continue;
								} 
								
								if ((s == Slot.Consumable1 || s == Slot.Consumable2) && itemInSlot != null)
								{
									continue;
								} 
								
								if(itemInSlot != null)
								{
									var pos = GetItemPosition(clicked);
									RemoveItem(clicked);
									PlaceItem(itemInSlot, pos);
									equipmentSystem.PlaceItemInSlot(clicked, s);
								}
								else
								{
									RemoveItem(clicked);
									equipmentSystem.PlaceItemInSlot(clicked, s);
								}
								equipmentSystem.NE_Send_SlotUpdate(s);
								break;
							}
						}
					} 
					else
					{
						RemoveItem(clicked);
						player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clicked);
					}
				}

				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
		}

		private void NE_Receive_CellRightClicked(NetIncomingMessage incMsg)
		{
			var data = new InventoryCellClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);

			Item onCursor = player.GetComponent<PlayerComponentServer>().GetItemOnCursor();
			Item clickedItem = GetItemAtCell(data.x, data.y);

			if (onCursor == null && clickedItem != null)
			{
				if (GetItemsAndTheirPositions().Count == 0 && m_Data.destroyWhenEmpty)
				{
					GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
				}

				SplitStack(clickedItem, player, incMsg);
			}
			else if (onCursor != null)
				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(PlaceItem(onCursor, new int2(data.x, data.y)));

			GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
		}

		private void NE_Receive_CellShiftRightClicked(NetIncomingMessage incMsg)
		{
			var data = new InventoryCellClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);
			var playerInventory = player.GetComponent<InventoryComponentServer>();

			Item clicked = GetItemAtCell(data.x, data.y);

			if (playerInventory != this && clicked != null)
			{
				if (GetItemsAndTheirPositions().Count == 0 && m_Data.destroyWhenEmpty)
				{
					GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
				}
				
				PlaceItemInInventory(clicked, playerInventory, incMsg);
			}
			else
			{
				if (data.InteractedEntityId > 0)
				{
					var entity = GameServer.NetConnector.GetFneEntity(data.InteractedEntityId);
					FNEComponent entityComponent;
					entityComponent = entity.GetComponent<InventoryComponentServer>();

					if (entityComponent != null)
					{
						PlaceItemInInventory(clicked, (InventoryComponentServer)entityComponent, incMsg);
						return;
					}

					entityComponent = entity.GetComponent<RefinementComponentServer>();
					if (entityComponent != null)
					{
						PlaceItemInRefinement(clicked, (RefinementComponentServer)entityComponent, incMsg);
						return;
					}
				}
			}
		}

		private void NE_Receive_ModClicked(NetIncomingMessage incMsg)
		{
			var data = new ItemModClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);

			Item ItemOnCursor = player.GetComponent<PlayerComponentServer>().GetItemOnCursor();
			Item itemToMod = GetItemAtCell(data.itemX, data.itemY);

            if (!itemToMod.HasComponent<ItemWeaponComponent>())
            {
				return;
            }

			var weaponComp = itemToMod.GetComponent<ItemWeaponComponent>();
			var previousMod = weaponComp.ModSlots[data.modIndex];
			var clickedModItem = previousMod.ModItem;
			if (ItemOnCursor == null)
			{
				weaponComp.ModSlots[data.modIndex] = new WeaponMod
				{
					ModItem = null,
					ModColor = previousMod.ModColor
				};

				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clickedModItem);
				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
			else if (ItemOnCursor != null && ItemOnCursor.HasComponent<ItemWeaponModComponent>())
			{
				var modComp = ItemOnCursor.GetComponent<ItemWeaponModComponent>();
				if (modComp.Data.modColor != weaponComp.ModSlots[data.modIndex].ModColor)
					return;

				var temp = ItemOnCursor;
				weaponComp.ModSlots[data.modIndex] = new WeaponMod
				{
					ModItem = temp,
					ModColor = previousMod.ModColor
				};

				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clickedModItem);
				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
		}

		private void NE_Receive_ModRightClicked(NetIncomingMessage incMsg)
		{
			var data = new ItemModClickedData();
			data.Deserialize(incMsg);

			FNEEntity player = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);

			Item ItemOnCursor = player.GetComponent<PlayerComponentServer>().GetItemOnCursor();
			Item itemToMod = GetItemAtCell(data.itemX, data.itemY);

			if (!itemToMod.HasComponent<ItemWeaponComponent>())
			{
				return;
			}

			var weaponComp = itemToMod.GetComponent<ItemWeaponComponent>();
			var previousMod = weaponComp.ModSlots[data.modIndex];
			var clickedModItem = previousMod.ModItem;
			if (ItemOnCursor == null)
			{
				weaponComp.ModSlots[data.modIndex] = new WeaponMod
				{
					ModItem = null,
					ModColor = previousMod.ModColor
				};

				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clickedModItem);
				GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
			}
		}

		private void SplitStack(Item clicked, FNEEntity player, NetIncomingMessage incMsg)
		{
			if (clicked.amount == 1)
			{
				RemoveItem(clicked);
				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(clicked);
			}
			else
			{
				player.GetComponent<PlayerComponentServer>().SetItemOnCursor(
					Item.GenerateItem(clicked.Data.Id, clicked.amount / 2)
				);
				clicked.amount -= clicked.amount / 2;
			}
			
			GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
		}

		private void PlaceItemInInventory(Item clicked, InventoryComponentServer receivingInventory, NetIncomingMessage incMsg)
		{
			if (receivingInventory.AutoPlaceIfPossible(clicked))
				RemoveItem(clicked);
			else
				GameServer.NetAPI.Notification_SendNotification_STC(clicked.Data.iconRef, "red", "false", "Item placement not possible. Is inventory full?", incMsg.SenderConnection);

			GameServer.NetAPI.Entity_UpdateComponent_BAR(receivingInventory);
			GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
		}

		private void PlaceItemInRefinement(Item clicked, RefinementComponentServer refineComp, NetIncomingMessage incMsg)
		{
			var activeRecipe = refineComp.GetActiveRecipe();

			for (int i = 0; i < activeRecipe.requiredMaterials.Count; i++)
			{
				foreach (var reqMat in activeRecipe.requiredMaterials)
				{
					if (reqMat.itemRef == clicked.Data.Id && activeRecipe.requiredMaterials[i].itemRef == clicked.Data.Id)
					{
						var inputMats = refineComp.GetMaterialsInInputSlot(i);
						if (inputMats != null)
						{
							if (inputMats.amount < inputMats.Data.maxStackSize)
							{
								var missingAmount = inputMats.Data.maxStackSize - inputMats.amount;

								if (clicked.amount > missingAmount)
								{
									if (RemoveItem(clicked, missingAmount))
										refineComp.AddMaterialToInput(i, Item.GenerateItem(clicked.Data.Id, missingAmount));
								}
								else
								{
									if (RemoveItem(clicked, clicked.amount))
										refineComp.AddMaterialToInput(i, clicked);
								}

							}
						}
						else if (RemoveItem(clicked, clicked.amount))
						{
							refineComp.AddMaterialToInput(i, clicked);
						}

						GameServer.NetAPI.Entity_UpdateComponent_BAR(refineComp);
						GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
					}
				}
			}
		}

		private void GenerateLoot()
		{
			var loot = LootGenerator.GenerateLoot(m_Data.produceLootTable);

			foreach (var item in loot)
			{
				AutoPlaceIfPossible(item);
			}

		}
		
		public override void OnReplaced(FNEEntity replacement)
		{
			base.OnReplaced(replacement);
			var newInvComp = replacement.GetComponent<InventoryComponentServer>();
			if (newInvComp != null)
			{
				foreach (var item in GetItemsAndTheirPositions())
				{
					newInvComp.PlaceItem(item.Key, item.Value);
				}
			}
			
			GameServer.NetAPI.Entity_UpdateComponent_BAR(newInvComp);
		}
	}

}