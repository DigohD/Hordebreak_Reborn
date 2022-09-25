using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Tile
{
	[XmlType("TileTimeEffectData")]
	public class TileTimeEffectData
	{
		[XmlElement("effectRef")]
		public string effectRef { get; set; }

		[XmlElement("weight")]
		public byte weight { get; set; }

		[XmlElement("centerTime")]
		public byte centerTime { get; set; }

		[XmlElement("timeOffset")]
		public byte timeOffset { get; set; }
	}
}
