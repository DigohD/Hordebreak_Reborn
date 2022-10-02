using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Shared.Model.World.MetaWorld 
{
	public class MetaWorldModel
	{
		public List<Place> Places = new List<Place>();

		public void SerializeMetaWorld(NetBuffer writer)
		{
			writer.Write(Places.Count);
			for (int i = 0; i < Places.Count; i++)
			{
				Places[i].Serialize(writer);
			}
		}

		public void DeserializeMetaWorld(NetBuffer reader)
		{
			Places = new List<Place>();

			var count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				var place = new Place();
				place.Deserialize(reader);
				Places.Add(place);
			}
		}
	}
}