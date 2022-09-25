using System.Collections.Generic;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.Messages
{
	internal class ClientEntityMessageFactory
	{
		private readonly NetClient m_NetClient;

		public ClientEntityMessageFactory(NetClient netClient)
		{
			m_NetClient = netClient;
		}

		public NetMessage CreateUpdateHealthMessage(float amount, string damageTypeRef, bool isDamage)
		{
			var sendBuffer = m_NetClient.CreateMessage(1 + 4 + damageTypeRef.Length + 1);

			sendBuffer.Write((byte)NetMessageType.PLAYER_HEALTH_UPDATE);
			sendBuffer.Write(amount);
			sendBuffer.Write(damageTypeRef);
			sendBuffer.Write(isDamage);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.PLAYER_HEALTH_UPDATE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateUpdateComponentMessage(FNEComponent component)
		{
			int componentDataSizeInBytes = component.GetSizeInBytes();

			var sendBuffer = m_NetClient.CreateMessage(1 + 4 + 2 + componentDataSizeInBytes);

			sendBuffer.Write((byte)NetMessageType.UPDATE_COMPONENT);
			sendBuffer.Write(component.ParentEntity.NetId);
			sendBuffer.Write(FNEComponent.GetTypeIdOfComponent(component));

			component.Serialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_COMPONENT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateUpdateComponentsMessage(FNEEntity parent)
		{
			int componentDataSizeInBytes = 0;

			foreach (var component in parent.Components)
				componentDataSizeInBytes += component.GetSizeInBytes();

			var sendBuffer = m_NetClient.CreateMessage(1 + 1 + 4 + componentDataSizeInBytes);

			sendBuffer.Write((byte)NetMessageType.UPDATE_ENTITY);

			sendBuffer.Write((byte)parent.Components.Count);
			sendBuffer.Write(parent.NetId);

			foreach (var component in parent.Components)
			{
				// sendBuffer.Write(GetCompIdFromType(component.GetType()));
				component.Serialize(sendBuffer);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_ENTITY,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateComponentNetEventMessage(FNEComponent component, byte eventId, IComponentNetEventData data = null)
		{
			var dataLength = data != null ? data.GetSizeInBytes() : 0;
			var sendBuffer = m_NetClient.CreateMessage(1 + 4 + 2 + 1 + dataLength);

			sendBuffer.Write((byte)NetMessageType.COMPONENT_NET_EVENT);
			sendBuffer.Write(component.ParentEntity.NetId);
			sendBuffer.Write(FNEComponent.GetTypeIdOfComponent(component));
			sendBuffer.Write(eventId);
			if (data != null)
				data.Serialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.COMPONENT_NET_EVENT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateClientDisconnectsMessage()
		{
			var sendBuffer = m_NetClient.CreateMessage(1);

			sendBuffer.Write((byte)NetMessageType.CLIENT_LEFT_SERVER);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CLIENT_LEFT_SERVER,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateConfirmHordeEntityBatchSpawnedMessage(List<int> netIds)
		{
			var sendBuffer = m_NetClient.CreateMessage(1 + 2 + (netIds.Count * 4));
			
			sendBuffer.Write((byte)NetMessageType.CLIENT_CONFIRM_SPAWN_HORDE_ENTITY_BATCH);
			sendBuffer.Write((ushort)netIds.Count);
			
			foreach (var netId in netIds)
				sendBuffer.Write(netId);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CLIENT_CONFIRM_SPAWN_HORDE_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
		
		public NetMessage CreateConfirmHordeEntityBatchDestroyedMessage(List<int> netIds)
		{
			var sendBuffer = m_NetClient.CreateMessage(1 + 2 + (netIds.Count * 4));
			
			sendBuffer.Write((byte)NetMessageType.CLIENT_CONFIRM_DESTROY_HORDE_ENTITY_BATCH);
			sendBuffer.Write((ushort)netIds.Count);
			
			foreach (var netId in netIds)
				sendBuffer.Write(netId);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.CLIENT_CONFIRM_DESTROY_HORDE_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}
	}
}

