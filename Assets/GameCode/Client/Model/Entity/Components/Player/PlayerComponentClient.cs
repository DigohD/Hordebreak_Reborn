using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Systems;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Player;
using FNZ.Shared.Model.Entity.Components.PlayerViewSetup;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.Model.Entity.Components.Player
{
	public delegate void OnAfkChange(bool afk);
	public delegate void OnCursorItemChange();
	public delegate void OnUnlockBuildingsChange();
	public delegate void OnPlayerDeath(); 
	public delegate void OnPlayerRevival();
	public delegate void OnCurrentSiteChanged(bool isOnSite, string siteId);

	public class PlayerComponentClient : PlayerComponentShared, IInteractableComponent
	{
		private bool m_IsSprinting;

		public OnAfkChange d_AfkChange;
		public OnCursorItemChange d_OnCursorItemChange;
		public OnUnlockBuildingsChange d_OnUnlockBuildingsChange;
		public OnPlayerDeath d_OnPlayerDeath;
		public OnPlayerRevival d_OnPlayerRevival;
		public OnCurrentSiteChanged d_OnCurrentSiteChanged;
		
		private EquipmentSystemComponentClient m_Equipment;
		private ExcavatorComponentClient m_Excavator;

		public override void InitComponentLinks()
		{
			m_Equipment = ParentEntity.GetComponent<EquipmentSystemComponentClient>();
			m_Excavator = ParentEntity.GetComponent<ExcavatorComponentClient>();
		}

		public void SetOpenedInteractable(FNEComponent comp)
		{
			OpenedInteractable = comp;
			UIManager.Instance.d_OnInteractableOpen?.Invoke(comp);
		}

		public void EnableSprinting()
		{
			m_IsSprinting = true;
		}

		public void DisableSprinting()
		{
			m_IsSprinting = false;
		}

		public bool IsSprinting()
		{
			return m_IsSprinting;
		}

		public override void Deserialize(NetBuffer br)
		{
			bool tempAfk = Afk;
			int unlockedBuildingsCount = unlockedBuildings.Count;

			base.Deserialize(br);

			if (tempAfk != Afk)
				d_AfkChange?.Invoke(Afk);
			if (unlockedBuildingsCount != unlockedBuildings.Count)
				d_OnUnlockBuildingsChange?.Invoke();

			d_OnCursorItemChange?.Invoke();

			UIManager.Instance.SetCursorAsItem(ItemOnCursor);
		}

		public List<string> GetUnlockedBuildings()
		{
			return unlockedBuildings;
		}

		public void PrimaryFirePress(PlayerAnimationSystem animSystem)
		{
			if(ItemOnCursor != null)
			{
				NE_Send_RequestThrowItem();
				return;
			}

			if (m_IsSprinting)
				return;

			var activeItem = m_Equipment.GetActiveItem();
			Slot activeActionBarSlot = m_Equipment.ActiveActionBarSlot;

			if (activeItem != null)
			{
				var eqComp = activeItem.GetComponent<ItemEquipmentComponent>();
				eqComp?.OnActivate();
			}
			else if (m_Equipment.ActiveActionBarSlot == Shared.Model.Entity.Components.EquipmentSystem.Slot.Excavator)
			{
				m_Excavator.OnFirePressed();
			}
		}

		public void PrimaryFireRelease(PlayerAnimationSystem animSystem)
		{
			var activeItem = m_Equipment.GetActiveItem();

			m_Excavator.OnFireReleased();

			if (activeItem != null)
			{
				var eqComp = activeItem.GetComponent<ItemEquipmentComponent>();
				eqComp?.OnDeactivate();
			}
		}

		public void SecondaryFirePress(PlayerAnimationSystem animSystem)
		{
			if (m_IsSprinting)
				return;

			if (m_Equipment.ActiveActionBarSlot == Shared.Model.Entity.Components.EquipmentSystem.Slot.Excavator)
			{
				m_Excavator.OnSecondaryFirePressed();
			}
		}

		public void SecondaryFireRelease(PlayerAnimationSystem animSystem)
		{

		}

		public void ReloadActiveWeapon(PlayerAnimationSystem animSystem)
		{
			var activeItem = m_Equipment.GetActiveItem();
			var eqComp = activeItem?.GetComponent<ItemEquipmentComponent>();

			if (eqComp != null && eqComp is ItemWeaponComponent)
			{
				((ItemWeaponComponent)eqComp).Reload();
			}
		}

		

		public void NE_Send_AnimatorData(PlayerAnimatorData data)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.ANIMATOR_DATA,
				data
			);
		}

		public void NE_Send_RevivePlayer()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.REVIVE_PLAYER
			);
		}

		public void NE_Send_RespawnRequest()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.RESPAWN_REQUEST
			);
		}

		public void NE_Send_RequestThrowItem()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.REQUEST_THROW_ITEM
			);
		}

		public void NE_Send_RequestPickUpItem(long identfier)
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(
				this,
				(byte)PlayerNetEvent.REQUEST_PICK_UP_ITEM,
				new RequestPickUpItemData { Identifier = identfier }
			);
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((PlayerNetEvent)incMsg.ReadByte())
			{
				case PlayerNetEvent.ANIMATOR_DATA:
					NE_Receive_AnimatorData(incMsg);
					break;

				case PlayerNetEvent.KILL_PLAYER:
					NE_Receive_KillPlayer(incMsg);
					break;

				case PlayerNetEvent.REVIVE_PLAYER:
					NE_Receive_RevivePlayer(incMsg);
					break;
				
				case PlayerNetEvent.UPDATE_CURRENT_SITE:
					NE_Receive_UpdateCurrentSite(incMsg);
					break;
			}
		}

		private void NE_Receive_AnimatorData(NetIncomingMessage incMsg)
		{
			var data = new PlayerAnimatorData();
			data.Deserialize(incMsg);

			var view = GameClient.ViewConnector.GetGameObject(ParentEntity);

			if (view == null)
			{
				Debug.LogWarning("ENTITY VIEW IS NULL.  NOT YET CREATED FOR ENTITY?");
				return;
			}

			var playerController = view.GetComponent<PlayerControllerRemote>();
			playerController.NetUpdateAnimator(data);
		}

		private void NE_Receive_KillPlayer(NetIncomingMessage incMsg)
		{
			IsDead = true;
			d_OnPlayerDeath?.Invoke();
		}

		private void NE_Receive_RevivePlayer(NetIncomingMessage incMsg)
		{
			IsDead = false;
			var view = GameClient.ViewConnector.GetGameObject(ParentEntity);
			d_OnPlayerRevival?.Invoke();
		}

		private void NE_Receive_UpdateCurrentSite(NetIncomingMessage incMsg)
		{
			var data = new UpdateCurrentSiteData();
			data.Deserialize(incMsg);

			if (m_IsOnSite != data.IsOnSite)
			{
				m_IsOnSite = data.IsOnSite;
				// Player entered site
				if (m_IsOnSite)
				{
					m_CurrentSiteId = data.CurrentSiteId;
				}
				// Player left site
				else
				{
					m_CurrentSiteId = "";
				}
				
				Debug.LogWarning("CLIENT SITE CHANGED " + m_IsOnSite + " : " + m_CurrentSiteId);
				d_OnCurrentSiteChanged?.Invoke(m_IsOnSite, m_CurrentSiteId);
			}
		}
		
		public List<PlayerViewData> GetPlayerViewSetup()
		{
			return m_Data.viewVariations;
		}

		public void OnPlayerInRange()
		{
			Debug.LogWarning("Close to another Player");
		}

		public void OnPlayerExitRange()
		{
			Debug.LogWarning("Left range of another Player");
		}

		public void OnInteract()
		{
			Debug.LogWarning("Reviving another player");
			NE_Send_RevivePlayer();
		}

		public string InteractionPromptMessageRef()
		{
			return "player_revive_interact";
		}

		public bool IsInteractable()
		{
			return IsDead;
		}
	}
}
