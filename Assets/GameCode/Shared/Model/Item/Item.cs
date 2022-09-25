using FNZ.Shared.Model.Items.Components;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Shared.Model.Items
{
	public class Item
	{
		public List<ItemComponent> ItemComponents = new List<ItemComponent>();

		public ItemData Data;
		
		public int amount;

		public T GetComponent<T>() where T : ItemComponent
		{
			return ItemComponents
				.Where(comp => comp is T)
				.FirstOrDefault() as T;
		}

		public ItemComponent AddComponent(Type newCompType, ItemComponentData data = null)
		{
			foreach (var comp in ItemComponents)
			{
				if (comp.GetType() == newCompType)
				{
					Debug.LogError("COMPONENT ADDED TWICE TO ENTITY!");
				}
			}

			var newComp = (ItemComponent)Activator.CreateInstance(newCompType);

			ItemComponents.Add(newComp);
			newComp.ParentItem = this;

			if (data == null)
			{
				Debug.LogError("WARNING: " + newCompType + " Was added without any data!");
			}

			newComp.SetData(data);
			newComp.Init();

			return newComp;
		}

		public Item()
		{

		}

		private Item(string id, int amount)
		{
			if (id == null)
				return;

			Data = DataBank.Instance.GetData<ItemData>(id);
			this.amount = amount;

			if (Data.components != null)
			{
				foreach (var compData in Data.components)
				{
					AddComponent(compData.GetComponentType(), compData);
				}
			}
		}

		// GenerateItem is used to create references to Items in all situations.
		// creatingItem should be true if this is the first time the item is generated on the server.
		public static Item GenerateItem(string id, int amount = 1, bool creatingItem = false)
		{
			if (amount == 0 || id == null) return null;

			var item = new Item(id, amount);

            if (creatingItem)
            {
				GenerateItemSpecifics(item);
			}

			return item;
		}

		private static void GenerateItemSpecifics(Item item)
        {
			foreach(ItemComponent comp in item.ItemComponents)
            {
				if(comp is ItemWeaponComponent)
					((ItemWeaponComponent) comp).GenerateModSlots();
            }
        }

		public static int GetItemMaxStackSizeFromId(string id)
		{
			return GenerateItem(id).Data.maxStackSize;
		}

		public static Item GenerateItem(NetBuffer br)
		{
			Item item = new Item();

			item.Deserialize(br);

			return item;
		}

		public void Serialize(NetBuffer bw)
		{
			bw.Write(IdTranslator.Instance.GetIdCode<ItemData>(Data.Id));
			bw.Write(amount);

			foreach (var itemComp in ItemComponents)
			{
				itemComp.Serialize(bw);
			}
		}

		public void Deserialize(NetBuffer br)
		{
			string id = IdTranslator.Instance.GetId<ItemData>(br.ReadUInt16());
			int amount = br.ReadInt32();

			Data = DataBank.Instance.GetData<ItemData>(id);
			this.amount = amount;

			if (Data.components != null)
			{
				foreach (var compData in Data.components)
				{
					AddComponent(compData.GetComponentType(), compData);
				}
			}

			foreach (var itemComp in ItemComponents)
			{
				itemComp.Deserialize(br);
			}
		}

		public static ushort GetNetSizeInBytes()
		{
			// 2 + 4
			return 6;
		}

		public bool HasComponent<T>()
		{
			return ItemComponents.FindAll(c => c is T).Count > 0;
		}

		public static bool IsItemIdentical(Item item1, Item item2)
		{
			if (item1 == null && item2 == null)
				return true;

			if (item1 != null && item2 != null)
			{
				if (item1.Data.Id == item2.Data.Id 
					&& item1.amount == item2.amount
					&& CompareModSlots(item1, item2))
				{
					return true;
				}
			}

			return false;
		}

		private static bool CompareModSlots(Item item1, Item item2)
        {
			var weaponComp1 = item1.GetComponent<ItemWeaponComponent>();
			var weaponComp2 = item2.GetComponent<ItemWeaponComponent>();

			// They are checked to be same Id, so if one is null, both are
			if (weaponComp1 == null)
            {
				return true;
            }

			if (weaponComp1.ModSlots.Count != weaponComp2.ModSlots.Count)
				return false;

			for(int i = 0; i < weaponComp1.ModSlots.Count; i++)
            {
				var modSlot1 = weaponComp1.ModSlots[i];
				var modSlot2 = weaponComp2.ModSlots[i];

				if (modSlot1.ModColor != modSlot2.ModColor)
					return false;

				if (modSlot1.ModItem == null && modSlot2.ModItem == null)
					continue;

				if (modSlot1.ModItem != null && modSlot2.ModItem == null)
					return false;
					
				if (modSlot1.ModItem == null && modSlot2.ModItem != null)
					return false;

				if (modSlot1.ModItem.Data.Id != modSlot2.ModItem.Data.Id)
					return false;
			}

			return true;
        }

		public bool HasTargetFunctionality()
		{
			bool toReturn = false;

			var consumeComp = GetComponent<ItemConsumableComponent>();
			if (consumeComp != null)
			{
				return !string.IsNullOrEmpty(consumeComp.Data.buildingRef);
			}

			return false;
		}
	}


}