using FNZ.Shared.Model.Entity;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		/// <summary>
		///		Net API module: Player
		///		NetMessage Type: SpawnRemotePlayer
		///		Send method: STC (Send To Client)
		/// 
		/// Signals for a specific client that another player should be spawned.
		/// </summary>
		/// <param name="player">Player Entity for the spawning player.</param>
		/// <param name="clientConnection">The client connection of the player whose entity has been spawned.</param>
		public void Player_SpawnRemote_STC(FNEEntity player, NetConnection clientConnection)
		{
			var message = m_PlayerMessageFactory.CreateSpawnRemotePlayerMessage(player);
			SendToClient(message, clientConnection);
		}

		public void Player_RemoveRemote_BA(FNEEntity player)
		{
			var message = m_PlayerMessageFactory.CreateRemoveRemotePlayerMessage(player);
			Broadcast_All(message);
		}

		/// <summary>
		///		Net API module: Player
		///		NetMessage Type: SpawnRemotePlayer
		///		Send method: BO (Broadcast Other)
		/// 
		/// Signals for all clients except excluded client that another player should be spawned.
		/// </summary>
		/// <param name="player">Player Entity for the spawning player.</param>
		/// <param name="clientConnection">The client connection of the player whose entity has been spawned.</param>
		public void Player_SpawnRemote_BO(FNEEntity player, NetConnection clientConnection)
		{
			var message = m_PlayerMessageFactory.CreateSpawnRemotePlayerMessage(player);
			Broadcast_Other(message, clientConnection);
		}

		/// <summary>
		///		Net API module: Player
		///		NetMessage Type: SpawnLocalPlayer
		///		Send method: STC (Send To Client)
		/// 
		/// Signals for a specific client that its corresponding player object has been
		/// spawned on the server, and prompts the client to do the same.
		/// </summary>
		/// <param name="player">Player Entity for the given player.</param>
		/// <param name="clientConnection">The client connection of the player whose entity has been spawned.</param>
		public void Player_SpawnLocal_STC(FNEEntity player, NetConnection clientConnection)
		{
			var message = m_PlayerMessageFactory.CreateSpawnLocalPlayerMessage(player);
			SendToClient(message, clientConnection);
		}

		public void Player_Teleport_STC(NetConnection clientConnection, float2 destination)
		{
			var message = m_PlayerMessageFactory.CreateTeleportPlayerMessage(destination);
			SendToClient(message, clientConnection);
		}

	}
}