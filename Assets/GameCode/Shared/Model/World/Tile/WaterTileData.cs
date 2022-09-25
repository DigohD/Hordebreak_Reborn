using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.World.Tile 
{
	[XmlType("WaterTileData")]
	public class WaterTileData
	{
		[XmlElement("edgeMeshRef")]
		public string edgeMeshRef { get; set; }

		[XmlElement("tileHeightOffset")]
		public float tileHeightOffset { get; set; }
	}
}