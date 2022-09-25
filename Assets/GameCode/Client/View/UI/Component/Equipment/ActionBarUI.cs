using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Component.Equipment
{
	public class ActionBarUI : MonoBehaviour
	{
		[SerializeField] private GameObject G_Weapon1;
		[SerializeField] private GameObject G_Weapon2;
		[SerializeField] private GameObject G_Consumable1;
		[SerializeField] private GameObject G_Consumable2;
		[SerializeField] private GameObject G_Consumable3;

		[SerializeField] private Sprite I_WeaponFrameInactive;
		[SerializeField] private Sprite I_WeaponFrameActive;

		[SerializeField] private Sprite I_ItemFrameInactive;
		[SerializeField] private Sprite I_ItemFrameActive;

		private EquipmentSystemComponentClient m_Equipment;
		private ExcavatorComponentClient m_Excavator;

		private Dictionary<Slot, GameObject> SlotViewDict = new Dictionary<Slot, GameObject>();

		private bool m_IsInitialized = false;

		// Overlay colors
		private readonly Color COLOR_COOLDOWN =new Color(1.0f, 0.53f, 0f, 0.9f);
		private readonly Color COLOR_RELOAD = new Color(0.76f, 0.35f, 0f, 0.9f);
		private readonly Color COLOR_HIDDEN = new Color(0f, 0f, 0f, 0f);

		public void Update()
		{
			if (!m_IsInitialized && GameClient.LocalPlayerEntity != null)
			{
				Init(GameClient.LocalPlayerEntity);
				m_IsInitialized = true;
			}

			if (m_IsInitialized)
			{
				foreach (var slot in SlotViewDict.Keys)
				{
					var itemEqComp = m_Equipment.GetItemInSlot(slot)?.GetComponent<ItemEquipmentComponent>();
					if (itemEqComp != null)
						UpdateItemComp(itemEqComp, slot);
					else if (slot == Slot.Excavator)
						UpdateExcavatorComp();
				}
			}
		}

		private void UpdateItemComp(ItemEquipmentComponent itemEqComp, Slot slot)
		{
			var background = SlotViewDict[slot].transform.GetChild(0).GetComponent<Image>();
			var backgroundOverlay = SlotViewDict[slot].transform.GetChild(1).GetComponent<Image>();
			var text = SlotViewDict[slot].transform.GetChild(4).GetComponent<Text>();
			var textShadow = SlotViewDict[slot].transform.GetChild(5).GetComponent<Text>();

			if (m_Equipment.ActiveActionBarSlot == slot)
			{
				var cdStart = itemEqComp.GetStartCooldownTime();
				var cdTimer = itemEqComp.CooldownTimer;

				backgroundOverlay.fillAmount = 1f - (cdTimer / cdStart);

				if (cdTimer / cdStart <= 0)
				{
					backgroundOverlay.color = COLOR_HIDDEN;
				}
				else
				{
					backgroundOverlay.color = COLOR_COOLDOWN;
				}

				if (itemEqComp is ItemWeaponComponent)
				{
					var weaponComp = (ItemWeaponComponent)itemEqComp;
					if (weaponComp.Reloading)
					{
						var reloadStart = weaponComp.FinalReloadTime;
						var reloadTimer = weaponComp.ReloadTimer;

						backgroundOverlay.fillAmount = 1f - (reloadTimer / reloadStart);

						if (reloadTimer / reloadStart <= 0)
						{
							backgroundOverlay.color = COLOR_HIDDEN;
						}
						else
						{
							backgroundOverlay.color = COLOR_RELOAD;
						}

						text.text = "R";
						textShadow.text = "R";
					}
					else
					{
						text.text = weaponComp.CurrentAmmoInClip.ToString();
						textShadow.text = weaponComp.CurrentAmmoInClip.ToString();
					}
				}
			}
			else
			{
				backgroundOverlay.fillAmount = 0f;
				text.text = string.Empty;
				textShadow.text = string.Empty;
			}
		}

		private void UpdateExcavatorComp()
		{
			var background = SlotViewDict[Slot.Excavator].transform.GetChild(0).GetComponent<Image>();
			var backgroundOverlay = SlotViewDict[Slot.Excavator].transform.GetChild(1).GetComponent<Image>();
			var text = SlotViewDict[Slot.Excavator].transform.GetChild(4).GetComponent<Text>();
			var textShadow = SlotViewDict[Slot.Excavator].transform.GetChild(5).GetComponent<Text>();

			if (m_Equipment.ActiveActionBarSlot == Slot.Excavator)
			{
				var cdStart = m_Excavator.GetCooldownStartTime();
				var cdTimer = m_Excavator.GetCooldownTimer();

				backgroundOverlay.fillAmount = 1f - (cdTimer / cdStart);

				if (cdTimer / cdStart <= 0)
				{
					backgroundOverlay.color = COLOR_HIDDEN;
					background.sprite = I_WeaponFrameActive;
				}
				else
				{
					backgroundOverlay.color = COLOR_COOLDOWN;
					background.sprite = I_WeaponFrameInactive;
				}
			}
			else
			{
				backgroundOverlay.fillAmount = 0f;
				background.sprite = I_WeaponFrameInactive;
				if (text && textShadow)
				{
					text.text = string.Empty;
					textShadow.text = string.Empty;
				}
			}
		}

		public void Init(FNEEntity entity)
		{
			m_Equipment = entity.GetComponent<EquipmentSystemComponentClient>();
			m_Excavator = entity.GetComponent<ExcavatorComponentClient>();

			m_Equipment.d_OnEquipmentUpdate += RerenderItems;
			m_Equipment.d_OnEquipmentUpdate += RerenderFrames;

			BindEvents(Slot.Weapon1, G_Weapon1, I_WeaponFrameActive, I_WeaponFrameInactive);
			BindEvents(Slot.Weapon2, G_Weapon2, I_WeaponFrameActive, I_WeaponFrameInactive);

			BindEvents(Slot.Consumable1, G_Consumable1, I_ItemFrameActive, I_ItemFrameInactive);
			BindEvents(Slot.Consumable2, G_Consumable2, I_ItemFrameActive, I_ItemFrameInactive);
			BindEvents(Slot.Consumable3, G_Consumable3, I_ItemFrameActive, I_ItemFrameInactive);

			RerenderItems();
			RerenderFrames();
		}

		private void BindEvents(Slot slot, GameObject view, Sprite activeSprite, Sprite inactiveSprie)
		{
			SlotViewDict.Add(slot, view);

			// view.GetComponent<Image>().color = COLOR_INACTIVE;
			
			view.transform.GetChild(0).GetComponent<Image>().sprite = inactiveSprie;

			EventTrigger eventTrigger = view.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnSlotClick(slot, view);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnSlotHover(slot, view, activeSprite, inactiveSprie);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				if (slot == m_Equipment.ActiveActionBarSlot)
				{
					view.transform.GetChild(0).GetComponent<Image>().sprite = activeSprite;
				}
				else
				{
					view.transform.GetChild(0).GetComponent<Image>().sprite = inactiveSprie;
				}

			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnSlotClick(Slot slot, GameObject view)
		{
			m_Equipment.NE_Send_ActiveActionBarChange(slot);
		}

		public void OnSlotHover(Slot slot, GameObject view, Sprite activeSprite, Sprite inactiveSprie)
		{
			Item hovered = m_Equipment.GetItemInSlot(slot);

			if (hovered != null && slot != m_Equipment.ActiveActionBarSlot)
			{
				view.transform.GetChild(0).GetComponent<Image>().sprite = activeSprite;
			}
			else if (hovered != null && slot == m_Equipment.ActiveActionBarSlot)
			{
				view.transform.GetChild(0).GetComponent<Image>().sprite = activeSprite;
			}
			else
			{
				view.transform.GetChild(0).GetComponent<Image>().sprite = inactiveSprie;
			}
		}

		private void RerenderItems()
		{
			foreach (var slot in SlotViewDict.Keys)
			{
				var view = SlotViewDict[slot];

				var itemImage = view.transform.GetChild(2).GetComponent<Image>();
				var itemInSlot = m_Equipment.GetItemInSlot(slot);
				if (itemInSlot != null)
				{
					var itemSprite = SpriteBank.GetSprite(itemInSlot.Data.iconRef);
					itemImage.sprite = itemSprite;
					itemImage.color = Color.white;
				}
				else if (slot != Slot.Excavator)
				{
					itemImage.color = COLOR_HIDDEN;
					var text = SlotViewDict[slot].transform.GetChild(4).GetComponent<Text>();
					var textShadow = SlotViewDict[slot].transform.GetChild(5).GetComponent<Text>();
					text.text = "";
					textShadow.text = "";
					view.transform.GetChild(0).GetComponent<Image>().sprite = I_ItemFrameInactive;
					view.transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
				}
			}
		}

		private void RerenderFrames()
		{
			foreach (var slot in SlotViewDict.Keys)
			{
				var viewImage = SlotViewDict[slot].transform.GetChild(0).GetComponent<Image>();

				if (slot == m_Equipment.ActiveActionBarSlot)
				{
					if (slot == Slot.Weapon1 || slot == Slot.Weapon2)
					{
						viewImage.sprite = I_WeaponFrameActive;
					} else
					{
						viewImage.sprite = I_ItemFrameActive;
					}
				}
				else
				{
					if (slot == Slot.Weapon1 || slot == Slot.Weapon2)
					{
						viewImage.sprite = I_WeaponFrameInactive;
					}
					else
					{
						viewImage.sprite = I_ItemFrameInactive;
					}
				}
			}
		}
	}
}