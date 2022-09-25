using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;


namespace FNZ.Shared.Model.Entity.Components.Spawner
{
	[XmlType("SpawnerComponentData")]
	public class SpawnerComponentData : DataComponent
	{
		[XmlElement("spawnInterval")]
		public float spawnInterval = 1;

		[XmlElement("spawnAmount")]
		public int spawnAmount = 1;

		[XmlElement("enemyRef")]
		public string enemyRef = "default_zombie";

		public override Type GetComponentType()
		{
			return typeof(SpawnerComponentShared);
		}

		public override bool ValidateComponentXMLData(List<Tuple<string, string>> errorMessages, string parentId, string fileName)
		{
			return true;
		}
	}
}