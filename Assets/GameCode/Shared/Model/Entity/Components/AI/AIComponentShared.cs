using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.AI
{
	public class AIComponentShared : FNEComponent
	{
		new public AIComponentData m_Data
		{
			get
			{
				return (AIComponentData)base.m_Data;
			}
		}

		public override void Serialize(NetBuffer bw)
		{ }

		public override void Deserialize(NetBuffer br)
		{ }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
