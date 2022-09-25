using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Lidgren.Network;
using System;

namespace FNZ.Shared.Model.Entity.Components.EquipmentSystem
{
	public delegate void OnEquipmentUpdate();

	public class EquipmentSystemComponentShared : FNEComponent
	{
		public OnEquipmentUpdate d_OnEquipmentUpdate;

		public Item HeadGear;
		public Item TorsoGear;
		public Item LegsGear;
		public Item FeetGear;
		public Item BackGear;
		public Item HandsGear;
		public Item WaistGear;

		public Item Weapon1;
		public Item Weapon2;

		public Item Trinket1;
		public Item Trinket2;

		public Item Consumable1;
		public Item Consumable2;
		public Item Consumable3;
		public Item Consumable4;

		public Slot ActiveActionBarSlot;

		public bool CanBePlaced(Item itemToBePlaced, Slot slot)
		{
			var equipmentComp = itemToBePlaced.GetComponent<ItemEquipmentComponent>();
			if (equipmentComp == null)
				return false;

			switch (equipmentComp.Data.GetEquipmentType())
			{
				case EquipmentType.Weapon:
					return (slot == Slot.Weapon1 || slot == Slot.Weapon2);

				case EquipmentType.Torso:
					return (slot == Slot.Torso);

				case EquipmentType.Hands:
					return (slot == Slot.Hands);

				case EquipmentType.Feet:
					return (slot == Slot.Feet);

				case EquipmentType.Back:
					return (slot == Slot.Back);

				case EquipmentType.Waist:
					return (slot == Slot.Waist);

				case EquipmentType.Head:
					return (slot == Slot.Head);

				case EquipmentType.Legs:
					return (slot == Slot.Legs);

				case EquipmentType.Trinket:
					return (slot == Slot.Trinket1 || slot == Slot.Trinket2);

				case EquipmentType.Consumable:
					return (slot == Slot.Consumable1 || slot == Slot.Consumable2 || slot == Slot.Consumable3 || slot == Slot.Consumable4);

				case EquipmentType.None:
					return false;
			}
			return false;
		}

		public void SetItemInSlot(Item item, Slot slot)
		{
			switch (slot)
			{
				case Slot.Head:
					HeadGear = item;
					break; ;
				case Slot.Torso:
					TorsoGear = item;
					break;
				case Slot.Legs:
					LegsGear = item;
					break;
				case Slot.Feet:
					FeetGear = item;
					break;
				case Slot.Back:
					BackGear = item;
					break;
				case Slot.Hands:
					HandsGear = item;
					break;
				case Slot.Waist:
					WaistGear = item;
					break;

				case Slot.Weapon1:
					Weapon1 = item;
					Weapon1?.GetComponent<ItemWeaponComponent>().RecalculateFinalModdedProperties();
					break;
				case Slot.Weapon2:
					Weapon2 = item;
					Weapon2?.GetComponent<ItemWeaponComponent>().RecalculateFinalModdedProperties();
					break;

				case Slot.Trinket1:
					Trinket1 = item;
					break;
				case Slot.Trinket2:
					Trinket2 = item;
					break;

				case Slot.Consumable1:
					Consumable1 = item;
					break;
				case Slot.Consumable2:
					Consumable2 = item;
					break;
				case Slot.Consumable3:
					Consumable3 = item;
					break;
				case Slot.Consumable4:
					Consumable4 = item;
					break;
			}

			d_OnEquipmentUpdate?.Invoke();
		}

		public Item GetItemInSlot(Slot slot)
		{
			switch (slot)
			{
				case Slot.Head:
					return HeadGear;

				case Slot.Torso:
					return TorsoGear;

				case Slot.Legs:
					return LegsGear;

				case Slot.Feet:
					return FeetGear;

				case Slot.Back:
					return BackGear;

				case Slot.Hands:
					return HandsGear;

				case Slot.Waist:
					return WaistGear;

				case Slot.Weapon1:
					return Weapon1;
				case Slot.Weapon2:
					return Weapon2;

				case Slot.Trinket1:
					return Trinket1;
				case Slot.Trinket2:
					return Trinket2;

				case Slot.Consumable1:
					return Consumable1;
				case Slot.Consumable2:
					return Consumable2;
				case Slot.Consumable3:
					return Consumable3;
				case Slot.Consumable4:
					return Consumable4;
			}

			return null;
		}

		public Item GetActiveItem()
		{
			return GetItemInSlot(ActiveActionBarSlot);
		}

		public override void Init()
		{
			ActiveActionBarSlot = Slot.Weapon1;
		}

		public override void Serialize(NetBuffer bw)
		{
			byte equippedItems = GetEquippedItemCount();

			bw.Write(equippedItems);

			foreach (Slot slot in Enum.GetValues(typeof(Slot)))
			{
				var item = GetItemInSlot(slot);

				if (item != null)
				{
					bw.Write((byte)slot);
					item.Serialize(bw);
				}
			}

			bw.Write((byte)ActiveActionBarSlot);
		}

		public override void Deserialize(NetBuffer br)
		{
			byte count = br.ReadByte();

			for (int i = 0; i < count; i++)
			{
				Slot slot = (Slot)br.ReadByte();

				switch (slot)
				{
					case Slot.Head:
						HeadGear = Item.GenerateItem(br);
						break;
					case Slot.Torso:
						TorsoGear = Item.GenerateItem(br);
						break;
					case Slot.Legs:
						LegsGear = Item.GenerateItem(br);
						break;
					case Slot.Feet:
						FeetGear = Item.GenerateItem(br);
						break;
					case Slot.Waist:
						WaistGear = Item.GenerateItem(br);
						break;
					case Slot.Back:
						BackGear = Item.GenerateItem(br);
						break;
					case Slot.Hands:
						HandsGear = Item.GenerateItem(br);
						break;

					case Slot.Weapon1:
						Weapon1 = Item.GenerateItem(br);
						Weapon1?.GetComponent<ItemWeaponComponent>().RecalculateFinalModdedProperties();
						break;
					case Slot.Weapon2:
						Weapon2 = Item.GenerateItem(br);
						Weapon2?.GetComponent<ItemWeaponComponent>().RecalculateFinalModdedProperties();
						break;

					case Slot.Trinket1:
						Trinket1 = Item.GenerateItem(br);
						break;
					case Slot.Trinket2:
						Trinket2 = Item.GenerateItem(br);
						break;
					case Slot.Consumable1:
						Consumable1 = Item.GenerateItem(br);
						break;
					case Slot.Consumable2:
						Consumable2 = Item.GenerateItem(br);
						break;
					case Slot.Consumable3:
						Consumable3 = Item.GenerateItem(br);
						break;
					case Slot.Consumable4:
						Consumable4 = Item.GenerateItem(br);
						break;

					default:
						break;

				}
			}

			ActiveActionBarSlot = (Slot)br.ReadByte();
		}

		private byte GetEquippedItemCount()
		{
			byte equippedItems = 0;

			foreach (Slot slot in Enum.GetValues(typeof(Slot)))
			{
				if (GetItemInSlot(slot) != null)
				{
					equippedItems++;
				}
			}

			return equippedItems;
		}

		public override ushort GetSizeInBytes() {
			byte equippedItems = GetEquippedItemCount();

			return  (ushort) (1 + (equippedItems * Item.GetNetSizeInBytes())); 
		}
	}
}
