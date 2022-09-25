using FNZ.Client.View.Player.Systems;
using FNZ.Shared.Model.Entity;

namespace FNZ.Client.Net.API
{
	public partial class ClientNetworkAPI
	{
		public void CMD_Player_UpdatePosAndRot(FNEEntity player)
		{
			var message = m_PlayerMessageFactory.CreatePlayerPositionAndRotationUpdateMessage(player);
			Command(message, false);
		}

		public void CMD_Player_animationEvent(FNEEntity player, OneShotAnimationType animationType)
		{
			var message = m_PlayerMessageFactory.CreatePlayerAnimationEventMessage(player, animationType);
			Command(message);
		}

	}
}