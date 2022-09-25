using FNZ.Shared.Model.Items;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Excavator
{
	public enum ExcavatorNetEvent
	{
		FUEL_UPDATE = 1,
		REFUEL_SLOT_CLICK = 2,
		REFUEL_SLOT_UPDATE = 3,
		REFUEL_MAX_CLICK = 4,
		REFUEL_ONE_CLICK = 5,
        LOCK_ENTITY = 6,
        UNLOCK_ENTITY = 7,
		TRIGGER_EXCAVATE = 8
	}

	public struct FuelUpdateData : IComponentNetEventData
	{
		public int fuel;

		public void Deserialize(NetBuffer reader)
		{
			fuel = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 4;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(fuel);
		}
	}

	public struct RefuelSlotUpdateData : IComponentNetEventData
	{
		public Item slotItem;

		public void Deserialize(NetBuffer reader)
		{
			if (reader.ReadBoolean())
			{
				var itemId = IdTranslator.Instance.GetId<ItemData>(reader.ReadUInt16());
				var itemAmount = reader.ReadInt32();

				slotItem = Item.GenerateItem(itemId, itemAmount, true);
			}
			else
			{
				slotItem = null;
			}
		}

		public int GetSizeInBytes()
		{
			return 6;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(slotItem != null);
			if (slotItem != null)
			{
				writer.Write(IdTranslator.Instance.GetIdCode<ItemData>(slotItem.Data.Id));
				writer.Write(slotItem.amount);
			}
		}
	}

    public struct LockEntityData : IComponentNetEventData
    {
        public int NetId;

        public LockEntityData(int netId)
        {
            NetId = netId;
        }

        public void Deserialize(NetBuffer reader)
        {
            NetId = reader.ReadInt32();
        }

        public int GetSizeInBytes()
        {
            return 4;
        }

        public void Serialize(NetBuffer writer)
        {
            writer.Write(NetId);
        }
    }

	public struct TriggerExcavateData : IComponentNetEventData
	{
		public byte BonusIndex;

		public TriggerExcavateData(byte bonusIndex)
		{
			BonusIndex = bonusIndex;
		}

		public void Deserialize(NetBuffer reader)
		{
			BonusIndex = reader.ReadByte();
		}

		public int GetSizeInBytes()
		{
			return 1;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(BonusIndex);
		}
	}

}