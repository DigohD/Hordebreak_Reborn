using FNZ.Client.Net.Messages;
using FNZ.Shared.Net;
using Lidgren.Network;
using UnityEngine;

namespace FNZ.Client.Net.API
{
	public partial class ClientNetworkAPI
	{
		private readonly NetClient m_NetClient;

		// All network modules 
		private readonly ClientWorldMessageFactory m_WorldMessageFactory;
		private readonly ClientEntityMessageFactory m_EntityMessageFactory;
		private readonly ClientPlayerMessageFactory m_PlayerMessageFactory;
		private readonly ClientChatMessageFactory m_ChatMessageFactory;
		private readonly ClientEffectMessageFactory m_EffectMessageFactory;

		public ClientNetworkAPI(NetClient netClient)
		{
			m_NetClient = netClient;

			m_WorldMessageFactory = new ClientWorldMessageFactory(netClient);
			m_EntityMessageFactory = new ClientEntityMessageFactory(netClient);
			m_PlayerMessageFactory = new ClientPlayerMessageFactory(netClient);
			m_ChatMessageFactory = new ClientChatMessageFactory(netClient);
			m_EffectMessageFactory = new ClientEffectMessageFactory(netClient);
		}

		private void Command(NetMessage message, bool shouldLog = true)
		{
			var result = m_NetClient.SendMessage(
				message.Buffer,
				message.DeliveryMethod,
				(int)message.Channel
			);

			//HandleSendResult(result, message.Type, shouldLog);
		}

		private void HandleSendResult(NetSendResult result, NetMessageType messageType, bool shouldLog)
		{
			string messageTypeName = messageType.ToString();

			switch (result)
			{
				case NetSendResult.FailedNotConnected:
					Debug.LogError("[Client] Message failed to enqueue because there is no connection, CommandType: " + messageTypeName);
					break;
				case NetSendResult.Sent:
					if (shouldLog)
						Debug.LogWarning("[Client] Message was immediately sent, CommandType: " + messageTypeName);
					break;
				case NetSendResult.Queued:
					Debug.LogWarning("[Client] Message was queued for delivery, CommandType: " + messageTypeName);
					break;
				case NetSendResult.Dropped:
					Debug.LogWarning("[Client] Message was dropped immediately since too many message were queued, CommandType: " + messageTypeName);
					break;
			}
		}
	}
}

