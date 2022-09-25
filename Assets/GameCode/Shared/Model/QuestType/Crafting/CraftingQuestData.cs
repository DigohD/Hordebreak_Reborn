using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.Crafting
{

	[XmlType("CraftingQuestData")]
	public class CraftingQuestData : QuestTypeData
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}