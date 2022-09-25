using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType.HarvestCrop
{

	[XmlType("HarvestCropQuestData")]
	public class HarvestCropQuestData : QuestTypeData
	{
		[XmlElement("itemRef")]
		public string itemRef { get; set; }

		[XmlElement("amount")]
		public int amount { get; set; }
	}
}