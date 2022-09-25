using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.Excavate
{

	public class ExcavateItemsQuest : Quest
	{
		public ExcavateItemsQuestData Data => (ExcavateItemsQuestData)m_Data.questTypeData;

		public ExcavateItemsQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}