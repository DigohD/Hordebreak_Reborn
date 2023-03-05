using FNZ.Client.Model.World;
using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.View.World
{
	public class ClientWorldChunkView : MonoBehaviour
	{
		public byte ChunkX, ChunkY;

		private ClientWorldChunk m_ChunkModel;

		private Mesh m_OverlapMesh;
		private MeshRenderer m_OverlapMeshRenderer;
		private MeshFilter m_OverlapMeshFilter;

		public void Init(ClientWorldChunk chunkModel, byte chunkX, byte chunkY)
		{
			this.ChunkX = chunkX;
			this.ChunkY = chunkY;

			m_ChunkModel = chunkModel;

			m_ChunkModel.d_OnGenerateUVsEvent += (int tileX, int tileY) =>
			{
				if(IsTilePartOfChunkView(tileX, tileY))
                {
					Debug.LogWarning("RENDER CHUNK " + chunkX + " : " + chunkY);
					BuildTileSheetMeshes();
					GenerateUVs();
					BuildOverlapMesh();
					ViewUtils.GenerateTileEdgeMeshes(m_ChunkModel, ChunkX, ChunkY);
				}
			};

			m_OverlapMesh = new Mesh();

			m_OverlapMeshRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();
			m_OverlapMeshFilter = transform.GetChild(0).GetComponent<MeshFilter>();

			Profiler.BeginSample("BuildTileSheetMeshes");
			BuildTileSheetMeshes();
			Profiler.EndSample();

			Profiler.BeginSample("GenerateUVs");
			GenerateUVs();
			Profiler.EndSample();

			Profiler.BeginSample("BuildOverlapMesh");
			BuildOverlapMesh();
			Profiler.EndSample();
			
			Profiler.BeginSample("GenerateTileEdgeMeshes");
			ViewUtils.GenerateTileEdgeMeshes(m_ChunkModel, chunkX, chunkY);
			Profiler.EndSample();
		}

		public bool IsTilePartOfChunkView(int tileX, int tileY)
        {
			return tileX > ChunkX * 32 && tileX < ChunkX * 32 + 32 && tileY > ChunkY * 32 && tileY < ChunkY * 32 + 32;
		}

		public void GenerateUVs()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
			Vector2[] uvs = new Vector2[mesh.vertices.Length];
			byte size = 32;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					int tileX = ChunkX * 32 + x;
					int tileY = ChunkY * 32 + y;

					int tileIndex = tileX + tileY * m_ChunkModel.SideSize;
					var id = IdTranslator.Instance.GetId<TileData>(m_ChunkModel.TileIdCodes[tileIndex]);
					var tileData = DataBank.Instance.GetData<TileData>(id);

					CalculateUV(uvs, x + y * size, ClientTileSheetPacker.GetAtlasIndex(tileData.Id));
				}
			}

			mesh.uv = uvs;
			enabled = false;
		}

		private void CalculateUV(Vector2[] uvs, int tileIndex, int tileID)
		{
			var tpr = ClientTileSheetPacker.TILES_PER_ROW;
			var UVOffset = 1f / tpr;
			
			float xLeft = (tileID % tpr) * UVOffset;
			float yBottom = ((tileID / tpr) * UVOffset);

			float xRight = xLeft + UVOffset;
			float yTop = yBottom + UVOffset;

			// upper left
			uvs[tileIndex * 4 + 0] = new Vector2(xLeft, yTop);
			// upper right
			uvs[tileIndex * 4 + 2] = new Vector2(xRight, yTop);
			// lower left
			uvs[tileIndex * 4 + 3] = new Vector2(xLeft, yBottom);
			// lower right
			uvs[tileIndex * 4 + 1] = new Vector2(xRight, yBottom);
		}

		public void BuildTileSheetMeshes()
		{
			Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

			byte size = 32;

			Dictionary<string, List<int>> submeshData = new Dictionary<string, List<int>>();
			List<int> waterSubmesh = null;
			
			var meshVerts = mesh.vertices;

			int triCounter = 0;
			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					int tileX = ChunkX * 32 + x;
					int tileY = ChunkY * 32 + y;

					int tileIndex = tileX + tileY * m_ChunkModel.SideSize;
					var id = IdTranslator.Instance.GetId<TileData>(m_ChunkModel.TileIdCodes[tileIndex]);
					TileData td = DataBank.Instance.GetData<TileData>(id);
					if (td.isTransparent)
					{
						CalculateTileSubmesh(submeshData, td, triCounter);
						
						float offset;

						if (td != null && td.TransparentTileData != null && td.TransparentTileData.tileHeightOffset != 0)
						{
							offset = td.TransparentTileData.tileHeightOffset;
						}
						else
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
					}else if (td.isWater)
					{
						if (waterSubmesh == null)
							waterSubmesh = new List<int>();
						
						waterSubmesh.Add(triCounter);
						waterSubmesh.Add(triCounter + 1);
						waterSubmesh.Add(triCounter + 2);
						waterSubmesh.Add(triCounter);
						waterSubmesh.Add(triCounter + 3);
						waterSubmesh.Add(triCounter + 1);
						
						float offset;

						if (td != null && td.WaterTileData != null && td.WaterTileData.tileHeightOffset != 0)
						{
							offset = td.WaterTileData.tileHeightOffset;
						}
						else
						{
							offset = -0.2f;
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
					}
					else
					{
						CalculateTileSubmesh(submeshData, td, triCounter);
						
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

			short matCount = (short)submeshData.Keys.Count;
			if (waterSubmesh != null)
				matCount++;

			mesh.subMeshCount = matCount;

			Material[] newMats = new Material[matCount];

			int submeshIndex = 0;
			foreach (string submeshTilesheetCategory in submeshData.Keys)
			{
				newMats[submeshIndex] = ViewUtils.s_ChunkMaterials[submeshTilesheetCategory];
				mesh.SetTriangles(submeshData[submeshTilesheetCategory].ToArray(), submeshIndex++);
			}

			if (waterSubmesh != null)
			{
				newMats[submeshIndex] = ViewUtils.WaterMat;
				mesh.SetTriangles(waterSubmesh.ToArray(), submeshIndex++);
			}
			
			
			gameObject.GetComponent<MeshRenderer>().sharedMaterials = newMats;

			mesh.vertices = meshVerts;

			mesh.MarkDynamic();

			gameObject.GetComponent<MeshFilter>().mesh = mesh;

		}

		public void BuildOverlapMesh()
		{
			Tuple<Mesh, Material[]> overlapMeshAndMats = ViewUtils.CreateChunkOverlapMesh(
				m_ChunkModel,
				ChunkX,
				ChunkY
			);
			m_OverlapMeshFilter.mesh = overlapMeshAndMats.Item1;
			m_OverlapMeshRenderer.sharedMaterials = overlapMeshAndMats.Item2;
		}

		private void CalculateTileSubmesh(Dictionary<string, List<int>> submeshData, TileData tileData, int triCounter)
		{
            /*if (tileData.isTransparent)
            {
				return;
            }*/

			if (!submeshData.ContainsKey(tileData.category))
			{
				submeshData.Add(tileData.category, new List<int>());
			}

			List<int> matchingSubmesh = submeshData[tileData.category];

			matchingSubmesh.Add(triCounter);
			matchingSubmesh.Add(triCounter + 1);
			matchingSubmesh.Add(triCounter + 2);
			matchingSubmesh.Add(triCounter);
			matchingSubmesh.Add(triCounter + 3);
			matchingSubmesh.Add(triCounter + 1);
		}
	}
}
