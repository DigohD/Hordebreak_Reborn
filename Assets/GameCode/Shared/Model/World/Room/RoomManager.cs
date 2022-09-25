using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Model.World.Rooms;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Model.World
{
	public class RoomManager
	{
		protected Dictionary<long, BaseData> m_Bases = new Dictionary<long, BaseData>();
		
		protected Dictionary<long, Room> m_Rooms = new Dictionary<long, Room>();

        protected List<FNEEntity> m_FailedCalculationList = new List<FNEEntity>();

        public Dictionary<long, Room> GetRooms()
		{
			return m_Rooms;
		}

		public Room GetRoom(long id)
		{
			if (!m_Rooms.ContainsKey(id))
				return null;

			return m_Rooms[id];
		}
		
		public bool BaseExists(long id)
		{
			return m_Bases.ContainsKey(id);
		}
		
		public BaseData GetBase(long id)
		{
			if (!m_Bases.ContainsKey(id))
				return default;

			return m_Bases[id];
		}

		public string GetBaseName(long id)
		{
			if (!m_Bases.ContainsKey(id))
				return null;

			return m_Bases[id].Name;
		}

		public bool IsBaseOnline(long id)
		{
			if (!m_Bases.ContainsKey(id))
				return false;

			return m_Bases[id].IsOnline;
		}

		public void SetBaseName(long id, string newName)
		{
			if (!m_Bases.ContainsKey(id))
				return;
		
			m_Bases[id] = new BaseData(newName, m_Bases[id].IsOnline, m_Bases[id].Position, m_Bases[id].radius);
		}

		public void SetBaseOnline(long id, bool isOnline)
		{
			if (!m_Bases.ContainsKey(id))
				return;

			m_Bases[id] = new BaseData(m_Bases[id].Name, isOnline, m_Bases[id].Position, m_Bases[id].radius);
		}

		public void Serialize(NetBuffer sendBuffer)
		{
			sendBuffer.Write(m_Bases.Count);
			foreach (var baseId in m_Bases.Keys)
			{
				sendBuffer.Write(baseId);
				m_Bases[baseId].Serialize(sendBuffer);
			}
			
			sendBuffer.Write(m_Rooms.Count);
			foreach (var roomId in m_Rooms.Keys)
			{
				sendBuffer.Write(roomId);

				var room = m_Rooms[roomId];
				sendBuffer.Write(room.Size);

				foreach (var pos in room.Tiles)
				{
					sendBuffer.Write(pos.x);
					sendBuffer.Write(pos.y);
				}

				var resources = room.Resources;
				sendBuffer.Write(resources.Count);
				foreach (var resourceKey in resources.Keys)
				{
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<RoomResourceData>(resourceKey));
					sendBuffer.Write(resources[resourceKey]);
				}

                var statusAffectors = room.roomEnvironmentAffectors;
                sendBuffer.Write(statusAffectors.Count);
                foreach (var valueKey in statusAffectors.Keys)
                {
                    sendBuffer.Write(IdTranslator.Instance.GetIdCode<EnvironmentData>(valueKey));
                    sendBuffer.Write(statusAffectors[valueKey]);
                }

                var statusValues = room.roomEnvironmentValues;
				sendBuffer.Write(statusValues.Count);
				foreach (var valueKey in statusValues.Keys)
				{
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<EnvironmentData>(valueKey));
                    sendBuffer.Write(statusValues[valueKey]);
				}

				var properties = room.RoomProperties;
				sendBuffer.Write(properties.Count);
				foreach (var propertyKey in properties.Keys)
				{
					sendBuffer.Write(IdTranslator.Instance.GetIdCode<RoomPropertyData>(propertyKey));
					sendBuffer.Write(properties[propertyKey]);
				}

				sendBuffer.Write(room.ParentBaseId);
				sendBuffer.Write(room.Name);
			}
			
            sendBuffer.Write(m_FailedCalculationList.Count);
            foreach (var failedEntity in m_FailedCalculationList)
            {
                sendBuffer.Write(failedEntity.NetId);
            }
		}

		public bool TryGetClosestBase(float2 sourcePos, out float2 basePos)
        {
			basePos = float2.zero;
			if (m_Bases.Count == 0)
				return false;

			float distance = float.MaxValue;
			foreach (var b in m_Bases.Values){
				var d = math.distance(sourcePos, b.Position);
				if (d < distance)
                {
					basePos = b.Position;
					distance = d;
				}
            }

			return true;
        }
		
		public bool IsTileWithinBase(int2 tilePos, out long baseId)
		{
			baseId = 0;
			
			if (m_Bases.Count == 0)
				return false;
			
			foreach (var baseKey in m_Bases.Keys)
			{
				var b = m_Bases[baseKey];
				
				var min = new int2((int) b.Position.x - b.radius, (int) b.Position.y - b.radius);
				var max = new int2((int) b.Position.x + b.radius, (int) b.Position.y + b.radius);
				if (tilePos.x <= max.x && tilePos.y <= max.y && tilePos.x >= min.x && tilePos.y >= min.y)
				{
					baseId = baseKey;
					return true;
				}
			}

			return false;
		}
		
		public bool IsTileWithinBase(int2 tilePos)
		{
			if (m_Bases.Count == 0)
				return false;
			
			foreach (var baseKey in m_Bases.Keys)
			{
				var b = m_Bases[baseKey];
				
				var min = new int2((int) b.Position.x - b.radius, (int) b.Position.y - b.radius);
				var max = new int2((int) b.Position.x + b.radius, (int) b.Position.y + b.radius);
				if (tilePos.x <= max.x && tilePos.y <= max.y && tilePos.x >= min.x && tilePos.y >= min.y)
				{
					return true;
				}
			}

			return false;
		}
		
		public bool IsEdgeWithinBase(float2 edgePos)
		{
			if (edgePos.x % 1 != 0)
			{
				var northTileInBase = IsTileWithinBase((int2) edgePos);
				var southTileInBase = IsTileWithinBase(new int2((int) edgePos.x, (int) edgePos.y - 1));
				return northTileInBase || southTileInBase;
			}
			else
			{
				var eastTileInBase = IsTileWithinBase((int2) edgePos);
				var westTileInBase = IsTileWithinBase(new int2((int) edgePos.x - 1, (int) edgePos.y));
				return eastTileInBase || westTileInBase;
			}
		}

		public Room GetTileRoomWithoutWorldData(int2 position)
        {
			foreach(var room in m_Rooms.Values)
            {
				if (room.DoesRoomContainTile(position))
					return room;
            }

			return null;
        }
	}
}