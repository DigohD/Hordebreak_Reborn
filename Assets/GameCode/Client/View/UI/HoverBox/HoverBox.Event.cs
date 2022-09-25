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
using FNZ.Shared.Model.WorldEvent;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.String;

namespace FNZ.Client.View.UI.HoverBox 
{

	public partial class HoverBoxGen
    {
        public void CreateEventHoverBox(string eventRef, Vector3 position)
        {

            var eventData = DataBank.Instance.GetData<WorldEventData>(eventRef);

            HB_Main hb = CreateNewHoverBox(m_Canvas, eventData);
            if (hb == null)
                return;

            switch(eventData.EventType)
            {
                case WorldEventType.Survival:
                    RenderSurvivalEvent(eventData, hb);
                    break;

                case WorldEventType.DropPod:
                    RenderDropPodEvent(eventData, hb);
                    break;

                default:
                    RenderEmptyEvent(hb);
                    break;
            }

            hb.FinishConstruction();

            hb.SetAnchorMode_WorldObject(position);
        }

        public void RenderEmptyEvent(HB_Main hb)
        {
            hb.AddTextItem(
                HB_Text.TextStyle.HEADER,
                new Color32(170, 170, 170, 255),
                "Empty event"
            );

            hb.AddTextItem(
                HB_Text.TextStyle.BREAD_TEXT,
                new Color32(170, 170, 170, 255),
                "This hould not happen..."
            );
        }

        public void RenderSurvivalEvent(WorldEventData eventData, HB_Main hb)
        {
            hb.AddTextItem(
                HB_Text.TextStyle.HEADER,
                new Color32(170, 170, 170, 255),
                Localization.GetString(eventData.NameRef) + " (" + eventData.Duration + "s)"
            );

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
                HB_Text.TextStyle.SUB_HEADER,
                new Color32(170, 170, 170, 255),
                Localization.GetString("string_difficulty")
            );

            hb.AddRepeatingconRow(
                SpriteBank.GetSprite("map_hover_box_icon_danger_level_skull_filled"), eventData.Difficulty
            );

            hb.AddTextItem(
                HB_Text.TextStyle.BREAD_TEXT,
                new Color32(170, 170, 170, 255),
                Localization.GetString(eventData.DescriptionRef)
            );

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
                HB_Text.TextStyle.SUB_HEADER,
                new Color32(170, 170, 170, 255),
                Localization.GetString("string_rewards")
            );

            RenderEventRewards(eventData, hb);
        }

        public void RenderDropPodEvent(WorldEventData eventData, HB_Main hb)
        {
            hb.AddTextItem(
                HB_Text.TextStyle.HEADER,
                new Color32(170, 170, 170, 255),
                Localization.GetString(eventData.NameRef)
            );

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
                HB_Text.TextStyle.BREAD_TEXT,
                new Color32(170, 170, 170, 255),
                Localization.GetString(eventData.DescriptionRef)
            );
        }

        private void RenderEventRewards(WorldEventData eventData, HB_Main hb)
        {
            IconTextItem[] rewards = new IconTextItem[eventData.Rewards.Count];
            for (int i = 0; i < rewards.Length; i++)
            {
                ItemData itemData = DataBank.Instance.GetData<ItemData>(eventData.Rewards[i]);
                rewards[i] = new IconTextItem(
                    Localization.GetString(itemData.nameRef),
                    SpriteBank.GetSprite(itemData.iconRef)
                );
            }

            hb.AddIconTextRow(
                rewards,
                new Color32(170, 170, 170, 255)
            );
        }
	}
}