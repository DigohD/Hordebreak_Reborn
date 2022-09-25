using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.ActionMenuBar 
{

	public class ActionMenuBar : MonoBehaviour
	{

        [SerializeField]
        private GameObject G_BuildButton;
        [SerializeField]
        private GameObject G_InventoryButton;
        [SerializeField]
        private GameObject G_MainMenuButton;
        [SerializeField]
        private GameObject G_Excav4Button;
        [SerializeField]
        private GameObject G_PurgopediaButton;
        [SerializeField]
        private GameObject G_MapButton;

        [SerializeField]
        private Sprite ActiveButton;
        [SerializeField]
        private Sprite ToggledButton;
        [SerializeField]
        private Sprite HoveredButton;
        [SerializeField]
        private Sprite InactiveButton;

        private bool buildMode = false;
        private bool excav4Mode = false;
        private bool mainMenuOpen = false;
        private bool inventoryOpen = false;
        private bool purgopediaOpen = false;
        private bool mapOpen = false;

        public void Start()
        {
            UIManager.Instance.d_OnToggleBuildMode += OnToggleBuildMode;
            UIManager.Instance.d_OnToggleExcav4Mode += OnToggleExcav4Mode;
            UIManager.Instance.d_OnToggleInventory += OnToggleInventory;
            UIManager.Instance.d_OnTogglePurgopedia += OnTogglePurgopedia;
            UIManager.Instance.d_OnToggleMainMenu += OnToggleMainMenu;
            UIManager.Instance.d_OnToggleMap += OnToggleMap; 
        }

        // -- Delegate methods --

        private void OnToggleBuildMode(bool isActive)
        {
            if (isActive)
            {
                G_BuildButton.GetComponent<Image>().sprite = ToggledButton;
                buildMode = true;
            }
            else
            {
                G_BuildButton.GetComponent<Image>().sprite = InactiveButton;
                buildMode = false;
            }
        }
        private void OnToggleExcav4Mode(bool isActive)
        {
            if (isActive)
            {
                G_Excav4Button.GetComponent<Image>().sprite = ToggledButton;
                excav4Mode = true;
            }
            else
            {
                G_Excav4Button.GetComponent<Image>().sprite = InactiveButton;
                excav4Mode = false;
            }
        }
        private void OnToggleInventory(bool isActive)
        {
            if (isActive)
            {
                G_InventoryButton.GetComponent<Image>().sprite = ToggledButton;
                inventoryOpen = true;
            }
            else
            {
                G_InventoryButton.GetComponent<Image>().sprite = InactiveButton;
                inventoryOpen = false;
            }
        }
        private void OnTogglePurgopedia(bool isActive)
        {
            return;
            //if (isActive)
            //{
            //    G_PurgopediaButton.GetComponent<Image>().sprite = ToggledButton;
            //    purgopediaOpen = true;
            //}
            //else
            //{
            //    G_PurgopediaButton.GetComponent<Image>().sprite = InactiveButton;
            //    purgopediaOpen = false;
            //}
        }
        private void OnToggleMainMenu(bool isActive)
        {
            if (isActive)
            {
                G_MainMenuButton.GetComponent<Image>().sprite = ToggledButton;
                mainMenuOpen = true;
            }
            else
            {
                G_MainMenuButton.GetComponent<Image>().sprite = InactiveButton;
                mainMenuOpen = false;
            }
        }
        private void OnToggleMap(bool isActive)
        {
            if (isActive)
            {
                G_MapButton.GetComponent<Image>().sprite = ToggledButton;
                mapOpen = true;
            }
            else
            {
                G_MapButton.GetComponent<Image>().sprite = InactiveButton;
                mapOpen = false;
            }
        }

        // -- Build mode button --
        public void OnBuildMenuClickDown()
        {
            G_BuildButton.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnBuildMenuClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.TogglePlayerBuilder();
        }
        public void OnBuildMenuHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("build_menu_hoverbox");

            if (!buildMode)
            {
                G_BuildButton.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnBuildMenuExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!buildMode)
            {
                G_BuildButton.GetComponent<Image>().sprite = InactiveButton;
            }
        }

        // -- Excav4 mode button --
        public void OnExcav4ModeClickDown()
        {
            G_Excav4Button.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnExcav4ModeClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.TogglePlayerExtract();
        }
        public void OnExcav4ModeHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("excav4_hoverbox");

            if (!excav4Mode)
            {
                G_Excav4Button.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnExcav4ModeExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!excav4Mode)
            {
                G_Excav4Button.GetComponent<Image>().sprite = InactiveButton;
            }
        }

        // -- Inventory button --
        public void OnInventoryClickDown()
        {
            G_InventoryButton.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnInventoryClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.TogglePlayerInventory();
        }
        public void OnInventoryHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("inventory_hoverbox");

            if (!inventoryOpen)
            {
                G_InventoryButton.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnInventoryExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!inventoryOpen)
            {
                G_InventoryButton.GetComponent<Image>().sprite = InactiveButton;
            }
        }

        // -- Purgopedia button --
        public void OnPurgopediaClickDown()
        {
            G_PurgopediaButton.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnPurgopediaClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.TogglePurgopedia();
        }
        public void OnPurgopediaHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("purgopedia_hoverbox");

            if (!purgopediaOpen)
            {
                G_PurgopediaButton.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnPurgopediaExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!purgopediaOpen)
            {
                G_PurgopediaButton.GetComponent<Image>().sprite = InactiveButton;
            }
        }

        // -- Main menu button --
        public void OnMainMenuClickDown()
        {
            G_MainMenuButton.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnMainMenuClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.ToggleMainMenu();
        }
        public void OnMainMenuHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("main_menu_hoverbox");

            if (!mainMenuOpen)
            {
                G_MainMenuButton.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnMainMenuExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!mainMenuOpen)
            {
                G_MainMenuButton.GetComponent<Image>().sprite = InactiveButton;
            }
        }

        // -- Main menu button --
        public void OnMapClickDown()
        {
            G_MapButton.GetComponent<Image>().sprite = ActiveButton;
        }
        public void OnMapClickUp()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
            UIManager.Instance.ShowWorldMapUI();
            HB_Factory.DestroyHoverbox();
        }
        public void OnMapHover()
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            UIManager.HoverBoxGen.CreateSimpleTextHoverBox("map_hoverbox");

            if (!mapOpen)
            {
                G_MapButton.GetComponent<Image>().sprite = HoveredButton;
            }

        }
        public void OnMapExit()
        {
            HB_Factory.DestroyHoverbox();

            if (!mapOpen)
            {
                G_MapButton.GetComponent<Image>().sprite = InactiveButton;
            }
        }

    }
}