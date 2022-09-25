using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Shared.Model.QuestType.Event 
{

	public class EventQuest : Quest
	{
		public EventQuestData Data => (EventQuestData) m_Data.questTypeData;
		
		public EventQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}