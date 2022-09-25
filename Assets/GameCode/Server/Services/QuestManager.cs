using FNZ.Server.Model.Entity.Components.Player;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Model.QuestType.Addon;
using FNZ.Shared.Model.QuestType.Building;
using FNZ.Shared.Model.QuestType.Crafting;
using FNZ.Shared.Model.QuestType.Excavate;
using FNZ.Shared.Model.QuestType.HarvestCrop;
using FNZ.Shared.Model.QuestType.Refinement;
using FNZ.Shared.Model.QuestType.Room;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;
using FNZ.Server.Model.World;
using FNZ.Server.Model.World.Blueprint;
using FNZ.Shared.Model.QuestType.Event;
using Unity.Mathematics;

namespace FNZ.Server.Services.QuestManager
{
	public static class QuestManager
	{
		private static List<Quest> activeQuests = new List<Quest>();
		private static List<Quest> finishedQuests = new List<Quest>();
		public static string m_Message, m_StringRef;

		private static bool isInitialized = false;

		public static void Serialize(NetBuffer writer)
		{
			writer.Write((ushort)activeQuests.Count);
			foreach (var quest in activeQuests)
			{
				quest.Serialize(writer);
			}
			
			writer.Write((ushort)finishedQuests.Count);
			foreach (var quest in finishedQuests)
			{
				quest.Serialize(writer);
			}
		}

		public static void Deserialize(NetBuffer reader)
		{
			var activeQuestsCount = reader.ReadUInt16();

			for (var i = 0; i < activeQuestsCount; i++)
			{
				var activeQuest = new Quest();
				activeQuest.Deserialize(reader);
				activeQuests.Add(activeQuest);
			}
			
			var finishedQuestsCount = reader.ReadUInt16();

			for (var i = 0; i < finishedQuestsCount; i++)
			{
				var finishedQuest = new Quest();
				finishedQuest.Deserialize(reader);
				finishedQuests.Add(finishedQuest);
			}
		}

		public static void InitIfNotInitialized(string questRef = "quest_start")
		{
			if (!isInitialized)
			{
				isInitialized = true;
				if (activeQuests.Count > 0)
				{
					var qref = activeQuests[activeQuests.Count - 1].m_Data.Id;
					activeQuests.Clear();
					StartNewQuest(qref);
				}
				else
				{
					StartNewQuest(questRef);
				}
			}
		}
		
		public static void StartNewQuest(string questRef)
		{
			var questData = DataBank.Instance.GetData<QuestData>(questRef);

			switch (questData.questTypeData)
			{
				case ExcavateItemsQuestData excavateItemsQuestdata:
					var excavateQuest = new ExcavateItemsQuest(questRef);
					activeQuests.Add(excavateQuest);

					m_StringRef = DataBank.Instance.GetData<ItemData>(excavateQuest.Data.itemRef).nameRef;
					string excavateItemName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("harvest_string");
					m_Message = string.Format(
						m_Message,
						excavateItemsQuestdata.amount,
						excavateItemName
					);
					
					float2[] floats = new float2[0];
					
					
					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						floats
					);

					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						excavateQuest.m_Identifier
					);*/
					break;

				case BuildingQuestData buildingQuestData:
					var buildingQuest = new BuildingQuest(questRef);
					activeQuests.Add(buildingQuest);

					m_StringRef = DataBank.Instance.GetData<BuildingData>(buildingQuest.Data.buildingRef).nameRef;
					string buildingName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("build_string");
					m_Message = string.Format(
						m_Message,
						buildingQuestData.amount,
						buildingName
					);

					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						new float2[0]
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						buildingQuest.m_Identifier
					);*/
					break;

				case BuildingAddonQuestData buildingAddonQuestData:
					var buildingAddonQuest = new BuildingAddonQuest(questRef);
					activeQuests.Add(buildingAddonQuest);

					m_StringRef = DataBank.Instance.GetData<BuildingAddonData>(buildingAddonQuest.Data.buildingRef).nameRef;
					buildingName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("build_string");
					m_Message = string.Format(
						m_Message,
						buildingAddonQuestData.amount,
						buildingName
					);

					floats = new float2[0];

					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						floats
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						buildingAddonQuest.m_Identifier
					);*/
					break;

				case CraftingQuestData craftingQuestData:
					var craftingQuest = new CraftingQuest(questRef);
					activeQuests.Add(craftingQuest);

					m_StringRef = DataBank.Instance.GetData<ItemData>(craftingQuest.Data.itemRef).nameRef;
					string craftingName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("craft_string");
					m_Message = string.Format(
						m_Message,
						craftingQuestData.amount,
						craftingName
					);

					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						new float2[0]
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						craftingQuest.m_Identifier
					);*/
					break;

				case RefinementQuestData refinementQuestData:
					var refinementQuest = new RefinementQuest(questRef);
					activeQuests.Add(refinementQuest);

					m_StringRef = DataBank.Instance.GetData<ItemData>(refinementQuest.Data.itemRef).nameRef;
					string refinementName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("refinement_string");
					m_Message = string.Format(
						m_Message,
						refinementQuest.Data.amount,
						refinementName
					);
					
					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						new float2[0]
					);

					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						refinementQuest.m_Identifier
					);*/
					break;

				case HarvestCropQuestData harvestCropQuestData:
					var harvestCropQuest = new HarvestCropQuest(questRef);
					activeQuests.Add(harvestCropQuest);

					m_StringRef = DataBank.Instance.GetData<ItemData>(harvestCropQuest.Data.itemRef).nameRef;
					string harvestCropName = Localization.GetString(m_StringRef);

					m_Message = Localization.GetString("harvest_crop_string");
					m_Message = string.Format(
						m_Message,
						harvestCropQuest.Data.amount,
						harvestCropName
					);

					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						new float2[0]
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						harvestCropQuest.m_Identifier
					);*/
					break;

				case ConstructRoomQuestData roomQuestData:
					var roomQuest = new ConstructRoomQuest(questRef);
					activeQuests.Add(roomQuest);

					if (roomQuest.Data.roomQuestType == RoomQuestType.SIZE)
					{
						m_Message = Localization.GetString("string_construct_room_quest_size");
						m_Message = string.Format(
							m_Message,
							roomQuest.Data.width,
							roomQuest.Data.height
						);
					}
					else if (roomQuest.Data.roomQuestType == RoomQuestType.PROPERTY)
					{
						var propertyData = DataBank.Instance.GetData<RoomPropertyData>(roomQuest.Data.propertyRef);
						var propertyLevel = roomQuest.Data.propertyLevel;
						m_Message = Localization.GetString("string_construct_room_quest_property");
						m_Message = string.Format(
							m_Message,
							Localization.GetString(propertyLevel == 0 ? "" : propertyLevel == 1 ? "room_property_half" : "room_property_full"),
							Localization.GetString(propertyData.displayNameRef)
						);
					}

					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						new float2[0]
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						roomQuest.m_Identifier
					);*/
					break;
				
				case EventQuestData eventQuestData:
					var eventQuest = new EventQuest(questRef);
					activeQuests.Add(eventQuest);
					
					floats = new float2[0];
					if (questRef.Equals("quest_get_circuit_board"))
					{
						var event1Pos = WorldBlueprintGen.event1Sites;
						floats = new float2[event1Pos.Count];
						for (int i = 0; i < event1Pos.Count; i++)
							floats[i] = event1Pos[i];
					}
					
					if (questRef.Equals("quest_city_event"))
					{
						floats = new float2[1];
						floats[0] = WorldBlueprintGen.event2Site;
					}
					
					GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
						questRef,
						0,
						floats
					);
					
					/*GameServer.NetAPI.Notification_SendNotification_BA(
						"warning_icon",
						"silver",
						"true",
						m_Message,
						roomQuest.m_Identifier
					);*/
					break;
			}

			/*if (questData.questForkRef != null)
			{
				StartNewQuest(questData.questForkRef);
			}*/
		}

		public static void SendActiveQuestsToPlayer(NetConnection playerConn)
		{
			foreach (var quest in activeQuests)
			{
				switch (quest.GetData().questTypeData)
				{
					case ExcavateItemsQuestData excavateItemsQuestdata:

						m_StringRef = DataBank.Instance.GetData<ItemData>(excavateItemsQuestdata.itemRef).nameRef;
						string excavateItemName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvest_string");
						m_Message = string.Format(
							m_Message,
							excavateItemsQuestdata.amount,
							excavateItemName
						);

						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);
						
						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case BuildingQuestData buildingQuestData:
						m_StringRef = DataBank.Instance.GetData<BuildingData>(buildingQuestData.buildingRef).nameRef;
						string buildingName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("build_string");
						m_Message = string.Format(
							m_Message,
							buildingQuestData.amount,
							buildingName
						);
						
						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);

						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case BuildingAddonQuestData buildingAddonQuestData:
						m_StringRef = DataBank.Instance.GetData<BuildingAddonData>(buildingAddonQuestData.buildingRef)
							.nameRef;
						buildingName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("build_string");
						m_Message = string.Format(
							m_Message,
							buildingAddonQuestData.amount,
							buildingName
						);
						
						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);

						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case CraftingQuestData craftingQuestData:
						m_StringRef = DataBank.Instance.GetData<ItemData>(craftingQuestData.itemRef).nameRef;
						string craftingName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("craft_string");
						m_Message = string.Format(
							m_Message,
							craftingQuestData.amount,
							craftingName
						);

						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);
						
						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case RefinementQuestData refinementQuestData:
						m_StringRef = DataBank.Instance.GetData<ItemData>(refinementQuestData.itemRef).nameRef;
						string refinementName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("refinement_string");
						m_Message = string.Format(
							m_Message,
							refinementQuestData.amount,
							refinementName
						);
						
						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);

						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case HarvestCropQuestData harvestCropQuestData:

						m_StringRef = DataBank.Instance.GetData<ItemData>(harvestCropQuestData.itemRef).nameRef;
						string harvestCropName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvest_crop_string");
						m_Message = string.Format(
							m_Message,
							harvestCropQuestData.amount,
							harvestCropName
						);
						
						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);

						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;

					case ConstructRoomQuestData roomQuestData:
						if (roomQuestData.roomQuestType == RoomQuestType.SIZE)
						{
							m_Message = Localization.GetString("string_construct_room_quest_size");
							m_Message = string.Format(
								m_Message,
								roomQuestData.width,
								roomQuestData.height
							);
						}
						else if (roomQuestData.roomQuestType == RoomQuestType.PROPERTY)
						{
							var propertyData = DataBank.Instance.GetData<RoomPropertyData>(roomQuestData.propertyRef);
							var propertyLevel = roomQuestData.propertyLevel;
							m_Message = Localization.GetString("string_construct_room_quest_property");
							m_Message = string.Format(
								m_Message,
								Localization.GetString(propertyLevel == 0 ? "" :
									propertyLevel == 1 ? "room_property_half" : "room_property_full"),
								Localization.GetString(propertyData.displayNameRef)
							);
						}

						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);
						
						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;
					
					case EventQuestData eventQuestData:
						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							quest.GetData().Id,
							quest.Progress,
							new float2[0]
						);
						
						/*GameServer.NetAPI.Notification_SendNotification_STC(
							"warning_icon",
							"silver",
							"true",
							m_Message,
							playerConn,
							quest.m_Identifier
						);*/
						break;
				}
			}
		}

		public static void OnExcavateResource(Item itemExcavated)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(ExcavateItemsQuest)))
			{
				var excavateQuest = (ExcavateItemsQuest)q;

				if (itemExcavated.Data.Id == excavateQuest.Data.itemRef)
				{
					excavateQuest.Progress += itemExcavated.amount;

					if (excavateQuest.Progress >= excavateQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in excavateQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<ItemData>(itemExcavated.Data.Id).nameRef;
						string resource = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvested_progress_message");
						m_Message = string.Format(
							m_Message,
							excavateQuest.Progress,
							excavateQuest.Data.amount,
							resource
						);

						GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
							q.GetData().Id,
							excavateQuest.Progress,
							new float2[0]
						);
						
						/*GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							q.m_Identifier
						);*/
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnConstruction(BuildingData buildingData)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(BuildingQuest)))
			{
				var buildingQuest = (BuildingQuest)q;

				if (buildingData.Id == buildingQuest.Data.buildingRef)
				{
					buildingQuest.Progress++;

					if (buildingQuest.Progress >= buildingQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in buildingQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<BuildingData>(buildingData.Id).nameRef;
						string building = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("building_progress_message");
						m_Message = string.Format(
							m_Message,
							buildingQuest.Progress,
							buildingQuest.Data.amount,
							building
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							q.m_Identifier
						);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnAddonConstruction(BuildingAddonData buildingAddonData)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(BuildingAddonQuest)))
			{
				var addonQuest = (BuildingAddonQuest)q;

				if (buildingAddonData.Id == addonQuest.Data.buildingRef)
				{
					addonQuest.Progress += addonQuest.Data.amount;

					if (addonQuest.Progress >= addonQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in addonQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<BuildingData>(buildingAddonData.Id).nameRef;
						string building = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("building_progress_message");
						m_Message = string.Format(
							m_Message,
							addonQuest.Progress,
							addonQuest.Data.amount,
							building
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							q.m_Identifier
						);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnCrafting(Item craftingData)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(CraftingQuest)))
			{
				var craftingQuest = (CraftingQuest)q;

				if (craftingData.Data.Id == craftingQuest.Data.itemRef)
				{
					craftingQuest.Progress += craftingData.amount;

					if (craftingQuest.Progress >= craftingQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in craftingQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<ItemData>(craftingQuest.Data.itemRef).nameRef;
						string craftingName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("crafting_progress_message");
						m_Message = string.Format(
							m_Message,
							craftingQuest.Progress,
							craftingQuest.Data.amount,
							craftingName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
								"warning_icon",
								"gold",
								"true",
								m_Message,
								q.m_Identifier
							);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnRefinement(Item itemRefined)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(RefinementQuest)))
			{
				var refinementQuest = (RefinementQuest)q;

				if (itemRefined.Data.Id == refinementQuest.Data.itemRef)
				{
					refinementQuest.Progress += itemRefined.amount;

					if (refinementQuest.Progress >= refinementQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in refinementQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<ItemData>(refinementQuest.Data.itemRef).nameRef;
						string refinementName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("refinement_progress_message");
						m_Message = string.Format(
							m_Message,
							refinementQuest.Progress,
							refinementQuest.Data.amount,
							refinementName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							q.m_Identifier
						);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnHarvestCrop(Item itemHarvestCrop)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(HarvestCropQuest)))
			{
				var harvestCropQuest = (HarvestCropQuest)q;

				if (itemHarvestCrop.Data.Id == harvestCropQuest.Data.itemRef)
				{
					harvestCropQuest.Progress += itemHarvestCrop.amount;

					if (harvestCropQuest.Progress >= harvestCropQuest.Data.amount)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in harvestCropQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
					else
					{
						m_StringRef = DataBank.Instance.GetData<ItemData>(harvestCropQuest.Data.itemRef).nameRef;
						string harvestCropName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvest_crop_progress_message");
						m_Message = string.Format(
							m_Message,
							harvestCropQuest.Progress,
							harvestCropQuest.Data.amount,
							harvestCropName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							q.m_Identifier
						);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}
		
		public static void OnEventSuccess(string eventRef)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(EventQuest)))
			{
				var eventQuest = (EventQuest)q;

				if (eventQuest.Data.eventRef.Equals(eventRef))
				{
					UpdateQuestDone(q.m_Identifier);
					finishedQuests.Add(q);
					foreach (var building in eventQuest.GetBuildingUnlocks())
						UnlockBuilding(building);
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		public static void OnRoomCreation(Room newRoom)
		{
			foreach (var q in activeQuests.Where(cq => cq.GetType() == typeof(ConstructRoomQuest)))
			{
				var roomQuest = (ConstructRoomQuest)q;

				if (roomQuest.Data.roomQuestType == RoomQuestType.SIZE && newRoom.width >= roomQuest.Data.width && newRoom.height >= roomQuest.Data.height)
				{
					UpdateQuestDone(q.m_Identifier);
					finishedQuests.Add(q);
					foreach (var building in roomQuest.GetBuildingUnlocks())
						UnlockBuilding(building);
				}
				else if (roomQuest.Data.roomQuestType == RoomQuestType.PROPERTY)
				{
					var propertyRef = roomQuest.Data.propertyRef;
					if (newRoom.RoomProperties.ContainsKey(propertyRef) && newRoom.RoomProperties[propertyRef] >= roomQuest.Data.propertyLevel)
					{
						UpdateQuestDone(q.m_Identifier);
						finishedQuests.Add(q);
						foreach (var building in roomQuest.GetBuildingUnlocks())
							UnlockBuilding(building);
					}
				}
			}

			foreach (var fq in finishedQuests)
			{
				activeQuests.Remove(fq);
				PromptFollowupQuest(fq);
			}

			finishedQuests.Clear();
		}

		private static void UpdateQuestDone(float identifier)
		{
			GameServer.NetAPI.Notification_SendNotification_BA(
				"warning_icon",
				"green",
				"false",
				Localization.GetString("quest_done"),
				identifier
			);
		}

		private static void PromptFollowupQuest(Quest quest)
		{
			var nextQuest = quest.GetFollowupQuestRef();
			if (!string.IsNullOrEmpty(nextQuest))
				StartNewQuest(nextQuest);
			else
				GameServer.NetAPI.Quest_SendQuestUpdateToPlayers(
					"",
					0,
					new float2[0]
				);
		}

		private static void UnlockBuilding(string buildingId)
		{
			if (!string.IsNullOrEmpty(buildingId))
			{
				var buildingData = DataBank.Instance.GetData<BuildingData>(buildingId);

				foreach (var player in GameServer.World.GetAllPlayers())
				{
					var playerComponent = player.GetComponent<PlayerComponentServer>();
					playerComponent.UnlockBuilding(buildingId);

					GameServer.NetAPI.Entity_UpdateComponent_STC(playerComponent, GameServer.NetConnector.GetConnectionFromPlayer(player));
				}

				GameServer.NetAPI.Notification_SendNotification_BA(
					buildingData.iconRef,
					"green",
					"false",
					$"You've unlocked {Localization.GetString(buildingData.nameRef)}!"
				);
			}
		}

		public static void GetQuestProgression(NetConnection senderConnection)
		{
			var player = GameServer.NetConnector.GetPlayerFromConnection(senderConnection);
			var playerComponent = player.GetComponent<PlayerComponentServer>();

			foreach (var quest in activeQuests)
			{
				var questData = quest.GetData();
				string localizedStringRef;

				switch (questData.questTypeData)
				{
					case ExcavateItemsQuestData excavateItemsQuestdata:
						var excavQuest = (ExcavateItemsQuest)quest;

						m_StringRef = DataBank.Instance.GetData<ItemData>(excavQuest.Data.itemRef).nameRef;
						localizedStringRef = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvested_progress_message");
						m_Message = string.Format(
							m_Message,
							excavQuest.Progress,
							excavQuest.Data.amount,
							localizedStringRef
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							excavQuest.m_Identifier
						);
						break;

					case BuildingQuestData buildingQuestData:
						var buildingQuest = (BuildingQuest)quest;

						m_StringRef = DataBank.Instance.GetData<BuildingData>(buildingQuest.Data.buildingRef).nameRef;
						localizedStringRef = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("building_progress_message");
						m_Message = string.Format(
							m_Message,
							buildingQuest.Progress,
							buildingQuest.Data.amount,
							localizedStringRef
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							quest.m_Identifier
						);
						break;

					case BuildingAddonQuestData buildingAddonQuestData:
						var buildingAddonQuest = (BuildingAddonQuest) quest;

						m_StringRef = DataBank.Instance.GetData<BuildingAddonData>(buildingAddonQuest.Data.buildingRef).nameRef;
						localizedStringRef = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("building_progress_message");
						m_Message = string.Format(
							m_Message,
							buildingAddonQuest.Progress,
							buildingAddonQuest.Data.amount,
							localizedStringRef
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							quest.m_Identifier
						);
						break;

					case CraftingQuestData craftingQuestData:
						var craftingQuest = (CraftingQuest)quest;

						m_StringRef = DataBank.Instance.GetData<ItemData>(craftingQuest.Data.itemRef).nameRef;
						string craftingName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("crafting_progress_message");
						m_Message = string.Format(
							m_Message,
							craftingQuest.Progress,
							craftingQuest.Data.amount,
							craftingName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
								"warning_icon",
								"gold",
								"true",
								m_Message,
								quest.m_Identifier
							);
						break;

					case RefinementQuestData refinementQuestData:
						var refinementQuest = (RefinementQuest)quest;

						m_StringRef = DataBank.Instance.GetData<ItemData>(refinementQuest.Data.itemRef).nameRef;
						string refinementName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("refinement_progress_message");
						m_Message = string.Format(
							m_Message,
							refinementQuest.Progress,
							refinementQuest.Data.amount,
							refinementName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							quest.m_Identifier
						);
						break;

					case HarvestCropQuestData harvestCropQuestData:
						var harvestCropQuest = (HarvestCropQuest)quest;

						m_StringRef = DataBank.Instance.GetData<ItemData>(harvestCropQuest.Data.itemRef).nameRef;
						string harvestCropName = Localization.GetString(m_StringRef);

						m_Message = Localization.GetString("harvest_crop_progress_message");
						m_Message = string.Format(
							m_Message,
							harvestCropQuest.Progress,
							harvestCropQuest.Data.amount,
							harvestCropName
						);

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							quest.m_Identifier
						);
						break;

					case ConstructRoomQuestData roomQuestData:
						var roomQuest = new ConstructRoomQuest(quest.GetData().Id);

						if (roomQuest.Data.roomQuestType == RoomQuestType.SIZE)
						{
							m_Message = Localization.GetString("string_construct_room_quest_size");
							m_Message = string.Format(
								m_Message,
								roomQuest.Data.width,
								roomQuest.Data.height
								);
						}
						else if (roomQuest.Data.roomQuestType == RoomQuestType.PROPERTY)
						{
							var propertyData = DataBank.Instance.GetData<RoomPropertyData>(roomQuest.Data.propertyRef);
							var propertyLevel = roomQuest.Data.propertyLevel;
							m_Message = Localization.GetString("string_construct_room_quest_property");
							m_Message = string.Format(
								m_Message,
								Localization.GetString(propertyLevel == 0 ? "" : propertyLevel == 1 ? "room_property_half" : "room_property_full"),
								Localization.GetString(propertyData.displayNameRef)
							);
						}

						GameServer.NetAPI.Notification_SendNotification_BA(
							"warning_icon",
							"gold",
							"true",
							m_Message,
							quest.m_Identifier
						);
						break;
				}
			}

			var hostEntity = GameServer.NetConnector.GetPlayerFromConnection(GameServer.NetConnector.GetServerHostConnection());
			foreach (var buildingRef in hostEntity.GetComponent<PlayerComponentServer>().GetUnlockedBuildings())
				playerComponent.UnlockBuilding(buildingRef);

			GameServer.NetAPI.Entity_UpdateComponent_STC(playerComponent, senderConnection);
		}
	}
}