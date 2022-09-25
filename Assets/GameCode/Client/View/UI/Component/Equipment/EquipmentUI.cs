using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Component.Equipment
{
	public class EquipmentUI : MonoBehaviour
	{
		private PlayerComponentClient m_Player;
		private EquipmentSystemComponentClient m_Equipment;

		private Dictionary<Slot, GameObject> m_SlotViewDict = new Dictionary<Slot, GameObject>();
		private Dictionary<Slot, Sprite> m_SlotIconDict = new Dictionary<Slot, Sprite>();

		[SerializeField]
		private GameObject G_TorsoSlot;
		[SerializeField]
		private GameObject G_HandsSlot;
		[SerializeField]
		private GameObject G_LegsSlot;
		[SerializeField]
		private GameObject G_WaistSlot;
		[SerializeField]
		private GameObject G_BackSlot;
		[SerializeField]
		private GameObject G_FeetSlot;
		[SerializeField]
		private GameObject G_HeadSlot;

		[SerializeField]
		private GameObject G_Weapon1;
		[SerializeField]
		private GameObject G_Weapon2;

		[SerializeField]
		private GameObject G_Trinket1;
		[SerializeField]
		private GameObject G_Trinket2;

		[SerializeField]
		private GameObject G_Consumable1;
		[SerializeField]
		private GameObject G_Consumable2;
		[SerializeField]
		private GameObject G_Consumable3;
		// [SerializeField]
		// private GameObject G_Consumable4;

		private readonly Color COLOR_INACTIVE = new Color(0.1f, 0.1f, 0.1f, 1);
		private readonly Color COLOR_HIGHLIGHT = new Color(0.25f, 0.25f, 0.25f, 1);
		private readonly Color COLOR_ACCEPTABLE = new Color(0.1f, 0.25f, 0.1f, 1);
		private readonly Color COLOR_DENIED = new Color(0.25f, 0.1f, 0.1f, 1);
		private readonly Color COLOR_ACTIVE_BORDER = new Color(0, 0.5f, 0, 1);

		private Sprite m_ItemIcon_Weapon;
		private Sprite m_ItemIcon_Torso;
		private Sprite m_ItemIcon_Legs;
		private Sprite m_ItemIcon_Feet;
		private Sprite m_ItemIcon_Hands;

		public void Start()
		{
			BindEvents(Slot.Torso, G_TorsoSlot);
			BindEvents(Slot.Hands, G_HandsSlot);
			BindEvents(Slot.Legs, G_LegsSlot);
			BindEvents(Slot.Back, G_BackSlot);
			BindEvents(Slot.Feet, G_FeetSlot);
			BindEvents(Slot.Head, G_HeadSlot);
			BindEvents(Slot.Waist, G_WaistSlot);

			BindEvents(Slot.Weapon1, G_Weapon1);
			BindEvents(Slot.Weapon2, G_Weapon2);

			BindEvents(Slot.Trinket1, G_Trinket1);
			BindEvents(Slot.Trinket2, G_Trinket2);

			BindEvents(Slot.Consumable1, G_Consumable1);
			BindEvents(Slot.Consumable2, G_Consumable2);
			BindEvents(Slot.Consumable3, G_Consumable3);
			// BindEvents(Slot.Consumable4, G_Consumable4);

			RerenderItems();
			RerenderFrames();
		}

		public void Init()
		{
			m_Equipment = GameClient.LocalPlayerEntity.GetComponent<EquipmentSystemComponentClient>();
			m_Equipment.d_OnEquipmentUpdate += RerenderItems;
			m_Player = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			m_Player.d_OnCursorItemChange += RerenderFrames;
		}

		private void BindEvents(Slot slot, GameObject view)
		{
			m_SlotViewDict.Add(slot, view);
			m_SlotIconDict.Add(slot, view.transform.GetChild(1).GetComponent<Image>().sprite);

			view.GetComponent<Image>().color = COLOR_INACTIVE;
			view.transform.GetChild(0).GetComponent<Image>().color = COLOR_INACTIVE;

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
				OnSlotHover(slot, view);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				view.transform.GetChild(0).GetComponent<Image>().color = COLOR_INACTIVE;

				HB_Factory.DestroyHoverbox();
			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnSlotClick(Slot slot, GameObject view)
		{
			var itemOnCursor = m_Player.GetItemOnCursor();
			Item clicked = m_Equipment.GetItemInSlot(slot);
			if(UnityEngine.Input.GetKey(KeyCode.LeftShift))
			{
				UIManager.Instance.PlaySound(m_Equipment.GetItemInSlot(slot).Data.laydownSoundRef);
				m_Equipment.NE_Send_SlotShiftClicked(slot);
			}
			else if (itemOnCursor == null && clicked != null)
			{
				UIManager.Instance.PlaySound(clicked.Data.pickupSoundRef);

				if (UnityEngine.Input.GetMouseButtonDown(1))
					m_Equipment.NE_Send_SlotRightClicked(slot);
				else
					m_Equipment.NE_Send_SlotClicked(slot);

			}
			else if (itemOnCursor != null && m_Equipment.CanBePlaced(itemOnCursor, slot))
			{
				UIManager.Instance.PlaySound(m_Player.GetItemOnCursor().Data.laydownSoundRef);
				m_Equipment.NE_Send_SlotClicked(slot);
			}
		}

		public void OnSlotHover(Slot slot, GameObject view)
		{
			Item hovered = m_Equipment.GetItemInSlot(slot);
			var itemOnCursor = m_Player.GetItemOnCursor();

			var backgroundImage = view.transform.GetChild(0).GetComponent<Image>();

			if (hovered != null && itemOnCursor == null)
			{
				backgroundImage.color = COLOR_HIGHLIGHT;

				UIManager.HoverBoxGen.CreateItemHoverBox(hovered);
			}
			else if (hovered == null && itemOnCursor != null)
			{
				if (m_Equipment.CanBePlaced(itemOnCursor, slot))
					backgroundImage.color = COLOR_ACCEPTABLE;
				else
					backgroundImage.color = COLOR_DENIED;
			}
			else
			{
				backgroundImage.color = COLOR_INACTIVE;
			}
		}

		private void RerenderItems()
		{
			foreach (var slot in m_SlotViewDict.Keys)
			{
				var view = m_SlotViewDict[slot];

				var backgroundImage = view.transform.GetChild(0).GetComponent<Image>();
				var itemImage = view.transform.GetChild(1).GetComponent<Image>();

				backgroundImage.color = COLOR_INACTIVE;

				var itemInSlot = m_Equipment.GetItemInSlot(slot);
				if (itemInSlot != null)
				{
					var itemSprite = SpriteBank.GetSprite(itemInSlot.Data.iconRef);
					itemImage.sprite = itemSprite;
					itemImage.color = Color.white;
					itemImage.transform.localScale = Vector3.one;
				}
				else
				{
					itemImage.sprite = m_SlotIconDict[slot];
					itemImage.color = new Color(1, 1, 1, 0.02f);
					itemImage.transform.localScale = Vector3.one * 0.8f;
				}
			}
		}

		private void RerenderFrames()
		{
			var itemOnCursor = m_Player.GetItemOnCursor();

			foreach (var slot in m_SlotViewDict.Keys)
			{
				var view = m_SlotViewDict[slot];

				if (itemOnCursor != null && m_Equipment.CanBePlaced(itemOnCursor, slot))
				{
					view.GetComponent<Image>().color = COLOR_ACTIVE_BORDER;
				}
				else
				{
					view.GetComponent<Image>().color = COLOR_INACTIVE;
				}
			}
		}

		private void OnDestroy()
		{
			m_Equipment.d_OnEquipmentUpdate -= RerenderItems;
			m_Player.d_OnCursorItemChange -= RerenderFrames;
		}
	}
}