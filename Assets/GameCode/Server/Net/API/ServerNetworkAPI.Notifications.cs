using FNZ.Shared.Model;
using FNZ.Shared.Model.Misc;
using FNZ.Shared.Model.Sprites;
using FNZ.Shared.Utils;
using Lidgren.Network;

namespace FNZ.Server.Net.API
{
	public partial class ServerNetworkAPI
	{
		private bool m_IsPermanent;

		private ushort m_SpriteId;
		private ushort m_ColorId;

		private void ConvertStrings(string spriteId, string color, string isPermanent)
		{
			var spriteData = DataBank.Instance.GetData<SpriteData>(spriteId);
			if (spriteData != null)
				m_SpriteId = IdTranslator.Instance.GetIdCode<SpriteData>(spriteData.Id);
			else
				throw new System.Exception($"{spriteId} was not found in SpriteData.");

			var colorData = DataBank.Instance.GetData<ColorData>(color);
			if (colorData != null)
				m_ColorId = IdTranslator.Instance.GetIdCode<ColorData>(colorData.Id);
			else
				throw new System.Exception($"{color} was not found in ColorData.");

			m_IsPermanent = isPermanent.ToLower() == "true" ? true : false;
		}

		public void Notification_SendNotification_BA(string spriteId, string color, string isPermanent, string playerMessage, float identifier = 0)
		{
			ConvertStrings(spriteId, color, isPermanent);

			if (identifier == 0)
				identifier = FNERandom.GetRandomFloatInRange(0, 1);

			var message = m_NotificationMessageFactory.CreateNotificationMessage(m_SpriteId, m_ColorId, m_IsPermanent, playerMessage, identifier);
			Broadcast_All(message);
		}

		public void Notification_SendNotification_STC(string spriteId, string color, string isPermanent, string playerMessage, NetConnection clientConnection, float identifier = 0)
		{
			ConvertStrings(spriteId, color, isPermanent);

			if (identifier == 0)
				identifier = FNERandom.GetRandomFloatInRange(0, 1);

			var message = m_NotificationMessageFactory.CreateNotificationMessage(m_SpriteId, m_ColorId, m_IsPermanent, playerMessage, identifier);
			SendToClient(message, clientConnection);
		}

		public void Notification_SendWarningNotification_STC(string warningMessage, NetConnection clientConnection)
		{
			Notification_SendNotification_STC(
				"warning_icon",
				"red",
				"false",
				warningMessage,
				clientConnection
			);
		}

		public void Notification_SendWarningNotification_BA(string warningMessage)
		{
			Notification_SendNotification_BA(
				"warning_icon",
				"red",
				"false",
				warningMessage
			);
		}

	}
}