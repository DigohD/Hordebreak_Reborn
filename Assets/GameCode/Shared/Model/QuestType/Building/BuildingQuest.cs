using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.Building
{

	public class BuildingQuest : Quest
	{
		public BuildingQuestData Data => (BuildingQuestData)m_Data.questTypeData;
		
		public BuildingQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}