using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.Building
{

	[XmlType("BuildingQuestData")]
	public class BuildingQuestData : QuestTypeData
	{
		[XmlElement("buildingRef")]
		public string buildingRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}