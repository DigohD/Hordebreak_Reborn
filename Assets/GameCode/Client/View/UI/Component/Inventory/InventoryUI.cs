using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Component.Inventory
{
	public class InventoryUI : MonoBehaviour
	{
		public Transform CellParent;
		public Transform FrameParent;
		public Transform ItemParent;

		private InventoryComponentClient m_Inventory;
		private PlayerComponentClient m_Player;

		private GameObject[,] m_Cells;
		private int2 m_HoveredCellIndex = new int2(-1, -1);

		public static readonly int CELL_WIDTH = 48;
		public static readonly int HALF_CELL = 24;

		private bool m_Placeable;
		private int2 m_placementIndex;

		private Dictionary<int2, GameObject> m_ItemCellDict = new Dictionary<int2, GameObject>();

		public void Init(InventoryComponentClient inventory)
		{
			m_Inventory = inventory;
			m_Inventory.d_OnInventoryUpdate += ReRender;
			m_Player = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
		}

		void Start()
		{
			BuildUI();
		}

		private void Update()
		{
			if (m_Player.GetItemOnCursor() != null)
			{
				OnCellMove();
			}
		}

		private void BuildUI()
		{
			//int height = m_Inventory.m_Data.height;
			//int width = m_Inventory.m_Data.width;

			int height = 5;
			int width = 10;

			m_Cells = new GameObject[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					GameObject newCell = UIObjectPoolManager.GetObjectInstance(UIObjectType.INVENTORY_ITEM_CELL);
					newCell.transform.SetParent(CellParent, false);
					newCell.transform.localPosition = new Vector2(
						-(width - 1) * HALF_CELL + (x * CELL_WIDTH),
						(height - 1) * HALF_CELL - (y * CELL_WIDTH)
					);

					newCell.name = "CELL: " + x + ", " + y;
					newCell.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0f);

					m_Cells[x, y] = newCell;

					AddEventToCell(newCell, x, y);

					GameObject newFrame = UIObjectPoolManager.GetObjectInstance(UIObjectType.INVENTORY_CELL_FRAME);
					newFrame.transform.SetParent(FrameParent, false);
					newFrame.transform.localPosition = new Vector2(
						-(width - 1) * HALF_CELL + (x * CELL_WIDTH),
						-(height - 1) * HALF_CELL + (y * CELL_WIDTH)
					);
				}
			}

			RenderItems(width, height, null);
			RenderCellColors(width, height);
		}

		private void ReRender(List<int2> changedCells)
		{
			//int height = m_Inventory.m_Data.height;
			//int width = m_Inventory.m_Data.width;

			int height = 5;
			int width = 10;

			foreach (GameObject cell in m_Cells)
			{
				cell.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0f);
			}

			RenderItems(width, height, changedCells);
			RenderCellColors(width, height);

			HB_Factory.DestroyHoverbox();

			if (m_HoveredCellIndex.x >= 0 && m_HoveredCellIndex.x < width && m_HoveredCellIndex.y >= 0 && m_HoveredCellIndex.y < height)
				OnCellHover(m_HoveredCellIndex.x, m_HoveredCellIndex.y);
		}

		private List<GameObject> toRecycle = new List<GameObject>();
		private void RenderItems(int width, int height, List<int2> changedCells)
		{
			toRecycle.Clear();
			if (changedCells != null)
				foreach (var cii in changedCells)
				{
					if (m_ItemCellDict != null && m_ItemCellDict.ContainsKey(cii))
					{
						if (!toRecycle.Contains(m_ItemCellDict[cii]))
						{
							toRecycle.Add(m_ItemCellDict[cii]);
						}
						m_ItemCellDict.Remove(cii);
					}
				}

			foreach (var go in toRecycle)
			{
				UIObjectPoolManager.RecycleObject(UIObjectType.INVENTORY_ITEM, go);

				var SlotParent = (RectTransform) go.transform.GetChild(0).GetChild(1);
				for (int i = 0; i < SlotParent.childCount; i++)
				{
					Transform slotObject = SlotParent.GetChild(i);
					UIObjectPoolManager.RecycleObject(UIObjectType.WEAPON_MOD_SLOT, slotObject.gameObject);
				}
			}

			Dictionary<Item, int2> itemAndPosDict = m_Inventory.GetItemsAndTheirPositions();
			foreach (var item in itemAndPosDict.Keys)
			{
				int2 pos = itemAndPosDict[item];

				if (changedCells != null && !changedCells.Contains(pos))
					continue;

				int2 size = new int2(item.Data.width, item.Data.height);

				GameObject newItem = UIObjectPoolManager.GetObjectInstance(UIObjectType.INVENTORY_ITEM);
				var rectTransform = (RectTransform)newItem.transform;
				newItem.transform.SetParent(ItemParent, false);
				rectTransform.sizeDelta = new Vector2(size.x * CELL_WIDTH, size.y * CELL_WIDTH);
				newItem.transform.localPosition = new Vector2(
					-(width - 1) * HALF_CELL + (pos.x * CELL_WIDTH) - HALF_CELL,
					(height - 1) * HALF_CELL - (pos.y * CELL_WIDTH) + HALF_CELL
				);

				newItem.GetComponentInChildren<Text>().text = item.amount.ToString();

				newItem.transform.GetChild(0).GetComponent<Image>().sprite = SpriteBank.GetSprite(item.Data.iconRef);

				for (int i = pos.x; i < pos.x + size.x; i++)
				{
					for (int j = pos.y; j < pos.y + size.y; j++)
					{
						m_ItemCellDict.Add(new int2(i, j), newItem);
					}
				}

				var SlotParent = (RectTransform) newItem.transform.GetChild(0).GetChild(1);
				for (int i = 0; i < SlotParent.childCount; i++)
				{
					Transform slotObject = SlotParent.GetChild(i);
					slotObject.SetParent(null, false);
					UIObjectPoolManager.RecycleObject(UIObjectType.WEAPON_MOD_SLOT, slotObject.gameObject);
				}
				
				var weaponComp = item.GetComponent<ItemWeaponComponent>();
                if (weaponComp != null)
                {
					RenderWeaponModSlots(newItem, weaponComp, pos.x, pos.y);
                }
			}
		}

		private void RenderWeaponModSlots(GameObject go, ItemWeaponComponent weaponComp, int x, int y)
        {
	        var SlotParent = (RectTransform) go.transform.GetChild(0).GetChild(1);

	        byte counter = 0;
			foreach (var modslot in weaponComp.ModSlots)
			{
				GameObject newSlot = UIObjectPoolManager.GetObjectInstance(UIObjectType.WEAPON_MOD_SLOT);
				var rectTransform = (RectTransform) newSlot.transform;
				newSlot.transform.SetParent(SlotParent, false);

				newSlot.GetComponent<Image>().color = ItemWeaponModComponent.GetModColorColor(modslot.ModColor);

				var itemImage = newSlot.transform.GetChild(0).GetComponent<Image>();
				if (modslot.ModItem != null){
					itemImage.enabled = true;
					itemImage.sprite = SpriteBank.GetSprite(modslot.ModItem.Data.iconRef);
                }
                else
                {
					itemImage.enabled = false;
				}

				AddEventToModSlot(newSlot, x, y, counter, modslot.ModItem);

				if (counter == 0)
                {
                    switch (weaponComp.ModSlots.Count)
                    {
						case 1:
							rectTransform.localPosition = Vector2.zero;
							break;

						case 2:
							rectTransform.localPosition = Vector2.left * 30;
							break;

						case 3:
							rectTransform.localPosition = Vector2.left * 45;
							break;
					}
                }
				else if (counter == 1)
				{
					switch (weaponComp.ModSlots.Count)
					{
						case 2:
							rectTransform.localPosition = Vector2.right * 30;
							break;

						case 3:
							rectTransform.localPosition = Vector2.zero;
							break;
					}
				}
				else if (counter == 2)
				{
					switch (weaponComp.ModSlots.Count)
					{
						case 3:
							rectTransform.localPosition = Vector2.right * 45;
							break;
					}
				}

				counter++;
			}
		}

		private void RenderCellColors(int width, int height)
		{
			Dictionary<Item, int2> itemAndPosDict = m_Inventory.GetItemsAndTheirPositions();
			foreach (var item in itemAndPosDict.Keys)
			{
				int2 pos = itemAndPosDict[item];
				int2 size = new int2(item.Data.width, item.Data.height);

				for (int i = pos.x; i < pos.x + size.x; i++)
				{
					for (int j = pos.y; j < pos.y + size.y; j++)
					{
						m_Cells[i, j].GetComponent<Image>().color = new Color(0.5f, 0.5f, 1, 1);
					}
				}
			}
		}

		// Adding events directly in the for loops leads to weirdness
		private void AddEventToCell(GameObject cell, int x, int y)
		{
			EventTrigger eventTrigger = cell.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnCellClick(x, y, cell);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnCellHover(x, y);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				foreach (GameObject c in m_Cells)
				{
					c.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0f);
				}

				RenderCellColors(10, 5);
				OnCellExit(x, y, cell);
			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnCellClick(int x, int y, GameObject cell)
		{
			Item clicked = m_Inventory.GetItemAtCell(x, y);
			if (clicked != null)
				UIManager.Instance.PlaySound(clicked.Data.pickupSoundRef);

			if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
			{
				if (UnityEngine.Input.GetMouseButton(0))
					m_Inventory.NE_Send_CellShiftClicked(x, y);
				else if (UnityEngine.Input.GetMouseButton(1))
					m_Inventory.NE_Send_CellShiftRightClicked(x, y);
			}
			else
			{
				if (UnityEngine.Input.GetMouseButton(0))
				{
					var cursorItem = m_Player.GetItemOnCursor();
					if (cursorItem != null)
					{
						m_Inventory.NE_Send_CellClicked(m_placementIndex.x, m_placementIndex.y);
						if (!string.IsNullOrEmpty(cursorItem.Data.laydownSoundRef))
							UIManager.Instance.PlaySound(cursorItem.Data.laydownSoundRef);
					}
					else
						m_Inventory.NE_Send_CellClicked(x, y);
				}
				else if (UnityEngine.Input.GetMouseButton(1))
					m_Inventory.NE_Send_CellRightClicked(x, y);
			}
		}

		public void OnCellHover(int x, int y, bool showHoverBox = true)
		{
			Item hovered = m_Inventory.GetItemAtCell(x, y);

			if (hovered != null)
			{
				int2 pos = m_Inventory.GetItemsAndTheirPositions()[hovered];
				int2 size = new int2(hovered.Data.width, hovered.Data.height);

				for (int i = pos.x; i < pos.x + size.x; i++)
				{
					for (int j = pos.y; j < pos.y + size.y; j++)
					{
						m_Cells[i, j].GetComponent<Image>().color = new Color(1, 1, 1, 1);
					}
				}

				if(showHoverBox)
					UIManager.HoverBoxGen.CreateItemHoverBox(hovered);
			}

			m_HoveredCellIndex = new int2(x, y);
		}

		int previousTopLeftX = -1;
		int previousTopLeftY = -1;
		public void OnCellMove()
		{
			if (m_HoveredCellIndex.x > -1)
			{
				Item item = m_Player.GetItemOnCursor();
				GameObject hoveredCell = m_Cells[m_HoveredCellIndex.x, m_HoveredCellIndex.y];

				var RT = (RectTransform)hoveredCell.transform;
				Vector2 delta = UnityEngine.Input.mousePosition - RT.position;

				bool overflowX = delta.x > 0;
				bool overflowY = delta.y < 0;

				int topLeftX = m_HoveredCellIndex.x - (item.Data.width / 2);
				int topLeftY = m_HoveredCellIndex.y - (item.Data.height / 2);

				if (topLeftX == previousTopLeftX && topLeftY == previousTopLeftY)
					return;

				if (item.Data.width % 2 == 0)
				{
					topLeftX += item.Data.width / 2;
					if (!overflowX)
						topLeftX--;
				}
				if (item.Data.height % 2 == 0)
				{
					topLeftY += item.Data.height / 2;
					if (!overflowY)
						topLeftY--;
				}

				int bottomRightX = topLeftX + item.Data.width;
				int bottomRightY = topLeftY + item.Data.height;

				foreach (GameObject cell in m_Cells)
				{
					cell.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0f);
				}

				RenderCellColors(10, 5);

				m_Placeable =
					topLeftX >= 0 &&
					topLeftY >= 0 &&
					topLeftX + item.Data.width <= 10 &&
					topLeftY + item.Data.height <= 5;

				if (topLeftX < 0)
					topLeftX = 0;
				if (topLeftY < 0)
					topLeftY = 0;
				if (bottomRightX > 10)
					bottomRightX = 10;
				if (bottomRightY > 5)
					bottomRightY = 5;

				m_Placeable = m_Placeable && m_Inventory.CanPlaceItem(
					m_Player.GetItemOnCursor(),
					new int2(topLeftX, topLeftY),
					true
				);

				for (int i = topLeftX; i < bottomRightX; i++)
				{
					for (int j = topLeftY; j < bottomRightY; j++)
					{
						m_Cells[i, j].GetComponent<Image>().color = m_Placeable ? new Color(0, 1, 0, 1) : new Color(1, 0, 0, 1);
					}
				}

				m_placementIndex = new int2(topLeftX, topLeftY);
			}
		}

		public void OnCellExit(int x, int y, GameObject cell)
		{
			Item wasHovered = m_Inventory.GetItemAtCell(x, y);

			if (wasHovered != null)
			{
				int2 pos = m_Inventory.GetItemsAndTheirPositions()[wasHovered];
				int2 size = new int2(wasHovered.Data.width, wasHovered.Data.height);

				for (int i = pos.x; i < pos.x + size.x; i++)
				{
					for (int j = pos.y; j < pos.y + size.y; j++)
					{
						m_Cells[i, j].GetComponent<Image>().color = new Color(0.5f, 0.5f, 1, 1);
					}
				}
			}

			m_HoveredCellIndex = new int2(-1, -1);

			HB_Factory.DestroyHoverbox();
		}

		// Adding events directly in the for loops leads to weirdness
		private void AddEventToModSlot(GameObject slot, int x, int y, byte modIndex, Item modItem)
		{
			EventTrigger eventTrigger = slot.GetComponent<EventTrigger>();
			if (eventTrigger != null)
				eventTrigger.triggers.Clear();
			else
				eventTrigger = slot.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
                if (UnityEngine.Input.GetMouseButton(1))
                {
					OnModClick(x, y, modIndex);
				}
                else
                {
					if (m_Player.GetItemOnCursor() == null)
						OnCellClick(x, y, m_Cells[x, y]);
					else
						OnModClick(x, y, modIndex);
				}
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				if(m_Player.GetItemOnCursor() == null)
					OnCellHover(x, y, modItem == null);

				if(modItem != null)
                {
					HB_Factory.DestroyHoverbox();

					UIManager.HoverBoxGen.CreateItemHoverBox(modItem);
				}
			});
			eventTrigger.triggers.Add(entry);
		}

		private void OnModClick(int x, int y, byte modIndex)
        {
			Item clicked = m_Inventory.GetItemAtCell(x, y);
			if (clicked != null)
				UIManager.Instance.PlaySound(clicked.Data.pickupSoundRef);

			if (UnityEngine.Input.GetMouseButton(0))
			{
				var cursorItem = m_Player.GetItemOnCursor();
				if (cursorItem != null)
				{
					m_Inventory.NE_Send_ModClicked(m_placementIndex.x, m_placementIndex.y, modIndex);
					if (!string.IsNullOrEmpty(cursorItem.Data.laydownSoundRef))
						UIManager.Instance.PlaySound(cursorItem.Data.laydownSoundRef);
				}
				else
					m_Inventory.NE_Send_ModClicked(x, y, modIndex);
			}
			else if (UnityEngine.Input.GetMouseButton(1))
				m_Inventory.NE_Send_ModRightClicked(x, y, modIndex);
		}

		public void OnClose()
		{
			m_HoveredCellIndex = new int2(-1, -1);
		}

		private void OnDestroy()
		{
			m_ItemCellDict.Clear();
			m_Inventory.d_OnInventoryUpdate -= ReRender;
		}
	}
}