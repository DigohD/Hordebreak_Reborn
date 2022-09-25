using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.Room
{

	public class ConstructRoomQuest : Quest
	{
		public ConstructRoomQuestData Data => (ConstructRoomQuestData)m_Data.questTypeData;
        
        public ConstructRoomQuest(string id) : base(id)
        {
            Progress = 0;
            m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
        }
    }
}