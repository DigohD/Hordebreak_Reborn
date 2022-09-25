using FNZ.Server.Services.QuestManager;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Rooms;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using FNZ.Server.Controller.Systems;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Environment;
using Lidgren.Network;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Server.Model.World
{
	public class ServerRoomManager : RoomManager
	{
		private List<long> m_AdjacentBasesFound = new List<long>();

		public void FileDeserialize(NetBuffer reader)
		{
			m_Bases.Clear();

			var count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				long baseId = reader.ReadInt64();
				var BaseData = new BaseData();
				BaseData.Deserialize(reader);

				m_Bases.Add(baseId, BaseData);
			}
			
			// foreach (var room in m_Rooms.Values)
			// 	foreach (var tilePos in room.Tiles)
			// 		GameClient.World.RemoveTileRoom(tilePos);

			m_Rooms.Clear();

			var roomSystem = GameServer.ECS_ServerWorld.GetExistingSystem<ServerRoomSystem>();
			
			int roomCount = reader.ReadInt32();
			for (int i = 0; i < roomCount; i++)
			{
				long roomId = reader.ReadInt64();

				var newRoom = new ServerRoom(roomId);

				int roomSize = reader.ReadByte();
				for (int j = 0; j < roomSize; j++)
				{
					int2 tilePos = new int2(reader.ReadInt32(), reader.ReadInt32());
					newRoom.AddTileToRoom(tilePos);
					
					// GameServer.World.AddTileRoom(tilePos, roomId);
					roomSystem.AddRoomDataToQueue(new RoomData
					{
						Position = tilePos,
						RoomId = roomId
					});
				}

				newRoom.CalculateRoomBounds();

				int roomResourcesCount = reader.ReadInt32();
				for (int j = 0; j < roomResourcesCount; j++)
				{
					string id = IdTranslator.Instance.GetId<RoomResourceData>(reader.ReadUInt16());
					newRoom.Resources[id] = reader.ReadInt32();
				}

                int roomAffectorsCount = reader.ReadInt32();
                for (int j = 0; j < roomAffectorsCount; j++)
                {
                    string typeRef = IdTranslator.Instance.GetId<EnvironmentData>(reader.ReadUInt16());
                    newRoom.roomEnvironmentAffectors[typeRef] = reader.ReadInt32();
                }

                int roomValuesCount = reader.ReadInt32();
				for (int j = 0; j < roomValuesCount; j++)
				{
					string typeRef = IdTranslator.Instance.GetId<EnvironmentData>(reader.ReadUInt16());
					newRoom.roomEnvironmentValues[typeRef] = reader.ReadInt32();
				}

				int roomPropertiesCount = reader.ReadInt32();
				for (int j = 0; j < roomPropertiesCount; j++)
				{
					string propertyRef = IdTranslator.Instance.GetId<RoomPropertyData>(reader.ReadUInt16());
					newRoom.RoomProperties[propertyRef] = reader.ReadByte();
				}

				newRoom.ParentBaseId = reader.ReadInt64();
				newRoom.Name = reader.ReadString();

				m_Rooms.Add(roomId, newRoom);
			}
			
            m_FailedCalculationList.Clear();
            // Populate failed list
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int netId = reader.ReadInt32();
                m_FailedCalculationList.Add(GameServer.NetConnector.GetEntity(netId));
            }
		}

		public void PromptRoomConstruction(float2 position, FNEEntity builder)
		{
			int tileX = (int)position.x;
			int tileY = (int)position.y;

			bool isWest = position.x % 1 == 0;
			bool isSouth = position.y % 1 == 0;

			int room1Created = -1;
			int room2Created = -1;
			long roomId = System.DateTime.Now.Ticks;
			if (isWest)
			{
				room1Created = TryCreateRoom(new int2(tileX - 1, tileY), roomId, builder);
				room2Created = TryCreateRoom(new int2(tileX, tileY), ++roomId, builder);
			}
			else if (isSouth)
			{
				room1Created = TryCreateRoom(new int2(tileX, tileY - 1), roomId, builder);
				room2Created = TryCreateRoom(new int2(tileX, tileY), ++roomId, builder);
			}
			else
			{
				Debug.LogError("EDGE OBJECT IS NEITHER SOUTH, NOR WEST! Should be impossible");
			}

			if (room1Created == 1 && room2Created == 0)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
				   "indoors_icon",
				   "yellow",
				   "false",
				   "A room was split into two new rooms!"
			   );
			}
			else if (room2Created == 1 && room1Created == 0)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
				   "indoors_icon",
				   "yellow",
				   "false",
				   "A room was split into two new rooms!"
			   );
			}

			if (room2Created == 1 || room2Created == 1)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
				   "indoors_icon",
				   "green",
				   "false",
				   "A new room has been created!"
			   );
			}

			if (room1Created == 1 || room2Created == 1)
			{
				GameServer.NetAPI.World_RoomManager_BA();
			}
		}

		private int TryCreateRoom(int2 position, long roomId, FNEEntity builder)
		{
			List<int2> roomTiles = new List<int2>();

			m_AdjacentBasesFound.Clear();

			long baseId;
			if (IsTileWithinBase(position, out baseId))
			{
				var success = RoomFill(position, roomTiles);

				if (success)
				{
					return CreateRoom(roomTiles, roomId, builder, baseId) ? 1 : 0;
				}
				else
				{
					return -1;
				}
			}

			return -1;
		}

		public ServerRoom GetRoom(int2 tilePos)
		{
			var id = GameServer.World.GetTileRoom(tilePos);
			if (!m_Rooms.ContainsKey(id))
				return null;

			return (ServerRoom) m_Rooms[id];
		}

		private bool RoomFill(int2 tilePos, List<int2> roomTiles)
		{
			// Maximum size in tiles
			if (roomTiles.Count >= 100)
			{
				return false;
			}

			roomTiles.Add(tilePos);

			// Maximum size in x axis
			if (math.abs(tilePos.x - roomTiles[0].x) > 10)
			{
				return false;
			}
			// Maximum size in y axis
			if (math.abs(tilePos.y - roomTiles[0].y) > 10)
			{
				return false;
			}

			var world = GameServer.World;
			
			// Do Room fill
			bool traverseNorth =  !world.IsTileNorthEdgeOccupied(tilePos);
			bool traverseEast = !world.IsTileEastEdgeOccupied(tilePos);

			bool traverseSouth = false;
			if (tilePos.y - 1 >= 0)
				traverseSouth = !world.IsTileSouthEdgeOccupied(tilePos);

			bool traverseWest = false;
			if (tilePos.x - 1 >= 0)
				traverseWest = !world.IsTileWestEdgeOccupied(tilePos);

			bool Success = true;
			
			if (traverseNorth && !roomTiles.Contains(new int2(tilePos.x, tilePos.y + 1)))
				Success = Success && RoomFill(new int2(tilePos.x, tilePos.y + 1), roomTiles);

			if (traverseEast && !roomTiles.Contains(new int2(tilePos.x + 1, tilePos.y)))
				Success = Success && RoomFill(new int2(tilePos.x + 1, tilePos.y), roomTiles);

			if (traverseSouth && !roomTiles.Contains(new int2(tilePos.x, tilePos.y - 1)))
				Success = Success && RoomFill(new int2(tilePos.x, tilePos.y - 1), roomTiles);

			if (traverseWest && !roomTiles.Contains(new int2(tilePos.x - 1, tilePos.y)))
				Success = Success && RoomFill(new int2(tilePos.x - 1, tilePos.y), roomTiles);

			return Success;
		}

		private bool CreateRoom(List<int2> roomTiles, long roomId, FNEEntity builder, long baseId)
		{
			long previousRoomId = GameServer.World.GetTileRoom(roomTiles[0]);
			var previousRoom = GameServer.RoomManager.GetRoom(previousRoomId);

			if (previousRoomId != 0 && previousRoom.Size == roomTiles.Count)
			{
				// Wall was built inside another room, and did not necessarilly enclose a new space
				// However, room might have been previously split. Hence, recalculate its values
				var room = (ServerRoom)GameServer.RoomManager.GetRoom(previousRoomId);
				room.ParentBaseId = previousRoom.ParentBaseId;
				RecalculateBaseStatus(room.ParentBaseId, builder);

				return false;
			}

			ServerRoom newRoom = new ServerRoom(roomId);
			newRoom.ParentBaseId = baseId;
			newRoom.Name = "New Room";
			GameServer.NetAPI.Notification_SendNotification_BA(
				"indoors_icon",
				"yellow",
				"false",
				("New room attached to base: " + GameServer.RoomManager.GetBaseName(baseId) + "!")
			);
			
			foreach (var tilePos in roomTiles)
			{
				previousRoomId = GameServer.World.GetTileRoom(tilePos);
				if (previousRoomId == 0)
				{
					// Add tile to new room
					newRoom.AddTileToRoom(tilePos);
					GameServer.World.AddTileRoom(tilePos, roomId);

					GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(tilePos.x + 0.5f, tilePos.y + 0.5f), 0);
				}
				else
				{
					// Remove tile from old room
					GameServer.RoomManager.GetRoom(previousRoomId).RemoveTileFromRoom(tilePos);
					GameServer.World.RemoveTileRoom(tilePos);
					if (GameServer.RoomManager.GetRoom(previousRoomId).Size == 0)
						RemoveRoom(previousRoomId);

					// Add tile to new room
					newRoom.AddTileToRoom(tilePos);
					GameServer.World.AddTileRoom(tilePos, roomId);

					GameServer.NetAPI.Effect_SpawnEffect_BAR("tile_debug2", new float2(tilePos.x + 0.5f, tilePos.y + 0.5f), 0);
				}
			}

			newRoom.CalculateRoomBounds();

            QuestManager.OnRoomCreation(newRoom);
            m_Rooms.Add(roomId, newRoom);

            RecalculateBaseStatus(newRoom.ParentBaseId, builder);
            
			/*if (m_AdjacentBasesFound.Count == 1)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
					"default",
					"yellow",
					"false",
					("New room attached to base: " + GameServer.RoomManager.GetBaseName(m_AdjacentBasesFound[0]) + "!")
				);
				newRoom.ParentBaseId = m_AdjacentBasesFound[0];
				newRoom.Name = "Room: " + (Time.time * 10000);
				RecalculateBaseStatus(newRoom.ParentBaseId, builder);
			}
			else if (m_AdjacentBasesFound.Count == 0)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
					"default",
					"yellow",
					"false",
					("A Base with name Base: " + ((int)(Time.time * 10000)) + " was created!")
				);
				m_Bases.Add(
					(long)(Time.time * 10000), 
					new BaseData(
						"Base: " + (int)(Time.time * 10000),
						true,
						new float2(
							newRoom.minX + ((newRoom.maxX - newRoom.minX) / 2),
							newRoom.minY + ((newRoom.maxY - newRoom.minY) / 2)
						)
					)
				);
				newRoom.ParentBaseId = (long)(Time.time * 10000);
				newRoom.Name = "Room: " + ((int)(Time.time * 10000));
				RecalculateBaseStatus(newRoom.ParentBaseId, builder);
			}
			else if (m_AdjacentBasesFound.Count > 1)
			{
				var finalBase = m_AdjacentBasesFound[0];
				m_AdjacentBasesFound.RemoveAt(0);

				foreach (var remainingBase in m_AdjacentBasesFound)
				{
					foreach (var room in m_Rooms.Values)
					{
						if (room.ParentBaseId == remainingBase)
						{
							room.ParentBaseId = finalBase;
						}
					}

					m_Bases.Remove(remainingBase);
				}

				foreach (var baseFound in m_AdjacentBasesFound)
				{
					GameServer.NetAPI.Notification_SendNotification_BA(
						"default",
						"yellow",
						"false",
						("Merged base " + GameServer.RoomManager.GetBaseName(baseFound) + " into " + GameServer.RoomManager.GetBaseName(finalBase) + "!")
					);
				}

				GameServer.NetAPI.Notification_SendNotification_BA(
					"default",
					"yellow",
					"false",
					("New room attached to base: " + GameServer.RoomManager.GetBaseName(finalBase) + "!")
				);

				newRoom.ParentBaseId = finalBase;
				newRoom.Name = "Room: " + ((int)(Time.time * 10000));
				RecalculateBaseStatus(newRoom.ParentBaseId, builder);
			}*/

			return true;
		}

		public void RecalculateBaseStatus(long baseId, FNEEntity triggeringPlayer = null)
		{
			var roomsToCalculate = m_Rooms.Where(r => r.Value.ParentBaseId == baseId);

			// First calculate properties
			foreach (var roomEntry in roomsToCalculate)
			{
				var room = (ServerRoom) roomEntry.Value;

				room.InitRoomRecalculation();
			}

			bool roomsCalculating = false;
			do
			{
				roomsCalculating = false;
				foreach (var roomEntry in roomsToCalculate)
				{
					var room = (ServerRoom)roomEntry.Value;
					roomsCalculating = roomsCalculating || room.CalculateRoomStatus();
				}
			} while (roomsCalculating);

			m_FailedCalculationList.Clear();
			foreach (var roomEntry in roomsToCalculate)
			{
				var room = (ServerRoom) roomEntry.Value;
				room.AddRoomCalculationFailsToLists(m_FailedCalculationList);
                if(triggeringPlayer != null)
                    QuestManager.OnRoomCreation(room);
            }

            var baseData = m_Bases[baseId];
			if(m_FailedCalculationList.Count > 0 && baseData.IsOnline)
			{
				GameServer.NetAPI.Notification_SendWarningNotification_BA(
					m_Bases[baseId].Name + " " + Localization.GetString("base_calculation_fail_info")
				);

				SetBaseOnline(baseId, false);
			}
			else if(m_FailedCalculationList.Count == 0 && !baseData.IsOnline)
			{
				GameServer.NetAPI.Notification_SendNotification_BA(
					"warning_icon",
					"blue",
					"false",
					m_Bases[baseId].Name + " " + Localization.GetString("base_back_online_info")
				);

				SetBaseOnline(baseId, true);
			}
		}

		public void RemoveRoom(long id)
		{
			m_Rooms.Remove(id);
		}

		public void CreateNewBase(int2 position)
		{
			m_Bases.Add(
				(long)(Time.time * 10000), 
				new BaseData(
					"New Base",
					true,
					new float2(
						position.x,
						position.y
					),
					4
				)
			);
		}
	}
}