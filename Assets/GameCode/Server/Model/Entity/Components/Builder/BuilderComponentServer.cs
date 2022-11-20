using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Model.Entity.Components.Inventory;
using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Server.Model.World;
using FNZ.Server.Services;
using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Builder;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using FNZ.Server.Model.Entity.Components.BaseTerminal;
using Unity.Mathematics;
using static FNZ.Shared.Model.Entity.Components.Builder.BuilderComponentNet;

namespace FNZ.Server.Model.Entity.Components.Builder
{
	public class BuilderComponentServer : BuilderComponentShared
	{
		public override void Init()
		{
			base.Init();

			foreach (var buildingData in DataBank.Instance.GetAllDataIdsOfType<BuildingData>())
			{
				BuildingCategoryLists[buildingData.categoryRef].Add(buildingData);
			}
		}

		public override void OnNetEvent(NetIncomingMessage incMsg)
		{
			base.OnNetEvent(incMsg);

			switch ((BuilderNetEvent)incMsg.ReadByte())
			{
				case BuilderNetEvent.BUILD:
					NE_Receive_Build(incMsg);
					break;

				case BuilderNetEvent.BUILD_WALL:
					NE_Receive_Buildwall(incMsg);
					break;

				case BuilderNetEvent.BUILD_TILES:
					NE_Receive_BuildTiles(incMsg);
					break;

				case BuilderNetEvent.BUILD_ADDON:
					NE_Receive_BuildAddon(incMsg);
					break;

				case BuilderNetEvent.BUILD_MOUNTED_OBJECT:
					NE_Receive_BuildMountedObject(incMsg);
					break;

				case BuilderNetEvent.REMOVE_MOUNTED_OBJECT:
					NE_Receive_RemoveMountedObject(incMsg);
					break;
			}
		}

		private void NE_Receive_Build(NetIncomingMessage incMsg)
		{
			var data = new BuildData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var invComp = player.GetComponent<InventoryComponentServer>();
			var playerComp = player.GetComponent<PlayerComponentServer>();

			BuildingData recipe = DataBank.Instance.GetData<BuildingData>(data.recipeId);

			bool isTile = false;
			if (DataBank.Instance.IsIdOfType<TileData>(recipe.productRef))
			{
				isTile = true;
			}

			bool hasMaterials = true;
			foreach (MaterialDef md in recipe.requiredMaterials)
			{
				hasMaterials &= invComp.GetItemCount(md.itemRef) >= md.amount;
			}

			bool successfullyBuilt = false;
			if (!hasMaterials)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("no_material"),
					senderConnection
				);
			}
			else if (isTile)
			{
				successfullyBuilt = FNEService.Tile.TryNetChangeTile(recipe.productRef, new float2(data.x, data.y));
				if (successfullyBuilt)
				{
					bool traverseNorth = !GameServer.MainWorld.IsTileNorthEdgeOccupied(new int2((int)data.x, (int)data.y));
					bool traverseEast = !GameServer.MainWorld.IsTileEastEdgeOccupied(new int2((int)data.x, (int)data.y));

					bool traverseSouth = !GameServer.MainWorld.IsTileSouthEdgeOccupied(new int2((int)data.x, (int)data.y));
					bool traverseWest = !GameServer.MainWorld.IsTileWestEdgeOccupied(new int2((int)data.x, (int)data.y));

					if (traverseNorth)
					{
						GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(data.x, data.y + 1), 0);
					}
					if (traverseSouth)
					{
						GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(data.x, data.y - 1), 0);
					}
					if (traverseEast)
					{
						GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(data.x + 1, data.y), 0);
					}
					if (traverseWest)
					{
						GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(data.x - 1, data.y), 0);
					}

					GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_TILE, new float2(data.x, data.y), 0);
				}
			}
			else
			{
				FNEEntity newEntity = null;
				var objectType = DataBank.Instance.GetData<FNEEntityData>(recipe.productRef).entityType;
				if (objectType == EntityType.TILE_OBJECT)
				{
					var existingTileObject = GameServer.MainWorld.GetTileObject((int)data.x, (int)data.y);
					if (existingTileObject != null && existingTileObject.Data.blocksBuilding)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("building_in_the_way_message"),
							senderConnection
						);
						return;
					}
					else if (existingTileObject != null)
					{
						GameServer.EntityAPI.NetDestroyEntityImmediate(existingTileObject);
					}

					newEntity = GameServer.EntityAPI.NetSpawnEntityImmediate(
						recipe.productRef,
						new float2(data.x, data.y),
						data.rot
					);

					if (newEntity != null)
					{
						var baseTerminalComponent = newEntity.GetComponent<BaseTerminalComponentServer>();
						if (baseTerminalComponent != null)
						{
							GameServer.RoomManager.CreateNewBase((int2) newEntity.Position);
						}
						
						var tileId = GameServer.MainWorld.GetTileRoom(new float2(data.x, data.y));
						var room = (ServerRoom)GameServer.RoomManager.GetRoom(tileId);

						if (room != null)
						{
							GameServer.RoomManager.RecalculateBaseStatus(room.ParentBaseId, ParentEntity);
						}

						GameServer.NetAPI.World_RoomManager_BA();
						QuestManager.OnConstruction(recipe);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_TILE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), 0);

						if (recipe.unlockRefs != null && recipe.unlockRefs.Count > 0)
							HandleUnlockRefs(recipe, playerComp, senderConnection);

					}
				}
				else if (objectType == EntityType.EDGE_OBJECT)
				{
					var existingEdgeObject = GameServer.MainWorld.GetEdgeObjectAtPosition(new float2(data.x, data.y));
					if (existingEdgeObject == null)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("Must target a wall"),
							senderConnection
						);
						return;
					}
					else if (existingEdgeObject != null)
					{
						GameServer.EntityAPI.NetDestroyEntityImmediate(existingEdgeObject);
					}

					newEntity = GameServer.EntityAPI.NetSpawnEntityImmediate(
						recipe.productRef,
						new float2(data.x, data.y),
						data.rot
					);

					if (newEntity != null)
					{
						QuestManager.OnConstruction(recipe);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), newEntity.RotationDegrees);

						if (recipe.unlockRefs != null && recipe.unlockRefs.Count > 0)
							HandleUnlockRefs(recipe, playerComp, senderConnection);

					}
				}

				if (newEntity != null)
					successfullyBuilt = true;
			}

			if (successfullyBuilt)
			{
				foreach (MaterialDef md in recipe.requiredMaterials)
				{
					invComp.RemoveItemOfId(md.itemRef, md.amount);
				}

				GameServer.NetAPI.Entity_UpdateComponent_STC(invComp, incMsg.SenderConnection);
			}
		}

		private void NE_Receive_BuildAddon(NetIncomingMessage incMsg)
		{
			var data = new BuildAddonData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var invComp = player.GetComponent<InventoryComponentServer>();

			BuildingAddonData addonData = DataBank.Instance.GetData<BuildingAddonData>(data.addonId);

			bool hasMaterials = true;
			foreach (MaterialDef md in addonData.requiredMaterials)
			{
				hasMaterials &= invComp.GetItemCount(md.itemRef) >= md.amount;
			}

			bool successfullyBuilt = false;
			if (!hasMaterials)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("no_material"),
					senderConnection
				);
			}
			else
			{
				FNEEntity newEntity = null;
				var objectType = DataBank.Instance.GetData<FNEEntityData>(addonData.productRef).entityType;
				if (objectType == EntityType.EDGE_OBJECT)
				{
					var existingEdgeObject = GameServer.MainWorld.GetEdgeObjectAtPosition(new float2(data.x, data.y));
					if (existingEdgeObject == null)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("error_targeted_wall_does_not_exist"),
							senderConnection
						);
						return;
					}
					else if (existingEdgeObject != null)
					{
						GameServer.EntityAPI.NetDestroyEntityImmediate(existingEdgeObject);
					}

					newEntity = GameServer.EntityAPI.NetSpawnEntityImmediate(
						addonData.productRef,
						new float2(data.x, data.y),
						data.rot
					);

					if (newEntity != null)
					{
						QuestManager.OnAddonConstruction(addonData);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), newEntity.RotationDegrees);
					}
				}
				if (objectType == EntityType.TILE_OBJECT)
				{
					var existingTileObject = GameServer.MainWorld.GetTileObject((int) data.x, (int) data.y);
					if (existingTileObject == null)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("error_targeted_wall_does_not_exist"),
							senderConnection
						);
						return;
					}
					else
					{
						GameServer.EntityFactory.QueueEntityForReplacement(existingTileObject, addonData.productRef);
						
						QuestManager.OnAddonConstruction(addonData);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_TILE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), existingTileObject.RotationDegrees);

						successfullyBuilt = true;
					}
				}

				if (newEntity != null)
					successfullyBuilt = true;
			}

			if (successfullyBuilt)
			{
				foreach (MaterialDef md in addonData.requiredMaterials)
				{
					invComp.RemoveItemOfId(md.itemRef, md.amount);
				}

				foreach (MaterialDef md in addonData.refundedMaterials)
				{
					var item = Item.GenerateItem(md.itemRef, md.amount);
					bool placed = invComp.AutoPlaceIfPossible(item);
					if (!placed)
					{
						GameServer.NetAPI.Notification_SendWarningNotification_STC(
							Localization.GetString("ERROR: There was no room in inventory for refunding" + item.amount + "x " + Localization.GetString(item.Data.nameRef) + "!"),
							senderConnection
						);
					}
				}

				GameServer.NetAPI.Entity_UpdateComponent_STC(invComp, incMsg.SenderConnection);
			}
		}

		private void NE_Receive_BuildMountedObject(NetIncomingMessage incMsg)
		{
			var data = new BuildMountedObjectData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var invComp = player.GetComponent<InventoryComponentServer>();

			BuildingData buildingData = DataBank.Instance.GetData<BuildingData>(data.buildingId);

			bool hasMaterials = true;
			foreach (MaterialDef md in buildingData.requiredMaterials)
			{
				hasMaterials &= invComp.GetItemCount(md.itemRef) >= md.amount;
			}

			bool successfullyBuilt = false;
			if (!hasMaterials)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("no_material"),
					senderConnection
				);
			}
			else
			{
				var MountedData = DataBank.Instance.GetData<MountedObjectData>(buildingData.productRef);

				var existingEdgeObject = GameServer.MainWorld.GetEdgeObjectAtPosition(new float2(data.x, data.y));
				if (existingEdgeObject == null)
				{
					GameServer.NetAPI.Notification_SendWarningNotification_STC(
						Localization.GetString("error_targeted_wall_does_not_exist"),
						senderConnection
					);
					return;
				}

				if (existingEdgeObject.GetComponent<EdgeObjectComponentServer>().MountedObjectData != null)
				{
					GameServer.NetAPI.Notification_SendWarningNotification_STC(
						Localization.GetString("error_wall_already_has_mounted_object"),
						senderConnection
					);
					return;
				}

				FNEService.EdgeObject.NetSpawnMountedObject(
					buildingData.productRef,
					existingEdgeObject,
					data.oppositeDirection
				);

				QuestManager.OnConstruction(buildingData);
				GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), existingEdgeObject.RotationDegrees);

				successfullyBuilt = true;
			}

			if (successfullyBuilt)
			{
				foreach (MaterialDef md in buildingData.requiredMaterials)
				{
					invComp.RemoveItemOfId(md.itemRef, md.amount);
				}

				var room = GameServer.RoomManager.GetRoom(new int2((int)data.x, (int)data.y));
				if(room != null)
					GameServer.RoomManager.RecalculateBaseStatus(room.ParentBaseId, ParentEntity);

				// Check west neighbor
				if (data.x % 1 == 0)
				{
					room = GameServer.RoomManager.GetRoom(new int2((int)data.x - 1, (int)data.y));
					if (room != null)
						GameServer.RoomManager.RecalculateBaseStatus(room.ParentBaseId, ParentEntity);
				}
				// Check South Neighbor
				else
				{
					room = GameServer.RoomManager.GetRoom(new int2((int)data.x, (int)data.y - 1));
					if (room != null)
						GameServer.RoomManager.RecalculateBaseStatus(room.ParentBaseId, ParentEntity);
				}

				GameServer.NetAPI.Entity_UpdateComponent_STC(invComp, incMsg.SenderConnection);
				GameServer.NetAPI.World_RoomManager_BA();
			}
		}

		private void NE_Receive_RemoveMountedObject(NetIncomingMessage incMsg)
		{
			var data = new RemoveMountedObjectData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var existingEdgeObject = GameServer.MainWorld.GetEdgeObjectAtPosition(new float2(data.x, data.y));
			if (existingEdgeObject == null)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("error_targeted_wall_does_not_exist"),
					senderConnection
				);
				return;
			}

			if (existingEdgeObject.GetComponent<EdgeObjectComponentServer>().MountedObjectData == null)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("error_wall_already_has_mounted_object"),
					senderConnection
				);
				return;
			}

			FNEService.EdgeObject.NetRemoveMountedObject(existingEdgeObject);

			GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(data.x, data.y) + new float2(0.5f, 0.5f), existingEdgeObject.RotationDegrees);
		}

		private void NE_Receive_Buildwall(NetIncomingMessage incMsg)
		{
			var data = new BuildWallData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var invComp = player.GetComponent<InventoryComponentServer>();

			BuildingData recipe = DataBank.Instance.GetData<BuildingData>(data.recipeId);

			bool directionVertical = true;
			if (data.startY == data.endY)
			{
				directionVertical = false;
			}

			int wallCount = 0;
			if (directionVertical)
			{
				wallCount = data.endY - data.startY;
			}
			else
			{
				wallCount = data.endX - data.startX;
			}
			var batch = new List<FNEEntity>();
			int successfullWalls = 0;
			if (directionVertical)
			{
				for (int y = data.startY; y < data.endY; y++)
				{
					var newEntity = GameServer.EntityAPI.SpawnEntityImmediate(
						recipe.productRef,
						new float2(data.startX, y + 0.5f),
						90
					);

					if (newEntity != null)
					{
						batch.Add(newEntity);
						GameServer.NetConnector.SyncEntity(newEntity);
						GameServer.RoomManager.PromptRoomConstruction(new float2(newEntity.Position.x, newEntity.Position.y), ParentEntity);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(newEntity.Position.x, newEntity.Position.y), 90);
						successfullWalls++;
					}
				}

				GameServer.NetAPI.Entity_SpawnEntity_BAR_Batched(batch.ToArray());
			}
			else
			{
				for (int x = data.startX; x < data.endX; x++)
				{
					var newEntity = GameServer.EntityAPI.SpawnEntityImmediate(
						recipe.productRef,
						new float2(x + 0.5f, data.startY),
						0
					);

					if (newEntity != null)
					{
						batch.Add(newEntity);
						GameServer.NetConnector.SyncEntity(newEntity);
						GameServer.RoomManager.PromptRoomConstruction(new float2(newEntity.Position.x, newEntity.Position.y), ParentEntity);
						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_EDGE_OBJECT, new float2(newEntity.Position.x, newEntity.Position.y), 0);
						successfullWalls++;
					}
				}

				GameServer.NetAPI.Entity_SpawnEntity_BAR_Batched(batch.ToArray());
			}

			foreach (MaterialDef md in recipe.requiredMaterials)
			{
				invComp.RemoveItemOfId(md.itemRef, md.amount * successfullWalls);
			}

			GameServer.NetAPI.Entity_UpdateComponent_STC(invComp, incMsg.SenderConnection);
		}

		private void NE_Receive_BuildTiles(NetIncomingMessage incMsg)
		{
			var data = new BuildTileData();
			data.Deserialize(incMsg);

			var senderConnection = incMsg.SenderConnection;

			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var invComp = player.GetComponent<InventoryComponentServer>();

			BuildingData recipe = DataBank.Instance.GetData<BuildingData>(data.recipeId);

			List<long> basesToCalculate = new List<long>();

			// used to calculate corners for world map updates
			var minX = int.MaxValue;
			var maxX = 0;
			var minY = int.MaxValue;
			var maxY = 0;

			bool oneTileBlocked = false;
			
			int successfullTiles = 0;
			for (int y = data.startY; y <= data.endY; y++)
			{
				for (int x = data.startX; x <= data.endX; x++)
				{
					var existingTileObject = GameServer.MainWorld.GetTileObject((int)x, (int)y);
					if (existingTileObject != null)
					{
						if (existingTileObject.Data.blocksTileBuilding)
						{
							oneTileBlocked = true;
							continue;
						}

						if (!existingTileObject.Data.blocksBuilding)
						{
							GameServer.EntityAPI.NetDestroyEntityImmediate(existingTileObject);
						}
					}

					var success = FNEService.Tile.TryNetChangeTile(
						recipe.productRef,
						new float2(x, y)
					);

					if (success)
					{
						minX = x < minX ? x : minX;
						minY = y < minY ? y : minY;
						maxX = x > maxX ? x : maxX;
						maxY = y > maxY ? y : maxY;
						
						var tileRoomId = GameServer.MainWorld.GetTileRoom(new float2(x, y));
						if (tileRoomId != 0)
						{
							var room = (ServerRoom) GameServer.RoomManager.GetRoom(tileRoomId);
							if (!basesToCalculate.Contains(room.ParentBaseId))
							{
								basesToCalculate.Add(room.ParentBaseId);
							}

							//room.RecalculateRoomStatus();
							//QuestManager.OnRoomCreation(room, GameServer.NetConnector.GetConnectionFromPlayer(ParentEntity));
						}

						GameServer.NetAPI.Effect_SpawnEffect_BAR(EffectIdConstants.BUILD_TILE, new float2(x + 0.5f, y + 0.5f), 0);
						successfullTiles++;
					}
				}
			}
			if (oneTileBlocked)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_STC(
					Localization.GetString("building_in_the_way_message"),
					senderConnection
				);
			}
			
			foreach(var baseId in basesToCalculate)
			{
				GameServer.RoomManager.RecalculateBaseStatus(baseId, ParentEntity);
			}

			foreach (MaterialDef md in recipe.requiredMaterials)
			{
				invComp.RemoveItemOfId(md.itemRef, md.amount * successfullTiles);
			}

			if (successfullTiles > 0)
			{
				GameServer.NetAPI.World_RoomManager_BA();
				
				// Dictionary<long, bool> chunksToMapUpdate = new Dictionary<long, bool>();
				// chunksToMapUpdate.Add((minX / 32) + (minY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS, true);
				// var chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(
				// 	new float2(minX, minY)
				// );
				// GameServer.NetAPI.World_ChunkMapUpdate_BA(chunk);
				//
				// if (!chunksToMapUpdate.ContainsKey((maxX / 32) + (minY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS))
				// {
				// 	chunksToMapUpdate.Add((maxX / 32) + (minY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS, true);
				// 	chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(
				// 		new float2(maxX, minY)
				// 	);
				// 	GameServer.NetAPI.World_ChunkMapUpdate_BA(chunk);
				// }
				//
				// if (!chunksToMapUpdate.ContainsKey((minX / 32) + (maxY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS))
				// {
				// 	chunksToMapUpdate.Add((minX / 32) + (maxY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS, true);
				// 	chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(
				// 		new float2(minX, maxY)
				// 	);
				// 	GameServer.NetAPI.World_ChunkMapUpdate_BA(chunk);
				// }
				//
				// if (!chunksToMapUpdate.ContainsKey((maxX / 32) + (maxY / 32) * GameServer.MainWorld.WIDTH_IN_CHUNKS))
				// {
				// 	chunk = GameServer.MainWorld.GetWorldChunk<ServerWorldChunk>(
				// 		new float2(maxX, maxY)
				// 	);
				// 	GameServer.NetAPI.World_ChunkMapUpdate_BA(chunk);					
				// }
			}

			GameServer.NetAPI.Entity_UpdateComponent_STC(invComp, incMsg.SenderConnection);
		}

		public void HandleUnlockRefs(BuildingData recipe, PlayerComponentServer playerComp, NetConnection senderConnection)
		{
			foreach (var unlockRef in recipe.unlockRefs)
			{
				if (playerComp.HasUnlockedBuilding(unlockRef))
					continue;

                var unlockRefData = DataBank.Instance.GetData<BuildingData>(unlockRef);

                foreach (var player in GameServer.MainWorld.GetAllPlayers())
                {
                    var playerComponent = player.GetComponent<PlayerComponentServer>();
                    playerComponent.UnlockBuilding(unlockRef);

                    GameServer.NetAPI.Entity_UpdateComponent_STC(playerComponent, GameServer.NetConnector.GetConnectionFromPlayer(player));
                }

				GameServer.NetAPI.Notification_SendNotification_BA(
					unlockRefData.iconRef,
					"green",
					"false",
					$"You've unlocked {Localization.GetString(unlockRefData.nameRef)}!"
                );
			}

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerComp, senderConnection);
		}
	}
}
