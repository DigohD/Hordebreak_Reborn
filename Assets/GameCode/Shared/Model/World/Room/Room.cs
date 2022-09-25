using FNZ.Shared.Model.Entity.Components.RoomRequirements;
using FNZ.Shared.Model.World.Environment;
using System.Collections.Generic;
using Unity.Mathematics;

namespace FNZ.Shared.Model.World
{
	public class Room
	{
		public long Id;
		public byte Size;
		public long ParentBaseId;

		public Dictionary<string, int> Resources = new Dictionary<string, int>();

		public Dictionary<string, int> roomEnvironmentValues;
		public Dictionary<string, int> roomEnvironmentAffectors;

		public List<int2> Tiles = new List<int2>();

		public Dictionary<string, byte> RoomProperties = new Dictionary<string, byte>();

		public string Name;

		public int width, height;
		public int minX, minY, maxX, maxY;

		public Room(long newId)
		{
			roomEnvironmentValues = new Dictionary<string, int>();
			roomEnvironmentAffectors = new Dictionary<string, int>();
			Id = newId;

            var envTypes = DataBank.Instance.GetAllDataIdsOfType<EnvironmentData>();
            foreach (var envType in envTypes)
            {
                roomEnvironmentValues.Add(envType.Id, 0);
                roomEnvironmentAffectors.Add(envType.Id, 0);
            }

			foreach (var data in DataBank.Instance.GetAllDataIdsOfType<RoomResourceData>())
				Resources.Add(data.Id, 0);				
		}

		public void AddTileToRoom(int2 pos)
		{
			Size++;
			Tiles.Add(pos);
		}

		public void RemoveTileFromRoom(int2 pos)
		{
			Size--;
			Tiles.Remove(pos);
		}

		public void CalculateRoomBounds()
		{
			var roomMaxX = int.MinValue;
			var roomMinX = int.MaxValue;
			var roomMaxY = int.MinValue;
			var roomMinY = int.MaxValue;

			foreach (var tile in Tiles)
			{
				maxX = maxX < tile.x ? tile.x : maxX;
				minX = minX > tile.x ? tile.x : minX;
				maxY = maxY < tile.y ? tile.y : maxY;
				minY = minY > tile.y ? tile.y : minY;

				roomMaxX = roomMaxX < tile.x ? tile.x : roomMaxX;
				roomMinX = roomMinX > tile.x ? tile.x : roomMinX;
				roomMaxY = roomMaxY < tile.y ? tile.y : roomMaxY;
				roomMinY = roomMinY > tile.y ? tile.y : roomMinY;
			}

			width = (roomMaxX - roomMinX) + 1;
			height = (roomMaxY - roomMinY) + 1;
			minX = roomMinX;
			minY = roomMinY;
		}

		public bool DoesRoomFulfillRequirements(List<RoomPropertyRequirementData> reqs)
		{
			foreach(var req in reqs)
			{
				if (!RoomProperties.ContainsKey(req.propertyRef))
					return false;

				if (RoomProperties[req.propertyRef] < req.level)
					return false;
			}

			return true;
		}

		public bool DoesRoomContainTile(int2 toCheck)
        {
			if(toCheck.x <= maxX && toCheck.x >= minX && toCheck.y <= maxY && toCheck.y >= minY)
            {
				return Tiles.Contains(toCheck);
            }

			return false;
        }
	}
}