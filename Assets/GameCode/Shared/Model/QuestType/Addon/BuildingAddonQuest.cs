using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Shared.Model.QuestType.Addon 
{

	public class BuildingAddonQuest : Quest
	{
		public BuildingAddonQuestData Data => (BuildingAddonQuestData) m_Data.questTypeData;

		public BuildingAddonQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}