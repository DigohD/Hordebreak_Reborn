using FNZ.Client.View.Input;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FNZ.Client.View.Input
{
    public class ExtractModeInputLayer : InputLayer
    {

		public Slot previouslyActiveSlot = Slot.None;
        public ExtractModeInputLayer() : base(false)
        {
            IsUIBlockingMouse = false;
		}

		protected override void AddActionMappings()
		{
			AddActionMapping("Extract", MouseButton.LEFT);
			AddActionMapping("ExitExtractModeQ", UnityEngine.KeyCode.Q);
			AddActionMapping("ExitExtractModeEsc", UnityEngine.KeyCode.Escape);

			AddActionMapping(ActionIdentifiers.ACTION_WEAPON1_SLOT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_WEAPON1_SLOT]);
			AddActionMapping(ActionIdentifiers.ACTION_WEAPON2_SLOT, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_WEAPON2_SLOT]);
			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT1, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT1]);
			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT2, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT2]);
			AddActionMapping(ActionIdentifiers.ACTION_HOTBAR_SLOT3, InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_HOTBAR_SLOT3]);
		}

		protected override void BindActions()
		{
			BindAction("ExitExtractModeQ", InputActionType.PRESS, ExitExtractMode);
			BindAction("ExitExtractModeEsc", InputActionType.PRESS, ExitExtractMode);

			BindAction(ActionIdentifiers.ACTION_WEAPON1_SLOT, InputActionType.PRESS, OnWeaponSlot1);
			BindAction(ActionIdentifiers.ACTION_WEAPON2_SLOT, InputActionType.PRESS, OnWeaponSlot2);
			BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT1, InputActionType.PRESS, OnConsumable1Pressed);
			BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT2, InputActionType.PRESS, OnConsumable2Pressed);
			BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT3, InputActionType.PRESS, OnConsumable3Pressed);
		}

		private void ExitExtractMode()
        {
			UIManager.Instance.ClosePlayerExtract(previouslyActiveSlot);
		}

		private void OnWeaponSlot1()
		{
			UIManager.Instance.ClosePlayerExtract(Slot.Weapon1);
		}

		private void OnWeaponSlot2()
		{
			UIManager.Instance.ClosePlayerExtract(Slot.Weapon2);
		}

		private void OnConsumable1Pressed()
		{
			UIManager.Instance.ClosePlayerExtract(Slot.Consumable1);
		}

		private void OnConsumable2Pressed()
		{
			UIManager.Instance.ClosePlayerExtract(Slot.Consumable2);
		}

		private void OnConsumable3Pressed()
		{
			UIManager.Instance.ClosePlayerExtract(Slot.Consumable3);
		}
	}
}
