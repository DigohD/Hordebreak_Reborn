using Lidgren.Network;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.TileObject
{
	public class TileObjectComponentShared : FNEComponent
	{
		public byte cost = 255;
		public bool seeThrough;
		public bool hittable;
		public bool canDestroy;

		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(cost);
			bw.Write(seeThrough);
			bw.Write(canDestroy);
			bw.Write(ParentEntity.Scale.x);
			bw.Write(ParentEntity.Scale.y);
			bw.Write(ParentEntity.Scale.z);
		}

		public override void Deserialize(NetBuffer br)
		{
			cost = br.ReadByte();
			seeThrough = br.ReadBoolean();
			canDestroy = br.ReadBoolean();
			ParentEntity.Scale = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		}

		public override ushort GetSizeInBytes()
		{
			// 1 + 1 + 1 + 4 + 4 + 4
			return 15;
		}
	}
}
