using FNZ.Shared.Model.Items;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Model.Entity.Components.Inventory
{
	public class InventoryComponentShared : FNEComponent
	{
		protected Item[] cells = null;

		new public InventoryComponentData m_Data
		{
			get
			{
				return (InventoryComponentData)base.m_Data;
			}
		}

		public override void Init()
		{
			cells = new Item[m_Data.height * m_Data.width];
		}

		public Item GetItemAtCell(int x, int y)
		{
			return GetItemAtCell(new int2(x, y));
		}

		public Item GetItemAtCell(int2 point)
		{
			return cells[point.x + point.y * m_Data.width];
		}

		public Item GetItemOverlapping(Item inItem, int2 cell)
		{
			List<Item> itemsInCells = new List<Item>();

			for (int x = cell.x; x < cell.x + inItem.Data.width; x++)
			{
				for (int y = cell.y; y < cell.y + inItem.Data.height; y++)
				{
					Item item = GetItemAtCell(new int2(x, y));
					if (!itemsInCells.Contains(item) && item != null)
					{
						itemsInCells.Add(item);
					}
				}
			}
			if (itemsInCells.Count > 1)
				throw new System.Exception("Grid inventory component error: Item overlap error");
			else if (itemsInCells.Count == 0)
				return null;

			return itemsInCells[0];
		}

		public int2? GetItemPosition(Item item)
		{
			for (var x = 0; x < m_Data.width; x++)
			{
				for (var y = 0; y < m_Data.height; y++)
				{
					if (cells[x + y * m_Data.width] == item)
					{
						return new int2(x, y);
					}
				}
			}

			return null;
		}

		public bool CanPlaceItem(Item item, int2? cell, bool allowSwap)
		{
			//check that we are in bounds
			if (cell.Value.x < 0 || cell.Value.y < 0 || cell.Value.x + item.Data.width > m_Data.width || cell.Value.y + item.Data.height > m_Data.height)
			{
				return false;
			}

			//check if we overlap more than 1 item
			Item foundItem = null;
			for (int y = cell.Value.y; y < cell.Value.y + item.Data.height; y++)
			{
				for (int x = cell.Value.x; x < cell.Value.x + item.Data.width; x++)
				{
					if (GetItemAtCell(new int2(x, y)) != null && !allowSwap)
					{
						return false;
					}

					if (foundItem == null)
					{
						foundItem = GetItemAtCell(new int2(x, y));
					}
					else if (GetItemAtCell(new int2(x, y)) != null && GetItemAtCell(new int2(x, y)) != foundItem)
					{
						return false;
					}
				}
			}

			return true;
		}

		public Dictionary<Item, int2> GetItemsAndTheirPositions()
		{
			Dictionary<Item, int2> allItemsInGrid = new Dictionary<Item, int2>();
			for (int x = 0; x < m_Data.width; x++)
			{
				for (int y = 0; y < m_Data.height; y++)
				{
					if (GetItemAtCell(x, y) != null && !allItemsInGrid.ContainsKey(GetItemAtCell(x, y)))
					{
						allItemsInGrid.Add(GetItemAtCell(x, y), new int2(x, y));
					}
				}
			}
			return allItemsInGrid;
		}

		public int GetItemCount(string id)
		{
			int counter = 0;
			List<Item> countedItems = new List<Item>();
			for (int x = 0; x < m_Data.width; x++)
			{
				for (int y = 0; y < m_Data.height; y++)
				{
					if (GetItemAtCell(x, y) != null && GetItemAtCell(x, y).Data.Id == id && !countedItems.Contains(GetItemAtCell(x, y)))
					{
						countedItems.Add(GetItemAtCell(x, y));
						counter += GetItemAtCell(x, y).amount;
					}
				}
			}

			return counter;
		}

		//Returns null if no spot was found
		public int2? GetFirstAvailableSlot(Item item)
		{
			for (int y = 0; y <= m_Data.height - item.Data.height; y++)
			{
				for (int x = 0; x <= m_Data.width - item.Data.width; x++)
				{
					int2 point = new int2(x, y);
					if (CanPlaceItem(item, point, false))
					{
						return point;
					}
				}
			}

			return null;
		}

		public override void Serialize(NetBuffer bw)
		{
			//content, beginning with length of item array
			Dictionary<Item, int2> itemsAndPositions = GetItemsAndTheirPositions();
			short count = (short)itemsAndPositions.Keys.Count;
			bw.Write(count);

			foreach (Item item in itemsAndPositions.Keys)
			{
				item.Serialize(bw);

				bw.Write((short)itemsAndPositions[item].x);
				bw.Write((short)itemsAndPositions[item].y);
			}
		}

		public override void Deserialize(NetBuffer br)
		{
			//clear grid from item refs and reinit with new grid size
			cells = new Item[m_Data.height * m_Data.width];

			//place items into new grid
			int count = br.ReadInt16();

			for (int i = 0; i < count; i++)
			{
				Item item = Item.GenerateItem(br);
				int2 position = new int2(br.ReadInt16(), br.ReadInt16());
				SetItemAtPosition(item, position);
			}
		}

		public Item SetItemAtPosition(Item item, int2? topLeftCell)
		{
			for (int x = topLeftCell.Value.x; x < topLeftCell.Value.x + item.Data.width; x++)
			{
				for (int y = topLeftCell.Value.y; y < topLeftCell.Value.y + item.Data.height; y++)
				{
					cells[x + y * m_Data.width] = item;
				}
			}

			return item;
		}

		public override ushort GetSizeInBytes() { return 0; }
	}
}
