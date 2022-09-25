using FNZ.Shared.Model.Items;
using FNZ.Shared.Net;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Shared.Model.Entity.Components.EquipmentSystem
{
	public enum EquipmentSystemNetEvent
	{
		SLOT_CLICKED = 0,
		SLOT_SHIFT_CLICKED = 1,
		ACTIVE_ACTION_BAR_CHANGE = 2,
		SLOT_RIGHT_CLICKED = 3,

		PRIMARY_FIRE = 10,
		PRIMARY_FIRE_POSITIONED = 11,

		SLOT_UPDATED = 100
	}

	public struct EquipmentSlotData : IComponentNetEventData
	{
		public Slot Slot;

		public void Deserialize(NetBuffer reader)
		{
			Slot = (Slot)reader.ReadByte();
		}

		public int GetSizeInBytes()
		{
			return 1;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write((byte)Slot);
		}
	}

	public struct EquipmentSlotUpdateData : IComponentNetEventData
	{
		public Slot Slot;
		public Item item;

		public void Deserialize(NetBuffer reader)
		{
			Slot = (Slot)reader.ReadByte();
			if(reader.ReadBoolean())
				item = Item.GenerateItem(reader);
		}

		public int GetSizeInBytes()
		{
			return Item.GetNetSizeInBytes() + 2;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write((byte)Slot);
			writer.Write(item != null);
			if(item != null)
				item.Serialize(writer);
		}
	}
	
	public struct PositionedFireData : IComponentNetEventData
	{
		public float2 Position;

		public void Deserialize(NetBuffer reader)
		{
			Position = new float2(reader.ReadFloat(), reader.ReadFloat());
		}

		public int GetSizeInBytes()
		{
			return 8;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(Position.x);
			writer.Write(Position.y);
		}
	}
}