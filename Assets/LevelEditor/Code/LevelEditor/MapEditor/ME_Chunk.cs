using FNZ.Client.Utils;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World.Tile;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.LevelEditor
{
	public class ME_Chunk : MonoBehaviour
	{

		public Material HeatmapMat;

		public string[,] TileSheetIds = new string[32, 32 * 6];
		public string[,] TileIds = new string[32, 32];
		public byte[,] DangerLevels = new byte[32, 32];
		public bool[,] BlockingMask = new bool[32, 32];
		public bool[,] NullMask = new bool[32, 32];

		public bool[,] DangerChanged = new bool[32, 32];
		public bool[,] BlockingChanged = new bool[32, 32];

		private static int[] flushedChunkTriangles = new int[32 * 32 * 6];

		public List<GameObject> tileEdges = new List<GameObject>();

		private bool isDirty = false;

		private int m_ChunkX, m_ChunkY;

		private const byte TilesPerRow = 8;
		private const float Tile_UV_Width = 0.125f;
		
		public enum ChunkMode
		{
			FLOOR,
			DANGERMAP,
			BLOCKINGMASK
		}

		public static ChunkMode ActivechunkMode = ChunkMode.FLOOR;
		private ChunkMode LastChunkmode = ActivechunkMode;

		static ME_Chunk()
		{
			List<int> tris = new List<int>();

			int triCounter = 0;
			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					tris.Add(triCounter);
					tris.Add(triCounter + 1);
					tris.Add(triCounter + 2);
					tris.Add(triCounter);
					tris.Add(triCounter + 3);
					tris.Add(triCounter + 1);

					triCounter += 4;
				}
			}

			flushedChunkTriangles = tris.ToArray();
		}

		public void Init(int chunkX, int chunkY)
		{
			m_ChunkX = chunkX;
			m_ChunkY = chunkY;

			for (int x = 0; 32 > x; x++)
			{
				for (int y = 0; 32 > y; y++)
				{
					TileIds[x, y] = "default_tile";
				}
			}

			RenderChunkMode();
		}

		void Update()
		{
			if (ActivechunkMode != LastChunkmode)
			{
				LastChunkmode = ActivechunkMode;
				DangerChanged = new bool[32, 32];
				BlockingChanged = new bool[32, 32];
				RenderChunkMode();
			}

			if (ME_Control.activePaintMode == ME_Control.PaintMode.DANGER_LEVEL
				&& (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
			{
				DangerChanged = new bool[32, 32];
			}

			if (ME_Control.activePaintMode == ME_Control.PaintMode.BLOCKING_MASK
				&& (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
			{
				BlockingChanged = new bool[32, 32];
			}

			if (isDirty)
			{
				isDirty = false;
				RenderChunkMode();
			}
		}

		public void WipeChunk()
		{
			TileIds = new string[32, 32];
			for (int x = 0; 32 > x; x++)
			{
				for (int y = 0; 32 > y; y++)
				{
					TileIds[x, y] = "default_tile";
				}
			}

			DangerLevels = new byte[32, 32];
			BlockingMask = new bool[32, 32];
			NullMask = new bool[32, 32];

			DangerChanged = new bool[32, 32];
			BlockingChanged = new bool[32, 32];

			RenderChunkMode();
		}

		public void RenderChunkMode()
		{
			switch (ActivechunkMode)
			{
				case ChunkMode.FLOOR:
					foreach(var edge in tileEdges)
                    {
						Destroy(edge);
                    }
					tileEdges.Clear();

					FlushSubmeshes();
					BuildTileSheetMeshes();
					GenerateFloorUVs();
					break;
				case ChunkMode.DANGERMAP:
					FlushSubmeshes();
					GetComponent<MeshRenderer>().sharedMaterials = new Material[] { HeatmapMat };
					GenerateDangerUVs();
					break;
				case ChunkMode.BLOCKINGMASK:
					FlushSubmeshes();
					GetComponent<MeshRenderer>().sharedMaterials = new Material[] { HeatmapMat };
					GenerateBlockingUVs();
					break;
				default:
					break;
			}
		}

		public void PaintTile(string id, int x, int y)
		{
			TileSheetIds[x, y] = DataBank.Instance.GetData<TileData>(id).textureSheet;
			TileIds[x, y] = id;
			isDirty = true;

			if(y == 0)
            {
				var downChunkHash = ME_Control.GetChunkHash((short)m_ChunkX, (short)(m_ChunkY - 1));
				if (ME_Control.ChunkDict.ContainsKey(downChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[downChunkHash].GetComponent<ME_Chunk>();
					meChunk.SetDirty();
				}
			}
			if (y == 31)
			{
				var upChunkHash = ME_Control.GetChunkHash((short)m_ChunkX, (short)(m_ChunkY + 1));
				if (ME_Control.ChunkDict.ContainsKey(upChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[upChunkHash].GetComponent<ME_Chunk>();
					meChunk.SetDirty();
				}
			}

			if (x == 0)
			{
				var leftChunkHash = ME_Control.GetChunkHash((short)(m_ChunkX - 1), (short)m_ChunkY);
				if (ME_Control.ChunkDict.ContainsKey(leftChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[leftChunkHash].GetComponent<ME_Chunk>();
					meChunk.SetDirty();
				}
			}
			if (x == 31)
			{
				var upChunkHash = ME_Control.GetChunkHash((short)(m_ChunkX + 1), (short)m_ChunkY);
				if (ME_Control.ChunkDict.ContainsKey(upChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[upChunkHash].GetComponent<ME_Chunk>();
					meChunk.SetDirty();
				}
			}
		}

		public void SetDangerLevel(byte danger, int x, int y)
		{
			DangerLevels[x, y] = danger;

			isDirty = true;
		}

		public void SetBlocking(bool blocking, int x, int y)
		{
			BlockingMask[x, y] = blocking;
			GenerateBlockingUVs();

			isDirty = true;
		}

		public void SetNull(bool isNull, int x, int y)
		{
			NullMask[x, y] = isNull;
			GenerateFloorUVs();

			isDirty = true;
		}

		public void AddDangerLevel(byte danger, int x, int y)
		{
			if (!DangerChanged[x, y])
			{
				DangerChanged[x, y] = true;
				DangerLevels[x, y] = danger + DangerLevels[x, y] >= byte.MaxValue ? (byte)byte.MaxValue : (byte)(DangerLevels[x, y] + danger);
			}

			GenerateDangerUVs();

			isDirty = true;
		}

		public void SubtractDangerLevel(byte danger, int x, int y)
		{
			if (!DangerChanged[x, y])
			{
				DangerChanged[x, y] = true;
				DangerLevels[x, y] = danger >= DangerLevels[x, y] ? (byte)0 : (byte)(DangerLevels[x, y] - danger);
			}

			GenerateDangerUVs();

			isDirty = true;
		}

		public void GenerateDangerLevels()
		{
			byte chunkX = (byte)transform.position.x;
			byte chunkY = (byte)transform.position.y;
		}

		public void GenerateFloorUVs()
		{
			Mesh mesh = GetComponent<MeshFilter>().mesh;
			Vector2[] uvs = new Vector2[mesh.vertices.Length];

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					int tileIndex = x + y * 32;
					if (NullMask[x, y])
					{
						CalculateFloorUV(uvs, tileIndex, "default_tile");
					}
					else
					{
						CalculateFloorUV(uvs, tileIndex, TileIds[x, y]);
					}
				}
			}

			mesh.uv = uvs;
		}

		public void GenerateDangerUVs()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			Vector2[] uvs = new Vector2[mesh.vertices.Length];

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					int tileIndex = x + y * 32;
					CalculateDangerUV(uvs, tileIndex, DangerLevels[x, y]);
				}
			}

			mesh.uv = uvs;
		}

		public void GenerateBlockingUVs()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			Vector2[] uvs = new Vector2[mesh.vertices.Length];

			for (int y = 0; y < 32; y++)
			{
				for (int x = 0; x < 32; x++)
				{
					int tileIndex = x + y * 32;
					CalculateBlockingUV(uvs, tileIndex, BlockingMask[x, y]);
				}
			}

			mesh.uv = uvs;
		}

		private void CalculateFloorUV(Vector2[] uvs, int tileIndex, string id)
		{
			var tileId = ClientTileSheetPacker.GetAtlasIndex(id);

			float xLeft = (tileId % TilesPerRow) * Tile_UV_Width;
			float yBottom = ((tileId / TilesPerRow) * Tile_UV_Width);

			float xRight = xLeft + Tile_UV_Width;
			float yTop = yBottom + Tile_UV_Width;

			// lower left
			uvs[tileIndex * 4 + 2] = new Vector2(xLeft, yBottom);
			// lower right
			uvs[tileIndex * 4 + 0] = new Vector2(xRight, yBottom);
			// upper left
			uvs[tileIndex * 4 + 1] = new Vector2(xLeft, yTop);
			// upper right
			uvs[tileIndex * 4 + 3] = new Vector2(xRight, yTop);
		}

		public void FlushSubmeshes()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

			mesh.subMeshCount = 1;
			mesh.SetTriangles(flushedChunkTriangles, 0);

			gameObject.GetComponent<MeshFilter>().mesh = mesh;
		}

		public void BuildTileSheetMeshes()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			var meshVerts = mesh.vertices;

			byte size = 32;

			Dictionary<string, List<int>> submeshData = new Dictionary<string, List<int>>();

			submeshData.Add("null", new List<int>());

			int triCounter = 0;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					TileData td = DataBank.Instance.GetData<TileData>(TileIds[x, y]);
					CalculateTileSubmesh(submeshData, td, triCounter, NullMask[x, y]);

					if (td.isTransparent)
					{
						float offset;

						if (td != null && td.TransparentTileData != null && td.TransparentTileData.tileHeightOffset != 0)
						{
							offset = td.TransparentTileData.tileHeightOffset;
						} else
                        {
							offset = -10.0f;
                        }
						meshVerts[triCounter] = new Vector3(
							meshVerts[triCounter].x,
							meshVerts[triCounter].y,
							offset
						); 
						meshVerts[triCounter + 1] = new Vector3(
							meshVerts[triCounter + 1].x,
							meshVerts[triCounter + 1].y,
							offset
						); 
						meshVerts[triCounter + 2] = new Vector3(
							meshVerts[triCounter + 2].x,
							meshVerts[triCounter + 2].y,
							offset
						); 
						meshVerts[triCounter + 3] = new Vector3(
							meshVerts[triCounter + 3].x,
							meshVerts[triCounter + 3].y,
							offset
						); 

						GenerateTransparentTile(td, x, y);
                    }
                    else
                    {
						meshVerts[triCounter] = new Vector3(
							meshVerts[triCounter].x,
							meshVerts[triCounter].y,
							0
						); 
						meshVerts[triCounter + 1] = new Vector3(
							meshVerts[triCounter + 1].x,
							meshVerts[triCounter + 1].y,
							0
						); 
						meshVerts[triCounter + 2] = new Vector3(
							meshVerts[triCounter + 2].x,
							meshVerts[triCounter + 2].y,
							0
						); 
						meshVerts[triCounter + 3] = new Vector3(
							meshVerts[triCounter + 3].x,
							meshVerts[triCounter + 3].y,
							0
						);
					}

					triCounter += 4;
				}
			}

			short matCount = (short) submeshData.Keys.Count;

			mesh.subMeshCount = matCount;

			Material[] newMats = new Material[matCount];

			int submeshIndex = 0;
			foreach (string submeshTilesheetId in submeshData.Keys)
			{
				newMats[submeshIndex] = ViewUtils.s_ChunkMaterials[submeshTilesheetId];
				mesh.SetTriangles(submeshData[submeshTilesheetId].ToArray(), submeshIndex++);
			}

			gameObject.GetComponent<MeshRenderer>().materials = newMats;

			mesh.vertices = meshVerts;

			mesh.MarkDynamic();

			gameObject.GetComponent<MeshFilter>().mesh = mesh;
		}

		private void CalculateTileSubmesh(Dictionary<string, List<int>> submeshData, TileData tileData, int triCounter, bool isNull)
		{
			/*if (tileData.isTransparent)
            {
				return;
            }*/

			List<int> matchingSubmesh = null;

            if (isNull)
            {
				matchingSubmesh = submeshData["null"];
            }
            else
            {
				if (!submeshData.ContainsKey(tileData.category))
				{
					submeshData.Add(tileData.category, new List<int>());
				}
				matchingSubmesh = submeshData[tileData.category];
			}

			matchingSubmesh.Add(triCounter);
			matchingSubmesh.Add(triCounter + 1);
			matchingSubmesh.Add(triCounter + 2);
			matchingSubmesh.Add(triCounter);
			matchingSubmesh.Add(triCounter + 3);
			matchingSubmesh.Add(triCounter + 1);
		}

        private void GenerateTransparentTile(TileData td, int x, int y)
        {
			var transparentTileData = td.TransparentTileData;

			if (transparentTileData == null || transparentTileData.edgeMeshRef == null)
				return;

			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(transparentTileData.edgeMeshRef);
			var edgePrefab = PrefabBank.GetPrefab("", transparentTileData.edgeMeshRef);

			edgePrefab.SetActive(true);
			edgePrefab.transform.localScale = Vector3.one * viewData.scaleMod;

			var chunkOffsetX = m_ChunkX * 32 + x;
			var chunkOffsetY = m_ChunkY * 32 + y;

			if (x < 31)
            {
				var rightNeighborId = TileIds[x + 1, y];
				if(rightNeighborId != td.Id)
                {
					tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 1, chunkOffsetY + 0.5f, 0), Quaternion.Euler(0, 90, 270)));
                }
            }
			else
			{
				var rightChunkHash = ME_Control.GetChunkHash((short)(m_ChunkX + 1), (short)m_ChunkY);
				if (ME_Control.ChunkDict.ContainsKey(rightChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[rightChunkHash].GetComponent<ME_Chunk>();
					var tileId = meChunk.TileIds[0, y];
					if (tileId != td.Id)
					{
						tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 1, chunkOffsetY + 0.5f, 0), Quaternion.Euler(0, 90, 270)));
					}
				}
			}


			if (x > 0)
			{
				var leftNeighborId = TileIds[x - 1, y];
				if (leftNeighborId != td.Id)
				{
					tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX, chunkOffsetY + 0.5f, 0), Quaternion.Euler(0, 270, 90)));
				}
			}
			else
			{
				var leftChunkHash = ME_Control.GetChunkHash((short)(m_ChunkX - 1), (short)m_ChunkY);
				if (ME_Control.ChunkDict.ContainsKey(leftChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[leftChunkHash].GetComponent<ME_Chunk>();
					var tileId = meChunk.TileIds[31, y];
					if (tileId != td.Id)
					{
						tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX, chunkOffsetY + 0.5f, 0), Quaternion.Euler(0, 270, 90)));
					}
				}
			}

			if (y < 31)
			{
				var upNeighborId = TileIds[x, y + 1];
				if (upNeighborId != td.Id)
				{
					tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 0.5f, chunkOffsetY + 1, 0), Quaternion.Euler(270, 90, 270)));
				}
			}
			else
			{
				var upChunkHash = ME_Control.GetChunkHash((short)m_ChunkX, (short)(m_ChunkY + 1));
				if (ME_Control.ChunkDict.ContainsKey(upChunkHash))
				{
					var meChunk = ME_Control.ChunkDict[upChunkHash].GetComponent<ME_Chunk>();
					var tileId = meChunk.TileIds[x, 0];
					if (tileId != td.Id)
					{
						tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 0.5f, chunkOffsetY + 1, 0), Quaternion.Euler(270, 90, 270)));
					}
				}
			}


			if (y > 0)
			{
				var downNeighborId = TileIds[x, y - 1];
				if (downNeighborId != td.Id)
				{
					tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 0.5f, chunkOffsetY, 0), Quaternion.Euler(90, 270, 90)));
				}
            }
            else
            {
				var downChunkHash = ME_Control.GetChunkHash((short)m_ChunkX, (short)(m_ChunkY - 1));
                if (ME_Control.ChunkDict.ContainsKey(downChunkHash))
                {
					var meChunk = ME_Control.ChunkDict[downChunkHash].GetComponent<ME_Chunk>();
					var tileId = meChunk.TileIds[x, 31];
					if (tileId != td.Id)
                    {
						tileEdges.Add(Instantiate(edgePrefab, new Vector3(chunkOffsetX + 0.5f, chunkOffsetY, 0), Quaternion.Euler(90, 270, 90)));
					}
				}
            }

			edgePrefab.SetActive(false);
		}

		private void CalculateDangerUV(Vector2[] uvs, int tileIndex, byte dangerLevel)
		{
			float step = 1f / byte.MaxValue;

			float xLeft = 0;
			float yTop = dangerLevel * step;

			float xRight = 1;
			float yBottom = (dangerLevel * step) - step;

			// lower left
			uvs[tileIndex * 4 + 2] = new Vector2(xLeft, yBottom);
			// lower right
			uvs[tileIndex * 4 + 0] = new Vector2(xRight, yBottom);
			// upper left
			uvs[tileIndex * 4 + 1] = new Vector2(xLeft, yTop);
			// upper right
			uvs[tileIndex * 4 + 3] = new Vector2(xRight, yTop);
		}

		private void CalculateBlockingUV(Vector2[] uvs, int tileIndex, bool blocking)
		{
			float step = 1f / byte.MaxValue;

			float xLeft = 0;
			float yTop = blocking ? 255 * step : step;

			float xRight = 1;
			float yBottom = blocking ? (255 * step) - step : 0;

			// lower left
			uvs[tileIndex * 4 + 2] = new Vector2(xLeft, yBottom);
			// lower right
			uvs[tileIndex * 4 + 0] = new Vector2(xRight, yBottom);
			// upper left
			uvs[tileIndex * 4 + 1] = new Vector2(xLeft, yTop);
			// upper right
			uvs[tileIndex * 4 + 3] = new Vector2(xRight, yTop);
		}

		public void SetDirty()
        {
			isDirty = true;
        }
	}
}