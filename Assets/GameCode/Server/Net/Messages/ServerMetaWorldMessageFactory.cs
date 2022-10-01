using FNZ.Server.Model.MetaWorld;
using FNZ.Shared.Net;
using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Server.Net.Messages 
{

	public class ServerMetaWorldMessageFactory : MonoBehaviour
	{
		private readonly NetServer m_NetServer;

		public ServerMetaWorldMessageFactory(NetServer netServer)
		{
			m_NetServer = netServer;
		}

		public NetMessage CreateServerMetaWorldMessage(
			List<Place> places
		)
		{
			var totalSize = 0;
			foreach (Place place in places)
            {
				totalSize += place.GetByteSize();
            }

			var sendBuffer = m_NetServer.CreateMessage(totalSize);

			sendBuffer.Write((byte)NetMessageType.META_WORLD_UPDATE);

			sendBuffer.Write(places.Count);
			for (int i = 0; i < places.Count; i++)
			{
				places[i].Serialize(sendBuffer);
			}

			return new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.META_WORLD_UPDATE,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.WORLD_MAP_STATE,
			};
		}
	}
}