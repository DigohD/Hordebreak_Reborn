using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Crop
{
	public class CropComponentShared : FNEComponent
	{
		public float growth = 0f;
		public GrowthStatus growthStatus;

		public CropComponentData Data
		{
			get
			{
				return (CropComponentData)base.m_Data;
			}
		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(growth);
			bw.Write((byte)growthStatus);
		}

		public override void Deserialize(NetBuffer br)
		{
			growth = br.ReadFloat();
			growthStatus = (GrowthStatus)br.ReadByte();
		}

		public override ushort GetSizeInBytes()
		{
			return sizeof(float) + sizeof(byte);
		}
	}
}