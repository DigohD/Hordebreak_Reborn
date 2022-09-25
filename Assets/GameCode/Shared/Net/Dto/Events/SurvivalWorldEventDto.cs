using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Shared.Net.Dto.Events 
{
	public struct SurvivalWorldEventDto : INetSerializeableData
	{
		public ushort IdCode;
		public float2 Position;
		public double StartTimeStamp;
		public long UniqueId;
		
		public void NetSerialize(NetBuffer writer)
		{
			writer.Write(IdCode);
			writer.Write(Position.x);
			writer.Write(Position.y);
			writer.Write(StartTimeStamp);
			writer.Write(UniqueId);
		}

		public void NetDeserialize(NetBuffer reader)
		{
			IdCode = reader.ReadUInt16();
			Position = new float2(reader.ReadSingle(), reader.ReadSingle());
			StartTimeStamp = reader.ReadDouble();
			UniqueId = reader.ReadInt64();
		}

		public int GetSizeInBytes()
		{
			return sizeof(ushort) + sizeof(float) * 2 + sizeof(double) + sizeof(long);
		}
	}
}