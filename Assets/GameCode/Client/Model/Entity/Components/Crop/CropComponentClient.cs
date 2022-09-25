using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Crop;
using FNZ.Shared.Model.World.Environment;
using Lidgren.Network;
using UnityEngine;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;
using static FNZ.Shared.Model.Entity.Components.Crop.CropComponentNet;

namespace FNZ.Client.Model.Entity.Components
{
	public class CropComponentClient : CropComponentShared, IInteractableComponent
	{
		public float GetGrowthProgress()
		{
			return growth / Data.growthTimeTicks;
		}

		public void OnInteract()
		{
			NE_Send_CropInteract();
		}

		public void NE_Send_CropInteract()
		{
			if (growth >= Data.growthTimeTicks)
				GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)CropNetEvent.CROP_INTERACT);
		}

		public void OnPlayerExitRange()
		{ }

		public void OnPlayerInRange()
		{
            Debug.LogWarning("STATUS: " + growthStatus.ToString() + "GROWTH: " + growth);
        }

		public override void Deserialize(NetBuffer br)
		{
			var tmpGrowth = growth;
			base.Deserialize(br);

			if (tmpGrowth != growth && growth == Data.growthTimeTicks)
			{
				if(GameClient.NetConnector.GetEntity(ParentEntity.NetId) != null)
                {
					GameClient.EntityFactory.ReplaceEntityView(ParentEntity, Data.harvestableViewRef);
                }
                else
                {
					growth = 0;
                }
			}
			else if (tmpGrowth != growth && tmpGrowth == Data.growthTimeTicks)
			{
				var viewRef = FNEEntity.GetEntityViewVariationId(ParentEntity.Data, ParentEntity.Position);
				GameClient.EntityFactory.ReplaceEntityView(ParentEntity, viewRef);
			}
		}

		public string InteractionPromptMessageRef()
		{
			return "crop_component_interact";
		}

		public bool IsInteractable()
		{
			return growth >= Data.growthTimeTicks;
		}
	}
}
