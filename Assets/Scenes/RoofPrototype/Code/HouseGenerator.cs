using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scenes.RoofPrototype.Code 
{
	public class GhostWall
	{
		public float X;
		public float Y;
		public float rotation;

		public GhostWall neighbor;
	}

	public enum WallDirection
	{
		WEST = 0,
		NORTH = 1,
		EAST = 2,
		SOUTH = 3,
		NONE = 4
	}

	public class HouseGenerator : MonoBehaviour
	{
		public GameObject[] WallPrefabs;
		public GameObject CornerPrefab;
		public GameObject[] WallDetailDecors;
		public GameObject RoofPlatePrefab;
		public GameObject RoofEdgePrefab;
		public GameObject RoofCornerPrefab;
		public GameObject[] WallBaseDecors;

		public GameObject[] RoofDetails;

		private Dictionary<Vector2Int, bool> m_Corners = new Dictionary<Vector2Int, bool>();

		private Dictionary<int, Dictionary<Vector2, GhostWall>> m_FloorWalls = new Dictionary<int, Dictionary<Vector2, GhostWall>>();
		private List<GhostWall> m_FloorWallOrigins = new List<GhostWall>();

		private List<RectInt> floorShelfMasks = new List<RectInt>();

		private RectInt m_FloorShelfMask;

		private List<Vector2Int> m_BlockList = new List<Vector2Int>();

		public void GenerateBuilding(Transform parent, bool[,] blockMap)
		{
			int width = blockMap.GetLength(0);
			int height = blockMap.GetLength(1);

			int floors = 10;

			m_FloorShelfMask = new RectInt
			{
				x = 0,
				y = 0,
				height = height,
				width = width
			};

			m_BlockList.Clear();

			for (int floor = 0; floor < floors; floor++)
            {
				m_Corners.Clear();
				m_FloorWalls.Clear();
				m_FloorWallOrigins.Clear();

				m_FloorWalls.Add(floor, new Dictionary<Vector2, GhostWall>());

				bool lastFloor = floor == floors - 1;
				bool generateRoof = false;

				var floorShelfMask = GetNewFloorShelfMask(m_FloorShelfMask, floor);

				if (m_FloorShelfMask.width <= 3 || m_FloorShelfMask.height <= 3)
				{
					floorShelfMask.x = 0;
					floorShelfMask.y = 0;
					floorShelfMask.width = 0;
					floorShelfMask.height = 0;
				}

				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if (blockMap[x, y])
						{
							generateRoof = !(x >= floorShelfMask.x && x <= floorShelfMask.width && y >= floorShelfMask.y && y <= floorShelfMask.height);
							GenerateBuildingFloorTile(x, y, width, height, floor, parent, blockMap, generateRoof);

							if (generateRoof)
                            {
								m_BlockList.Add(new Vector2Int(x, y));
                            }
						}
					}
				}

				foreach (var e in m_BlockList)
                {
					blockMap[e.x, e.y] = false;
				}

				GenerateXModuloSymmetricalWalls(SceneControl.SymetricXModulo, parent, floor);

				m_FloorShelfMask.x = floorShelfMask.x;
				m_FloorShelfMask.y = floorShelfMask.y;
				m_FloorShelfMask.width = floorShelfMask.width;
				m_FloorShelfMask.height = floorShelfMask.height;

				if (m_FloorShelfMask.width == 0 && m_FloorShelfMask.height == 0)
					break;
			}
		}

		private RectInt GetNewFloorShelfMask(RectInt lastMask, int floor)
		{
			var result = new RectInt();

			var newWidth = Random.Range(lastMask.width, lastMask.width * 0.5f);
			var newHeight = Random.Range(lastMask.height, lastMask.height * 0.5f);

			result.x = lastMask.x + Random.Range(0, lastMask.width - (int)newWidth);
			result.y = lastMask.y + Random.Range(0, lastMask.height - (int)newHeight);
			
			result.width = (int)newWidth;
			result.height = (int)newHeight;

			return result;
		}

		private void GenerateBuildingFloorTile(int x, int y, int width, int height, int floor, Transform parent, bool[,] blockMap, bool generateRoof)
        {
			bool westEmpty = x - 1 < 0 || !blockMap[x - 1, y];
			bool eastEmpty = x + 1 >= width || !blockMap[x + 1, y];

			bool northEmpty = y - 1 < 0 || !blockMap[x, y - 1];
			bool northWestEmpty = y - 1 < 0 || x - 1 < 0 || !blockMap[x - 1, y - 1];
			bool northEastEmpty = y - 1 < 0 || x + 1 >= width || !blockMap[x + 1, y - 1];

			bool southEmpty = y + 1 >= height || !blockMap[x, y + 1];
			bool southWestEmpty = y + 1 >= height || x - 1 < 0 || !blockMap[x - 1, y + 1];
			bool southEastEmpty = y + 1 >= height || x + 1 >= width || !blockMap[x + 1, y + 1];

			// Generate corners

			// upper left case

			bool shouldGenerateUpperLeftCornerCase1 = !westEmpty && northWestEmpty && !northEmpty;
			bool shouldGenerateUpperLeftCornerCase2 = westEmpty && !northWestEmpty && !northEmpty;
			bool shouldGenerateUpperLeftCornerCase3 = !westEmpty && !northWestEmpty && northEmpty;
			bool shouldGenerateUpperLeftCornerCase4 = westEmpty && northWestEmpty && northEmpty;

			if (shouldGenerateUpperLeftCornerCase1 || shouldGenerateUpperLeftCornerCase2 || shouldGenerateUpperLeftCornerCase3 || shouldGenerateUpperLeftCornerCase4)
			{
				CreateHörnIfNotExist(parent, x, y, floor, generateRoof);
			}

			// upper right case

			bool shouldGenerateUpperRightCornerCase1 = !eastEmpty && northEastEmpty && !northEmpty;
			bool shouldGenerateUpperRightCornerCase2 = eastEmpty && !northEastEmpty && !northEmpty;
			bool shouldGenerateUpperRightCornerCase3 = !eastEmpty && !northEastEmpty && northEmpty;
			bool shouldGenerateUpperRightCornerCase4 = eastEmpty && northEastEmpty && northEmpty;

			if (shouldGenerateUpperRightCornerCase1 || shouldGenerateUpperRightCornerCase2 || shouldGenerateUpperRightCornerCase3 || shouldGenerateUpperRightCornerCase4)
			{
				CreateHörnIfNotExist(parent, x + 1, y, floor, generateRoof);
			}

			// lower left case

			bool shouldGenerateLowerLeftCornerCase1 = !westEmpty && southWestEmpty && !southEmpty;
			bool shouldGenerateLowerLeftCornerCase2 = westEmpty && !southWestEmpty && !southEmpty;
			bool shouldGenerateLowerLeftCornerCase3 = !westEmpty && !southWestEmpty && southEmpty;
			bool shouldGenerateLowerLeftCornerCase4 = westEmpty && southWestEmpty && southEmpty;

			if (shouldGenerateLowerLeftCornerCase1 || shouldGenerateLowerLeftCornerCase2 || shouldGenerateLowerLeftCornerCase3 || shouldGenerateLowerLeftCornerCase4)
			{
				CreateHörnIfNotExist(parent, x, y + 1, floor, generateRoof);
			}

			// lower right case

			bool shouldGenerateLowerRightCornerCase1 = !eastEmpty && southEastEmpty && !southEmpty;
			bool shouldGenerateLowerRightCornerCase2 = eastEmpty && !southEastEmpty && !southEmpty;
			bool shouldGenerateLowerRightCornerCase3 = !eastEmpty && !southEastEmpty && southEmpty;
			bool shouldGenerateLowerRightCornerCase4 = eastEmpty && southEastEmpty && southEmpty;

			if (shouldGenerateLowerRightCornerCase1 || shouldGenerateLowerRightCornerCase2 || shouldGenerateLowerRightCornerCase3 || shouldGenerateLowerRightCornerCase4)
			{
				CreateHörnIfNotExist(parent, x + 1, y + 1, floor, generateRoof);
			}

			// Generate Walls

			// WEST
			if (westEmpty)
			{
				var newWall = new GhostWall
				{
					X = x,
					Y = y + 0.5f,
					rotation = 90
				};

				CalculateVerticalNeighbors(newWall, floor);

				var key = new Vector2(newWall.X, newWall.Y);

				if (!m_FloorWalls[floor].ContainsKey(key))
					m_FloorWalls[floor].Add(key, newWall);
			}

			// EAST
			if (eastEmpty)
			{
				var newWall = new GhostWall
				{
					X = x + 1,
					Y = y + 0.5f,
					rotation = -90
				};

				CalculateVerticalNeighbors(newWall, floor);

				var key = new Vector2(newWall.X, newWall.Y);
				if (!m_FloorWalls[floor].ContainsKey(key))
					m_FloorWalls[floor].Add(key, newWall);
			}

			// NORTH
			if (northEmpty)
			{
				var newWall = new GhostWall
				{
					X = x + 0.5f,
					Y = y
				};

				CalculateHorizontalNeighbors(newWall, floor);

				var key = new Vector2(newWall.X, newWall.Y);

				if (!m_FloorWalls[floor].ContainsKey(key))
					m_FloorWalls[floor].Add(key, newWall);
			}

			// SOUTH
			if (southEmpty)
			{
				var newWall = new GhostWall
				{
					X = x + 0.5f,
					Y = y + 1,
					rotation = 180
				};

				CalculateHorizontalNeighbors(newWall, floor);

				var key = new Vector2(newWall.X, newWall.Y);

				if (!m_FloorWalls[floor].ContainsKey(key))
					m_FloorWalls[floor].Add(key, newWall);
			}

			// Generate roof

			if (generateRoof)
			{
				var go = Instantiate(RoofPlatePrefab);
				go.transform.position = new Vector3(x + 0.5f, 2 * floor + 2, y + 0.5f);
				go.transform.rotation = Quaternion.Euler(0, 0, 0);

				go.transform.SetParent(parent);

				if (Random.Range(0, 100) < 15)
				{
					go = Instantiate(RoofDetails[Random.Range(0, RoofDetails.Length)]);
					go.transform.position = new Vector3(x + 0.5f, 2 * floor + 2, y + 0.5f);
					go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

					go.transform.SetParent(parent);
				}
			}
		}

		private void CreateHörnIfNotExist(Transform parent, int x, int y, int floor, bool shouldSpawnRoofCorners)
		{
			var key = new Vector2Int(x, y);

			if (m_Corners.ContainsKey(key))
				return;

			var corner = Instantiate(CornerPrefab);
			corner.transform.position = new Vector3(x, floor * 2, y);
			corner.transform.rotation = Quaternion.Euler(0, 0, 0);

			corner.transform.SetParent(parent);

			if (shouldSpawnRoofCorners)
            {
				var roofCorner = Instantiate(RoofCornerPrefab);
				roofCorner.transform.position = new Vector3(x, floor * 2 + 2, y);
				roofCorner.transform.rotation = Quaternion.Euler(0, 0, 0);

				roofCorner.transform.SetParent(parent);
			}

			m_Corners.Add(key, true);
		}

		private void GenerateXModuloSymmetricalWalls(int modulo, Transform parent, int floor)
		{
			/*foreach (var wall in m_FloorWalls.Values)
			{
				var go = Instantiate(WallPrefab);
				go.transform.position = new Vector3(wall.X, 0, wall.Y);
				go.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
				go.transform.SetParent(parent);

				if (m_FloorWallOrigins.Contains(wall))
				{
					var mats = go.GetComponentInChildren<MeshRenderer>().materials;
					mats[0].SetColor("_BaseColor", Color.green);
					go.GetComponentInChildren<MeshRenderer>().materials = mats;
				}
				else
				{
					var mats = go.GetComponentInChildren<MeshRenderer>().materials;
					mats[0].SetColor("_BaseColor", Color.yellow);
					go.GetComponentInChildren<MeshRenderer>().materials = mats;
				}
			}*/

			int wallHeight = floor * 2;
			int roofEdgeHeight = floor * 2 + 2;

			foreach(var wall in m_FloorWallOrigins){
				int wallLength = 1;
				var linkedWall = wall.neighbor;
				WallDirection dir;

				if(linkedWall != null && linkedWall.X > wall.X) {
					dir = WallDirection.EAST;
				}else if (linkedWall != null && linkedWall.X < wall.X)
				{
					dir = WallDirection.WEST;
				}
				else if (linkedWall != null && linkedWall.Y < wall.Y)
				{
					dir = WallDirection.SOUTH;
				}
				else if (linkedWall != null && linkedWall.Y > wall.Y)
				{
					dir = WallDirection.NORTH;
				}
				else
				{
					dir = WallDirection.NONE;
				}

				// Single wall
				if(dir == WallDirection.NONE)
				{
					var go = Instantiate(WallPrefabs[0]);
					go.transform.position = new Vector3(wall.X, wallHeight, wall.Y);
					go.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					go.transform.SetParent(parent);

					var roofEdgeGO = Instantiate(RoofEdgePrefab);
					roofEdgeGO.transform.position = new Vector3(wall.X, roofEdgeHeight, wall.Y);
					roofEdgeGO.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					roofEdgeGO.transform.SetParent(parent);
					
					continue;
				}

				while (linkedWall != null)
				{
					wallLength++;
					linkedWall = linkedWall.neighbor;
				}

				for(int i = 0; i < wallLength / 2; i++)
				{
					GameObject go;
					GameObject detail = null;
					if((i + 1) % modulo == 0)
					{
						go = Instantiate(WallBaseDecors[Random.Range(0, WallBaseDecors.Length)]);
					}
					else
					{
						go = Instantiate(WallPrefabs[Random.Range(0, WallPrefabs.Length)]);

						if (Random.Range(0, 100) < 10)
						{
							detail = Instantiate(WallDetailDecors[Random.Range(0, WallDetailDecors.Length)]);
						}
					}

					var roofEdgeGO = Instantiate(RoofEdgePrefab);

					switch (dir)
					{
						case WallDirection.EAST:
							go.transform.position = new Vector3(wall.X + i, wallHeight, wall.Y);
							roofEdgeGO.transform.position = new Vector3(wall.X + i, roofEdgeHeight, wall.Y);
							break;

						case WallDirection.NORTH:
							go.transform.position = new Vector3(wall.X, wallHeight, wall.Y + i);
							roofEdgeGO.transform.position = new Vector3(wall.X, roofEdgeHeight, wall.Y + i);
							break;

						case WallDirection.WEST:
							go.transform.position = new Vector3(wall.X - 1, wallHeight, wall.Y);
							roofEdgeGO.transform.position = new Vector3(wall.X - 1, roofEdgeHeight, wall.Y);
							break;

						case WallDirection.SOUTH:
							go.transform.position = new Vector3(wall.X, wallHeight, wall.Y - i);
							roofEdgeGO.transform.position = new Vector3(wall.X, roofEdgeHeight, wall.Y - i);
							break;
					}

					go.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					go.transform.SetParent(parent);

					roofEdgeGO.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					roofEdgeGO.transform.SetParent(parent);

					if (detail != null)
					{
						detail.transform.position = go.transform.position;
						detail.transform.rotation = go.transform.rotation;
						detail.transform.SetParent(parent);
					}
				}

				int mod = 0;
				for (int i = wallLength - 1; i > wallLength / 2 - 1; i--)
				{
					mod++;
					GameObject go;
					GameObject detail = null;
					if (mod % modulo == 0)
					{
						go = Instantiate(WallBaseDecors[Random.Range(0, WallBaseDecors.Length)]);
					}
					else
					{
						go = Instantiate(WallPrefabs[Random.Range(0, WallPrefabs.Length)]);
						if (Random.Range(0, 100) < 10)
						{
							detail = Instantiate(WallDetailDecors[Random.Range(0, WallDetailDecors.Length)]);
						}
					}

					var roofEdgeGO = Instantiate(RoofEdgePrefab);

					switch (dir)
					{
						case WallDirection.EAST:
							go.transform.position = new Vector3(wall.X + i, wallHeight, wall.Y);
							roofEdgeGO.transform.position = new Vector3(wall.X + i, roofEdgeHeight, wall.Y);
							break;

						case WallDirection.NORTH:
							go.transform.position = new Vector3(wall.X, wallHeight, wall.Y + i);
							roofEdgeGO.transform.position = new Vector3(wall.X, roofEdgeHeight, wall.Y + i);
							break;

						case WallDirection.WEST:
							go.transform.position = new Vector3(wall.X - 1, wallHeight, wall.Y);
							roofEdgeGO.transform.position = new Vector3(wall.X - 1, roofEdgeHeight, wall.Y);
							break;

						case WallDirection.SOUTH:
							go.transform.position = new Vector3(wall.X, wallHeight, wall.Y - i);
							roofEdgeGO.transform.position = new Vector3(wall.X, roofEdgeHeight, wall.Y - i);
							break;
					}

					go.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					go.transform.SetParent(parent);

					roofEdgeGO.transform.rotation = Quaternion.Euler(0, wall.rotation, 0);
					roofEdgeGO.transform.SetParent(parent);
					
					if(detail != null)
					{
						detail.transform.position = go.transform.position;
						detail.transform.rotation = go.transform.rotation;
						detail.transform.SetParent(parent);
					}
				}
			}
		}

		private void CalculateVerticalNeighbors(GhostWall newWall, int floor)
		{
			var upNeighborKey = new Vector2(newWall.X, newWall.Y + 1);
			var downNeighborKey = new Vector2(newWall.X, newWall.Y - 1);

			bool foundNeighbor = false;
			if (m_FloorWalls[floor].ContainsKey(upNeighborKey) && m_FloorWalls[floor][upNeighborKey].rotation == newWall.rotation)
			{
				m_FloorWalls[floor][upNeighborKey].neighbor = newWall;
				foundNeighbor = true;
			}
			else if (m_FloorWalls[floor].ContainsKey(downNeighborKey) && m_FloorWalls[floor][downNeighborKey].rotation == newWall.rotation)
			{
				m_FloorWalls[floor][downNeighborKey].neighbor = newWall;
				foundNeighbor = true;
			}

			if (!foundNeighbor)
			{
				m_FloorWallOrigins.Add(newWall);
			}
		}

		private void CalculateHorizontalNeighbors(GhostWall newWall, int floor)
		{
			var westNeighborKey = new Vector2(newWall.X + 1, newWall.Y);
			var eastNeighborKey = new Vector2(newWall.X - 1, newWall.Y);

			bool foundNeighbor = false;
			if (m_FloorWalls[floor].ContainsKey(westNeighborKey) && m_FloorWalls[floor][westNeighborKey].rotation == newWall.rotation)
			{
				m_FloorWalls[floor][westNeighborKey].neighbor = newWall;
				foundNeighbor = true;
			}
			else if (m_FloorWalls[floor].ContainsKey(eastNeighborKey) && m_FloorWalls[floor][eastNeighborKey].rotation == newWall.rotation)
			{
				m_FloorWalls[floor][eastNeighborKey].neighbor = newWall;
				foundNeighbor = true;
			}

			if (!foundNeighbor)
			{
				m_FloorWallOrigins.Add(newWall);
			}
		}
	}
}