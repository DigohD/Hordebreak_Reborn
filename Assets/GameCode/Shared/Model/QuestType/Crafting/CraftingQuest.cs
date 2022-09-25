using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.Crafting
{

	public class CraftingQuest : Quest
	{
		public CraftingQuestData Data => (CraftingQuestData)m_Data.questTypeData;
		
		public CraftingQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}