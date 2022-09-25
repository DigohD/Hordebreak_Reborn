using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Tile
{
	[XmlType("LootableObjectData")]
	public class LootableObjectData
	{
		[XmlElement("objectRef")]
		public string objectRef { get; set; }

		[XmlElement("spawnFactor")]
		public float spawnFactor { get; set; }
	}

	[XmlType("TileSheetData")]
	public class TileSheetData : DataDef
	{
		[XmlElement("tileSheetPath")]
		public string tileSheetPath { get; set; }

		[XmlArray("lootableObjects")]
		[XmlArrayItem(typeof(LootableObjectData))]
		public List<LootableObjectData> lootableObjects { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(tileSheetPath))
				errorMessages.Add(new Tuple<string, string>("Error: tileSheetPath", $"tileSheetPath for {Id} must exist and must not be empty."));

			return errorMessages.Count > 0;
		}
	}
}



