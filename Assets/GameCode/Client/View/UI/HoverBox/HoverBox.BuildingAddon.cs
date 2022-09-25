using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox 
{

	public partial class HoverBoxGen
	{
		public void CreateBuildingAddonHoverBox(BuildingAddonData data)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Id);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.nameRef)
			);

			

			if (data.requiredMaterials.Count > 0)
			{
				hb.AddDividerLine(
					Color.grey
				);

				InventoryComponentClient inventory = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();

				IconTextItem[] entries = new IconTextItem[data.requiredMaterials.Count];
				for (int i = 0; i < entries.Length; i++)
				{
					var color = "<color=#abffc1ff>";
					if (inventory.GetItemCount(data.requiredMaterials[i].itemRef) < data.requiredMaterials[i].amount)
						color = "<color=#ff0000ff>";

					ItemData itemData = DataBank.Instance.GetData<ItemData>(data.requiredMaterials[i].itemRef);
					entries[i] = new IconTextItem(
						color + inventory.GetItemCount(data.requiredMaterials[i].itemRef) + " / " + data.requiredMaterials[i].amount + "</color> " + Localization.GetString(itemData.nameRef),
						SpriteBank.GetSprite(itemData.iconRef)
					);
				}

				hb.AddIconTextRow(
					entries,
					new Color32(170, 170, 170, 255)
				);
			}


			var entityData = DataBank.Instance.GetData<FNEEntityData>(data.productRef);
			if (entityData.roomPropertyRefs.Count > 0)
			{
				hb.AddDividerLine(
					Color.gray
				);

				if (entityData.entityType.Equals("EdgeObject"))
				{
					IconTextItem[] entries = new IconTextItem[entityData.roomPropertyRefs.Count];
					for (int i = 0; i < entries.Length; i++)
					{
						var propertyData = DataBank.Instance.GetData<RoomPropertyData>(entityData.roomPropertyRefs[i]);
						entries[i] = new IconTextItem(
							"<color=#abffc1ff>" + Localization.GetString(propertyData.displayNameRef) + "</color>",
							SpriteBank.GetSprite(propertyData.iconRef)
						);
					}

					hb.AddIconTextRow(
						entries,
						new Color32(170, 170, 170, 255)
					);
				}

				
			}

			hb.FinishConstruction();
		}

		public void CreateRemoveMountHoverBox()
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, "remove_mount_option");
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Localization.GetString("remove_mounted_object")
			);

			hb.FinishConstruction();
		}
	}
}