using FNZ.Client.View.Player;
using UnityEngine;

namespace FNZ.Client.View.Input
{

	public class PlayerDefaultInputLayer : InputLayer
	{
		public PlayerDefaultInputLayer() : base(false) { }

		private PlayerController m_PlayerController;

		protected override void AddActionMappings()
		{
			AddActionMapping(ActionIdentifiers.MOVE_FORWARD, InputKeybinds.Instance.Keybinds[ActionIdentifiers.MOVE_FORWARD]);
			AddActionMapping(ActionIdentifiers.MOVE_BACK, InputKeybinds.Instance.Keybinds[ActionIdentifiers.MOVE_BACK]);
			AddActionMapping(ActionIdentifiers.MOVE_RIGHT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.MOVE_RIGHT]);
			AddActionMapping(ActionIdentifiers.MOVE_LEFT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.MOVE_LEFT]);

			AddActionMapping(ActionIdentifiers.ACTION_SPRINT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_SPRINT]);
			AddActionMapping(ActionIdentifiers.ACTION_RELOAD, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_RELOAD]);

			AddActionMapping(ActionIdentifiers.ACTION_INTERACT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_INTERACT]);
			AddActionMapping(ActionIdentifiers.ACTION_INTERACT, GamepadButton.FACE_BTN_DOWN);

			AddActionMapping(ActionIdentifiers.ACTION_SHOOT_PRIMARY, InputKeybinds.Instance.Mousebinds[ActionIdentifiers.ACTION_SHOOT_PRIMARY]);
			AddActionMapping(ActionIdentifiers.ACTION_SHOOT_PRIMARY, GamepadButton.RIGHT_TRIGGER);

			AddActionMapping(ActionIdentifiers.ACTION_SHOOT_SECONDARY, InputKeybinds.Instance.Mousebinds[ActionIdentifiers.ACTION_SHOOT_SECONDARY]);
			AddActionMapping(ActionIdentifiers.ACTION_SHOOT_SECONDARY, GamepadButton.LEFT_TRIGGER);

			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_TOGGLE_INVENTORY]);
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, GamepadButton.BACK);
			AddActionMapping(ActionIdentifiers.ACTION_OPEN_CRAFTING_MENU, KeyCode.C);
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU]);
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_WORLD_MAP, KeyCode.M);
			AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_META_WORLD, KeyCode.N);

			AddActionMapping(ActionIdentifiers.ACTION_EXCAVATOR_SLOT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_EXCAVATOR_SLOT]);
			AddActionMapping(ActionIdentifiers.ACTION_WEAPON1_SLOT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_WEAPON1_SLOT]);
			AddActionMapping(ActionIdentifiers.ACTION_WEAPON2_SLOT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_WEAPON2_SLOT]);

			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT1, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT1]);
			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT2, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT2]);
			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT3, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT3]);

			AddActionMapping(ActionIdentifiers.ACTION_FLASHLIGHT, KeyCode.X);

			AddActionMapping(ActionIdentifiers.ACTION_EXIT, KeyCode.Escape);
			AddActionMapping(ActionIdentifiers.ACTION_OPEN_CHAT, KeyCode.Return);

			AddActionMapping(ActionIdentifiers.EQUIP_RIFLE, KeyCode.P);

		}

		public override void OnActivated()
		{
			base.OnActivated();
			UpdateKeyBinds();
		}

		public override void OnDeactivated()
		{
			base.OnDeactivated();
		}

		protected override void BindActions()
		{
			base.BindActions();
		}

		public void SetPlayerController(PlayerController pc)
		{
			m_PlayerController = pc;
		}
	}
}