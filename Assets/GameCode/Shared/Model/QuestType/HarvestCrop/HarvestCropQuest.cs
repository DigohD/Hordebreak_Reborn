using FNZ.Shared.Utils;

namespace FNZ.Shared.Model.QuestType.HarvestCrop
{

	public class HarvestCropQuest : Quest
	{
		public HarvestCropQuestData Data => (HarvestCropQuestData)m_Data.questTypeData;

		public HarvestCropQuest(string id) : base(id)
		{
			Progress = 0;
			m_Identifier = FNERandom.GetRandomFloatInRange(0, 1);
		}
	}
}