using FNZ.Shared.Model.Entity.MountedObject;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.EdgeObject
{
	public class EdgeObjectComponentShared : FNEComponent
	{
		public bool IsPassable;
		public bool IsHittable;
		public bool IsDestroyed;
		public bool IsSeethrough;
		public Vector2 CenterPosition;

		public MountedObjectData MountedObjectData;
		public bool OppositeMountedDirection;

		public override void Init() { }

		public override void Serialize(NetBuffer bw)
		{
			bw.Write(IsPassable);
			bw.Write(IsDestroyed);
			bw.Write(IsSeethrough);
			bw.Write(MountedObjectData != null);
			if(MountedObjectData != null)
			{
				bw.Write(IdTranslator.Instance.GetIdCode<MountedObjectData>(MountedObjectData.Id));
				bw.Write(OppositeMountedDirection);
			}
		}

		public override void Deserialize(NetBuffer br)
		{
			IsPassable = br.ReadBoolean();
			IsDestroyed = br.ReadBoolean();
			IsSeethrough = br.ReadBoolean();
			var hasMountedObject = br.ReadBoolean();
			if (hasMountedObject)
			{
				var mountedId = IdTranslator.Instance.GetId<MountedObjectData>(br.ReadUInt16());
				MountedObjectData = DataBank.Instance.GetData<MountedObjectData>(mountedId);
				OppositeMountedDirection = br.ReadBoolean();
			}
			else
			{
				MountedObjectData = null;
			}
		}

		public override ushort GetSizeInBytes()
		{
			// 1 + 1 + 1 + 2 + 1
			return 6;
		}
	}
}
