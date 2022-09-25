using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Net.Dto.Hordes 
{
	public struct HordeEntitySpawnBatchData : ISerializeableData
	{
		public int Count;
		public List<HordeEntitySpawnData> Entities;

		public void NetSerialize(NetBuffer writer)
		{
			writer.Write((byte)NetMessageType.SPAWN_HORDE_ENTITY_BATCH);
			writer.Write((ushort)Count);

			Entities.ForEach(e => e.NetSerialize(writer));
		}

		public void NetDeserialize(NetBuffer reader)
        {
			Count = reader.ReadUInt16();
			Entities = new List<HordeEntitySpawnData>(Count);

			for (var i = 0; i < Count; i++)
            {
				var entity = new HordeEntitySpawnData();
				entity.NetDeserialize(reader);
				Entities.Add(entity);
			}
		}

		public void FileSerialize(NetBuffer writer)
		{
			writer.Write((ushort)Count);
			Entities.ForEach(e => e.FileSerialize(writer));
		}

		public void FileDeserialize(NetBuffer reader)
		{
			Count = reader.ReadUInt16();
			Entities = new List<HordeEntitySpawnData>(Count);

			for (var i = 0; i < Count; i++)
			{
				var entity = new HordeEntitySpawnData();
				entity.FileDeserialize(reader);
				Entities.Add(entity);
			}
		}

		public int GetSizeInBytes(bool netSerialize)
        {
			var entitiesSizeInBytes = Entities?.Count > 0 ? Entities[0].GetSizeInBytes(netSerialize) * Entities.Count : 0;
			return (netSerialize ? sizeof(byte) : 0) + sizeof(ushort) + entitiesSizeInBytes;
		}
    }

	public struct HordeEntitySpawnData : ISerializeableData
	{
		public int NetId;
		public ushort EntityIdCode;
		public float2 Position;
		public float Rotation;

		public void NetSerialize(NetBuffer writer)
		{
			writer.Write(NetId);
			FileSerialize(writer);
		}

		public void NetDeserialize(NetBuffer reader)
		{
			NetId = reader.ReadInt32();
			FileDeserialize(reader);
		}

		public void FileSerialize(NetBuffer writer)
		{
			writer.Write(EntityIdCode);
			writer.Write(Position.x);
			writer.Write(Position.y);
			writer.Write(Rotation);
		}

		public void FileDeserialize(NetBuffer reader)
		{
			EntityIdCode = reader.ReadUInt16();
			Position = new float2(reader.ReadFloat(), reader.ReadFloat());
			Rotation = reader.ReadFloat();
		}

		public int GetSizeInBytes(bool netSerialize)
		{
			return (netSerialize ? sizeof(int) : 0) + sizeof(ushort) + (3 * sizeof(float));
		}
    }
}