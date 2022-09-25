using FNZ.Client.View.UI.Manager;
using UnityEngine;

namespace FNZ.Client.View.Input
{
	public class InputFieldInputLayer : InputLayer
	{
		private UIManager m_UIManager;

		public InputFieldInputLayer() : base(true) { }

		protected override void AddActionMappings()
		{
			AddActionMapping(ActionIdentifiers.ACTION_OPEN_CHAT, KeyCode.Return);
		}

		protected override void BindActions()
		{
			BindAction(ActionIdentifiers.ACTION_OPEN_CHAT, InputActionType.PRESS, OnToggleChat);
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

		}
	}
}