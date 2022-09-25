using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components.Crafting;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Lidgren.Network;

namespace FNZ.Server.Model.Entity.Components.Crafting
{
	public class CraftingComponentServer : CraftingComponentShared
	{
		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((CraftingNetEvent)incMsg.ReadByte())
			{
				case CraftingNetEvent.CRAFT:
					NE_Receive_Craft(incMsg);
					break;
			}
		}

		private void NE_Receive_Craft(NetIncomingMessage incMsg)
		{
			var data = new CraftData();
			data.Deserialize(incMsg);

			var craftingPlayer = GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection);
			var inventory = craftingPlayer.GetComponent<InventoryComponentServer>();
			var recipe = DataBank.Instance.GetData<CraftingRecipeData>(data.recipeRef);
			var product = DataBank.Instance.GetData<ItemData>(recipe.productRef);

			bool hasMaterials = true;

			foreach (var material in recipe.requiredMaterials)
			{
				hasMaterials = hasMaterials && inventory.GetItemCount(material.itemRef) >= material.amount * data.amount;
			}

			if (hasMaterials)
			{
				int successCounter = 0;
				int failCounter = 0;				

				for (int i = 0; i < data.amount; i++)
				{
					foreach (var material in recipe.requiredMaterials)
					{
						inventory.RemoveItemOfId(material.itemRef, material.amount);
					}

					var newItem = Item.GenerateItem(product.Id, recipe.productAmount, true);

					if (inventory.AutoPlaceIfPossible(newItem))
						successCounter++;
					else
					{
						foreach (var material in recipe.requiredMaterials)
						{
							inventory.AutoPlaceIfPossible(Item.GenerateItem(material.itemRef, material.amount));
						}

						failCounter++;
					}
				}

				if (successCounter > 0)
				{
					QuestManager.OnCrafting(Item.GenerateItem(product.Id, recipe.productAmount * successCounter));

					GameServer.NetAPI.Notification_SendNotification_STC(
								"craft_notification_icon",
								"blue",
								"false",
								string.Format(
									Localization.GetString("crafted_message"),
									recipe.productAmount * successCounter,
									Localization.GetString(product.nameRef)
								),
								incMsg.SenderConnection
							);
				}

				if (failCounter > 0)
				{
					GameServer.NetAPI.Notification_SendWarningNotification_STC(
						   Localization.GetString("inventory_full_message"),
						   incMsg.SenderConnection
					   );
				}

				GameServer.NetAPI.Entity_UpdateComponent_BA(inventory);
				NE_Send_Craft(data.recipeRef, recipe.productAmount * successCounter);
			}
			else
			{
				if (!hasMaterials)
				{
					GameServer.NetAPI.Notification_SendWarningNotification_STC(
						Localization.GetString("no_material"),
						incMsg.SenderConnection
					);
				}
			}
		}

		public void NE_Send_Craft(string recipeRef, int amount)
		{
			GameServer.NetAPI.BAR_Entity_ComponentNetEvent(
				this,
				(byte)CraftingNetEvent.CRAFT,
				new CraftData()
				{
					recipeRef = recipeRef,
					amount = amount
				}
			);
		}
	}
}
