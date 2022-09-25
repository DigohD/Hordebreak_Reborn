using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Net.Dto.Hordes 
{
	public struct HordeEntityAttackTargetBatchData : INetSerializeableData
	{
		public List<HordeEntityAttackTargetData> Entities;

		public void NetSerialize(NetBuffer writer)
		{
			writer.Write((ushort)Entities.Count);
			Entities.ForEach(e => e.NetSerialize(writer));
		}

		public void NetDeserialize(NetBuffer reader)
		{
			var count = reader.ReadUInt16();
			Entities = new List<HordeEntityAttackTargetData>(count);

			for (var i = 0; i < count; i++)
			{
				var entity = new HordeEntityAttackTargetData();
				entity.NetDeserialize(reader);
				Entities.Add(entity);
			}
		}

		public int GetSizeInBytes()
		{
			return sizeof(ushort) + (Entities[0].GetSizeInBytes() * Entities.Count);
		}
	}

	public struct HordeEntityAttackTargetData : INetSerializeableData
	{
		public int AttackerNetId;
		public int TargetNetId;

		// @NOTE(Anders E): Should not be serialized, only used for broadcasting to all relevant players
		public float2 AttackerPosition;

		public void NetSerialize(NetBuffer writer)
		{
			writer.Write(AttackerNetId);
			writer.Write(TargetNetId);
		}

		public void NetDeserialize(NetBuffer reader)
		{
			AttackerNetId = reader.ReadInt32();
			TargetNetId = reader.ReadInt32();
		}

		public int GetSizeInBytes()
		{
			return 2 * sizeof(int);
		}
	}
}