using FNZ.Client.Model;
using FNZ.Client.Model.World;
using FNZ.Client.Net;
using FNZ.Client.Net.API;
using FNZ.Client.StaticData;
using FNZ.Client.View.World;
using FNZ.Shared;
using FNZ.Shared.Net;
using Lidgren.Network;
using Unity.Entities;

namespace FNZ.Client.Systems
{
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public class NetworkClientSystem : SystemBase
	{
		private NetClient m_Client;
		private ClientNetworkConnector m_NetConnector;

		protected override void OnCreate()
		{
			base.OnCreate();

			var config = new NetPeerConfiguration(SharedConfigs.AppIdentifier);
			m_Client = new NetClient(config);

			GameClient.NetAPI = new ClientNetworkAPI(m_Client);
			
			m_NetConnector = GameClient.NetConnector;
			m_NetConnector.Initialize();
			GameClient.World = new ClientWorld();
			GameClient.WorldView = new ClientWorldView(GameClient.World);

			GameClient.EntityFactory = new ClientEntityFactory(GameClient.World);

			m_Client.Start();

			NetOutgoingMessage approval = m_Client.CreateMessage();
			approval.Write("secret");

			NetData.IP_ADRESS = NetData.IP_ADRESS == null ? "localhost" : NetData.IP_ADRESS;
			NetData.PORT = NetData.PORT == 0 ? 7676 : NetData.PORT;

			m_Client.Connect(
				NetData.IP_ADRESS,
				NetData.PORT,
				approval
			);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			m_Client.Shutdown("Exit");
		}

		protected override void OnUpdate()
		{
			NetIncomingMessage incMsg;

			while ((incMsg = m_Client.ReadMessage()) != null)
			{
				switch (incMsg.MessageType)
				{
					case NetIncomingMessageType.Error:
						break;
					case NetIncomingMessageType.ConnectionApproval:
						break;
					case NetIncomingMessageType.Data:
						ParsePacket(incMsg);
						break;
					case NetIncomingMessageType.VerboseDebugMessage:
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.ErrorMessage:
						break;
					case NetIncomingMessageType.StatusChanged:
						switch (incMsg.SenderConnection.Status)
						{
							case NetConnectionStatus.Connected:
								OnConnected();
								break;
							case NetConnectionStatus.Disconnected:

								break;
						}
						break;
					default:

						break;
				}
			}
		}

		private void ParsePacket(NetIncomingMessage incMsg)
		{
			m_NetConnector.Dispatch((NetMessageType)incMsg.ReadByte(), incMsg);
			m_Client.Recycle(incMsg);
		}

		private void OnConnected()
		{

		}
	}
}


