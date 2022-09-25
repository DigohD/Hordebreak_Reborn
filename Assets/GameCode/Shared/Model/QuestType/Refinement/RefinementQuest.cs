using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.Refinement
{

	public class RefinementQuest : Quest
	{
		public RefinementQuestData Data => (RefinementQuestData)m_Data.questTypeData;
		
		public RefinementQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}