using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.Player.Systems;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World.Tile;
using UnityEngine;

namespace FNZ.Client.View.Input
{

	public class BuildModeInputLayer : InputLayer
	{
		private PlayerController m_PlayerController;

		public BuildModeInputLayer() : base(false)
		{
			IsUIBlockingMouse = true;
		}

		protected override void AddActionMappings()
		{
			//AddActionMapping("CloseBuildMenu", KeyCode.B);
			//AddActionMapping("CloseBuildMenu", KeyCode.E);
			//AddActionMapping("CloseBuildMenu", KeyCode.Escape);

			AddActionMapping("Build", MouseButton.LEFT);
			AddActionMapping("ExitBuildMode", MouseButton.RIGHT);
			//AddActionMapping(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, KeyCode.I);
			//AddActionMapping(ActionIdentifiers.ACTION_OPEN_CRAFTING_MENU, KeyCode.C);
		}

		protected override void BindActions()
		{
			//BindAction("CloseBuildMenu", InputActionType.PRESS, OnCloseBuildMenu);
			BindAction("Build", InputActionType.PRESS, OnBuildStart);
			BindAction("Build", InputActionType.RELEASE, OnBuildEnd);
			BindAction("ExitBuildMode", InputActionType.PRESS, OnExitBuildMode);
			//BindAction(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, InputActionType.PRESS, OnOpenInventory);
			//BindAction(ActionIdentifiers.ACTION_OPEN_CRAFTING_MENU, InputActionType.PRESS, OnOpenCrafting);
		}

		private void OnOpenCrafting()
		{
			//OnCloseBuildMenu();
			//m_PlayerController.OnCraftingMenuOpen();
		}

		private void OnOpenInventory()
		{
			UIManager.Instance.TogglePlayerInventory();
		}

		public override void OnActivated()
		{
			base.OnActivated();
		}

		public override void OnDeactivated()
		{
			base.OnDeactivated();
		}

		private void OnExitBuildMode()
		{
			if(UIManager.Instance.AddonAnchor.childCount > 0)
			{
				foreach (Transform t in UIManager.Instance.AddonAnchor)
					GameClient.Destroy(t.gameObject);

				return;
			}

            var buildView = m_PlayerController.GetComponentInChildren<PlayerBuildView>();
            buildView.CancelBuild();

            if (UIManager.Instance.IsBuilderOpen() && m_PlayerController.GetPlayerBuildSystem().IsBuildMode)
			{
				m_PlayerController.GetPlayerBuildSystem().IsBuildMode = false;
				m_PlayerController.GetPlayerBuildSystem().ActiveBuilding = null;
				HB_Factory.DestroyHoverbox();
			}
			else if (!m_PlayerController.GetPlayerBuildSystem().IsBuildMode && UIManager.Instance.IsBuilderOpen())
			{
				OnCloseBuildMenu();
			}
		}

		private void OnBuildStart()
		{
			PlayerBuildSystem buildSystem = m_PlayerController.GetPlayerBuildSystem();
			buildSystem.CanBuild = true;
			var currentRecipe = buildSystem.ActiveBuilding;

			GameObject playerView = GameClient.LocalPlayerView;
			FNEEntity player = GameClient.LocalPlayerEntity;

			var buildView = playerView.GetComponentInChildren<PlayerBuildView>();

			if (buildView.IsAddonMode())
			{
				buildView.OnBuildStart();
				return;
			}

			if (currentRecipe != null)
			{
				bool isTile = DataBank.Instance.IsIdOfType<TileData>(currentRecipe.productRef);

				if (isTile)
				{
					buildView.OnBuildStart();
					return;
				}

				if (DataBank.Instance.IsIdOfType<MountedObjectData>(currentRecipe.productRef))
				{
					buildView.OnBuildStart();
					return;
				}

				FNEEntityData data = DataBank.Instance.GetData<FNEEntityData>(currentRecipe.productRef);
				// Walls and floors are built on mouse release, and only initiated on mouse press.
				if (data.entityType == EntityType.EDGE_OBJECT)
				{
					buildView.OnBuildStart();
					return;
				}

				var gridComp = player.GetComponent<InventoryComponentClient>();

				foreach (MaterialDef md in currentRecipe.requiredMaterials)
				{
					buildSystem.CanBuild &= gridComp.GetItemCount(md.itemRef) >= md.amount;
				}

				buildSystem.CanBuild &= !buildView.IsBlocked;

				// if (true) only for test notification message
				if (buildSystem.CanBuild)
				{
					playerView.GetComponentInChildren<PlayerBuildView>().OnBuildStart();
				}
				else
				{
					Debug.LogWarning("Cannot build for some reason!");
				}
			}
		}

		private void OnBuildEnd()
		{
			PlayerBuildSystem buildSystem = m_PlayerController.GetPlayerBuildSystem();
			buildSystem.CanBuild = true;
			var currentRecipe = buildSystem.ActiveBuilding;

			if (currentRecipe != null)
			{
                bool isTile = DataBank.Instance.IsIdOfType<TileData>(currentRecipe.productRef);

				if (isTile)
				{
					GameClient.LocalPlayerView.GetComponentInChildren<PlayerBuildView>().OnBuildEnd();
					return;
				}

				if (DataBank.Instance.IsIdOfType<MountedObjectData>(currentRecipe.productRef))
				{
					return;
				}

				FNEEntityData data = DataBank.Instance.GetData<FNEEntityData>(currentRecipe.productRef);
				if (data.entityType == EntityType.EDGE_OBJECT)
				{
					GameClient.LocalPlayerView.GetComponentInChildren<PlayerBuildView>().OnBuildEnd();
				}
			}
		}

		private void OnCloseBuildMenu()
		{
			UIManager.Instance.ClosePlayerBuilder();
			InputManager.Instance.PopInputLayer();
			HB_Factory.DestroyHoverbox();
		}

		public void SetPlayerController(PlayerController pc)
		{
			m_PlayerController = pc;
		}
	}
}