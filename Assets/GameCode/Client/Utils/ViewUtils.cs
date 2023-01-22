using FNZ.Client.Model.World;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.World.Tile;
using System;
using System.Collections.Generic;
using FNZ.Client.Systems;
using FNZ.Client.View.Manager;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using FNZ.Shared.Utils;

namespace FNZ.Client.Utils
{
	public static class ViewUtils
	{
		public static Dictionary<string, Material> s_ChunkMaterials;
		public static Material WaterMat;
		
		private static readonly float OVERLAP_PADDING_PERCENT = 0.25f;
		// one fourth of the padding percent
		private static readonly float OVERLAP_PADDING_UV = 0.125f * OVERLAP_PADDING_PERCENT;

		static ViewUtils()
		{
			Texture2D noiseMap = Resources.Load<Texture2D>("Material/Debug/noise");

			s_ChunkMaterials = new Dictionary<string, Material>();

			var nullMat = Resources.Load<Material>("Material/LevelEditor/NullTile");
			s_ChunkMaterials.Add("null", nullMat);

			foreach (var categoryString in ClientTileSheetPacker.GetCategoryKeys())
			{
                Material mat = MaterialUtils.CreateTileSheetMaterial
                (
                    $"{categoryString}albedo",
                    $"{categoryString}normal",
                    $"{categoryString}mask",
                    $"{categoryString}emissive",
                    false
                );

				s_ChunkMaterials.Add(categoryString, mat);
			}

			WaterMat = Resources.Load<Material>("Material/Water/Water");
		}

		public static Tuple<Mesh, Material[]> CreateChunkOverlapMesh(
			ClientWorldChunk chunkModel,
			byte chunkX,
			byte chunkY
		)
		{
			Mesh overlapMesh = new Mesh();

			List<Vector3> overlapVerts = new List<Vector3>();
			List<int> overlapTris = new List<int>();
			List<Vector2> overlapUVs = new List<Vector2>();

			int overlapCount = 0;

			byte size = 32;

			Dictionary<string, List<int>> submeshData = new Dictionary<string, List<int>>();

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					int tileX = chunkX * 32 + x;
					int tileY = chunkY * 32 + y;

					int tileIndex = tileX + tileY * 512;
					var id = IdTranslator.Instance.GetId<TileData>(chunkModel.TileIdCodes[tileIndex]);
					TileData td = DataBank.Instance.GetData<TileData>(id);

					if (td.hardEdges)
						continue;

					if (!submeshData.ContainsKey(td.category))
					{
						submeshData.Add(td.category, new List<int>());
					}

					List<int> matchingSubmesh = submeshData[td.category];

					BuildOverlapsForTile(
						x,
						y,
						tileIndex,
						td,
						chunkModel,
						overlapVerts,
						matchingSubmesh,
						overlapUVs,
						ref overlapCount
					);
				}
			}

			short matCount = (short)submeshData.Keys.Count;
			overlapMesh.subMeshCount = matCount;

			Material[] newMats = new Material[matCount];

			overlapMesh.vertices = overlapVerts.ToArray();

			Vector3[] normals = new Vector3[overlapVerts.Count];
			for (int i = 0; i < normals.Length; i++)
				normals[i] = -Vector3.forward;

			int submeshIndex = 0;
			foreach (string submeshTilesheetId in submeshData.Keys)
			{
				newMats[submeshIndex] = s_ChunkMaterials[submeshTilesheetId];
				overlapMesh.SetTriangles(submeshData[submeshTilesheetId].ToArray(), submeshIndex++);
			}

			overlapMesh.normals = normals;
			overlapMesh.uv = overlapUVs.ToArray();
			overlapMesh.RecalculateBounds();

			return new Tuple<Mesh, Material[]>(overlapMesh, newMats);
		}

		private static void BuildOverlapsForTile(
			int x,
			int y,
			int tileIndex,
			TileData tileData,
			ClientWorldChunk chunkModel,
			List<Vector3> overlapVerts,
			List<int> overlapTris,
			List<Vector2> overlapUVs,
			ref int overlapCount
		)
		{
			byte size = 32;

			#region RIGHT NEIGHBOR

			TileData neighborData = null;

			string neighborId = null;
			int neighborIndex = -1;
			if (tileIndex % 512 < 511)
			{
				try
				{
					neighborIndex = tileIndex + 1;
					neighborId = IdTranslator.Instance.GetId<TileData>(chunkModel.TileIdCodes[neighborIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return;
				}
			}

			if (neighborId != null)
				neighborData = DataBank.Instance.GetData<TileData>(neighborId);
			
			int tx = x + 1;
			int ty = y;
			if (neighborData != null && neighborId != tileData.Id && tileData.overlapPriority > neighborData.overlapPriority)
			{
				// VERTICES!
				overlapVerts.Add(new Vector3(tx, y, 0));
				overlapVerts.Add(new Vector3(tx, y + 0.5f, 0));
				overlapVerts.Add(new Vector3(tx, y + 1, 0));
				overlapVerts.Add(new Vector3(tx + OVERLAP_PADDING_PERCENT, y + (1 - OVERLAP_PADDING_PERCENT), 0));
				overlapVerts.Add(new Vector3(tx + OVERLAP_PADDING_PERCENT, y + OVERLAP_PADDING_PERCENT, 0));

				//TRIANGLES!
				// Index of first vertex of overlap section
				int FIRST = overlapCount * 5;

				// Lower left triangle
				overlapTris.Add(FIRST);
				overlapTris.Add(FIRST + 1);
				overlapTris.Add(FIRST + 4);

				// upper left triangle
				overlapTris.Add(FIRST + 1);
				overlapTris.Add(FIRST + 2);
				overlapTris.Add(FIRST + 3);

				// middle triangle
				overlapTris.Add(FIRST + 1);
				overlapTris.Add(FIRST + 3);
				overlapTris.Add(FIRST + 4);

				// UVs
				CalculateOverlapUVRight(ClientTileSheetPacker.GetAtlasIndex(tileData.Id), overlapUVs);

				overlapCount++;
			}

            #endregion

            #region DOWN NEIGHBOR

            neighborId = null;
            neighborIndex = -1;
			if (tileIndex / 512 > 0)
			{
				try
				{
					neighborIndex = tileIndex - 512;
					neighborId = IdTranslator.Instance.GetId<TileData>(chunkModel.TileIdCodes[neighborIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return;
				}
			}

			if(neighborId != null)
				neighborData = DataBank.Instance.GetData<TileData>(neighborId);
			
            tx = x;
            ty = y - 1;
            if (neighborData != null && neighborId != tileData.Id && tileData.overlapPriority > neighborData.overlapPriority)
            {
                // VERTICES!
                overlapVerts.Add(new Vector3(tx, ty + 1, 0));
                overlapVerts.Add(new Vector3(tx + 0.5f, ty + 1, 0));
                overlapVerts.Add(new Vector3(tx + 1, ty + 1, 0));
                overlapVerts.Add(new Vector3(tx + (1 - OVERLAP_PADDING_PERCENT), ty + (1 - OVERLAP_PADDING_PERCENT), 0));
                overlapVerts.Add(new Vector3(tx + OVERLAP_PADDING_PERCENT, ty + (1 - OVERLAP_PADDING_PERCENT), 0));

                //TRIANGLES!
                // Index of first vertex of overlap section
                int FIRST = overlapCount * 5;

                // Lower left triangle
                overlapTris.Add(FIRST);
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 4);

                // upper left triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 2);
                overlapTris.Add(FIRST + 3);

                // middle triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 3);
                overlapTris.Add(FIRST + 4);

                // UVs
                CalculateOverlapUVDown(ClientTileSheetPacker.GetAtlasIndex(tileData.Id), overlapUVs);

                overlapCount++;
            }

            #endregion

            #region LEFT NEIGHBOR

            neighborId = null;
            neighborIndex = -1;
			if (tileIndex % 512 > 0)
			{
				try
				{
					neighborIndex = tileIndex - 1;
					neighborId = IdTranslator.Instance.GetId<TileData>(chunkModel.TileIdCodes[neighborIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return;
				}
			}

			if (neighborId != null)
				neighborData = DataBank.Instance.GetData<TileData>(neighborId);

			tx = x - 1;
            ty = y;
            if (neighborData != null && neighborId != tileData.Id && tileData.overlapPriority > neighborData.overlapPriority)
            {
                // VERTICES!
                overlapVerts.Add(new Vector3(tx + 1, ty + 1, 0));
                overlapVerts.Add(new Vector3(tx + 1, ty + 0.5f, 0));
                overlapVerts.Add(new Vector3(tx + 1, ty, 0));
                overlapVerts.Add(new Vector3(tx + (1 - OVERLAP_PADDING_PERCENT), ty + OVERLAP_PADDING_PERCENT, 0));
                overlapVerts.Add(new Vector3(tx + (1 - OVERLAP_PADDING_PERCENT), ty + (1 - OVERLAP_PADDING_PERCENT), 0));

                //TRIANGLES!
                // Index of first vertex of overlap section
                int FIRST = overlapCount * 5;

                // Lower left triangle
                overlapTris.Add(FIRST);
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 4);

                // upper left triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 2);
                overlapTris.Add(FIRST + 3);

                // middle triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 3);
                overlapTris.Add(FIRST + 4);

                // UVs
                CalculateOverlapUVLeft(ClientTileSheetPacker.GetAtlasIndex(tileData.Id),
                    overlapUVs);

                overlapCount++;
            }

            #endregion

            #region UP NEIGHBOR

            neighborId = null;
            neighborIndex = -1;
			if (tileIndex / 512 < 511)
			{
				try
				{
					neighborIndex = tileIndex + 512;
					neighborId = IdTranslator.Instance.GetId<TileData>(chunkModel.TileIdCodes[neighborIndex]);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					return;
				}
			}

			if (neighborId != null)
				neighborData = DataBank.Instance.GetData<TileData>(neighborId);

			tx = x;
            ty = y + 1;
            if (neighborData != null && neighborId != tileData.Id && tileData.overlapPriority > neighborData.overlapPriority)
            {
                // VERTICES!
                overlapVerts.Add(new Vector3(tx + 1, ty, 0));
                overlapVerts.Add(new Vector3(tx + 0.5f, ty, 0));
                overlapVerts.Add(new Vector3(tx, ty, 0));
                overlapVerts.Add(new Vector3(tx + OVERLAP_PADDING_PERCENT, ty + OVERLAP_PADDING_PERCENT, 0));
                overlapVerts.Add(new Vector3(tx + (1 - OVERLAP_PADDING_PERCENT), ty + OVERLAP_PADDING_PERCENT, 0));

                //TRIANGLES!
                // Index of first vertex of overlap section
                int FIRST = overlapCount * 5;

                // Lower left triangle
                overlapTris.Add(FIRST);
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 4);

                // upper left triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 2);
                overlapTris.Add(FIRST + 3);

                // middle triangle
                overlapTris.Add(FIRST + 1);
                overlapTris.Add(FIRST + 3);
                overlapTris.Add(FIRST + 4);

                // UVs
                CalculateOverlapUVUp(ClientTileSheetPacker.GetAtlasIndex(tileData.Id), overlapUVs);

                overlapCount++;
            }

            #endregion
        }

        private static void CalculateOverlapUVRight(int atlasIndex, List<Vector2> overlapUVs)
		{
			var tpr = ClientTileSheetPacker.TILES_PER_ROW;
			var UVOffset = 1f / tpr;
			
			float xLeft = (atlasIndex % tpr) * UVOffset;
			float yBottom = ((atlasIndex / tpr) * UVOffset);


			overlapUVs.Add(new Vector2(xLeft, yBottom + UVOffset));
			overlapUVs.Add(new Vector2(xLeft, yBottom + UVOffset / 2f));
			overlapUVs.Add(new Vector2(xLeft, yBottom));
			overlapUVs.Add(new Vector2(xLeft + OVERLAP_PADDING_UV, yBottom + OVERLAP_PADDING_UV));
			overlapUVs.Add(new Vector2(xLeft + OVERLAP_PADDING_UV, yBottom + (UVOffset - OVERLAP_PADDING_UV)));
        }

        private static void CalculateOverlapUVDown(int atlasIndex, List<Vector2> overlapUVs)
		{
			var tpr = ClientTileSheetPacker.TILES_PER_ROW;
			var UVOffset = 1f / tpr;
			
			float xLeft = (atlasIndex % tpr) * UVOffset;
			float yBottom = ((atlasIndex / tpr) * UVOffset);

			overlapUVs.Add(new Vector2(xLeft + UVOffset, yBottom));
			overlapUVs.Add(new Vector2(xLeft + UVOffset / 2f, yBottom));
			overlapUVs.Add(new Vector2(xLeft, yBottom));
			overlapUVs.Add(new Vector2(xLeft + OVERLAP_PADDING_UV, yBottom  - OVERLAP_PADDING_UV));
			overlapUVs.Add(new Vector2(xLeft + (UVOffset - OVERLAP_PADDING_UV), yBottom  - OVERLAP_PADDING_UV));
			
		}

		private static void CalculateOverlapUVLeft(int atlasIndex, List<Vector2> overlapUVs)
		{
			var tpr = ClientTileSheetPacker.TILES_PER_ROW;
			var UVOffset = 1f / tpr;
			
			float xLeft = (atlasIndex % tpr) * UVOffset;
			float yTop = ((atlasIndex / tpr) * UVOffset);

			//float xRight = xLeft + UVOffset;
			float yBottom = yTop - UVOffset;

			overlapUVs.Add(new Vector2(xLeft + UVOffset, yBottom + UVOffset));
			overlapUVs.Add(new Vector2(xLeft + UVOffset, yBottom + UVOffset / 2f));
			overlapUVs.Add(new Vector2(xLeft + UVOffset, yBottom));
			overlapUVs.Add(new Vector2(xLeft + (UVOffset - OVERLAP_PADDING_UV), yBottom + OVERLAP_PADDING_UV));
			overlapUVs.Add(new Vector2(xLeft + (UVOffset - OVERLAP_PADDING_UV), yBottom + (UVOffset - OVERLAP_PADDING_UV)));
		}

		private static void CalculateOverlapUVUp(int atlasIndex, List<Vector2> overlapUVs)
		{
			var tpr = ClientTileSheetPacker.TILES_PER_ROW;
			var UVOffset = 1f / tpr;
			
			float xLeft = (atlasIndex % tpr) * UVOffset;
			float yBottom = ((atlasIndex / tpr));

			overlapUVs.Add(new Vector2(xLeft + UVOffset, yBottom + UVOffset));
			overlapUVs.Add(new Vector2(xLeft + UVOffset / 2f, yBottom + UVOffset));
			overlapUVs.Add(new Vector2(xLeft, yBottom + UVOffset));
			overlapUVs.Add(new Vector2(xLeft + OVERLAP_PADDING_UV, yBottom + UVOffset - OVERLAP_PADDING_UV));
			overlapUVs.Add(new Vector2(xLeft + UVOffset - OVERLAP_PADDING_UV, yBottom + UVOffset - OVERLAP_PADDING_UV));
		}

		public static void GenerateTileEdgeMeshes(
			ClientWorldChunk chunkModel,
			byte chunkX,
			byte chunkY
		)
		{
			if (chunkModel == null) return;
			var size = 32;
			var worldSize = 512;

			var tileIdCodes = chunkModel.TileIdCodes;

			var renderMeshSpawnerSystem = GameClient.ECS_ClientWorld.GetExistingSystem<RenderMeshSpawnerSystem>();

			var initX = chunkX * 32;
			var initY = chunkY * 32;

			for (int y = initY; y < initY + size; y++)
			{
				for (int x = initX; x < initX + size; x++)
				{
					int tileIndex = x + y * 512;
					var id = IdTranslator.Instance.GetId<TileData>(tileIdCodes[tileIndex]);
					TileData td = DataBank.Instance.GetData<TileData>(id);

					if (!td.isTransparent && !td.isWater)
						continue;
					if (td.isTransparent && (td.TransparentTileData == null || td.TransparentTileData.edgeMeshRef == null))
						continue;
					if (td.isWater && (td.WaterTileData == null || td.WaterTileData.edgeMeshRefs == null))
						continue;

					FNEEntityViewData viewData = null;
					if(td.isTransparent)
						viewData = DataBank.Instance.GetData<FNEEntityViewData>(td.TransparentTileData.edgeMeshRef);
					
					else if (td.isWater)
                    {
						var randomVariant = FNERandom.GetRandomIntInRange(0, td.WaterTileData.edgeMeshRefs.Count);
						viewData = DataBank.Instance.GetData<FNEEntityViewData>(td.WaterTileData.edgeMeshRefs[randomVariant]);
					}
						

					var edgePrefab = PrefabBank.GetPrefab("", viewData.Id);

					edgePrefab.SetActive(true);
					edgePrefab.transform.localScale = Vector3.one * viewData.scaleMod;

					var chunkOffsetX = x;
					var chunkOffsetY = y;

					if (x < 512 - 1)
					{
						var rightNeighborId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[tileIndex + 1]);;
						if (rightNeighborId != td.Id)
						{
							var renderMeshEntitySpawnData = GameClient.ViewAPI.ConvertGameObjectToEntitySpawnData(edgePrefab,
								new Vector3(chunkOffsetX + 1, 0, chunkOffsetY + 0.5f),
								Quaternion.Euler(0, 90, 0),
								edgePrefab.transform.localScale);
							
							renderMeshEntitySpawnData.IsSubView = false;
							renderMeshSpawnerSystem.AddEntitySpawnData(ref renderMeshEntitySpawnData);
						}
					}


					if (x > 0)
					{
						var leftNeighborId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[tileIndex - 1]);
						if (leftNeighborId != td.Id)
						{
							var renderMeshEntitySpawnData = GameClient.ViewAPI.ConvertGameObjectToEntitySpawnData(edgePrefab,
								new Vector3(chunkOffsetX, 0, chunkOffsetY + 0.5f),
								Quaternion.Euler(0, 270, 0),
								edgePrefab.transform.localScale);
							
							renderMeshEntitySpawnData.IsSubView = false;
							renderMeshSpawnerSystem.AddEntitySpawnData(ref renderMeshEntitySpawnData);
						}
					}

					if (y < 512 - 1)
					{
						var upNeighborId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[tileIndex + worldSize]); ;
						if (upNeighborId != td.Id)
						{
							var renderMeshEntitySpawnData = GameClient.ViewAPI.ConvertGameObjectToEntitySpawnData(edgePrefab,
								new Vector3(chunkOffsetX + 0.5f, 0, chunkOffsetY + 1.0f),
								Quaternion.Euler(0, 0, 0),
								edgePrefab.transform.localScale);
							
							renderMeshEntitySpawnData.IsSubView = false;
							renderMeshSpawnerSystem.AddEntitySpawnData(ref renderMeshEntitySpawnData);
						}
					}

					if (y > 0)
					{
						var downNeighborId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[x + ((y - 1) * worldSize)]);
						if (downNeighborId != td.Id)
						{
							var renderMeshEntitySpawnData = GameClient.ViewAPI.ConvertGameObjectToEntitySpawnData(edgePrefab,
								new Vector3(chunkOffsetX + 0.5f, 0, chunkOffsetY),
								Quaternion.Euler(0, 180, 0),
								edgePrefab.transform.localScale);
							
							renderMeshEntitySpawnData.IsSubView = false;
							renderMeshSpawnerSystem.AddEntitySpawnData(ref renderMeshEntitySpawnData);
						}
					}

					edgePrefab.SetActive(false);
				}
			}
		}

		public static Mesh GetFramedPlane(int width, int height)
		{
			var halfThickness = 0.01f;
			
			Mesh frameMesh = new Mesh();
			
			List<Vector3> frameVerts = new List<Vector3>();
			List<int> frameTris = new List<int>();
			
			// South
			frameVerts.Add(new Vector3(0, 0, -halfThickness));
			frameVerts.Add(new Vector3(0, 0, halfThickness));
			frameVerts.Add(new Vector3(width, 0, halfThickness));
			frameVerts.Add(new Vector3(width, 0, -halfThickness));
			
			frameTris.Add(0);
			frameTris.Add(1);
			frameTris.Add(2);
			
			frameTris.Add(2);
			frameTris.Add(3);
			frameTris.Add(0);
			
			// North
			frameVerts.Add(new Vector3(0, 0, height - halfThickness));
			frameVerts.Add(new Vector3(0, 0, height + halfThickness));
			frameVerts.Add(new Vector3(width, 0, height + halfThickness));
			frameVerts.Add(new Vector3(width, 0, height -halfThickness));
			
			frameTris.Add(4);
			frameTris.Add(5);
			frameTris.Add(6);
			
			frameTris.Add(6);
			frameTris.Add(7);
			frameTris.Add(4);
			
			// West
			frameVerts.Add(new Vector3(0, 0, halfThickness));
			frameVerts.Add(new Vector3(halfThickness * 2, 0, halfThickness));
			frameVerts.Add(new Vector3(halfThickness * 2, 0, height - halfThickness));
			frameVerts.Add(new Vector3(0, 0, height - halfThickness));
			
			frameTris.Add(10);
			frameTris.Add(9);
			frameTris.Add(8);
			
			frameTris.Add(8);
			frameTris.Add(11);
			frameTris.Add(10);
			
			// East
			frameVerts.Add(new Vector3(width - halfThickness * 2, 0, halfThickness));
			frameVerts.Add(new Vector3(width, 0, halfThickness));
			frameVerts.Add(new Vector3(width, 0, height - halfThickness));
			frameVerts.Add(new Vector3(width - halfThickness * 2, 0, height - halfThickness));
			
			frameTris.Add(14);
			frameTris.Add(13);
			frameTris.Add(12);
			
			frameTris.Add(12);
			frameTris.Add(15);
			frameTris.Add(14);
			
			// Build mesh
			frameMesh.vertices = frameVerts.ToArray();

			Vector3[] normals = new Vector3[frameVerts.Count];
			for (int i = 0; i < normals.Length; i++)
				normals[i] = Vector3.up;

			frameMesh.SetTriangles(frameTris.ToArray(), 0);

			frameMesh.normals = normals;
			frameMesh.RecalculateBounds();

			return frameMesh;
		}

		public static string ScreenShotName(int width, int height) {
			return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png", 
				Application.dataPath, 
				width, height, 
				System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
		}
		
	}
}