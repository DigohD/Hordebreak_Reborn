using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Shared.Model.Entity.Components.Position
{
	public class PositionComponentShared : FNEComponent
	{
		new public PositionComponentData m_Data
		{
			get
			{
				return (PositionComponentData)base.m_Data;
			}
		}

		public override void Init()
		{

		}

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(ParentEntity.Position.x);
			bw.Write(ParentEntity.Position.y);
		}

		public override void Deserialize(NetBuffer br)
		{
			float x = br.ReadFloat();
			float y = br.ReadFloat();

			ParentEntity.Position = new float2(x, y);
		}

		public override ushort GetSizeInBytes() { return 6; }
	}
}
