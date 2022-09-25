using FNZ.Shared.Model.Building;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FNZ.Shared.Model.QuestType
{
	[XmlType("QuestData")]
	public class QuestData : DataDef
	{
		[XmlElement("titleRef")]
		public string titleRef { get; set; }
		
		[XmlElement("descriptionRef")]
		public string descriptionRef { get; set; }

		[XmlElement("followingQuestRef")]
		public string followingQuestRef { get; set; }

		[XmlElement("questForkRef")]
		public string questForkRef { get; set; }

		[XmlElement("questTypeData")]
		public QuestTypeData questTypeData { get; set; }

		[XmlArray("buildingUnlockRefs")]
		[XmlArrayItem("unlockRef")]
		public List<string> buildingUnlockRefs { get; set; }

		public override bool ValidateXMLData(out List<Tuple<string, string>> errorMessages)
		{
			errorMessages = new List<Tuple<string, string>>();

			if (followingQuestRef != null && !DataBank.Instance.DoesIdExist<QuestData>(followingQuestRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'followingQuestRef' for {Id} did not exist in quest databank."));

			if (questForkRef != null && !DataBank.Instance.DoesIdExist<QuestData>(questForkRef))
				errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XML entry 'questForkRef' for {Id} did not exist in quest databank."));


			if (buildingUnlockRefs != null && buildingUnlockRefs.Count > 0)
			{
				foreach (var building in buildingUnlockRefs)
				{
					if (!DataBank.Instance.DoesIdExist<BuildingData>(building))
					{
						errorMessages.Add(new Tuple<string, string>($"Error: {fileName}", $"XmL entry 'buildingId': '{building}' was not found in building databank."));
					}
				}
			}

			return errorMessages.Count > 0;
		}
	}

	[XmlType("QuestTypeData")]
	public abstract class QuestTypeData
	{

	}
}