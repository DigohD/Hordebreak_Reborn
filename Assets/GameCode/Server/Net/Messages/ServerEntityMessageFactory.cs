using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto;
using Lidgren.Network;
using System.Collections.Generic;

namespace FNZ.Server.Net.Messages
{
	internal class ServerEntityMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerEntityMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateSpawnEntityMessage(FNEEntity toSpawn)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + 2 + 4 + 4 + 4 + toSpawn.GetComponentDataSizeInBytes());

			sendBuffer.Write((byte)NetMessageType.SPAWN_ENTITY);
			sendBuffer.Write(toSpawn.NetId);
			sendBuffer.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(toSpawn.Data.Id));

			sendBuffer.Write(toSpawn.Position.x);
			sendBuffer.Write(toSpawn.Position.y);
			sendBuffer.Write(toSpawn.RotationDegrees);

			foreach (var component in toSpawn.Components)
			{
				component.Serialize(sendBuffer);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_ENTITY,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateSpawnEntityBatchMessage(FNEEntity[] toSpawn)
		{
			int entitiesComponentsSize = 0;
			foreach (var entity in toSpawn)
				entitiesComponentsSize += entity.GetComponentDataSizeInBytes();

			var sendBuffer = m_NetServer.CreateMessage(1 + 2 + (18 * toSpawn.Length) + entitiesComponentsSize);

			sendBuffer.Write((byte)NetMessageType.SPAWN_ENTITY_BATCH);
			sendBuffer.Write((ushort)toSpawn.Length);
			foreach (var entity in toSpawn)
			{
				sendBuffer.Write(entity.NetId);
				sendBuffer.Write(IdTranslator.Instance.GetIdCode<FNEEntityData>(entity.Data.Id));

				sendBuffer.Write(entity.Position.x);
				sendBuffer.Write(entity.Position.y);
				sendBuffer.Write(entity.RotationDegrees);

				foreach (var component in entity.Components)
				{
					component.Serialize(sendBuffer);
				}
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateUpdateEntityMessage(FNEEntity parent)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + parent.GetComponentDataSizeInBytes());

			sendBuffer.Write((byte)NetMessageType.UPDATE_ENTITY);
			sendBuffer.Write(parent.NetId);

			foreach (var component in parent.Components)
			{
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

		public NetMessage CreateUpdatePosAndRotMessage(FNEEntity entity)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 16);

			sendBuffer.Write((byte)NetMessageType.UPDATE_ENTITY_POS_AND_ROT);

			sendBuffer.Write(entity.NetId);

			sendBuffer.Write(entity.Position.x);
			sendBuffer.Write(entity.Position.y);
			sendBuffer.Write(entity.RotationDegrees);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_ENTITY_POS_AND_ROT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateUpdatePosAndRotBatchMessage(FNEEntity[] entities)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 2 + (16 * entities.Length));

			sendBuffer.Write((byte)NetMessageType.UPDATE_ENTITY_POS_AND_ROT_BATCH);
			sendBuffer.Write((ushort)entities.Length);

			foreach (var entity in entities)
			{
				sendBuffer.Write(entity.NetId);

				sendBuffer.Write(entity.Position.x);
				sendBuffer.Write(entity.Position.y);
				sendBuffer.Write(entity.RotationDegrees);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_ENTITY_POS_AND_ROT_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateUpdateComponentMessage(FNEComponent component)
		{
			int componentDataSizeInBytes = component.GetSizeInBytes();

			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + 2 + componentDataSizeInBytes);

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

		public NetMessage CreateUpdateComponentBatchMessage(FNEComponent[] components)
		{
			int componentBatchSizeInBytes = 0;
			foreach (var comp in components)
				componentBatchSizeInBytes += comp.GetSizeInBytes();

			var sendBuffer = m_NetServer.CreateMessage(1 + 1 + (4 * components.Length) + (2 * components.Length) + componentBatchSizeInBytes);

			sendBuffer.Write((byte)NetMessageType.UPDATE_COMPONENT_BATCH);
			sendBuffer.Write((ushort)components.Length);

			foreach (var comp in components)
			{
				if (comp.ParentEntity.NetId <= 0)
					UnityEngine.Debug.LogError($"NetId invalid for entity:{comp.ParentEntity.Data.Id}");
				sendBuffer.Write(comp.ParentEntity.NetId);
				sendBuffer.Write(FNEComponent.GetTypeIdOfComponent(comp));
				comp.Serialize(sendBuffer);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_COMPONENT_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateDestroyEntityMessage(FNEEntity toDestroy)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + 4);

			sendBuffer.Write((byte)NetMessageType.DESTROY_ENTITY);
			sendBuffer.Write(toDestroy.NetId);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.DESTROY_ENTITY,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateComponentNetEventMessage(FNEComponent component, byte eventId, IComponentNetEventData data = null)
		{
			var dataSize = data != null ? data.GetSizeInBytes() : 0;
			var sendBuffer = m_NetServer.CreateMessage(1 + 4 + 2 + 1 + dataSize);

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

		public NetMessage CreatePlayerServerConnectionErrorMessage(string errorName, string errorMessage)
		{
			var sendBuffer = m_NetServer.CreateMessage(1 + errorName.Length + errorMessage.Length);

			sendBuffer.Write((byte)NetMessageType.PLAYER_SERVER_CONNECTION_ERROR);
			sendBuffer.Write(errorName);
			sendBuffer.Write(errorMessage);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.PLAYER_SERVER_CONNECTION_ERROR,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ERROR_MESSAGE,
			};
		}
	}
}

