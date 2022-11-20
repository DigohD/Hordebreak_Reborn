using FNZ.Shared.Constants;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace FNZ.Shared.Net.Dto.Hordes 
{
    public struct HordeEntityUpdateBatchNetData : INetSerializeableData
    {
        public int Count;
        public List<HordeEntityUpdateNetData> Entities;

        public void NetSerialize(NetBuffer writer)
        {
            writer.Write((byte)NetMessageType.UPDATE_HORDE_ENTITY_BATCH);
            writer.Write((ushort)Count);

            Entities.ForEach(e => e.NetSerialize(writer));
        }

        public void NetDeserialize(NetBuffer reader)
        {
            Count = reader.ReadUInt16();
            Entities = new List<HordeEntityUpdateNetData>(Count);

            using var nativeArr = new NativeArray<byte>(4, Allocator.Temp);
            byte[] into = nativeArr.ToArray();

            for (var i = 0; i < Count; i++)
            {
                var entityUpdateData = new HordeEntityUpdateNetData();

                reader.ReadBits(into, 0, HordeEntityPacketHelperConstants.s_NetIdBits);

                entityUpdateData.NetId = (int)BitConverter.ToUInt32(into, 0);

                into[0] = 0;
                into[1] = 0;
                into[2] = 0;
                into[3] = 0;

                //read pos relative to chunk
                reader.ReadBits(into, 0, HordeEntityPacketHelperConstants.s_PositionIntegerBits);
                byte posXint = into[0];
                reader.ReadBits(into, 0, HordeEntityPacketHelperConstants.s_PositionIntegerBits);
                byte posYint = into[0];
                into[0] = 0;

                float posXdec = reader.ReadUnitSingle(HordeEntityPacketHelperConstants.s_PositionDecimalBits);
                float posYdec = reader.ReadUnitSingle(HordeEntityPacketHelperConstants.s_PositionDecimalBits);

                float newX = posXint + posXdec;
                float newY = posYint + posYdec;

                entityUpdateData.Position.x = newX;
                entityUpdateData.Position.y = newY;

                Entities.Add(entityUpdateData);
            }
        }

        public int GetSizeInBytes()
        {
            var entitiesSizeInBytes = Entities?.Count > 0 ? Entities[0].GetSizeInBytes() * Entities.Count : 0;
            return sizeof(byte) + sizeof(ushort) + entitiesSizeInBytes;
        }
    }

    public struct HordeEntityUpdateNetData : INetSerializeableData
    {
        public int NetId;
        public float2 Position;

        public void NetSerialize(NetBuffer writer)
        {
            // 17 bits
            writer.Write(NetId, HordeEntityPacketHelperConstants.s_NetIdBits);

            var posX = Position.x;
            var posY = Position.y;

            var posIntX = (byte)posX;
            var posIntY = (byte)posY;

            var posDecX = posX - posIntX;
            var posDecY = posY - posIntY;

            // 5 bits
            writer.Write(posIntX, HordeEntityPacketHelperConstants.s_PositionIntegerBits);
            // 5 bits
            writer.Write(posIntY, HordeEntityPacketHelperConstants.s_PositionIntegerBits);
            // 3 bits
            writer.WriteUnitSingle(posDecX, HordeEntityPacketHelperConstants.s_PositionDecimalBits);
            // 3 bits
            writer.WriteUnitSingle(posDecY, HordeEntityPacketHelperConstants.s_PositionDecimalBits);

            // total bits = 17+8+8+5+5+3+3 = 49 bits = 6 bytes
        }

        public void NetDeserialize(NetBuffer reader)
        {
        }

        public int GetSizeInBytes()
        {
            return (int)Math.Ceiling(HordeEntityPacketHelperConstants.bitsPerEnemy / 8.0f);
        }
    }
}