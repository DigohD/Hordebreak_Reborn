using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Server.Net.NetworkManager
{
	public class ServerPlayerNetworkManager : INetworkManager
	{
		public ServerPlayerNetworkManager()
		{
			GameServer.NetConnector.Register(NetMessageType.UPDATE_PLAYER_POS_AND_ROT, OnPlayerPosAndRotUpdate);
			GameServer.NetConnector.Register(NetMessageType.PLAYER_ANIMATION_EVENT, OnPlayerAnimationEvent);
		}

		private void OnPlayerPosAndRotUpdate(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			FNEEntity playerEntity = net.GetEntity(incMsg.ReadInt32());

			playerEntity.DeserializePosition(incMsg);
			playerEntity.DeserializeRotation(incMsg);

			// var currentChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(playerEntity.Position);
			// var playerComp = playerEntity.GetComponent<PlayerComponentServer>();
			//
			// if (currentChunk != playerComp.LastChunk)
			// {
			// 	GameServer.ChunkManager.OnPlayerEnteringNewChunk(playerEntity);
			// 	playerComp.LastChunk = currentChunk;
			// }

			GameServer.NetAPI.Client_Immediate_Forward_To_Other(incMsg.Data, incMsg.SenderConnection);
		}

		private void OnPlayerAnimationEvent(ServerNetworkConnector net, NetIncomingMessage incMsg)
		{
			GameServer.NetAPI.Client_Immediate_Forward_To_Other(incMsg.Data, incMsg.SenderConnection);
		}
	}
}