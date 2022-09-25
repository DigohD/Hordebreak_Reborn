using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Component.Excavator
{

	public class RefuelUI : MonoBehaviour
	{
		[SerializeField]
		private GameObject G_FuelBar;

		[SerializeField]
		private GameObject G_FuelSlot;

		[SerializeField]
		private GameObject G_FuelSlotFrame;

		[SerializeField]
		private Button BTN_RefuelOne;

		[SerializeField]
		private Button BTN_RefuelMax;

		[SerializeField]
		private Text TXT_NameAmount;

		[SerializeField]
		private Text TXT_FuelPerItem;

		[SerializeField]
		private Text TXT_FullRecharge;

		private PlayerComponentClient m_Player;
		private ExcavatorComponentClient m_Excavator;

		private float m_FuelBarWidth, m_FuelBarHeight;

		void Start()
		{
			BindFuelSlotEvents();

			m_FuelBarWidth = ((RectTransform)G_FuelBar.transform.parent).rect.width;
			m_FuelBarHeight = ((RectTransform)G_FuelBar.transform.parent).rect.height;

			ReRenderFuelBar(m_Excavator.GetFuel(), m_Excavator.GetFuel(), m_Excavator.GetMaximumFuel());
			ReRenderRefuelSlot(m_Excavator.RefuelSlot);

			BTN_RefuelMax.onClick.AddListener(() =>
			{
				m_Excavator.NE_Send_RefuelMaxClick();
			});

			BTN_RefuelOne.onClick.AddListener(() =>
			{
				m_Excavator.NE_Send_RefuelOneClick();
			});
		}

		public void Init(ExcavatorComponentClient excavatorComp, PlayerComponentClient playerComp)
		{
			m_Excavator = excavatorComp;
			m_Player = playerComp;

			m_Excavator.d_FuelChange += ReRenderFuelBar;
			m_Excavator.d_RefuelSlotChange += ReRenderRefuelSlot;
			m_Player.d_OnCursorItemChange += ReRenderRefuelSlotFrame;

			ReRenderFuelBar(m_Excavator.GetFuel(), m_Excavator.GetFuel(), m_Excavator.GetMaximumFuel());
			ReRenderRefuelSlot(m_Excavator.RefuelSlot);
		}

		private void ReRenderFuelBar(int fuel, int previousFuel, int maxFuel)
		{
			((RectTransform)G_FuelBar.transform).sizeDelta = new Vector2(m_FuelBarWidth * ((float)fuel / (float)maxFuel), m_FuelBarHeight - 6);
		}

		private void ReRenderRefuelSlot(Item newItem)
		{
			G_FuelSlot.GetComponent<Image>().sprite = newItem == null ? null : SpriteBank.GetSprite(newItem.Data.iconRef);
			G_FuelSlot.GetComponent<Image>().color = newItem == null ? Color.clear : Color.white;

			if (newItem != null)
			{
				var fuelComp = newItem.GetComponent<ItemFuelComponent>();
				TXT_NameAmount.text = newItem.amount + "x " + Localization.GetString(newItem.Data.nameRef);
				TXT_FuelPerItem.text = fuelComp.Data.fuelValue + " fuel per item";

				var fuelDiff = (m_Excavator.GetMaximumFuel() - m_Excavator.GetFuel());
				var maxCharge = fuelDiff / fuelComp.Data.fuelValue;
				maxCharge = fuelDiff % fuelComp.Data.fuelValue == 0 ? maxCharge : maxCharge + 1;

				TXT_FullRecharge.text = maxCharge + " items for full recharge";
			}
			else
			{
				TXT_NameAmount.text = "-";
				TXT_FuelPerItem.text = "-";
				TXT_FullRecharge.text = "-";
			}
		}

		private void ReRenderRefuelSlotFrame()
		{
			var item = m_Player.GetItemOnCursor();
			if (item != null && item.GetComponent<ItemFuelComponent>() != null)
			{
				G_FuelSlotFrame.GetComponent<Image>().color = new Color(0, 1, 0, 1);
			}
			else
			{
				G_FuelSlotFrame.GetComponent<Image>().color = new Color(0, 0, 0, 1);
			}
		}

		private void BindFuelSlotEvents()
		{
			EventTrigger eventTrigger = G_FuelSlot.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnFuelSlotClick();
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnFuelSlotHover();
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				OnFuelSlotExit();
			});
			eventTrigger.triggers.Add(entry);
		}

		private void OnFuelSlotClick()
		{
			m_Excavator.NE_Send_RefuelSlotClick();
		}

		private void OnFuelSlotHover()
		{

		}

		private void OnFuelSlotExit()
		{

		}

		private void OnDestroy()
		{
			m_Excavator.d_FuelChange -= ReRenderFuelBar;
			m_Excavator.d_RefuelSlotChange -= ReRenderRefuelSlot;
			m_Player.d_OnCursorItemChange -= ReRenderRefuelSlotFrame;
		}
	}
}