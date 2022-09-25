using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Environment
{
	public class EnvironmentComponentShared : FNEComponent
	{
		public EnvironmentComponentData Data
		{
			get
			{
				return (EnvironmentComponentData)base.m_Data;
			}
		}

		public override void Init() { }

		public override void Serialize(NetBuffer bw) { }

		public override void Deserialize(NetBuffer br) { }

		public override ushort GetSizeInBytes() { return 0; }
	}
}
