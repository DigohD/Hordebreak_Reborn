using FNZ.Shared.Model.Entity.Components.InteractionEvent;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FNZ.Server.WorldEvents;
using FNZ.Shared.Model.Entity.Components.Crop;
using Lidgren.Network;
using Unity.Mathematics;

namespace FNZ.Server.Model.Entity.Components.InteractionEvent
{
	public class InteractionEventComponentServer : InteractionEventComponentShared
	{
		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((InteractionEventComponentNet.InteractionEventNetEvent) incMsg.ReadByte())
			{
				case InteractionEventComponentNet.InteractionEventNetEvent.INTERACT:
					NE_Receive_Interact(incMsg);
					break;
			}
		}

		private void NE_Receive_Interact(NetIncomingMessage incMsg)
		{
			if (!string.IsNullOrEmpty(Data.transformedEntityRef))
			{
				GameServer.EntityFactory.QueueEntityForReplacement(ParentEntity, Data.transformedEntityRef);
			}
			
			if (!string.IsNullOrEmpty(Data.effectRef))
			{
				GameServer.NetAPI.Effect_SpawnEffect_BAR(Data.effectRef, ParentEntity.Position + new float2(0.5f, 0.5f), ParentEntity.RotationDegrees);
			}

			if (!string.IsNullOrEmpty(Data.eventRef))
			{
				GameServer.EventManager.SpawnWorldEvent(Data.eventRef, ParentEntity);
			}
		}
	}
}
