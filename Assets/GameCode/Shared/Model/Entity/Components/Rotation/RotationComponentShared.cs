using FNZ.Shared.Utils;
using Lidgren.Network;

namespace FNZ.Shared.Model.Entity.Components.Rotation
{
	public class RotationComponentShared : FNEComponent
	{
		new public RotationComponentData m_Data
		{
			get
			{
				return (RotationComponentData)base.m_Data;
			}
		}
		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
			// Negative angles leads to glitches on deserializing clients. Possibly due to conversion?
			bw.Write(FNEUtil.PackFloatAsShort(
				ParentEntity.RotationDegrees < 0 ? ParentEntity.RotationDegrees + 360 : ParentEntity.RotationDegrees
			));
		}

		public override void Deserialize(NetBuffer br)
		{
			ParentEntity.RotationDegrees = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
		}

		public override ushort GetSizeInBytes() { return 0; }
	}
}
