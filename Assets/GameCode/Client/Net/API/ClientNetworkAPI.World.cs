using FNZ.Shared.Model.World;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.API
{
	public partial class ClientNetworkAPI
	{
		public void CMD_World_RequestWorldSpawn()
		{
			var message = m_WorldMessageFactory.CreateRequestWorldSpawnMessage();
			Command(message);
		}

		public void CMD_World_ConfirmChunkLoaded(WorldChunk chunk)
		{
			var message = m_WorldMessageFactory.CreateConfirmChunkLoadedMessage(chunk);
			Command(message);
		}

		// public void CMD_World_ConfirmChunkUnloaded(WorldChunk chunk)
		// {
		// 	var message = m_WorldMessageFactory.CreateConfirmChunkUnloadedMessage(chunk);
		// 	Command(message);
		// }

		public void CMD_World_BaseRoomNameChange(bool isbase, long id, string newName)
		{
			var message = m_WorldMessageFactory.CreateBaseRoomNameChangeMessage(isbase, id, newName);
			Command(message);
		}

		public void CMD_Server_Ping_Response()
		{
			var sendBuffer = m_NetClient.CreateMessage(1);
			sendBuffer.Write((byte)NetMessageType.SERVER_PING_CHECK);

			var message = new NetMessage
			{
				Buffer = sendBuffer,
				Type = NetMessageType.SERVER_PING_CHECK,
				DeliveryMethod = NetDeliveryMethod.ReliableOrdered,
				Channel = SequenceChannel.PLAYER_STATE,
			};

			Command(message);
		}

	}
}