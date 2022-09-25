using FNZ.Client.View.UI.Manager;
using UnityEngine;

namespace FNZ.Client.View.Input
{

	public class UIInputLayer : InputLayer
	{
		private UIManager m_UIManager;

		public UIInputLayer() : base(false)
		{
			IsUIBlockingMouse = true;
		}

		protected override void AddActionMappings()
		{
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_TOGGLE_INVENTORY]);
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU]);
			AddActionMapping(ActionIdentifiers.ACTION_OPEN_CHAT, KeyCode.Return);
			AddActionMapping("CloseAll", KeyCode.Escape);
			AddActionMapping("DropItem", MouseButton.LEFT);
			AddActionMapping("ShiftClick", KeyCode.LeftShift);
		}

		protected override void BindActions()
		{
			BindAction(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, InputActionType.PRESS, OnToggleInventory);
			BindAction(ActionIdentifiers.ACTION_OPEN_CHAT, InputActionType.PRESS, OnToggleChat);
			BindAction(ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU, InputActionType.PRESS, OnToggleBuilder);

			//BindAction(ActionIdentifiers.ACTION_OPEN_BUILDING_MENU, InputActionType.PRESS, OnOpenBuildMenu);
			//BindAction(ActionIdentifiers.ACTION_OPEN_CRAFTING_MENU, InputActionType.PRESS, OnOpenOrCloseCraftMenu);
			BindAction("CloseAll", InputActionType.PRESS, OnCloseAll);
			//BindAction("DropItem", InputActionType.PRESS, OnDropItem);
			//BindAction("ShiftClick", InputActionType.PRESS, OnShiftClickPress);
			//BindAction("ShiftClick", InputActionType.RELEASE, OnShiftClickRelease);
		}

		//private void OnShiftClickPress()
		//{
		//    NewInv_Manager_Script.isShiftDown = true;
		//}

		//private void OnShiftClickRelease()
		//{
		//    NewInv_Manager_Script.isShiftDown = false;
		//}

		//private void OnDropItem()
		//{
		//    if (m_UiManager.RightSideMenu != null)
		//    {
		//        if (!m_PlayerView.DidRaycastHitUI() && ClientApp.ItemOnMouse != null)
		//        {
		//            m_PlayerView.DropItemOnGround(ClientApp.ItemOnMouse);
		//        }
		//    }
		//}

		private void OnCloseAll()
		{
			if (
				!m_UIManager.HasPopup()
				&& (m_UIManager.InventoryUI == null || !m_UIManager.InventoryUI.activeInHierarchy)
				&& !m_UIManager.BuilderUI.activeInHierarchy 
				&& !m_UIManager.Purgopedia.gameObject.activeInHierarchy
			)
				m_UIManager.ToggleMainMenu();
			else
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

		private void OnToggleInventory()
		{
			m_UIManager.TogglePlayerInventory();
		}

		private void OnToggleChat()
		{
			m_UIManager.ToggleChat();
		}

		private void OnToggleBuilder()
		{
			m_UIManager.TogglePlayerBuilder();
		}
	}
}