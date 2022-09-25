using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Crafting
{
	public class CraftingComponentShared : FNEComponent
	{
		public CraftingComponentData Data
		{
			get
			{
				return (CraftingComponentData)base.m_Data;
			}
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
