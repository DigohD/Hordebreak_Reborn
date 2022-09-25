using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Producer
{
	public class ProducerComponentShared : FNEComponent
	{
		public ProducerComponentData Data
		{
			get
			{
				return (ProducerComponentData)base.m_Data;
			}
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
