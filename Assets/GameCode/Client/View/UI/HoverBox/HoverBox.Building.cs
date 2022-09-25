using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;
using FNZ.Shared.Model.Entity.Components.Producer;
using FNZ.Shared.Model.Entity.Components.Consumer;
using FNZ.Shared.Model.World.Environment;
using FNZ.Client.Model.Entity.Components.Inventory;

namespace FNZ.Client.View.UI.HoverBox
{
	public partial class HoverBoxGen
	{
		public void CreateBuildingHoverBox(BuildingData data)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Id);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.nameRef)
			);

			hb.AddDividerLine(
				Color.gray
			);

			RenderMaterialsList(hb, data.requiredMaterials);

			var isTile = DataBank.Instance.IsIdOfType<TileData>(data.productRef);
			if (isTile)
			{
				var tileData = DataBank.Instance.GetData<TileData>(data.productRef);
                if(tileData.roomPropertyRefs.Count > 0)
                {
					RenderRoomProperties(hb, tileData.roomPropertyRefs);
				}
            }
			else if (DataBank.Instance.IsIdOfType<MountedObjectData>(data.productRef))
			{
				var mountedData = DataBank.Instance.GetData<MountedObjectData>(data.productRef);

				RenderMountedObject(hb, mountedData);
			}
			else
			{
                var entityData = DataBank.Instance.GetData<FNEEntityData>(data.productRef);
                if (entityData.roomPropertyRefs.Count > 0)
                {
					RenderRoomProperties(hb, entityData.roomPropertyRefs);
				}

				RenderComponents(hb, entityData);
			}

			hb.AddDividerLine(
				Color.gray
			);

			hb.AddTextItem(
				HB_Text.TextStyle.BREAD_TEXT,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.descriptionRef)
			);

			hb.FinishConstruction();
		}

		public void CreateBuildingCategoryHoverBox(BuildingCategoryData data)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Id);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.nameRef)
			);

			hb.FinishConstruction();
		}

		public void CreateWallCostHoverBox(BuildingData data, int wallCount, bool hasMaterials)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, "BUILD_MODE" + wallCount);

			if (hb == null)
				return;

			string colorString = hasMaterials ? "<color=#abffc1ff>" : "<color=#ff0000ff>";

			IconTextItem[] entries = new IconTextItem[data.requiredMaterials.Count];
			for (int i = 0; i < entries.Length; i++)
			{
				ItemData itemData = DataBank.Instance.GetData<ItemData>(data.requiredMaterials[i].itemRef);
				entries[i] = new IconTextItem(
					colorString + (data.requiredMaterials[i].amount * wallCount) + "x</color> " + Localization.GetString(itemData.nameRef),
					SpriteBank.GetSprite(itemData.iconRef)
				);
			}

			hb.AddIconTextRow(
				entries,
				new Color32(170, 170, 170, 255)
			);

			hb.FinishConstruction();
		}

		private void RenderMaterialsList(HB_Main hb, List<MaterialDef> materials)
		{
			var inventory = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();

			
			IconTextItem[] entries = new IconTextItem[materials.Count];
			for (int i = 0; i < entries.Length; i++)
			{
				var color = "<color=#abffc1ff>";
				if (inventory.GetItemCount(materials[i].itemRef) < materials[i].amount)
					color = "<color=#ff0000ff>";

				ItemData itemData = DataBank.Instance.GetData<ItemData>(materials[i].itemRef);
				entries[i] = new IconTextItem(
					color + inventory.GetItemCount(materials[i].itemRef) + " / " + materials[i].amount + "</color> " + Localization.GetString(itemData.nameRef),
					SpriteBank.GetSprite(itemData.iconRef)
				);
			}

			hb.AddIconTextRow(
				entries,
				new Color32(170, 170, 170, 255)
			);
		}

		private void RenderMountedObject(HB_Main hb, MountedObjectData mountedData)
		{
			var resources = mountedData.resourceTransfers;
			if(resources.Count > 0)
			{
				IconTextItem[] entries = new IconTextItem[resources.Count];

				hb.AddDividerLine(
					Color.gray
				);

				hb.AddTextItem(
					HB_Text.TextStyle.SUB_HEADER,
					new Color32(170, 170, 170, 255),
					Localization.GetString("transfers_resources")
				);

				for (int i = 0; i < entries.Length; i++)
				{
					var resData = DataBank.Instance.GetData<RoomResourceData>(resources[i].resourceRef);
					entries[i] = new IconTextItem(
						"<color=#abffc1ff>" + resources[i].amount + " </color>" + Localization.GetString(resData.nameRef),
						SpriteBank.GetSprite(resData.iconRef)
					);
				}

				hb.AddIconTextRow(
					entries,
					new Color32(170, 170, 170, 255)
				);
			}

			var envs = mountedData.environmentTransfers;
			if(envs.Count > 0)
			{
				IconTextItem[] entries = new IconTextItem[envs.Count];

				hb.AddDividerLine(
					Color.gray
				);

				hb.AddTextItem(
					HB_Text.TextStyle.SUB_HEADER,
					new Color32(170, 170, 170, 255),
					Localization.GetString("transfers_environment")
				);

				for (int i = 0; i < entries.Length; i++)
				{
					var envData = DataBank.Instance.GetData<EnvironmentData>(envs[i].typeRef);
					entries[i] = new IconTextItem(
						"<color=#abffc1ff>" + envs[i].amount + " </color>" + Localization.GetString(envData.nameRef),
						SpriteBank.GetSprite(envData.iconRef)
					);
				}

				hb.AddIconTextRow(
					entries,
					new Color32(170, 170, 170, 255)
				);
			}
			
		}
	}
}