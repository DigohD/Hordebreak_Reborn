using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.ItemTransferComponent
{
	public class ItemTransferComponentShared : FNEComponent
	{
		new public ItemTransferComponentData m_Data
		{
			get
			{
				return (ItemTransferComponentData)base.m_Data;
			}
		}


		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
