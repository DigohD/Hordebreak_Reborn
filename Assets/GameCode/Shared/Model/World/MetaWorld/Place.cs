using FNZ.Shared.Model.World.Site;
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

		// The main site id of the place
		public string SiteId;

		// Name of the place for display in game
		public string Name;

		public Place(float2 coords, string name, string siteId)
		{
			Coords = coords;
			Name = name;
			SiteId = siteId;
		}

		public void Deserialize(NetBuffer reader)
		{
			Coords = new float2(reader.ReadFloat(), reader.ReadFloat());
			Name = reader.ReadString();
			SiteId = IdTranslator.Instance.GetId<SiteData>(reader.ReadUInt16());
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(Coords.x);
			writer.Write(Coords.y);
			writer.Write(Name);
			writer.Write(IdTranslator.Instance.GetIdCode<SiteData>(SiteId));
		}

		public int GetByteSize()
		{
			return 8 + Name.Length * 4;
		}
	}
}