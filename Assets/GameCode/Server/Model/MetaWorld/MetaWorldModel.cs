using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FNZ.Server.Model.MetaWorld 
{
	public class MetaWorldModel
	{
	    public List<Place> Places = new List<Place>();

		public MetaWorldModel()
        {

        }

		public void CreateNewWorld()
        {
			Places = MetaWorldGenerator.GenerateStartWorld();
        }

		public void LoadFromFile()
        {
			var path = GameServer.FilePaths.GetMetaWorldFilePath();
			if (File.Exists(path))
			{
				var netBuffer = new NetBuffer
				{
					Data = FileUtils.ReadFile(path)
				};

				DeserializeMetaWorld(netBuffer);
			}
		}

		public void SerializeMetaWorld(NetBuffer writer)
        {
			writer.Write(Places.Count);
			for(int i = 0; i < Places.Count; i++)
            {
				Places[i].Serialize(writer);
            }
        }

		public void DeserializeMetaWorld(NetBuffer reader)
        {
			Places = new List<Place>();

			var count = reader.ReadInt32();
			for(int i = 0; i < count; i++)
            {
				var place = new Place();
				place.Deserialize(reader);	
				Places.Add(place);
            }
		}
	}
}