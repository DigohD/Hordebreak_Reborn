using FNZ.Shared.Model.World;
using Lidgren.Network;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		public void BA_Spawn_Item_On_Ground(ItemOnGround toSpawn)
		{
			var message = m_ItemOnGroundMessageFactory.CreateSpawnItemOnGroundMessage(toSpawn);
			Broadcast_All(message);
		}

		public void STC_Spawn_Item_On_Ground(ItemOnGround toSpawn, NetConnection clientConnection)
		{
			var message = m_ItemOnGroundMessageFactory.CreateSpawnItemOnGroundMessage(toSpawn);
			SendToClient(message, clientConnection);
		}

		public void BA_Remove_Item_On_Ground(long identifier)
		{
			var message = m_ItemOnGroundMessageFactory.CreateRemoveItemOnGroundMessage(identifier);
			Broadcast_All(message);
		}
	}
}