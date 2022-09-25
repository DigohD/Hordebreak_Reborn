using FNZ.Client.View.Player.Systems;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.Messages
{

	internal class ClientPlayerMessageFactory
	{
		private readonly NetClient m_NetClient;

		public ClientPlayerMessageFactory(NetClient netClient)
		{
			m_NetClient = netClient;
		}

		public NetMessage CreatePlayerPositionAndRotationUpdateMessage(FNEEntity player)
		{
			var sendBuffer = m_NetClient.CreateMessage(10);

			sendBuffer.Write((byte)NetMessageType.UPDATE_PLAYER_POS_AND_ROT);

			sendBuffer.Write(player.NetId);
			player.SerializePosition(sendBuffer);
			player.SerializeRotation(sendBuffer);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.UPDATE_PLAYER_POS_AND_ROT,
				DeliveryMethod = NetDeliveryMethod.Unreliable,
				Channel = SequenceChannel.DEFAULT,
			};
		}

		public NetMessage CreatePlayerAnimationEventMessage(FNEEntity player, OneShotAnimationType animType)
		{
			var sendBuffer = m_NetClient.CreateMessage();

			sendBuffer.Write((byte)NetMessageType.PLAYER_ANIMATION_EVENT);
			sendBuffer.Write(player.NetId);
			sendBuffer.Write((byte)animType);

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.PLAYER_ANIMATION_EVENT,
				DeliveryMethod = NetDeliveryMethod.Unreliable,
				Channel = SequenceChannel.DEFAULT,
			};
		}

	}
}