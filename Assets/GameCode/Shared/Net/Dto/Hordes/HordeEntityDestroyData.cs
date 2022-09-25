using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Net.Dto.Hordes
{
    public struct HordeEntityDestroyBatchNetData : INetSerializeableData
    {
        public int Count;
        public List<HordeEntityDestroyData> Entities;

        public void NetSerialize(NetBuffer writer)
        {
            writer.Write((byte)NetMessageType.DESTROY_HORDE_ENTITY_BATCH);
            writer.Write((ushort)Count);

            Entities.ForEach(e => e.NetSerialize(writer));
        }

        public void NetDeserialize(NetBuffer reader)
        {
            Count = reader.ReadUInt16();
            Entities = new List<HordeEntityDestroyData>(Count);

            for (var i = 0; i < Count; i++)
            {
                var entity = new HordeEntityDestroyData();
                entity.NetDeserialize(reader);
                Entities.Add(entity);
            }
        }

        public int GetSizeInBytes()
        {
            var entitiesSizeInBytes = Entities?.Count > 0 ? Entities[0].GetSizeInBytes() * Entities.Count : 0;
            return sizeof(byte) + sizeof(ushort) + entitiesSizeInBytes;
        }
    }

    public struct HordeEntityDestroyData : INetSerializeableData
    {
        public int NetId;

        // @NOTE(Anders E): used for broadcasting to relevant players
        public float2 Position;

        public void NetSerialize(NetBuffer writer)
        {
            writer.Write(NetId);
        }

        public void NetDeserialize(NetBuffer reader)
        {
            NetId = reader.ReadInt32();
        }

        public int GetSizeInBytes()
        {
            return sizeof(int);
        }
    }
}