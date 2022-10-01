using FNZ.Server.Model.MetaWorld;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Server.Net.API 
{

	public partial class ServerNetworkAPI
	{
		public void MetaWorld_Update_STC(NetConnection clientConnection)
		{
			var message = m_MetaWorldMessageFactory.CreateServerMetaWorldMessage(
				GameServer.MetaWorld.Places
			);
			SendToClient(message, clientConnection);
		}

		public void MetaWorld_Update_BA()
		{
			var message = m_MetaWorldMessageFactory.CreateServerMetaWorldMessage(
				GameServer.MetaWorld.Places
			);
			Broadcast_All(message);
		}
	}
}