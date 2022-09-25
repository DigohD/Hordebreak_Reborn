using System.Xml.Serialization;

namespace FNZ.Shared.Model.World.Site 
{

	[XmlType("EnemySpawnData")]
	public class EnemySpawnData
	{
		[XmlElement("enemyRef")]
		public string enemyRef { get; set; }

		[XmlElement("weight")]
		public ushort weight { get; set; }
	}
}