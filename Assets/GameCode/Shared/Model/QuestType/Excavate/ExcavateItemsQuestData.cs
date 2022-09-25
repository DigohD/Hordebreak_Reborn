using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.Excavate
{

	[XmlType("ExcavateItemsQuestData")]
	public class ExcavateItemsQuestData : QuestTypeData
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}