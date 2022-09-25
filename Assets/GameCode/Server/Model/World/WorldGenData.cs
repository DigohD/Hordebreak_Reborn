using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Site;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Server.Model.World
{
	[XmlType("BiomeTileData")]
	public class BiomeTileData
	{
		[XmlElement("tileRef")]
		public string tileRef { get; set; }

		[XmlElement("height")]
		public float height { get; set; }
	}

	[XmlType("WorldGenData")]
	public class WorldGenData : DataDef
	{
		[XmlElement("heightInChunks")]
		public byte heightInChunks { get; set; }

		[XmlElement("widthInChunks")]
		public byte widthInChunks { get; set; }

		[XmlElement("chunkSize")]
		public byte chunkSize { get; set; }

		[XmlElement("octaves")]
		public byte octaves { get; set; }

		[XmlElement("roughness")]
		public float roughness { get; set; }

		[XmlElement("layerFrequency")]
		public float layerFrequency { get; set; }

		[XmlElement("layerWeight")]
		public float layerWeight { get; set; }

		[XmlArray("tilesInBiome")]
		[XmlArrayItem(typeof(BiomeTileData))]
		public List<BiomeTileData> tilesInBiome { get; set; }

		[XmlArray("sites")]
		[XmlArrayItem(typeof(SiteEntry))]
		public List<SiteEntry> sites { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = null;
			return false;
		}
	}
}

