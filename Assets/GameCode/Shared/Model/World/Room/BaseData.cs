using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World.Rooms
{

	public struct BaseData
	{
		public string Name;
		public bool IsOnline;
		public float2 Position;
		public byte radius;

		public BaseData(string name, bool isOnline, float2 Position, byte radius)
		{
			Name = name;
			IsOnline = isOnline;
			this.Position = Position;
			this.radius = radius;
		}

		public void Serialize(NetBuffer sendBuffer)
		{
			sendBuffer.Write(Name);
			sendBuffer.Write(IsOnline);
			sendBuffer.Write(Position.x);
			sendBuffer.Write(Position.y);
			sendBuffer.Write(radius);
		}

		public void Deserialize(NetBuffer reader)
		{
			Name = reader.ReadString();
			IsOnline = reader.ReadBoolean();
			Position = new float2(reader.ReadFloat(), reader.ReadFloat());
			radius = reader.ReadByte();
		}
	}
}