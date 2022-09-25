using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Inventory
{
	public enum InventoryNetEvent
	{
		CELL_CLICKED = 0,
		CELL_SHIFT_CLICKED = 1,
		CELL_RIGHT_CLICKED = 2,
		CELL_SHIFT_RIGHT_CLICKED = 3,
		MOD_CLICKED = 4,
		MOD_RIGHT_CLICKED = 5
	}

	public struct InventoryCellClickedData : IComponentNetEventData
	{
		public int x;
		public int y;
		public int InteractedEntityId;

		public void Deserialize(NetBuffer reader)
		{
			x = reader.ReadInt32();
			y = reader.ReadInt32();
			InteractedEntityId = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 12;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(x);
			writer.Write(y);
			writer.Write(InteractedEntityId);
		}
	}

	public struct ItemModClickedData : IComponentNetEventData
	{
		public int itemX;
		public int itemY;
		public int InteractedEntityId;
		public byte modIndex;

		public void Deserialize(NetBuffer reader)
		{
			itemX = reader.ReadInt32();
			itemY = reader.ReadInt32();
			InteractedEntityId = reader.ReadInt32();
			modIndex = reader.ReadByte();
		}

		public int GetSizeInBytes()
		{
			return 13;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(itemX);
			writer.Write(itemY);
			writer.Write(InteractedEntityId);
			writer.Write(modIndex);
		}
	}
}