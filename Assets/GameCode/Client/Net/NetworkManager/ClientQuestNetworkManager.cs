using System.Collections;
using System.Collections.Generic;
using FNZ.Client.View.UI.WorldMap;
using FNZ.Shared.Model;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Net;
using GameCode.Client.Net.NetworkManager;
using Lidgren.Network;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.Net.NetworkManager 
{

	public delegate void OnQuestUpdateReceived(
		string questId,
		int questProgress,
		float2[] mapHighlights
	);
	
	public class ClientQuestNetworkManager : INetworkManager
	{
		public ClientQuestNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.QUEST_UPDATE, OnQuestUpdateReceived);
		}

		public static OnQuestUpdateReceived d_OnQuestUpdateReceived;
		
		private void OnQuestUpdateReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			var isQuest = incMsg.ReadBoolean();
			if (isQuest)
			{
				var questId = IdTranslator.Instance.GetId<QuestData>(incMsg.ReadUInt16());
				var progress = incMsg.ReadInt32();

				int count = incMsg.ReadInt32();
				float2[] mapHighlights = new float2[count];
				for (int i = 0; i < count; i++)
				{
					mapHighlights[i] = new float2(incMsg.ReadFloat(), incMsg.ReadFloat());
				}

				WorldMapUI.m_MapHighlights = mapHighlights;
				
				d_OnQuestUpdateReceived?.Invoke(questId, progress, mapHighlights);
			}
			else
			{
				d_OnQuestUpdateReceived?.Invoke("", 0, new float2[0]);
			}
		}
	}
}