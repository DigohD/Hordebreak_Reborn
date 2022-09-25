using System.Collections.Generic;
using Lidgren.Network;

namespace FNZ.Shared.Model.QuestType
{
	public class Quest
	{
		public QuestData m_Data;
		public float m_Identifier;
		public int Progress;
		
		public Quest(string id)
		{
			m_Data = DataBank.Instance.GetData<QuestData>(id);
		}

		public Quest()
		{
		}

		public void Serialize(NetBuffer writer)
		{
			writer.Write(IdTranslator.Instance.GetIdCode<QuestData>(m_Data.Id));
		}
		
		public void Deserialize(NetBuffer reader)
		{
			var idCode = reader.ReadUInt16();
			m_Data = DataBank.Instance.GetData<QuestData>(IdTranslator.Instance.GetId<QuestData>(idCode));
		}

		public QuestData GetData()
		{
			return m_Data;
		}

		public string GetFollowupQuestRef()
		{
			return m_Data.followingQuestRef;
		}

		public List<string> GetBuildingUnlocks()
		{
			return m_Data.buildingUnlockRefs;
		}
	}
}