using FNZ.Shared.Model.Entity.Components.InteractionEvent;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Crop;

namespace FNZ.Client.Model.Entity.Components.InteractionEvent
{
	public class InteractionEventComponentClient : InteractionEventComponentShared, IInteractableComponent
	{
		public void OnPlayerExitRange()
		{ }

		public void OnPlayerInRange()
		{ }
		
		private void NE_Send_CropInteract()
		{
			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InteractionEventComponentNet.InteractionEventNetEvent.INTERACT);
		}

		public void OnInteract()
		{
			NE_Send_CropInteract();
		}

		public string InteractionPromptMessageRef()
		{
			if (!string.IsNullOrEmpty(Data.interactionStringRef))
			{
				return Data.interactionStringRef;
			}
			return "string_interaction_default";
		}

		public bool IsInteractable()
		{
			return true;
		}
	}
}
