using FNZ.Shared.Model.Items;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Refinement
{
	public enum RefinementNetEvent
	{
		SET_ACTIVE_RECIPE = 0,
		SLOT_CLICK = 1,
		PROGRESS_UPDATE = 2,
		SLOT_UPDATE = 3,
		SLOT_RIGHT_CLICK = 4,
		SLOT_SHIFT_CLICK = 5,
		IS_WORKING_UPDATE = 6,
		SLOT_SHIFT_RIGHT_CLICK = 7
	}

	public struct SetRefinementRecipeData : IComponentNetEventData
	{
		public ushort RecipeIndex;

		public void Deserialize(NetBuffer reader)
		{
			RecipeIndex = reader.ReadUInt16();
		}

		public int GetSizeInBytes()
		{
			return 2;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(RecipeIndex);
		}
	}

	public struct RefinementProgressUpdateData : IComponentNetEventData
	{
		public int ProcessTicks;

		public void Deserialize(NetBuffer reader)
		{
			ProcessTicks = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 4;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(ProcessTicks);
		}
	}

	public struct RefinmentSlotClickData : IComponentNetEventData
	{
		public RefinementSlotType SlotType;
		public byte SlotIndex;

		public void Deserialize(NetBuffer reader)
		{
			SlotType = (RefinementSlotType)reader.ReadByte();
			SlotIndex = reader.ReadByte();
		}

		public int GetSizeInBytes()
		{
			return 2;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write((byte)SlotType);
			writer.Write((byte)SlotIndex);
		}
	}

	public struct RefinmentSlotUpdateData : IComponentNetEventData
	{
		public RefinementSlotType SlotType;
		public byte SlotIndex;
		public string itemRef;
		public int itemAmount;

		public void Deserialize(NetBuffer reader)
		{
			SlotType = (RefinementSlotType)reader.ReadByte();
			SlotIndex = reader.ReadByte();
			itemRef = IdTranslator.Instance.GetId<ItemData>(reader.ReadUInt16());
			itemAmount = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 8;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write((byte)SlotType);
			writer.Write((byte)SlotIndex);
			writer.Write(IdTranslator.Instance.GetIdCode<ItemData>(itemRef));
			writer.Write(itemAmount);
		}
	}

	public struct IsWorkingUpdateData : IComponentNetEventData
	{
		public bool IsWorking;

		public void Deserialize(NetBuffer reader)
		{
			IsWorking = reader.ReadBoolean();
		}

		public int GetSizeInBytes()
		{
			return 1;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(IsWorking);
		}
	}
}