using FNZ.Client.View.UI.Manager;
using UnityEngine;

namespace FNZ.Client.View.Input
{
	public class ChatInputLayer : InputLayer
	{
		private UIManager m_UIManager;

		public ChatInputLayer() : base(true) { }

		protected override void AddActionMappings()
		{
			AddActionMapping(ActionIdentifiers.ACTION_OPEN_CHAT, KeyCode.Return);
			AddActionMapping(ActionIdentifiers.ACTION_CLOSE_CHAT, KeyCode.Escape);
			AddActionMapping(ActionIdentifiers.ACTION_SCROLL_UP_SENT_MESSAGES, KeyCode.UpArrow);
			AddActionMapping(ActionIdentifiers.ACTION_SCROLL_DOWN_SENT_MESSAGES, KeyCode.DownArrow);
		}

		protected override void BindActions()
		{
			BindAction(ActionIdentifiers.ACTION_OPEN_CHAT, InputActionType.PRESS, OnToggleChat);
			BindAction(ActionIdentifiers.ACTION_CLOSE_CHAT, InputActionType.PRESS, OnCloseAll);
			BindAction(ActionIdentifiers.ACTION_SCROLL_UP_SENT_MESSAGES, InputActionType.PRESS, OnScrollMessagesUp);
			BindAction(ActionIdentifiers.ACTION_SCROLL_DOWN_SENT_MESSAGES, InputActionType.PRESS, OnScrollMessagesDown);
		}
		private void OnCloseAll()
		{
			m_UIManager.CloseAllActiveUI();
		}

		public override void OnActivated()
		{
			base.OnActivated();
			if (m_UIManager == null) m_UIManager = UIManager.Instance;
		}

		public override void OnDeactivated()
		{
			base.OnDeactivated();
		}

		private void OnToggleChat()
		{
			m_UIManager.ToggleChat();
		}

		private void OnScrollMessagesUp()
		{
			m_UIManager.ScrollMessages(true);
		}

		private void OnScrollMessagesDown()
		{
			m_UIManager.ScrollMessages(false);
		}
	}
}