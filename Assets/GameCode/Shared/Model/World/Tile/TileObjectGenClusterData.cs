using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Tile
{
	[XmlType("TileObjectGenClusterData")]
	public class TileObjectGenClusterData
	{
		[XmlElement("radius")]
		public float radius { get; set; }

		[XmlElement("density")]
		public byte density { get; set; }
	}
}