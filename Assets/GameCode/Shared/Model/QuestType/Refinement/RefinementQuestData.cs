using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.Refinement
{

	[XmlType("RefinementQuestData")]
	public class RefinementQuestData : QuestTypeData
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}