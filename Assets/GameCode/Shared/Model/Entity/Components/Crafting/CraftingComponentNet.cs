using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Crafting
{
	public enum CraftingNetEvent
	{
		CRAFT = 0
	}

	public struct CraftData : IComponentNetEventData
	{
		public string recipeRef;
		public int amount;

		public void Deserialize(NetBuffer reader)
		{
			recipeRef = IdTranslator.Instance.GetId<CraftingRecipeData>(reader.ReadUInt16());
			amount = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 6;
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(IdTranslator.Instance.GetIdCode<CraftingRecipeData>(recipeRef));
			writer.Write(amount);
		}
	}
}