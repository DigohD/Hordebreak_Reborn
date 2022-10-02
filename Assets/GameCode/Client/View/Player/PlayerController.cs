using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.Input;
using FNZ.Client.View.Player.Atmosphere;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.Player.Excavator;
using FNZ.Client.View.Player.PlayerStitching;
using FNZ.Client.View.Player.Systems;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using System;
using FNZ.Client.View.Player.Items;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.Player
{
	public class PlayerController : MonoBehaviour
	{
		public FNEEntity m_PlayerEntity;

		public readonly float Radius = 0.2f;

		private PlayerDefaultInputLayer m_BaseInputLayer;
		private UIInputLayer m_UInputLayer;

		private PlayerComponentClient m_PlayerComponentClient;
		private EquipmentSystemComponentClient m_Equipment;
		private ExcavatorComponentClient m_Excavator;

		private PlayerCollisionSystem m_PlayerCollisionSystem;
		private PlayerInteractionSystem m_PlayerInteractionSystem;

		private PlayerMovementSystem m_PlayerMovementSystem;
		private PlayerAnimationSystem m_PlayerAnimationSystem;
		private PlayerNetworkSystem m_PlayerNetworkSystem;
		private PlayerBuildSystem m_PlayerBuildSystem;
		private PlayerAimingSystem m_PlayerAimingSystem;

		private Text m_HealthBarText;
		private Image m_HealthBarImage;
		private SpriteRenderer m_HealthCircle;

		private PlayerMeshStitcher m_PlayerMeshStitcher;

		private bool m_IsInitialized = false;

		private ExcavatorView m_ExcavatorView;
        [SerializeField]
        private GameObject P_OutlinedObject;

		void Start()
		{
			m_BaseInputLayer = InputLayerStorage.GetInputLayer<PlayerDefaultInputLayer>();
			m_BaseInputLayer.SetPlayerController(this);
			InputManager.Instance.PushInputLayer(m_BaseInputLayer);

			m_BaseInputLayer.BindAction(ActionIdentifiers.MOVE_FORWARD, InputActionType.HOLD, OnMoveForward);
			m_BaseInputLayer.BindAction(ActionIdentifiers.MOVE_BACK, InputActionType.HOLD, OnMoveBackward);
			m_BaseInputLayer.BindAction(ActionIdentifiers.MOVE_RIGHT, InputActionType.HOLD, OnMoveRight);
			m_BaseInputLayer.BindAction(ActionIdentifiers.MOVE_LEFT, InputActionType.HOLD, OnMoveLeft);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SPRINT, InputActionType.PRESS, OnSprintPressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SPRINT, InputActionType.RELEASE, OnSprintReleased);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SHOOT_PRIMARY, InputActionType.PRESS, OnShootPressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SHOOT_PRIMARY, InputActionType.RELEASE, OnShootReleased);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SHOOT_SECONDARY, InputActionType.PRESS, OnShootSecondaryPressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_SHOOT_SECONDARY, InputActionType.RELEASE, OnShootSecondaryReleased);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_RELOAD, InputActionType.PRESS, OnReloadPressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_RELOAD, InputActionType.RELEASE, OnReloadReleased);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, InputActionType.PRESS, OnToggleInventory);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU, InputActionType.PRESS, OnToggleBuilder);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_TOGGLE_WORLD_MAP, InputActionType.PRESS, OnToggleWorldMap);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_TOGGLE_META_WORLD, InputActionType.PRESS, OnToggleMetaWorld);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_OPEN_CHAT, InputActionType.PRESS, OnOpenChat);

			//m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_OPEN_BUILDING_MENU, InputActionType.PRESS, OnBuildMenuOpen);
			//m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_OPEN_CRAFTING_MENU, InputActionType.PRESS, OnCraftingMenuOpen);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_INTERACT, InputActionType.PRESS, OnInteractPressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_INTERACT, InputActionType.RELEASE, OnInteractReleased);
			//m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_OPEN_MAP_OVERVIEW, InputActionType.PRESS, OnMapToggle);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_EXCAVATOR_SLOT, InputActionType.PRESS, OnExcavatorSlot);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_WEAPON1_SLOT, InputActionType.PRESS, OnWeaponSlot1);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_WEAPON2_SLOT, InputActionType.PRESS, OnWeaponSlot2);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT1, InputActionType.PRESS, OnConsumable1Pressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT2, InputActionType.PRESS, OnConsumable2Pressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT3, InputActionType.PRESS, OnConsumable3Pressed);
			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_HOTBAR_SLOT4, InputActionType.PRESS, OnConsumable4Pressed);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_FLASHLIGHT, InputActionType.PRESS, OnFlashLightInteract);

			m_BaseInputLayer.BindAction(ActionIdentifiers.ACTION_EXIT, InputActionType.PRESS, OnExit);

			m_PlayerComponentClient = m_PlayerEntity.GetComponent<PlayerComponentClient>();
			m_Equipment = m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>();
			m_Excavator = m_PlayerEntity.GetComponent<ExcavatorComponentClient>();
			m_PlayerMeshStitcher.SetDefaultMeshes(m_PlayerComponentClient.GetPlayerViewSetup(), 0);
			m_PlayerMeshStitcher.InitStitch(m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>());

			m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>().d_OnActiveItemChange += OnActiveItemChange;
			m_PlayerComponentClient.d_OnPlayerDeath += OnPlayerDeath;
			m_PlayerComponentClient.d_OnPlayerRevival += OnPlayerRevival;
		}

		private void OnFlashLightInteract()
		{

		}

		private void OnMapToggle()
		{

		}

		private void OnExit()
		{
			UIManager.Instance.ToggleMainMenu();
		}

		public void Init(FNEEntity playerEntity)
		{
			m_PlayerEntity = playerEntity;

			m_PlayerCollisionSystem = new PlayerCollisionSystem(this);
			m_PlayerInteractionSystem = new PlayerInteractionSystem(this, P_OutlinedObject);

			m_PlayerMovementSystem = new PlayerMovementSystem(this);
			m_PlayerAnimationSystem = new PlayerAnimationSystem(this);
			m_PlayerNetworkSystem = new PlayerNetworkSystem(this);
			m_PlayerBuildSystem = new PlayerBuildSystem();
			m_PlayerAimingSystem = new PlayerAimingSystem(this);

			m_HealthCircle = gameObject.GetComponentInChildren<SpriteRenderer>();

			m_IsInitialized = true;

			m_PlayerMeshStitcher = gameObject.GetComponentInChildren<PlayerMeshStitcher>();
			m_PlayerMeshStitcher.Init(playerEntity);

			m_ExcavatorView = GetComponent<ExcavatorView>();
			m_ExcavatorView.Init(m_PlayerEntity);

            GetComponent<PlayerBuildView>().Init(m_PlayerEntity);
            GetComponent<PlayerItemTargetingView>().Init(m_PlayerEntity);
            var sfxHandler = gameObject.AddComponent<PlayerAtmosphereSFX>();
            sfxHandler.Init(m_PlayerEntity);
		}

		void Update()
		{
			if (m_IsInitialized && !m_PlayerComponentClient.IsDead)
			{
				m_PlayerCollisionSystem.Update();
				m_PlayerInteractionSystem.Update();
				m_PlayerMovementSystem.Update();
				m_PlayerNetworkSystem.Update();
				m_PlayerAnimationSystem.Update();

				m_PlayerMovementSystem.ResetVelocityVector();

				m_Equipment.Tick(Time.deltaTime);
				m_Excavator.Tick(Time.deltaTime);
			}
		}

		/* Note! In order for the aiming system to work properly (in its current state) it is required to update it in LateUpdate.
		void LateUpdate()
        {
            if (m_IsInitialized)
            {
                m_PlayerAimingSystem.Update();
            }
        }
		*/

		private void UpdateHealthIndicators(float health, float previousHealth, float maxHealth)
		{
			float percentHealth = health / maxHealth;
			float percentLost = Mathf.Abs((health - previousHealth) / maxHealth);

			Color.RGBToHSV(m_HealthCircle.color, out float hue, out float saturation, out float value);

			if (health <= 0)
			{
				m_HealthCircle.color = Color.HSVToRGB(hue, saturation, 0);
				m_HealthCircle.transform.localScale = new Vector3(5, 5, 1);
			}
			else
			{
				float circleSize = FNEUtil.ScaleValueFloat(health, 0, 100, 2, 5);

				m_HealthCircle.color = Color.HSVToRGB(FNEUtil.ScaleValueFloat(health, 0, 100, 0, 0.333f), saturation, value);
				m_HealthCircle.transform.localScale = new Vector3(circleSize, circleSize, 1);
			}
		}

		public FNEEntity GetPlayerEntity()
		{
			return m_PlayerEntity;
		}

		public PlayerMovementSystem GetPlayerMovementSystem()
		{
			return m_PlayerMovementSystem;
		}

		public PlayerAnimationSystem GetPlayerAnimationSystem()
		{
			return m_PlayerAnimationSystem;
		}

		public PlayerBuildSystem GetPlayerBuildSystem()
		{
			return m_PlayerBuildSystem;
		}

		public PlayerAimingSystem GetPlayerAimingSystem()
		{
			return m_PlayerAimingSystem;
		}

		private void OnShootPressed()
		{
			m_PlayerComponentClient.PrimaryFirePress(m_PlayerAnimationSystem);
		}

		private void OnShootReleased()
		{
			m_PlayerComponentClient.PrimaryFireRelease(m_PlayerAnimationSystem);
		}

		private void OnShootSecondaryPressed()
		{
			m_PlayerComponentClient.SecondaryFirePress(m_PlayerAnimationSystem);
		}

		private void OnShootSecondaryReleased()
		{
			m_PlayerComponentClient.SecondaryFireRelease(m_PlayerAnimationSystem);
		}

		private void OnReloadPressed()
		{
			m_PlayerComponentClient.ReloadActiveWeapon(m_PlayerAnimationSystem);

		}

		private void OnReloadReleased()
		{

		}

		private void OnSprintPressed()
		{
			m_PlayerComponentClient.EnableSprinting();
		}

		private void OnSprintReleased()
		{
			m_PlayerComponentClient.DisableSprinting();
		}

		public void OnMoveForward()
		{
			m_PlayerMovementSystem.UpdateTargetVelocity((Vector3.up + Vector3.right).normalized);
		}

		public void OnMoveBackward()
		{
			m_PlayerMovementSystem.UpdateTargetVelocity((Vector3.down + Vector3.left).normalized);
		}

		public void OnMoveRight()
		{
			m_PlayerMovementSystem.UpdateTargetVelocity((Vector3.down + Vector3.right).normalized);
		}

		public void OnMoveLeft()
		{
			m_PlayerMovementSystem.UpdateTargetVelocity((Vector3.up + Vector3.left).normalized);
		}

		private void OnToggleInventory()
		{
			UIManager.Instance.TogglePlayerInventory();
		}

		private void OnToggleBuilder()
		{
			UIManager.Instance.TogglePlayerBuilder();
		}

		private void OnOpenChat()
		{
			UIManager.Instance.ToggleChat();
		}

		private void OnExcavatorSlot()
		{
			UIManager.Instance.OpenPlayerExtract(m_Equipment.ActiveActionBarSlot);
		}

		private void OnWeaponSlot1()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Weapon1)
				m_Equipment.NE_Send_ActiveActionBarChange(Slot.Weapon1);
		}

		private void OnWeaponSlot2()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Weapon2)
				m_Equipment.NE_Send_ActiveActionBarChange(
					Slot.Weapon2
				);
		}

		private void OnInteractPressed()
		{
			m_PlayerInteractionSystem.OnInteract();
		}

		private void OnInteractReleased()
		{

		}

		private void OnConsumable1Pressed()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Consumable1)
				m_Equipment.NE_Send_ActiveActionBarChange(Slot.Consumable1);
		}

		private void OnConsumable2Pressed()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Consumable2)
				m_Equipment.NE_Send_ActiveActionBarChange(Slot.Consumable2);
		}

		private void OnConsumable3Pressed()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Consumable3)
				m_Equipment.NE_Send_ActiveActionBarChange(Slot.Consumable3);
		}

		private void OnConsumable4Pressed()
		{
			if (m_Equipment.ActiveActionBarSlot != Slot.Consumable4)
				m_Equipment.NE_Send_ActiveActionBarChange(Slot.Consumable4);
		}

		public void OnToggleBaseView()
		{

			UIManager.Instance.ToggleBaseViewer();
		}
		
		public void OnToggleWorldMap()
		{
			UIManager.Instance.ToggleWorldMapUI();
		}

		public void OnToggleMetaWorld()
		{
			UIManager.Instance.ToggleMetaWorldUI();
		}

		private void OnActiveItemChange(Item item, Slot slot)
		{
			if (slot == Slot.Excavator)
			{
				m_PlayerAnimationSystem.SetCurrentWeaponType(WeaponPosture.HEAVY);
				m_PlayerAnimationSystem.SetChangeWeapon(true);

                m_ExcavatorView.OnExcavatorActivation();
                return;
            }
            else
            {
                m_Excavator.OnExcavatorDeactivation();
                m_ExcavatorView.OnExcavatorDeactivation();
            }

			var activeSlot = m_Equipment.ActiveActionBarSlot;
			if (slot == Slot.Weapon1 || slot == Slot.Weapon2)
			{
				if (slot == activeSlot && item != null)
				{
					OnEquipWeapon((ItemWeaponComponentData)item.Data.GetComponentData<ItemWeaponComponentData>());
				}
				else
				{
					m_PlayerAnimationSystem.SetCurrentWeaponType(WeaponPosture.UNARMED);
					m_PlayerAnimationSystem.SetChangeWeapon(true);
				}
			}else if (slot >= Slot.Consumable1 || slot <= Slot.Consumable4)
			{
				if (slot == activeSlot && item != null)
				{
					OnEquipConsumables((ItemConsumableComponentData)item.Data.GetComponentData<ItemConsumableComponentData>());
				}
				else
				{
					m_PlayerAnimationSystem.SetCurrentWeaponType(WeaponPosture.UNARMED);
					m_PlayerAnimationSystem.SetChangeWeapon(true);
				}
			}
		}

		private void OnEquipWeapon(ItemWeaponComponentData data)
		{
			if (data == null)
			{
				throw new Exception("ERROR: A weapon was equipped which does not have an attached ItemWeaponComponent!");
			}

			m_PlayerAnimationSystem.SetCurrentWeaponType(data.weaponPosture);
			m_PlayerAnimationSystem.SetChangeWeapon(true);
		}

		private void OnEquipConsumables(ItemConsumableComponentData data)
		{
			if (data == null)
			{
				throw new Exception("ERROR: A Consumable was equipped which does not have an attached ItemConsumableComponent!");
			}

			m_PlayerAnimationSystem.SetCurrentWeaponType(data.weaponPosture);
			m_PlayerAnimationSystem.SetChangeWeapon(true);
		}

		private void OnPlayerDeath()
		{
			m_PlayerAnimationSystem.PlayOneShotAnimation(OneShotAnimationType.Death);
		}

		private void OnPlayerRevival()
		{
			m_PlayerAnimationSystem.PlayOneShotAnimation(OneShotAnimationType.Revive);
		}
	}
}