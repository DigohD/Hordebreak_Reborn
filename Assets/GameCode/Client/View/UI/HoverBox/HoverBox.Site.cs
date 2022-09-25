using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using System;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.Name;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Rooms;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox
{

	public partial class HoverBoxGen
	{
		public void CreateSiteHoverBox(SiteData data)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Id);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.siteName) + " " + Localization.GetString(data.siteTypeRef)
			);

			hb.AddRepeatingconRow(
				SpriteBank.GetSprite("map_hover_box_icon_danger_level_skull_filled"), data.Difficulty
			);

			hb.AddDividerLine(
				Color.gray
			);

			RenderSitePossibleItems(hb, data.possibleLoot);

			hb.AddDividerLine(
				Color.gray
			);

			hb.AddTextItem(
				HB_Text.TextStyle.BREAD_TEXT,
				new Color32(170, 170, 170, 255),
				Localization.GetString(data.flavourText)
			);

			hb.FinishConstruction();
		}
		
		public void CreateMapBaseHoverBox(BaseData data)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, data.Name);
			if (hb == null)
				return;

			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				data.Name
			);

			hb.AddDividerLine(
				Color.gray
			);

			hb.AddTextItem(
				HB_Text.TextStyle.BREAD_TEXT,
				new Color32(170, 170, 170, 255),
				Localization.GetString("string_map_base")
			);

			hb.FinishConstruction();
		}
		
		public void CreateMapPlayerHoverBox(FNEEntity player)
		{
			HB_Main hb = HB_Factory.CreateNewHoverBox(m_Canvas, "Player:" + player.NetId);
			if (hb == null)
				return;

			var name = player.GetComponent<NameComponentClient>().entityName;
			
			hb.AddTextItem(
				HB_Text.TextStyle.HEADER,
				new Color32(170, 170, 170, 255),
				name
			);

			hb.AddDividerLine(
				Color.gray
			);

			hb.AddTextItem(
				HB_Text.TextStyle.BREAD_TEXT,
				new Color32(170, 170, 170, 255),
				Localization.GetString("string_map_player")
			);

			hb.FinishConstruction();
		}

		private Sprite GetItemPossibilityIcon(SiteLootData lootItem)
		{
			switch(lootItem.amountDefinition)
			{
				case AmountDefinition.SCARCE:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_scarce");

				case AmountDefinition.MODERATE:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_moderate");

				case AmountDefinition.ABUNDANT:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_abundance");

				case AmountDefinition.OCCASIONAL:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_occasional");

				case AmountDefinition.EVENT:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_event");

				default:
					return SpriteBank.GetSprite("map_hover_box_icon_loot_level_moderate");
			}
		}

		private void RenderSitePossibleItems(HB_Main hb, List<SiteLootData> loot)
		{
			IconTextIconItem[] itemEntries = new IconTextIconItem[loot.Count];
			for (int i = 0; i < itemEntries.Length; i++)
			{
				ItemData itemData = DataBank.Instance.GetData<ItemData>(loot[i].itemRef);
				itemEntries[i] = new IconTextIconItem(
					Localization.GetString(itemData.nameRef),
					SpriteBank.GetSprite(itemData.iconRef),
					GetItemPossibilityIcon(loot[i])
				);
			}

			hb.AddIconTextIconRow(
				itemEntries,
				new Color32(170, 170, 170, 255)
			);
		}
	}
}