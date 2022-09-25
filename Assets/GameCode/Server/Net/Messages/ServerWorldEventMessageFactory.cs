using FNZ.Shared.Net;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.Messages 
{
	public class ServerWorldEventMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerWorldEventMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}
		
		public NetMessage CreateWorldEventMessage(INetSerializeableData eventData)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + eventData.GetSizeInBytes());

			sendBuffer.Write((byte)NetMessageType.WORLD_EVENT);
			eventData.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.WORLD_EVENT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
		
		public NetMessage CreateEndWorldEventMessage(long uniqueId, bool success)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 8 + 1);

			sendBuffer.Write((byte)NetMessageType.WORLD_EVENT_END);
			sendBuffer.Write(uniqueId);
			sendBuffer.Write(success);
			
			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.WORLD_EVENT_END,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
	}
}