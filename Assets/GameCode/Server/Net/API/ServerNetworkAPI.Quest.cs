using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Net.API 
{

	public partial class ServerNetworkAPI
	{
		public void Quest_SendQuestUpdateToPlayers(string questId, int questProgress, float2[] mapHighlights)
		{
			var message = m_QuestMessageFactory.CreateQuestUpdateMessage(
				questId, 
				questProgress,
				mapHighlights
			);
			Broadcast_All(message);
		}
	}
}