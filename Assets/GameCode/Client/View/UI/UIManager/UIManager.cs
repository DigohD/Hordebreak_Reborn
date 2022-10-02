using FNZ.Client.Model.Entity.Components.Crafting;
using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Model.Entity.Components.Refinement;
using FNZ.Client.Utils;
using FNZ.Client.View.Audio;
using FNZ.Client.View.Camera;
using FNZ.Client.View.Input;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.BaseViewerUI;
using FNZ.Client.View.UI.Building;
using FNZ.Client.View.UI.Chat;
using FNZ.Client.View.UI.Component.Crafting;
using FNZ.Client.View.UI.Component.Equipment;
using FNZ.Client.View.UI.Component.Excavator;
using FNZ.Client.View.UI.Component.Inventory;
using FNZ.Client.View.UI.Component.Refinement;
using FNZ.Client.View.UI.FNECursor;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Rooms;
using FNZ.Client.View.UI.ScreenEffects;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using FNZ.Client.View.UI.Notifications;
using FNZ.Client.View.UI.WorldMap;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using FNZ.Client.View.UI.StartMenu;
using FNZ.Client.View.UI.MetaWorld;

namespace FNZ.Client.View.UI.Manager
{
	public delegate void OnBaseChanged(long baseId);
	public delegate void OnRoomChanged(long roomId);
	public delegate void OnInteractableOpen(FNEComponent comp);

	public delegate void OnToggleBuildMode(bool isActive);
	public delegate void OnToggleExcav4Mode(bool isActive);
	public delegate void OnToggleInventory(bool isActive);
	public delegate void OnToggleMainMenu(bool isActive);
	public delegate void OnTogglePurgopedia(bool isActive);
	public delegate void OnToggleMap(bool isActive);

	public class UIManager : FNESingleton<UIManager>
	{
		private List<Tuple<string, GameObject, UI_Arrow>> m_UIArrows = new List<Tuple<string, GameObject, UI_Arrow>>();

		public Transform PlayerNames;
		public Transform MainUIParent;
		public Transform RightSideMenu;
		public Transform LeftSideMenu;
		public Transform Popup;
		public Transform Chat;
		public Transform AddonAnchor;
		public Transform RoomUIAnchor;

		public FNECursor.Cursor Cursor;

		public GameObject BuilderUI;
		public GameObject InventoryUI;
		public GameObject BaseViewer;
		public GameObject OptionsMenu;

		public Transform InGameMenu;
		public Transform Purgopedia;
		public Transform RespawnPrompt;
		public Transform UIArrowsParent;
		public Transform ExcavatingItemSlotUI;
		public Transform ActionBarUI;

		public static Canvas Canvas;
		public static HoverBoxGen HoverBoxGen;
		public static UI_ErrorMessage ErrorHandler;

		private AudioManager m_AudioManager;

		private long m_CurrentBaseId;
		private long m_CurrentRoomId;

		public OnBaseChanged d_OnBaseChanged;
		public OnRoomChanged d_OnRoomChanged;

		public OnInteractableOpen d_OnInteractableOpen;
		public OnToggleBuildMode d_OnToggleBuildMode;
		public OnToggleExcav4Mode d_OnToggleExcav4Mode;
		public OnToggleInventory d_OnToggleInventory;
		public OnTogglePurgopedia d_OnTogglePurgopedia;
		public OnToggleMainMenu d_OnToggleMainMenu;
		public OnToggleMap d_OnToggleMap;

		private GameObject RoomUI;
		private RoomUI roomUIComp;

		private GameObject WorldMapUI;
		private WorldMapUI WorldMapUIComp;

		private GameObject MetaWorldMapUI;

		[SerializeField]
		private Text m_InteractPrompt;

		private void Start()
		{
			Canvas = GetComponent<Canvas>();
			HoverBoxGen = new HoverBoxGen(Canvas);
			m_AudioManager = GameObject.FindObjectOfType<AudioManager>();
			InGameMenu.gameObject.SetActive(false);
			ErrorHandler = GetComponentInChildren<UI_ErrorMessage>();

			d_OnInteractableOpen += DetermineInteractable;

			CanvasScaler CanvasScaler = gameObject.GetComponent<CanvasScaler>();
			UI_Options UIOptionsMenu = OptionsMenu.GetComponent<UI_Options>();

			if (CanvasScaler != null && UIOptionsMenu != null)
			{
				UIOptionsMenu.SetUIScaleWidthRef(CanvasScaler.referenceResolution.x);
				UIOptionsMenu.SetUIScaleHeightRef(CanvasScaler.referenceResolution.y);

				CanvasScaler.referenceResolution = new Vector2(
					(UIOptionsMenu.GetUIScaleWidthRef() / UI_Options.GetUIScale()),
					(UIOptionsMenu.GetUIScaleHeightRef() / UI_Options.GetUIScale())
				);

				UIOptionsMenu.UpdateUIScaleSliderValue();
			}
				
		}

		void Update()
		{
			if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
			{
				transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
			}
			
			if (UnityEngine.Input.GetKeyDown(KeyCode.F2))
			{
				Chat.gameObject.SetActive(!Chat.gameObject.activeInHierarchy);
			}
			
			if (UnityEngine.Input.GetKeyDown(KeyCode.F3))
			{
				Cursor.gameObject.SetActive(!Cursor.gameObject.activeInHierarchy);
			}
		}

		public void InstantiateRoomUI()
		{
			var roomPrefab = Resources.Load<GameObject>("Prefab/MainUI/Room/RoomUI");
			RoomUI = Instantiate(roomPrefab);
			RoomUI.transform.SetParent(RoomUIAnchor, false);
			roomUIComp = RoomUI.GetComponent<RoomUI>();
			roomUIComp.Init();
			RoomUI.SetActive(false);
		}
		
		public void ToggleWorldMapUI()
		{
			if (WorldMapUI != null)
			{
				CloseWorldMapUI();
			}
			else
			{
				ShowWorldMapUI();
			}
		}
		
		public void ShowWorldMapUI()
		{
			var worldMapPrefab = Resources.Load<GameObject>("Prefab/UI/WorldMap/WorldMapUI");
			WorldMapUI = Instantiate(worldMapPrefab);
			WorldMapUI.transform.SetParent(MainUIParent, false);
			WorldMapUIComp = WorldMapUI.GetComponent<WorldMapUI>();
			InputManager.Instance.PushInputLayer<UIInputLayer>();

			d_OnToggleMap?.Invoke(true);
		}
		
		public void CloseWorldMapUI()
		{
			Destroy(WorldMapUI);
			InputManager.Instance.PopInputLayer();
			HB_Factory.DestroyHoverbox();

			d_OnToggleMap?.Invoke(false);
		}

		public void ToggleMetaWorldUI()
		{
			if (MetaWorldMapUI != null)
			{
				CloseMetaWorldMapUI();
			}
			else
			{
				ShowMetaWorldMapUI();
			}
		}

		public void ShowMetaWorldMapUI()
		{
			var metaWorldPrefab = Resources.Load<GameObject>("Prefab/UI/MetaWorld/MetaWorldUI");
			MetaWorldMapUI = Instantiate(metaWorldPrefab);
			MetaWorldMapUI.transform.SetParent(MainUIParent, false);
			InputManager.Instance.PushInputLayer<UIInputLayer>();
		}

		public void CloseMetaWorldMapUI()
		{
			Destroy(MetaWorldMapUI);
			InputManager.Instance.PopInputLayer();
			HB_Factory.DestroyHoverbox();
		}

		public void RemovePlayerFromMap(FNEEntity player)
		{
			if (WorldMapUIComp != null)
				WorldMapUIComp.RemovePlayerFromMap(player);
		}

		public bool HasLeftSideUi()
		{
			return LeftSideMenu.childCount > 0;
		}

		public bool HasRightSideUi()
		{
			return RightSideMenu.childCount > 0;
		}

		public FNEComponent GetPopupComponent()
		{
			if (Popup.childCount > 0)
			{
				var inventory = Popup.GetComponentInChildren<LootableUI>();
				if (inventory != null)
					return inventory.GetInventoryComp();

				var refine = Popup.GetComponentInChildren<RefinementUI>();
				if (refine != null)
					return refine.GetRefinementComp();

				var crafting = Popup.GetComponentInChildren<CraftingUI>();
				if (crafting != null)
					return crafting.GetCraftingComp();
			}

			return null;
		}

		public bool HasPopup()
		{
			return Popup.childCount > 0;
		}

		public void ToggleChat()
		{
			Chat.GetComponent<UI_Chat>().ToggleChat();
		}

		public void CloseChat()
		{
			Chat.GetComponent<UI_Chat>().CloseChat();
		}

		public void ScrollMessages(bool up)
		{
			Chat.GetComponent<UI_Chat>().OnCycleMessages(up);
		}

		public void TogglePlayerInventory()
		{
			if (InventoryUI == null)
			{
				CreateInventoryUI();
				OpenPlayerInventory();
			}
			else if (InventoryUI.activeInHierarchy)
			{
				ClosePlayerInventory();
				PopInputLayerIfNoUI();
				CameraScript.SetRightUIPixelWidth(0);
			}
			else
			{
				OpenPlayerInventory();
			}
		}

		private void CreateInventoryUI()
		{
			GameObject prefab = PrefabBank.GetPrefab("Prefab/MainUI/PlayerInventoryUI");

			InventoryUI = (GameObject)GameObject.Instantiate(prefab);

			RectTransform uiTransform = (RectTransform)InventoryUI.transform;

			uiTransform.SetParent(RightSideMenu.transform);
			uiTransform.anchoredPosition = new Vector2(0, 0);

			uiTransform.offsetMin = new Vector2(uiTransform.offsetMin.x, 0);
			uiTransform.offsetMax = new Vector2(uiTransform.offsetMax.x, 0);

			uiTransform.localScale = prefab.transform.lossyScale;

			InventoryUI.GetComponentInChildren<InventoryUI>().Init(
				GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>()
			);

			InventoryUI.GetComponentInChildren<EquipmentUI>().Init();

			/*InventoryUI.GetComponentInChildren<RefuelUI>().Init(
				GameClient.LocalPlayerEntity.GetComponent<ExcavatorComponentClient>(),
				GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>()
			);*/
		}

		public void OpenPlayerInventory()
		{
			InventoryUI.SetActive(true);

			RectTransform uiTransform = (RectTransform)InventoryUI.transform;
			CameraScript.SetRightUIPixelWidth(uiTransform.sizeDelta.x * GameObject.FindObjectOfType<Canvas>().scaleFactor);
			InputManager.Instance.PushInputLayer<UIInputLayer>();
			PlaySound("sfx_ui_open");

			d_OnToggleInventory?.Invoke(true);
		}

		public void ClosePlayerInventory()
		{
			if (InventoryUI != null && InventoryUI.activeInHierarchy)
			{
				InventoryUI.SetActive(false);

				InventoryUI.GetComponentInChildren<InventoryUI>().OnClose();
				CameraScript.SetRightUIPixelWidth(0);
				HB_Factory.DestroyHoverbox();
				InputManager.Instance.PopInputLayer();
				PlaySound("sfx_ui_close");

				d_OnToggleInventory?.Invoke(false);
			}
		}

        public void TogglePlayerExtract()
        {
            if (ExcavatingItemSlotUI.gameObject.activeInHierarchy)
            {
                ClosePlayerExtract(((ExtractModeInputLayer)InputManager.Instance.PeekInputLayer()).previouslyActiveSlot);
            }
            else
            {
                OpenPlayerExtract(GameClient.LocalPlayerEntity.GetComponent<EquipmentSystemComponentClient>().ActiveActionBarSlot);
            }
        }

        public void OpenPlayerExtract(Slot slot)
        {
			var extractLayer = InputLayerStorage.GetInputLayer<ExtractModeInputLayer>();
			extractLayer.previouslyActiveSlot = slot;
			InputManager.Instance.PushInputLayer(extractLayer);
			ExcavatingItemSlotUI.gameObject.SetActive(true);
			ActionBarUI.gameObject.SetActive(false);
			GameClient.LocalPlayerEntity.GetComponent<EquipmentSystemComponentClient>().NE_Send_ActiveActionBarChange(Slot.Excavator);

			d_OnToggleExcav4Mode?.Invoke(true);
		}

		public void ClosePlayerExtract(Slot slot)
		{
			InputManager.Instance.PopInputLayer();
			ExcavatingItemSlotUI.gameObject.SetActive(false);
			ActionBarUI.gameObject.SetActive(true);
			GameClient.LocalPlayerEntity.GetComponent<EquipmentSystemComponentClient>().NE_Send_ActiveActionBarChange(slot);

			d_OnToggleExcav4Mode?.Invoke(false);
		}

		public void TogglePlayerBuilder()
		{
			if (BuilderUI.activeInHierarchy)
			{
				ClosePlayerBuilder();
				InputManager.Instance.PopInputLayer();
				InputManager.Instance.PopInputLayer();
				PopInputLayerIfNoUI();
			}
			else
			{
				var pos = GameClient.LocalPlayerEntity.Position;
				var isInBase = GameClient.RoomManager.IsTileWithinBase(
					new int2((int) pos.x, (int) pos.y)
				);
				if (isInBase)
				{
					OpenPlayerBuilder();
				}
				else
				{
					UI_Notification.Instance.CreateNewNotification(
						"warning_icon",
						"#FF0000",
						false,
						Localization.GetString("string_build_outside_base"),
						FNERandom.GetRandomFloatInRange(0f, 1f)
					);
				}
			}
		}

		public void OpenPlayerBuilder()
		{
			BuilderUI.SetActive(true);

			PlayerController pc = GameClient.LocalPlayerView.GetComponent<PlayerController>();
			pc.GetPlayerBuildSystem().IsBuildMode = false;
			pc.GetPlayerBuildSystem().ActiveBuilding = null;

			var buildLayer = InputLayerStorage.GetInputLayer<BuildModeInputLayer>();

			buildLayer.SetPlayerController(pc);

			InputManager.Instance.PushInputLayer<UIInputLayer>();
			InputManager.Instance.PushInputLayer(buildLayer);

			PlayerBuildView pbv = GameClient.LocalPlayerView.GetComponent<PlayerBuildView>();
			pbv.OnOpenBuildMode();

			BuilderUI.GetComponentInChildren<BuildingUI>().Init();
			PlaySound("sfx_ui_open");

			d_OnToggleBuildMode?.Invoke(true);
		}

		public void ClosePlayerBuilder()
		{
			PlayerBuildView pbv = GameClient.LocalPlayerView.GetComponent<PlayerBuildView>();
			PlayerController pc = GameClient.LocalPlayerView.GetComponent<PlayerController>();

			pc.GetPlayerBuildSystem().IsBuildMode = false;
			pbv.OnExitBuildMode();

			BuilderUI.SetActive(false);
			HB_Factory.DestroyHoverbox();
			PlaySound("sfx_ui_close");

			d_OnToggleBuildMode?.Invoke(false);
		}

		public bool IsBuilderOpen()
		{
			return BuilderUI.activeInHierarchy;
		}

		private void DetermineInteractable(FNEComponent comp)
		{
			if (comp == null)
			{
				ClosePopup();
				return;
			}

			if (comp is InventoryComponentClient)
			{
				if (GetPopupComponent() == comp)
				{
					ClosePopup();
					if (InventoryUI != null && InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
				else
				{
					OpenLootablePopup((InventoryComponentClient)comp);

					if (InventoryUI == null)
						TogglePlayerInventory();
					else if (!InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
			}

			if (comp is RefinementComponentClient)
			{
				if (GetPopupComponent() == comp)
				{
					ClosePopup();
					if (InventoryUI != null && InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
				else
				{
					OpenRefinementPopup((RefinementComponentClient)comp);

					if (InventoryUI == null)
						TogglePlayerInventory();
					else if (!InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
			}

			if (comp is CraftingComponentClient)
			{
				if (GetPopupComponent() == comp)
				{
					ClosePopup();
					if (InventoryUI != null && InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
				else
				{
					OpenCraftingPopup((CraftingComponentClient)comp);

					if (InventoryUI == null)
						TogglePlayerInventory();
					else if (!InventoryUI.activeInHierarchy)
						TogglePlayerInventory();

					return;
				}
			}
		}

		public void OpenLootablePopup(InventoryComponentClient inventoryComp)
		{
			foreach (Transform t in Popup.transform)
				Destroy(t.gameObject);

			GameObject prefab = PrefabBank.GetPrefab("Prefab/MainUI/LootableInventoryUI");
			GameObject newLootablePopup = GameObject.Instantiate(prefab);

			RectTransform uiTransform = (RectTransform)newLootablePopup.transform;
			uiTransform.SetParent(Popup.transform);
			uiTransform.anchoredPosition = new Vector2(0, 0);
			uiTransform.localScale = prefab.transform.lossyScale;

			newLootablePopup.GetComponentInChildren<LootableUI>().Init(inventoryComp);

			InputManager.Instance.PushInputLayer<UIInputLayer>();

			PlaySound("sfx_ui_open");

			uiTransform.offsetMin = new Vector2(uiTransform.offsetMin.x, 0);
			uiTransform.offsetMax = new Vector2(uiTransform.offsetMax.x, 0);
		}

		public void OpenCraftingPopup(CraftingComponentClient craftingComp)
		{
			foreach (Transform t in Popup.transform)
				Destroy(t.gameObject);

			GameObject prefab = PrefabBank.GetPrefab("Prefab/MainUI/CraftingUI");

			GameObject newCraftPopup = (GameObject)GameObject.Instantiate(prefab);
			RectTransform uiTransform = (RectTransform)newCraftPopup.transform;

			uiTransform.SetParent(Popup.transform);
			uiTransform.anchoredPosition = new Vector2(0, 0);

			uiTransform.localScale = prefab.transform.lossyScale;

			newCraftPopup.GetComponentInChildren<CraftingUI>().Init(craftingComp);

			InputManager.Instance.PushInputLayer<UIInputLayer>();

			PlaySound("sfx_ui_open");
		}

		public void OpenRefinementPopup(RefinementComponentClient refinementComp)
		{
			foreach (Transform t in Popup.transform)
				Destroy(t.gameObject);

			GameObject prefab = PrefabBank.GetPrefab("Prefab/MainUI/RefinementUI");

			GameObject newRefinementPopup = (GameObject)GameObject.Instantiate(prefab);
			RectTransform uiTransform = (RectTransform)newRefinementPopup.transform;

			uiTransform.SetParent(Popup.transform);
			uiTransform.anchoredPosition = new Vector2(0, 0);

			uiTransform.localScale = prefab.transform.lossyScale;

			newRefinementPopup.GetComponentInChildren<RefinementUI>().Init(refinementComp);

			InputManager.Instance.PushInputLayer<UIInputLayer>();

			PlaySound("sfx_ui_open");
		}

		public void ClosePopup()
		{
			if (Popup.childCount > 0)
			{
				Destroy(Popup.GetChild(0).gameObject);
				Popup.GetChild(0).SetParent(null);
				HB_Factory.DestroyHoverbox();
				PlaySound("sfx_ui_close");
			}
		}

		public void CloseAllActiveUI()
		{
			CameraScript.SetRightUIPixelWidth(0);

			if(ExcavatingItemSlotUI.gameObject.activeInHierarchy)
            {
				ClosePlayerExtract(Slot.Weapon1);
			}
			
			ClosePlayerBuilder();
			ClosePurgopedia();
			ClosePopup();
			ClosePlayerInventory();
			CloseChat();
			CloseMainMenu();
			CloseWorldMapUI();
		}

		private void PopInputLayerIfNoUI()
		{
			bool hasLeft = HasLeftSideUi();
			bool hasRight = HasRightSideUi();
			bool buildOpened = BuilderUI.activeInHierarchy;
			if (!hasLeft && !hasRight && !buildOpened)
			{
				InputManager.Instance.PopInputLayer();
			}
		}

		public void SetCursorAsItem(Item item)
		{
			Cursor.SetItemCursor(item);
		}

		public void ResetCursor()
		{
			Cursor.EnableDefaultCursor();
		}

		public void PlaySound(string id)
		{
			m_AudioManager.PlaySfxUiClip(id);
		}

		public void ToggleMainMenu()
		{
			if (InGameMenu.gameObject.activeInHierarchy)
			{
				CloseMainMenu();
			}
			else
			{
				InGameMenu.gameObject.SetActive(true);
				InputManager.Instance.PushInputLayer<IngameMenuInputLayer>();
				PlaySound("sfx_ui_open");

				d_OnToggleMainMenu?.Invoke(true);
			}
		}

		public void CloseMainMenu()
		{
			InGameMenu.gameObject.SetActive(false);
			InputManager.Instance.PopInputLayer();
			PlaySound("sfx_ui_close");

			d_OnToggleMainMenu?.Invoke(false);
		}

		public void TogglePurgopedia()
		{
			if (Purgopedia.gameObject.activeInHierarchy)
			{
				ClosePurgopedia();
			}
			else
			{
				Purgopedia.gameObject.SetActive(true);
				InputManager.Instance.PushInputLayer<UIInputLayer>();
				UIManager.Instance.PlaySound("sfx_ui_close");

				d_OnTogglePurgopedia?.Invoke(true);
			}
		}

		public void ClosePurgopedia()
		{
			if (Purgopedia.gameObject.activeInHierarchy)
			{
				Purgopedia.gameObject.SetActive(false);
				InputManager.Instance.PopInputLayer();
				UIManager.Instance.PlaySound("sfx_ui_open");

				d_OnTogglePurgopedia?.Invoke(false);
			}
		}

		public void UpdatePlayerRoom()
		{
			if (roomUIComp == null)
				return;

			var playerPos = GameClient.LocalPlayerEntity.Position;
			long baseId = 0;

			GameClient.RoomManager.IsTileWithinBase((int2) playerPos, out baseId);

			if (baseId != m_CurrentBaseId)
			{
				// Entered Outside
				if (baseId == 0)
				{
					roomUIComp.SetNoRoom();
					d_OnBaseChanged?.Invoke(baseId);
					if (BuilderUI.activeInHierarchy)
					{
						TogglePlayerBuilder();
					}
				}
				// Entered Base
				else
				{
					d_OnBaseChanged?.Invoke(baseId);
				}
			}
			

			long roomId = GameClient.World.GetTileRoom(playerPos);
			if (roomId != m_CurrentRoomId)
			{
				// Entered Outside
				if (roomId == 0)
				{
					roomUIComp.SetNoRoom();
					d_OnRoomChanged?.Invoke(roomId);
				}
				// Entered Room
				else
				{
					d_OnRoomChanged?.Invoke(roomId);
				}
			}
			
			m_CurrentBaseId = baseId;
			m_CurrentRoomId = roomId;
		}

		public void ForceUpdatePlayerRoomAndEnvironment()
		{
			m_CurrentRoomId = 0;
			UpdatePlayerRoom();
		}

		public void ShowInteractionPrompt(string message)
		{
			m_InteractPrompt.text = message + $" [{InputKeybinds.Instance.Keybinds[ActionIdentifiers.ACTION_INTERACT]}]";
			m_InteractPrompt.gameObject.SetActive(true);
		}

		public void HideInteractionPrompt()
		{
			m_InteractPrompt.gameObject.SetActive(false);
		}

		public void ToggleBaseViewer()
		{
			if (BaseViewer != null)
			{
				Destroy(BaseViewer);

				HB_Factory.DestroyHoverbox();

				InputManager.Instance.PopInputLayer();
			}
			else
			{
				BaseViewer = Instantiate(
					Resources.Load<GameObject>("Prefab/MainUI/BaseViewer/BaseViewer"),
					Vector2.zero,
					Quaternion.identity,
					MainUIParent
				);
				var rt = (RectTransform)BaseViewer.transform;
				rt.sizeDelta = Vector2.zero;
				rt.localPosition = Vector2.zero;
				BaseViewer.GetComponent<BaseViewer>().Init();

				InputManager.Instance.PushInputLayer<UIInputLayer>();
			}
		}

		public void NewUIArrow(string name, float2 pos)
		{
			var arrow = Instantiate(Resources.Load<GameObject>("Prefab/UI/Arrow"), UIArrowsParent);
			var arrowComp = arrow.GetComponent<UI_Arrow>();
			arrowComp.Init(name, pos);

			m_UIArrows.Add(new Tuple<string, GameObject, UI_Arrow>(name, arrow, arrowComp));
		}

		public void UpdateUIArrow(string name, float2 pos)
		{
			foreach (var tuple in m_UIArrows)
			{
				if (tuple.Item1 == name)
				{
					tuple.Item3.UpdatePosition(pos);
					break;
				}
			}
		}

		public void RemoveUIArrow(string name)
		{
			var tuple = m_UIArrows.First(entry => entry.Item1 == name);

			if (tuple != null)
				RemoveUIArrow(tuple);
		}

		public void RemoveUIArrow(Tuple<string, GameObject, UI_Arrow> tuple)
		{
			Destroy(tuple.Item2);
			m_UIArrows.Remove(tuple);
		}

		public void ClearUIBaseArrows()
		{
			var baseArrows = new List<Tuple<string, GameObject, UI_Arrow>>();
			foreach (var tuple in m_UIArrows.Where(tuple => tuple.Item1.ToLower().Contains("base:")))
				baseArrows.Add(tuple);

			foreach (var tuple in baseArrows)
				RemoveUIArrow(tuple);
		}
	}
}