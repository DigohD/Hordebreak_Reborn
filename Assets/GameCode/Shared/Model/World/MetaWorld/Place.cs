using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Shared.Model.World.MetaWorld 
{

	public struct Place
	{
		// Coords are in kilometers relative to home at 0:0
		public float2 Coords;

		// Name of the place for display in game
		public string Name;

		public Place(float2 coords, string name)
		{
			Coords = coords;
			Name = name;
		}

		public void Deserialize(NetBuffer reader)
		{
			Coords = new float2(reader.ReadFloat(), reader.ReadFloat());
			Name = reader.ReadString();
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(Coords.x);
			writer.Write(Coords.y);
			writer.Write(Name);
		}

		public int GetByteSize()
		{
			return 8 + Name.Length * 4;
		}
	}
}