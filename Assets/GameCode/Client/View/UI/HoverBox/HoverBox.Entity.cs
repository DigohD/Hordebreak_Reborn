using FNZ.Client.Model.Entity.Components.Consumer;
using FNZ.Client.Model.Entity.Components.Producer;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Consumer;
using FNZ.Shared.Model.Entity.Components.Crafting;
using FNZ.Shared.Model.Entity.Components.Crop;
using FNZ.Shared.Model.Entity.Components.Environment;
using FNZ.Shared.Model.Entity.Components.Inventory;
using FNZ.Shared.Model.Entity.Components.Producer;
using FNZ.Shared.Model.Entity.Components.Refinement;
using FNZ.Shared.Model.Entity.Components.RoomRequirements;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox 
{

	public partial class HoverBoxGen
	{
		public void RenderRoomProperties(HB_Main hb, List<string> roomPropertyRefs)
		{
            IconTextItem[] entries = new IconTextItem[roomPropertyRefs.Count];

            hb.AddDividerLine(
                Color.gray
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var propertyData = DataBank.Instance.GetData<RoomPropertyData>(roomPropertyRefs[i]);
                entries[i] = new IconTextItem(
                    "<color=#00ffffff>" + Localization.GetString(propertyData.displayNameRef) + "</color>",
                    SpriteBank.GetSprite(propertyData.iconRef)
                );
            }

            hb.AddIconTextRow(
                entries,
                new Color32(170, 170, 170, 255)
            );
        }

        public void RenderComponents(HB_Main hb, FNEEntityData entityData)
        {
            RenderProducerComponent(hb, entityData.GetComponentData<ProducerComponentData>());
            RenderConsumerComponent(hb, entityData.GetComponentData<ConsumerComponentData>());
            RenderEnvironmentComponent(hb, entityData.GetComponentData<EnvironmentComponentData>());
            RenderInventoryComponent(hb, entityData.GetComponentData<InventoryComponentData>());
            RenderCraftingComponent(hb, entityData.GetComponentData<CraftingComponentData>());
            RenderRefinementComponent(hb, entityData.GetComponentData<RefinementComponentData>());
            RenderRoomRequirementsComponent(hb, entityData.GetComponentData<RoomRequirementsComponentData>());
            RenderCropComponent(hb, entityData.GetComponentData<CropComponentData>());
        }

        private void RenderProducerComponent(HB_Main hb, ProducerComponentData prodCompData)
        {
            if (prodCompData == null)
                return;

            var resources = prodCompData.resources;
            IconTextItem[] entries = new IconTextItem[resources.Count];

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               Localization.GetString("produces_resources")
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var resourceData = DataBank.Instance.GetData<RoomResourceData>(resources[i].resourceRef);
                entries[i] = new IconTextItem(
                    "<color=#00ff00ff>+" + resources[i].amount + " </color>" + Localization.GetString(resourceData.nameRef),
                    SpriteBank.GetSprite(resourceData.iconRef)
                );
            }

            hb.AddIconTextRow(
                entries,
                new Color32(170, 170, 170, 255)
            );
        }

        private void RenderConsumerComponent(HB_Main hb, ConsumerComponentData consumeCompData)
        {
            if (consumeCompData == null)
                return;

            var resources = consumeCompData.resources;
            IconTextItem[] entries = new IconTextItem[resources.Count];

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
                HB_Text.TextStyle.SUB_HEADER, 
                Color.grey, 
                Localization.GetString("consumes_resources")
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var resourceData = DataBank.Instance.GetData<RoomResourceData>(resources[i].resourceRef);
                entries[i] = new IconTextItem(
                    "<color=#ff0000ff>-" + resources[i].amount + " </color>" + Localization.GetString(resourceData.nameRef),
                    SpriteBank.GetSprite(resourceData.iconRef)
                );
            }

            hb.AddIconTextRow(
                entries,
                new Color32(170, 170, 170, 255)
            );
        }

        private void RenderEnvironmentComponent(HB_Main hb, EnvironmentComponentData envCompData)
        {
            if (envCompData == null)
                return;

            var envs = envCompData.environment;
            IconTextItem[] entries = new IconTextItem[envs.Count];

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               Localization.GetString("generates_environment")
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var envData = DataBank.Instance.GetData<EnvironmentData>(envs[i].typeRef);
                entries[i] = new IconTextItem(
                    "<color=#ffff00ff>+" + envs[i].amount + " </color>" + Localization.GetString(envData.nameRef),
                    SpriteBank.GetSprite(envData.iconRef)
                );
            }

            hb.AddIconTextRow(
                entries,
                new Color32(170, 170, 170, 255)
            );
        }

        private void RenderCropComponent(HB_Main hb, CropComponentData cropCompData)
        {
            if (cropCompData == null)
                return;

            hb.AddDividerLine(Color.grey);

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               Localization.GetString("crop_header")
            );
            
            if(cropCompData.environmentSpans.Count > 0)
            {
                hb.AddDividerLine(Color.grey);
                hb.AddTextItem(HB_Text.TextStyle.SUB_HEADER, Color.grey, Localization.GetString("crop_conditions"));

                IconTextItem[] entries = new IconTextItem[cropCompData.environmentSpans.Count];

                for (int i = 0; i < entries.Length; i++)
                {
                    var spanData = cropCompData.environmentSpans[i];
                    var envData = DataBank.Instance.GetData<EnvironmentData>(spanData.environmentRef);
                    entries[i] = new IconTextItem(
                        "<color=#00ffffff>" + spanData.lowPoint + "</color> <color=#00ff00ff>-</color> <color=#ff7700ff>" + spanData.highPoint + "</color>",
                        SpriteBank.GetSprite(envData.iconRef)
                    );
                }

                hb.AddIconTextRow(
                    entries,
                    new Color32(170, 170, 170, 255)
                );
            }

            if(cropCompData.matureEntityRef != null)
            {
                var matureData = DataBank.Instance.GetData<FNEEntityData>(cropCompData.matureEntityRef);
                if(matureData.components.Count > 0)
                {
                    hb.AddDividerLine(Color.magenta);
                    hb.AddTextItem(HB_Text.TextStyle.SUB_HEADER, Color.magenta, Localization.GetString("when_crop_matures"));

                    RenderComponents(hb, matureData);
                }
            }
        }

        private void RenderInventoryComponent(HB_Main hb, InventoryComponentData inventoryCompData)
        {
            if (inventoryCompData == null)
                return;

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               "<color=#00ff00ff>" + inventoryCompData.width + "x" + inventoryCompData.height + "</color> " + Localization.GetString("has_storage")
            );
        }

        private void RenderCraftingComponent(HB_Main hb, CraftingComponentData craftingCompData)
        {
            if (craftingCompData == null)
                return;

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               "<color=#00ff00ff>" + craftingCompData.recipes.Count + "</color> " + Localization.GetString("has_crafting_recipes")
            );
        }

        private void RenderRefinementComponent(HB_Main hb, RefinementComponentData refinementCompData)
        {
            if (refinementCompData == null)
                return;

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
               HB_Text.TextStyle.SUB_HEADER,
               Color.grey,
               "<color=#00ff00ff>" + refinementCompData.recipes.Count + "</color> " + Localization.GetString("has_refinement_recipes")
            );
        }

        private void RenderRoomRequirementsComponent(HB_Main hb, RoomRequirementsComponentData requirementCompData)
        {
            if (requirementCompData == null)
                return;

            var reqs = requirementCompData.propertyRequirements;
            IconTextItem[] entries = new IconTextItem[reqs.Count];

            hb.AddDividerLine(
                Color.gray
            );

            hb.AddTextItem(
                HB_Text.TextStyle.SUB_HEADER,
                Color.grey,
                Localization.GetString("room_requirements_string")
            );

            for (int i = 0; i < entries.Length; i++)
            {
                var propertyData = DataBank.Instance.GetData<RoomPropertyData>(reqs[i].propertyRef);

                var levelString = reqs[i].level == 1 ? 
                    Localization.GetString("room_property_half") + " " : 
                    Localization.GetString("room_property_full") + " ";

                entries[i] = new IconTextItem(
                    "<color=#ffaa00ff>" + levelString + Localization.GetString(propertyData.displayNameRef) + "</color>",
                    SpriteBank.GetSprite(propertyData.iconRef)
                );
            }

            hb.AddIconTextRow(
               entries,
               new Color32(170, 170, 170, 255)
           );

            var modPercent = (int) (requirementCompData.unsatisfiedMod * 100);
            hb.AddTextItem(
                HB_Text.TextStyle.SUB_HEADER,
                Color.grey,
                Localization.GetString("unsatisfied_punishment") + "<color=#ff5555ff> " + modPercent + "%</color>"
            );
        }
    }
}