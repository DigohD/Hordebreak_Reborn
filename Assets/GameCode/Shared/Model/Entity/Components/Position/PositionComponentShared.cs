using FNZ.Shared.Utils;
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
			byte chunkX = (byte)(ParentEntity.Position.x / 32);
			byte chunkY = (byte)(ParentEntity.Position.y / 32);

			ushort localX = FNEUtil.PackFloatAsShort(ParentEntity.Position.x % 32);
			ushort localY = FNEUtil.PackFloatAsShort(ParentEntity.Position.y % 32);

			bw.Write(chunkX);
			bw.Write(chunkY);

			bw.Write(localX);
			bw.Write(localY);
		}

		public override void Deserialize(NetBuffer br)
		{
			byte chunkX = br.ReadByte();
			byte chunkY = br.ReadByte();

			float x = FNEUtil.UnpackShortToFloat(br.ReadUInt16());
			float y = FNEUtil.UnpackShortToFloat(br.ReadUInt16());

			ParentEntity.Position = new float2(x + chunkX * 32, y + chunkY * 32);
		}

		public override ushort GetSizeInBytes() { return 6; }
	}
}
