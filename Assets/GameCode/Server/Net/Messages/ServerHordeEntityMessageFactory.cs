using FNZ.Shared.Net;
using FNZ.Shared.Net.Dto.Hordes;
using Lidgren.Network;
using System.Collections.Generic;

namespace FNZ.Server.Net.Messages 
{
	internal class ServerHordeEntityMessageFactory
	{
		readonly NetServer m_NetServer;

		public ServerHordeEntityMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateHordeEntityAttackTargetMessage(List<HordeEntityAttackTargetData> attackers)
		{
			var attackBatchData = new HordeEntityAttackTargetBatchData
			{
				Entities = attackers
			};

			var sendBuffer = m_NetServer.CreateMessage(attackBatchData.GetSizeInBytes());
			sendBuffer.Write((byte)NetMessageType.ATTACK_ENTITY);
			attackBatchData.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.ATTACK_ENTITY,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.ENTITY_STATE,
			};
		}

		public NetMessage CreateSpawnHordeEntityBatchMessage(List<HordeEntitySpawnData> hordeEntitiesToSpawn)
        {
			var spawnBatchData = new HordeEntitySpawnBatchData
			{
				Count = hordeEntitiesToSpawn.Count,
				Entities = hordeEntitiesToSpawn
			};

			var sendBuffer = m_NetServer.CreateMessage(spawnBatchData.GetSizeInBytes(true));
			spawnBatchData.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_HORDE_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateDestroyHordeEntityBatchMessage(List<HordeEntityDestroyData> entitiesToDestroyy)
		{
			var destroyBatchData = new HordeEntityDestroyBatchNetData
			{
				Count = entitiesToDestroyy.Count,
				Entities = entitiesToDestroyy
			};

			var sendBuffer = m_NetServer.CreateMessage(destroyBatchData.GetSizeInBytes());
			destroyBatchData.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.DESTROY_HORDE_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_STATE,
			};
		}

		public NetMessage CreateUpdateHordeEntityBatchMessage(List<HordeEntityUpdateNetData> hordeEntitiesToUpdate)
		{
			var spawnBatchData = new HordeEntityUpdateBatchNetData
			{
				Count = hordeEntitiesToUpdate.Count,
				Entities = hordeEntitiesToUpdate
			};

			var sendBuffer = m_NetServer.CreateMessage(spawnBatchData.GetSizeInBytes());
			spawnBatchData.NetSerialize(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_HORDE_ENTITY_BATCH,
				DeliveryMethod = NetDeliveryMethod.Unreliable,
				Channel = SequenceChannel.DEFAULT,
			};
		}
	}
}