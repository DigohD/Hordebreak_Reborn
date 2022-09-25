using FNZ.Client.View.UI.Notifications;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Misc;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Net;
using Lidgren.Network;

namespace FNZ.Client.Net.NetworkManager
{
	public class ClientNotificationNetworkManager : INetworkManager
	{
		public ClientNotificationNetworkManager()
		{
			GameClient.NetConnector.Register(NetMessageType.NOTIFICATION, OnNotificationReceived);
		}

		private void OnNotificationReceived(ClientNetworkConnector net, NetIncomingMessage incMsg)
		{
			ushort spriteIdCode = incMsg.ReadUInt16();
			ushort colorIdCode = incMsg.ReadUInt16();
			bool isPermanent = incMsg.ReadBoolean();
			string message = incMsg.ReadString();
			float notificationId = incMsg.ReadFloat();

			var spriteId = IdTranslator.Instance.GetId<SpriteData>(spriteIdCode);
			var spriteData = DataBank.Instance.GetData<SpriteData>(spriteId);

			var colorId = IdTranslator.Instance.GetId<ColorData>(colorIdCode);
			var colorData = DataBank.Instance.GetData<ColorData>(colorId);

			UI_Notification.Instance.ManageNotification(spriteData.Id, colorData.colorCode, isPermanent, message, notificationId);
		}
	}
}