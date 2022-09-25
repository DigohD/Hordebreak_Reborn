using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Tile
{
	[XmlType("TileObjectGenData")]
	public class TileObjectGenData : DataDef
	{
		[XmlElement("objectRef")]
		public string objectRef { get; set; }

		[XmlElement("weight")]
		public int weight { get; set; }

		[XmlElement("tileObjectGenClusterData")]
		public TileObjectGenClusterData clusterData { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (string.IsNullOrEmpty(objectRef))
				errorMessages.Add(new Tuple<string, string>("objectRef", $"objectRef for {Id} must exist and must not be empty."));

			return errorMessages.Count > 0;
		}
	}
}