using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Consumer
{
	public class ConsumerComponentShared : FNEComponent
	{
		public ConsumerComponentData Data
		{
			get
			{
				return (ConsumerComponentData)base.m_Data;
			}
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
