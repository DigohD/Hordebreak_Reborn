using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Client.Net.Messages
{

	public class ClientEffectMessageFactory
	{
		private readonly NetClient m_NetClient;

		public ClientEffectMessageFactory(NetClient netClient)
		{
			m_NetClient = netClient;
		}

		public NetMessage CreateSpawnProjectileMessage(string id, float2 position, float rotationZDegrees, string[] modItemIds)
		{
			// 1 + 2 + 4 + 4 + 2 + 1 + (3 * 2)
			var sendBuffer = m_NetClient.CreateMessage(20);

			sendBuffer.Write((byte)NetMessageType.SPAWN_PROJECTILE);

			sendBuffer.Write(IdTranslator.Instance.GetIdCode<EffectData>(id));

			sendBuffer.Write(position.x);
			sendBuffer.Write(position.y);

			sendBuffer.Write(FNEUtil.PackFloatAsShort(rotationZDegrees));

			sendBuffer.Write(modItemIds != null);
			if(modItemIds != null)
			{
				sendBuffer.Write((byte)modItemIds.Length);
				foreach (var modId in modItemIds)
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<ItemData>(modId));
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_PROJECTILE,
				DeliveryMethod = NetDeliveryMethod.ReliableUnordered,
				Channel = SequenceChannel.DEFAULT,
			};
		}

		public NetMessage CreateSpawnProjectileMessageBatch(string id, List<float2> positions, List<float> finalZs, string[] modItemIds)
		{
			var sendBuffer = m_NetClient.CreateMessage(1 + 1 + 2 + 10 * positions.Count);

			sendBuffer.Write((byte)NetMessageType.SPAWN_PROJECTILE_BATCH);
			sendBuffer.Write((byte)positions.Count);

			sendBuffer.Write(IdTranslator.Instance.GetIdCode<EffectData>(id));

			sendBuffer.Write(modItemIds != null);
			if (modItemIds != null)
			{
				sendBuffer.Write((byte)modItemIds.Length);
				foreach (var modId in modItemIds)
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<ItemData>(modId));
			}

			for (int i = 0; i < positions.Count; i++)
			{
				sendBuffer.Write(positions[i].x);
				sendBuffer.Write(positions[i].y);

				sendBuffer.Write(FNEUtil.PackFloatAsShort(finalZs[i]));
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_PROJECTILE_BATCH,
				DeliveryMethod = NetDeliveryMethod.ReliableUnordered,
				Channel = SequenceChannel.DEFAULT,
			};

		}
	}
}