using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.Entity.Components.Excavatable 
{

	[XmlType("ExcavatableBonusData")]
	public class ExcavatableBonusData
	{
		[XmlElement("chance")]
		public float chance { get; set; } = 100;

		[XmlElement("bonusTime")]
		public float bonusTime { get; set; }

		[XmlElement("LootTable")]
		public LootTableData lootTable { get; set; }

		[XmlElement("colorRef")]
		public string colorRef { get; set; }
	}
}