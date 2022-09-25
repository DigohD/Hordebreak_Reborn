using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.QuestType.Addon 
{

	[XmlType("BuildingAddonQuestData")]
	public class BuildingAddonQuestData : QuestTypeData
	{
		[XmlElement("buildingRef")]
		public string buildingRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}