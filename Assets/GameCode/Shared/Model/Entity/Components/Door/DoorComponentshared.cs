using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Door
{
	public class DoorComponentShared : FNEComponent
	{
		protected bool IsOpen;

		public DoorComponentData Data
		{
			get
			{
				return (DoorComponentData)base.m_Data;
			}
		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(IsOpen);
		}

		public override void Deserialize(NetBuffer br)
		{
			IsOpen = br.ReadBoolean();
		}

		public override ushort GetSizeInBytes() { return 1; }

	}
}