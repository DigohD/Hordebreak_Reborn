using System.Collections;
using System.Collections.Generic;
using System.IO;
using FNZ.Shared.Model;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Net;
using Lidgren.Network;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Net.Messages 
{

	public class ServerQuestMessageFactory
	{
		
		private readonly NetServer m_NetServer;

		public ServerQuestMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}
		
		public NetMessage CreateQuestUpdateMessage(
			string questId, 
			int questProgress,
			float2[] mapHighlights
		)
		{
			var sendBuffer = m_NetServer.CreateMessage(6);

			sendBuffer.Write((byte)NetMessageType.QUEST_UPDATE);
			sendBuffer.Write(!string.IsNullOrEmpty(questId));
			if (!string.IsNullOrEmpty(questId))
			{
				sendBuffer.Write(IdTranslator.Instance.GetIdCode<QuestData>(questId));
				sendBuffer.Write(questProgress);
				
				sendBuffer.Write(mapHighlights.Length);
				for (int i = 0; i < mapHighlights.Length; i++)
				{
					sendBuffer.Write(mapHighlights[i].x);
					sendBuffer.Write(mapHighlights[i].y);
				}
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.QUEST_UPDATE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.QUEST,
			};
		}
	}
}