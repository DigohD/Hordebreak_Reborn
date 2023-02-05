using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.World;
using FNZ.Server.Net.API;
using FNZ.Server.Services.QuestManager;
using FNZ.Server.Utils;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Crop;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using Lidgren.Network;
using Unity.Mathematics;
using static FNZ.Shared.Model.Entity.Components.Crop.CropComponentNet;

namespace FNZ.Server.Model.Entity.Components
{
	public class CropComponentServer : CropComponentShared, ITickable
	{
		private ServerWorld _world;
		
		public override void Init()
		{
			_world = GameServer.WorldInstanceManager.GetWorldInstance(ParentEntity.WorldInstanceIndex);
			growth = 0;
		}

		public void Tick(float dt)
		{
			GrowCrop();
		}

		private void GrowCrop()
		{
			if (ParentEntity == null || growth >= Data.growthTimeTicks)
			{
				if (FNERandom.GetRandomIntInRange(0, 50) == 0)
				{
					GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
				}
				return;
			}
			
			var room = GameServer.RoomManager.GetRoom(_world.GetTileRoom(ParentEntity.Position));

			if(Data.environmentSpans.Count == 0)
            {
				growth += 1f;
				growthStatus = GrowthStatus.Optimal;
			}
			else if (room == null)
			{
                growth += 0.01f;
                growthStatus = GrowthStatus.Dormant;
            }
			else
			{
                float totalGrowthMod = 1;
                foreach (var span in Data.environmentSpans)
                {
                    var envValue = room.roomEnvironmentValues[span.environmentRef];
                    if (span.IsValueInSpan(envValue))
                    {
                        totalGrowthMod *= span.GetOptimalModifier(envValue);
                    }
                    else
                    {
                        totalGrowthMod = 0;
                        break;
                    }
                }

                if(totalGrowthMod > 0)
                {
                    growth += 0.02f + (0.18f * totalGrowthMod);
                    if (totalGrowthMod > 0.9f)
                    {
                        growthStatus = GrowthStatus.Optimal;
                    }
                    else
                    {
                        growthStatus = GrowthStatus.Growing;
                    }
                }
                else
                {
                    growth += 0.01f;
                    growthStatus = GrowthStatus.Dormant;
                }
            }

            if (growth >= Data.growthTimeTicks)
            {
                if (TryMatureTransformation())
                {
                    Enabled = false;
                    return;
                }

                growth = Data.growthTimeTicks;
            }

            GameServer.NetAPI.Entity_UpdateComponent_BAR(this);
        }

		private bool TryMatureTransformation()
		{
			if (!string.IsNullOrEmpty(Data.matureEntityRef))
			{
				GameServer.EntityFactory.QueueEntityForReplacement(ParentEntity, Data.matureEntityRef);
				return true;
			}

			return false;
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((CropNetEvent)incMsg.ReadByte())
			{
				case CropNetEvent.CROP_INTERACT:
					NE_Receive_CropInteract(incMsg);
					break;
			}
		}

		private void NE_Receive_CropInteract(NetIncomingMessage incMsg)
		{
			if (growth >= Data.growthTimeTicks)
			{
				HarvestCrop(GameServer.NetConnector.GetPlayerFromConnection(incMsg.SenderConnection), incMsg);
			}
		}

		public void HarvestCrop(FNEEntity harvsestingPlayer, NetIncomingMessage incMsg)
		{
			InventoryComponentServer inventory = harvsestingPlayer.GetComponent<InventoryComponentServer>();
			var crop = new Item();

			var loot = LootGenerator.GenerateLoot(Data.produceLootTable);

			foreach (var item in loot)
			{
				inventory.AutoPlaceIfPossible(item);
				crop = item;
			}

			GameServer.NetAPI.Entity_UpdateComponent_BA(inventory);
			GameServer.NetAPI.Notification_SendNotification_STC("harvest_notification_icon", "grey", "false",
				crop.amount + "x " + Localization.GetString(crop.Data.nameRef), incMsg.SenderConnection);
			GameServer.NetAPI.Effect_SpawnEffect_BAR(Data.harvestEffectRef, ParentEntity.Position + new float2(0.5f, 0.5f), ParentEntity.RotationDegrees);
			QuestManager.OnHarvestCrop(crop);

            if (Data.consumedOnHarvest)
            {
				GameServer.EntityAPI.NetDestroyEntityImmediate(ParentEntity);
            }
			else if (Data.transformsOnHarvest && !string.IsNullOrEmpty(Data.transformedEntityRef))
            {
				GameServer.EntityFactory.QueueEntityForReplacement(ParentEntity, Data.transformedEntityRef);
			}
            else
            {
                growth = 0;
            }
        }

		public void ForceMature()
		{
			growth = Data.growthTimeTicks - 0.001f;
		}

        public override void FileSerialize(NetBuffer writer)
        {
			// Rollback plant a bit when unloaded
			growth -= Data.growthTimeTicks - 5;

			base.FileSerialize(writer);
        }
    }
}
