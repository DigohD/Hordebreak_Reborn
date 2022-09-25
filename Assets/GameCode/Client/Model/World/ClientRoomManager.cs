using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Model.World.Rooms;
using Lidgren.Network;
using System.Collections.Generic;
using System.Linq;
using FNZ.Client.Systems;
using Unity.Mathematics;

namespace FNZ.Client.Model.World
{
	public delegate void OnBaseRoomNameChange();

	public class ClientRoomManager : RoomManager
	{
		public OnBaseRoomNameChange d_OnBaseRoomNameChange;

		public List<long> GetBases()
		{
			return m_Bases.Keys.ToList();
		}

		public List<Room> GetBaseRooms(long baseId)
		{
			return m_Rooms.Values.Where(room => room.ParentBaseId == baseId).ToList();
		}

		public Room GetRoom(int2 tilePos)
		{
			var id = GameClient.World.GetTileRoom(tilePos);
			if (!m_Rooms.ContainsKey(id))
				return null;

			return m_Rooms[id];
		}

		public void Deserialize(NetBuffer reader)
		{
			m_Bases.Clear();
			UIManager.Instance.ClearUIBaseArrows();

			var count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				long baseId = reader.ReadInt64();
				var BaseData = new BaseData();
				BaseData.Deserialize(reader);

				m_Bases.Add(baseId, BaseData);
				UIManager.Instance.NewUIArrow(BaseData.Name, BaseData.Position);
			}
			
			var roomSystem = GameClient.ECS_ClientWorld.GetExistingSystem<ClientRoomSystem>();

			foreach (var room in m_Rooms.Values)
			{
				foreach (var tilePos in room.Tiles)
				{
					roomSystem.AddRoomToBeRemvoedQueue(tilePos);
				}
			}
			
			m_Rooms.Clear();

			int roomCount = reader.ReadInt32();
			for (int i = 0; i < roomCount; i++)
			{
				long roomId = reader.ReadInt64();

				Room newRoom = new Room(roomId);

				int roomSize = reader.ReadByte();
				for (int j = 0; j < roomSize; j++)
				{
					int2 tilePos = new int2(reader.ReadInt32(), reader.ReadInt32());
					newRoom.AddTileToRoom(tilePos);
					// GameClient.World.AddTileRoom(tilePos, roomId);
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
                m_FailedCalculationList.Add(GameClient.NetConnector.GetEntity(netId));
            }

            HighlightFails();

            d_OnBaseRoomNameChange?.Invoke();
		}

        private void HighlightFails()
        {
            GameClient.ViewAPI.HighlightBaseFails(m_FailedCalculationList);
        }
    }
}