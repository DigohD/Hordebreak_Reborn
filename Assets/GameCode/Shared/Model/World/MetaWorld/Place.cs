using System;
using FNZ.Shared.Model.World.Site;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Shared.Model.World.MetaWorld 
{

	public struct Place
	{
		// Coords are in kilometers relative to home at 0:0
		public float2 Coords;

		// The main site id of the place
		public string SiteId;

		public Guid Id;

		public Place(float2 coords, Guid id, string siteId)
		{
			Coords = coords;
			Id = id;
			SiteId = siteId;
		}

		public void Deserialize(NetBuffer reader)
		{
			Coords = new float2(reader.ReadFloat(), reader.ReadFloat());
			Id = Guid.Parse(reader.ReadString());
			SiteId = IdTranslator.Instance.GetId<SiteData>(reader.ReadUInt16());
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(Coords.x);
			writer.Write(Coords.y);
			writer.Write(Id.ToString());
			writer.Write(IdTranslator.Instance.GetIdCode<SiteData>(SiteId));
		}

		public int GetByteSize()
		{
			return 8 + Id.ToString().Length * 4;
		}
	}
}