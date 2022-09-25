using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace FNZ.Shared.Model.QuestType.Event 
{
	[XmlType("EventQuestData")]
	public class EventQuestData : QuestTypeData
	{
		[XmlElement("eventRef")]
		public string eventRef { get; set; }
	}
}