using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Refinement;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.ItemTransferComponent;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using System.Linq;

namespace FNZ.Server.Model.Entity.Components.ItemTransferComponent
{
	public class ItemTransferComponentServer : ItemTransferComponentShared, ITickable
	{
		private FNEEntity[] m_Neighbours = new FNEEntity[4];

		private InventoryComponentServer[] m_InventoryNeighbours = new InventoryComponentServer[4];
		private RefinementComponentServer[] m_RefinementNeighbours = new RefinementComponentServer[4];

		private const byte REFILL_THRESHHOLD = 15;
		private byte ticker = 0;

		public void Tick(float dt)
		{
			ticker++;

			if (ticker >= m_Data.transferIntervalTicks)
			{
				ticker = 0;
				CheckNeighbours();
				if (m_InventoryNeighbours != null && m_RefinementNeighbours != null)
				{
					CollectOutput();
					FillInput();
					FillBurnable();
				}
			}
		}

		private void CheckNeighbours()
		{
			m_Neighbours[0] = GameServer.MainWorld.GetTileObject((int)ParentEntity.Position.x - 1, (int)ParentEntity.Position.y);
			m_Neighbours[1] = GameServer.MainWorld.GetTileObject((int)ParentEntity.Position.x + 1, (int)ParentEntity.Position.y);
			m_Neighbours[2] = GameServer.MainWorld.GetTileObject((int)ParentEntity.Position.x, (int)ParentEntity.Position.y - 1);
			m_Neighbours[3] = GameServer.MainWorld.GetTileObject((int)ParentEntity.Position.x, (int)ParentEntity.Position.y + 1);

			//assign neighbours
			for (int i = 0; i < m_Neighbours.Length; i++)
			{
				if (m_Neighbours[i] != null)
				{
					var inv = m_Neighbours[i].GetComponent<InventoryComponentServer>();

					if (inv != null)
					{
						m_InventoryNeighbours[i] = inv;
						continue;
					}

					var refine = m_Neighbours[i].GetComponent<RefinementComponentServer>();
					if (refine != null)
					{
						m_RefinementNeighbours[i] = refine;
						continue;
					}
				}
			}

		}

		private void CollectOutput()
		{
			for (int i = 0; i < m_RefinementNeighbours.Length; i++)
			{
				if (m_RefinementNeighbours[i] == null)
					continue;

				var activeRecipe = m_RefinementNeighbours[i].GetActiveRecipe();

				for (int j = 0; j < activeRecipe.producedMaterials.Count; j++)
				{
					if (m_RefinementNeighbours[i].HasOutputItem(j))
					{
						var outputItem = m_RefinementNeighbours[i].CollectOutputItem(j);

						for (int k = 0; k < m_InventoryNeighbours.Length; k++)
						{
							if (m_InventoryNeighbours[k] == null)
								continue;

							if (!m_InventoryNeighbours[k].AutoPlaceIfPossible(outputItem))
								m_RefinementNeighbours[i].ReturnOutputItem(outputItem, j);
						}
					}
				}
			}
		}

		private void FillInput()
		{
			for (int i = 0; i < m_RefinementNeighbours.Length; i++)
			{
				if (m_RefinementNeighbours[i] == null)
					continue;

				var requiredMaterials = m_RefinementNeighbours[i].GetActiveRecipe().requiredMaterials;

				for (int j = 0; j < requiredMaterials.Count; j++)
				{
					var inputItem = m_RefinementNeighbours[i].GetMaterialsInInputSlot(j);

					if (inputItem != null)
					{
						if (inputItem.amount < inputItem.Data.maxStackSize)
						{
							for (int k = 0; k < m_InventoryNeighbours.Length; k++)
							{
								if (m_InventoryNeighbours[k] == null)
									continue;

								if (m_InventoryNeighbours[k].RemoveItemOfId(inputItem.Data.Id, 1))
								{
									m_RefinementNeighbours[i].AddMaterialToInput(j);
									GameServer.NetAPI.Entity_UpdateComponent_BAR(m_InventoryNeighbours[k]);
								}
							}
						}
					}
					else
					{
						for (int k = 0; k < m_InventoryNeighbours.Length; k++)
						{
							if (m_InventoryNeighbours[k] == null)
								continue;

							if (m_InventoryNeighbours[k].RemoveItemOfId(requiredMaterials[j].itemRef, requiredMaterials[j].amount))
							{
								m_RefinementNeighbours[i].AddMaterialToInput(j, Item.GenerateItem(requiredMaterials[j].itemRef, requiredMaterials[j].amount));
								GameServer.NetAPI.Entity_UpdateComponent_BAR(m_InventoryNeighbours[k]);
							}
						}
					}

				}

			}
		}

		private void FillBurnable()
		{
			for (int i = 0; i < m_RefinementNeighbours.Length; i++)
			{
				if (m_RefinementNeighbours[i] == null)
					continue;

				var burnableItem = m_RefinementNeighbours[i].GetBurnableItem();

				if (burnableItem == null)
				{
					var burnGradeRef = m_RefinementNeighbours[i].GetBurnGradeRef();
					bool shouldBreak = false;

					foreach (var itemData in DataBank.Instance.GetAllDataIdsOfType<ItemData>().Where(itemData => itemData.HasComponent<ItemBurnableComponentData>()))
					{
						var burnComp = (ItemBurnableComponentData)itemData.components.Find(c => c is ItemBurnableComponentData);
						if (burnComp.gradeRef != burnGradeRef)
							continue;

						for (int j = 0; j < m_InventoryNeighbours.Length; j++)
						{
							if (m_InventoryNeighbours[j] == null)
								continue;

							if (m_InventoryNeighbours[j].RemoveItemOfId(itemData.Id, 1))
							{
								m_RefinementNeighbours[i].AddBurnable(Item.GenerateItem(itemData.Id));
								GameServer.NetAPI.Entity_UpdateComponent_BAR(m_InventoryNeighbours[j]);
								shouldBreak = true;
								break;
							}
						}


						if (shouldBreak)
							break;
					}
				}
				else
				{
					if (burnableItem.amount < REFILL_THRESHHOLD)
					{
						for (int j = 0; j < m_InventoryNeighbours.Length; j++)
						{
							if (m_InventoryNeighbours[j] == null)
								continue;

							if (m_InventoryNeighbours[j].RemoveItemOfId(burnableItem.Data.Id, 1))
							{
								m_RefinementNeighbours[i].AddBurnable();
								GameServer.NetAPI.Entity_UpdateComponent_BAR(m_InventoryNeighbours[j]);
								break;
							}
						}
					}
				}

			}

		}
	}
}
