using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Net;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.Messages
{
	internal class ServerEffectMessageFactory
	{
		private readonly NetServer m_NetServer;

		public ServerEffectMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateEffectMessage(string effectId, float2 location, float rotation)
		{
			var idCode = IdTranslator.Instance.GetIdCode<EffectData>(effectId);

			// 1 + 2 + 4 + 4 + 2
			var sendBuffer = m_NetServer.CreateMessage(13);
			sendBuffer.Write((byte)NetMessageType.SPAWN_EFFECT);
			sendBuffer.Write(idCode);
			sendBuffer.Write(location.x);
			sendBuffer.Write(location.y);
			sendBuffer.Write(FNEUtil.PackFloatAsShort(rotation));

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SPAWN_EFFECT,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.EFFECT,
			};
		}

        public NetMessage CreateSpawnProjectileMessage(string id, float2 position, float rotationZDegrees, string[] modItemIds, int ownerNetId)
        {
            // 1 + 2 + 4 + 4 + 2 + 4 + 1 + (3 * 2)
            var sendBuffer = m_NetServer.CreateMessage(24);

            sendBuffer.Write((byte)NetMessageType.SPAWN_PROJECTILE);

            sendBuffer.Write(IdTranslator.Instance.GetIdCode<EffectData>(id));

            sendBuffer.Write(position.x);
            sendBuffer.Write(position.y);

            sendBuffer.Write(FNEUtil.PackFloatAsShort(rotationZDegrees));
            sendBuffer.Write(ownerNetId);

			sendBuffer.Write(modItemIds != null);
			if(modItemIds != null)
			{
				sendBuffer.Write((byte)modItemIds.Length);
				foreach (var Id in modItemIds)
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<ItemData>(Id));
			}
			

			return new NetMessage
            {
                Buffer = sendBuffer,
                Type = NetMessageType.SPAWN_PROJECTILE,
                DeliveryMethod = NetDeliveryMethod.ReliableUnordered,
                Channel = SequenceChannel.DEFAULT,
            };
        }
    }
}