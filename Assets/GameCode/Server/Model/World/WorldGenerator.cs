using System;
using System.Collections.Generic;
using FNZ.Server.Controller;
using FNZ.Server.Model.Entity.Components;
using FNZ.Server.Model.Entity.Components.EdgeObject;
using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System.IO;
using FNZ.Server.Services;
using FNZ.Shared.Net.Dto.Hordes;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Server.Model.World
{
	public class WorldGenerator
	{
		private readonly WorldGenData m_WorldGenData;

		private bool[] m_TileObjectGenerationMap;

		public WorldGenerator(WorldGenData data)
		{
			m_WorldGenData = data;

			m_TileObjectGenerationMap = new bool[32*32];

			for (int i = 0; i < 32*32; i++)
            {
				m_TileObjectGenerationMap[i] = false;
			}
		}

		public ServerWorld GenerateWorld(int seedX, int seedY)
		{
			IdTranslator.Instance.GenerateMissingIds();

			var world = new ServerWorld(256 * m_WorldGenData.chunkSize, 256 * m_WorldGenData.chunkSize, m_WorldGenData.chunkSize)
			{
				SeedX = seedX,
				SeedY = seedY
			};
			
			return world;
		}

		public void GenerateChunk(ServerWorldChunk chunk)
		{
			for (int i = 0; i < 32 * 32; i++)
			{
				m_TileObjectGenerationMap[i] = false;
			}

			Profiler.BeginSample("GenerateTileMap");
			GenerateTileMap(chunk);
			Profiler.EndSample();
			
			Profiler.BeginSample("GenerateSites");
			GenerateSitesV2(chunk);
			Profiler.EndSample();
			
			Profiler.BeginSample("GenerateEnvironmentObjects");
			GenerateEnvironmentObjects(chunk);
			Profiler.EndSample();
			
			Profiler.BeginSample("GenerateEnemies");
			//GenerateEnemies(chunk);
			Profiler.EndSample();

			var nb = new NetBuffer();
			nb.EnsureBufferSize(chunk.TotalBitsFileBuffer());
			chunk.FileSerialize(nb);
			
			FNEService.File.WriteFile(
				GameServer.FilePaths.GetOrCreateChunkFilePath(chunk), 
				nb.Data
			);
		}

		private void GenerateTileMap(ServerWorldChunk chunk)
		{
			byte chunkX = chunk.ChunkX;
			byte chunkY = chunk.ChunkY;
			byte chunkSize = chunk.Size;

			float[] heightMap = GenerateHeightMap(chunkX, chunkY, chunkSize, GameServer.World.SeedX, GameServer.World.SeedY);

			for (int y = 0; y < chunkSize; y++)
			{
				for (int x = 0; x < chunkSize; x++)
				{
					float height = Mathf.Clamp(heightMap[x + y * chunkSize], -1.0f, 1.0f);

					foreach (var biomeTileData in m_WorldGenData.tilesInBiome)
					{
						TileData tileData = DataBank.Instance.GetData<TileData>(biomeTileData.tileRef);
						byte tileId = tileData.textureSheetIndex;

						if (biomeTileData.height >= height)
						{
							int index = x + y * chunkSize;

							chunk.TileIdCodes[index] = IdTranslator.Instance.GetIdCode<TileData>(tileData.Id);

							chunk.BlockingTiles[index] = tileData.isBlocking;

							chunk.TilePositionsX[index] = x + chunkX * chunkSize;
							chunk.TilePositionsY[index] = y + chunkY * chunkSize;

							if (FNERandom.GetRandomIntInRange(0, 30) == 0)
								chunk.TileDangerLevels[index] = (byte)FNERandom.GetRandomIntInRange(0, 2);
							
							break;
						}
					}
				}
			}
		}

		private void GenerateEnvironmentObjects(ServerWorldChunk chunk)
		{
			byte chunkSize = chunk.Size;

			var serverWorld = GameServer.World;

			// Spawn drop pod near player on initial spawn chunk
			if (chunk.ChunkX == serverWorld.WIDTH_IN_CHUNKS / 2 && chunk.ChunkY == serverWorld.HEIGHT_IN_CHUNKS / 2)
			{
				var pos = new float2(
					chunk.TilePositionsX[1 + 1 * chunkSize], 
					chunk.TilePositionsY[1 + 1 * chunkSize]
				);

				GenerateTileObject(
					pos,
					0,
					chunk,
					"to_player_drop_pod"
				);
			}
			
			for (int y = 0; y < chunkSize; y++)
			{
				for (int x = 0; x < chunkSize; x++)
				{
					int index = x + y * chunkSize;
					int totalWeight = 0;
					ushort tileIdCode = chunk.TileIdCodes[index];
					string tileId = IdTranslator.Instance.GetId<TileData>(tileIdCode);
					var tileData = DataBank.Instance.GetData<TileData>(tileId);

					if (HasTileObject(chunk.TilePositionsX[index], chunk.TilePositionsY[index]))
						continue;

					int rnd = FNERandom.GetRandomIntInRange(0, 101);
					if (rnd <= tileData.chanceToSpawnTileObject)
					{
						var tileObjData = tileData.tileObjectGenDataList;
						// calc total weight
						foreach (var entry in tileObjData)
						{
							totalWeight += entry.weight;
						}

						foreach (var entry in tileObjData)
						{
							var tileObjId = entry.objectRef;

							rnd = FNERandom.GetRandomIntInRange(0, totalWeight + 1);
							if (rnd <= entry.weight)
							{
								var pos = new float2(chunk.TilePositionsX[index], chunk.TilePositionsY[index]);

								if (HasTileObject((int)pos.x, (int)pos.y))
									continue;

								GenerateTileObject(
									pos,
									index,
									chunk,
									tileObjId
								);

								var cluster = entry.clusterData;
								if (cluster == null) continue;

								GenerateClusterObjects(pos, cluster.radius, cluster.density, tileObjId, tileId);
							}
							else
							{
								rnd -= entry.weight;
							}
						}
					}
				}
			}
		}

		private void GenerateClusterObjects(float2 basePos, float radius, float density, string tileObjectId, string originalTileId)
		{
			var originalChunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(new int2((int)basePos.x, (int)basePos.y));
			foreach (var tile in GameServer.World.GetSurroundingTilesInRadius(new int2((int)basePos.x, (int)basePos.y), (int)radius))
			{
				var chunk = GameServer.World.GetWorldChunk<ServerWorldChunk>(tile);
				if (originalChunk != chunk)
					continue;

				var chunkTileIndices = GameServer.World.GetChunkTileIndices<ServerWorldChunk>(chunk, tile);
				int index = chunkTileIndices.x + chunkTileIndices.y * chunk.Size;
				ushort tileIdCode = chunk.TileIdCodes[index];
				string tileId = IdTranslator.Instance.GetId<TileData>(tileIdCode);

				if (tileId != originalTileId) continue;
				
				var rndClusterDensity = FNERandom.GetRandomIntInRange(0, 101);
				if (rndClusterDensity <= density)
				{
					if (HasTileObject(tile.x, tile.y))
						continue;

					GenerateTileObject(tile, index, chunk, tileObjectId);
				}
			}
		}

		private void GenerateSites(ServerWorldChunk chunk)
        {
			int totalWeight = 0;

			var sites = DataBank.Instance.GetData<WorldGenData>("default_world").sites;

			foreach (var siteEntry in sites)
			{
				totalWeight += siteEntry.weight;
			}


			var siteRandom = FNERandom.GetRandomIntInRange(1, totalWeight + 1);

			foreach (var siteEntry in sites)
			{
				if (siteRandom <= siteEntry.weight)
				{
					var siteId = siteEntry.siteRef;

					var siteData = DataBank.Instance.GetData<SiteData>(siteId);

					// 0 => 0* | 1 => 90* | 2 => 180* | 3 => 270*
					//byte siteRot = (byte)(FNERandom.GetRandomIntInRange(0, 4));
					byte siteRot = 0;
					
					var startX = 0;
					var startY = 0;
					if(siteRot % 2 == 0)
					{
						startX = FNERandom.GetRandomIntInRange(1, 32 - siteData.width);
						startY = FNERandom.GetRandomIntInRange(1, 32 - siteData.height);
					}
					else
					{
						startX = FNERandom.GetRandomIntInRange(1, 32 - siteData.height);
						startY = FNERandom.GetRandomIntInRange(1, 32 - siteData.width);
					}
					

					ImportSite(siteData, $"{Application.streamingAssetsPath}/{siteData.filePath}", startX, startY, chunk, siteRot);
					break;
				}
				else
				{
					siteRandom -= siteEntry.weight;
				}
			}
		}

		private void GenerateSitesV2(ServerWorldChunk chunk)
		{
			var fullSiteMetaData = GameServer.World.GetSiteMetaData();
			
			if(!fullSiteMetaData.ContainsKey(chunk.ChunkX + chunk.ChunkY * GameServer.World.WIDTH_IN_CHUNKS))
			{
				return;
			}
			
			var siteMetaData = fullSiteMetaData[chunk.ChunkX + chunk.ChunkY * GameServer.World.WIDTH_IN_CHUNKS];
			
			var siteData = DataBank.Instance.GetData<SiteData>(siteMetaData.siteId);

			//Debug.LogWarning("LOAD SITE: " + siteData.Id);

			// Import minor site
			if (siteData.width < 32 && siteData.height < 32)
			{
				//Debug.LogWarning("LOAD MINOR: " + siteData.Id);
				// 0 => 0* | 1 => 90* | 2 => 180* | 3 => 270*
				//byte siteRot = (byte)(FNERandom.GetRandomIntInRange(0, 4));

				byte siteRot = 0;

				var startX = siteMetaData.centerWorldX % 32 - siteMetaData.width / 2;
				var startY = siteMetaData.centerWorldY % 32 - siteMetaData.height / 2;

				/*if (siteRot % 2 == 0)
				{
					startX = FNERandom.GetRandomIntInRange(1, 32 - siteData.width);
					startY = FNERandom.GetRandomIntInRange(1, 32 - siteData.height);
				}
				else
				{
					startX = FNERandom.GetRandomIntInRange(1, 32 - siteData.height);
					startY = FNERandom.GetRandomIntInRange(1, 32 - siteData.width);
				}*/
				
				ImportSite(
					siteData,
					$"{Application.streamingAssetsPath}/{siteData.filePath}", 
					startX, 
					startY, 
					chunk, 
					siteRot
				);
				return;
			}

			//Debug.LogWarning("LOAD MAJOR PART: " + siteData.Id);
			ImportMajorSite(
				$"{Application.streamingAssetsPath}/{siteData.filePath}",
				siteMetaData.centerWorldX,
				siteMetaData.centerWorldY,
				siteMetaData.width,
				siteMetaData.height,
				chunk,
				0
			);
		}

		// Import multi-chunk site
		private void ImportMajorSite(
			string filePath,
			int centerWorldX,
			int centerWorldY,
			ushort width,
			ushort height,
			ServerWorldChunk chunk,
			byte siteRot
		)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			var versionCode = nb.ReadInt32();

			if(versionCode != (int) FileUtils.FNS_File_Version_Code.FNS_FILE_VERSION_1)
            {
				//Debug.Log("NOT LATEST VERSION: " + filePath);
				return;
            }

			var idTransl = new IdTranslator();
			idTransl.Deserialize(nb);

			// site width in tiles
			var widthInTiles = nb.ReadInt16();
			// site height in tiles
			var heightInTiles = nb.ReadInt16();

			if(widthInTiles != width || heightInTiles != height)
            {
				Debug.LogError("Site height or width does not match xml declaration");
				return;
            }

			siteRot = 0;

			if (siteRot == 0)
			{
				#region 0DegreeImport

				var startX = centerWorldX - widthInTiles / 2;
				var endX = startX + widthInTiles;
				var startY = centerWorldY - heightInTiles / 2;
				var endY = startY + heightInTiles;

				for (int x = startX; x < endX; x++)
				{
					for (int y = startY; y < endY; y++)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileMajorSite(x, y, nb, chunk, idTransl);
							ReadTileEdgeObjectsMajorSite(x, y, nb, chunk, idTransl, siteRot);
							ReadTileObjectMajorSite(x, y, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
			/*else if (siteRot == 1)
			{
				#region 90DegreeImport

				for (int y = 0; y < widthInTiles; y++)
				{
					for (int x = heightInTiles; x > 0; --x)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
			else if (siteRot == 2)
			{
				#region 180DegreeImport

				for (int x = widthInTiles; x > 0; --x)
				{
					for (int y = heightInTiles; y > 0; --y)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
			else if (siteRot == 3)
			{
				#region 270DegreeImport

				for (int y = widthInTiles; y > 0; --y)
				{
					for (int x = 0; x < heightInTiles; x++)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}*/
		}

		private void ImportSite(
			SiteData siteData,
			string filePath,
			int startTileX,
			int startTileY,
			ServerWorldChunk chunk,
			byte siteRot
		)
		{
			NetBuffer nb = new NetBuffer();
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				using (var br = new BinaryReader(fs))
				{
					nb.Data = br.ReadBytes((int)br.BaseStream.Length);
				}
			}

			var versionCode = nb.ReadInt32();

			if (versionCode != (int)FileUtils.FNS_File_Version_Code.FNS_FILE_VERSION_1)
			{
				Debug.Log("NOT LATEST VERSION: " + filePath);
				return;
			}

			var idTransl = new IdTranslator();
			idTransl.Deserialize(nb);

			// site width in tiles
			var widthInTiles = nb.ReadInt16();
			// site height in tiles
			var heightInTiles = nb.ReadInt16();

			if (siteData.width != widthInTiles)
			{
				Debug.LogError("Site width in XML does not match site file: " + siteData.Id + " - Site width in site file is " + widthInTiles);
				return;
			}
			if (siteData.height != heightInTiles)
			{
				Debug.LogError("Site height in XML does not match site file: " + siteData.Id + " - Site height in site file is " + heightInTiles);
				return;
			}
			
			Debug.Log("CHUNK ON: " + chunk.ChunkX + " : " + chunk.ChunkY);
			Debug.Log("TILE POS: " + startTileX + " : " + startTileY);
			
			if(siteRot == 0)
            {
				#region 0DegreeImport

				for (int x = 0; x < widthInTiles; x++)
				{
					for (int y = 0; y < heightInTiles; y++)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
            else if(siteRot == 1)
            {
				#region 90DegreeImport

				for (int y = 0; y < widthInTiles; y++)
				{
					for (int x = heightInTiles; x > 0; --x)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
			else if (siteRot == 2)
			{
				#region 180DegreeImport

				for (int x = widthInTiles; x > 0; --x)
				{
					for (int y = heightInTiles; y > 0; --y)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}
			else if (siteRot == 3)
			{
				#region 270DegreeImport

				for (int y = widthInTiles; y > 0; --y)
				{
					for (int x = 0; x < heightInTiles; x++)
					{
						bool isNullTile = nb.ReadBoolean();
						if (!isNullTile)
						{
							ReadTileSite(x + startTileX, y + startTileY, nb, chunk, idTransl);
							ReadTileEdgeObjectsSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
							ReadTileObjectSite(x + startTileX, y + startTileY, nb, chunk, idTransl, siteRot);
						}
					}
				}

				#endregion
			}

			var siteCenterX = startTileX + (widthInTiles / 2f);
			var siteCenterY = startTileY + (heightInTiles / 2f);
			
			GameServer.EntityAPI.SpawnEnemiesWithBudget(
				siteData.enemyBudget,
				siteData.enemySpawning,
				new float2((chunk.ChunkX * 32) + siteCenterX, (chunk.ChunkY * 32) + siteCenterY),
				0,
				widthInTiles > heightInTiles ? (float) widthInTiles / 2f : (float) heightInTiles / 2f,
				false,
				float2.zero,
				0,
				0.5f,
				""
			);
		}

		private void ReadTileMajorSite(int tileX, int tileY, NetBuffer br, ServerWorldChunk chunk, IdTranslator idTransl)
		{
			string id = idTransl.GetId<TileData>(br.ReadUInt16());

			var chunkRect = new RectInt(chunk.ChunkX * 32, chunk.ChunkY * 32, 32, 32);

			if (!chunkRect.Contains(new Vector2Int(tileX, tileY)))
				return;

			ushort chunkTileX = (ushort)(tileX - chunk.ChunkX * 32);
			ushort chunkTileY = (ushort)(tileY - chunk.ChunkY * 32);

			chunk.TileIdCodes[chunkTileX + chunkTileY * 32] = IdTranslator.Instance.GetIdCode<TileData>(id);
			chunk.BlockingTiles[chunkTileX + chunkTileY * 32] = DataBank.Instance.GetData<TileData>(id).isBlocking;
		}

		private void ReadTileSite(int tileX, int tileY, NetBuffer br, ServerWorldChunk chunk, IdTranslator idTransl)
		{
			string id = idTransl.GetId<TileData>(br.ReadUInt16());
			
			chunk.TileIdCodes[tileX + tileY * 32] = IdTranslator.Instance.GetIdCode<TileData>(id);
			chunk.BlockingTiles[tileX + tileY * 32] = DataBank.Instance.GetData<TileData>(id).isBlocking;
		}

		private void ReadTileEdgeObjectsSite(int tileX, int tileY, NetBuffer br, ServerWorldChunk chunk, IdTranslator idTransl, byte siteRotation)
		{
			bool hasWestEdge = br.ReadBoolean();
			if (hasWestEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				FNEEntity entity = null;
                switch (siteRotation)
                {
					// Still West
					case 0:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rotation
						);

						if (rotation == 180 || rotation == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes South
					case 1:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot90
						);

						if (rot90 == 180 || rot90 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes East
					case 2:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 1, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot180
						);
						if (rot180 == 180 || rot180 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes North
					case 3:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY + 1),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot270
						);
						if (rot270 == 180 || rot270 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;
                }

                if (hasMountedObject)
                {
					var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
					var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

					entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
					entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
				}
			}

			bool hasSouthEdge = br.ReadBoolean();
			if (hasSouthEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				FNEEntity entity = null;
				switch (siteRotation)
                {
					// Still South
					case 0:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rotation
						);
						if (rotation == 180 || rotation == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes East
					case 1:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 1, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot90
						);
						if (rot90 == 180 || rot90 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes North
					case 2:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY + 1),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot180
						);
						if (rot180 == 180 || rot180 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes West
					case 3:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot270
						);
						if (rot270 == 180 || rot270 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;
                }

				if (hasMountedObject)
				{
					var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
					var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

					entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
					entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
				}
			}

			bool hasEastEdge = br.ReadBoolean();
			if (hasEastEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				FNEEntity entity = null;
				switch (siteRotation)
				{
					// Still East
					case 0:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 1, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rotation
						);
						if (rotation == 180 || rotation == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes North
					case 1:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY + 1),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot90
						);
						if (rot90 == 180 || rot90 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes West
					case 2:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot180
						);
						if (rot180 == 180 || rot180 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes South
					case 3:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot270
						);
						if (rot270 == 180 || rot270 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;
				}

				if (hasMountedObject)
				{
					var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
					var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

					entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
					entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
				}
			}

			bool hasNorthEdge = br.ReadBoolean();
			if (hasNorthEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				FNEEntity entity = null;
				switch (siteRotation)
				{
					// Still North
					case 0:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY + 1),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rotation
						);
						if (rotation == 180 || rotation == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes West
					case 1:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot90
						);
						if (rot90 == 180 || rot90 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes South
					case 2:
						entity = GenerateSouthEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 0.5f, chunk.ChunkY * 32 + tileY),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot180
						);
						if (rot180 == 180 || rot180 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;

					// Becomes East
					case 3:
						entity = GenerateWestEdgeObject(
							new float2(chunk.ChunkX * 32 + tileX + 1, chunk.ChunkY * 32 + tileY + 0.5f),
							chunk,
							idTransl.GetId<FNEEntityData>(idCode),
							rot270
						);
						if (rot270 == 180 || rot270 == 90)
						{
							mountedRotation = !mountedRotation;
						}
						break;
				}

				if (hasMountedObject)
				{
					var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
					var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

					entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
					entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
				}
			}
		}

		private void ReadTileObjectSite(int tileX, int tileY, NetBuffer br, ServerWorldChunk chunk, IdTranslator idTransl, byte siteRotation)
        {
			bool hasTileObject = br.ReadBoolean();
			if (!hasTileObject)
				return;

			string id = idTransl.GetId<FNEEntityData>(br.ReadUInt16());
			ushort rotation = br.ReadUInt16();

			var entity = GenerateTileObject(
				new float2(chunk.ChunkX * 32 + tileX, chunk.ChunkY * 32 + tileY),
				tileX + tileY * 32,
				chunk,
				id
			);

			entity.RotationDegrees = rotation + (siteRotation * 90);
		}

		private void ReadTileObjectMajorSite(int tileX, int tileY, NetBuffer br, ServerWorldChunk chunk, IdTranslator idTransl, byte siteRotation)
		{
			bool hasTileObject = br.ReadBoolean();
			if (!hasTileObject)
				return;

			var chunkRect = new RectInt(chunk.ChunkX * 32, chunk.ChunkY * 32, 32, 32);
			var idCode = br.ReadUInt16();
			ushort rotation = br.ReadUInt16();

			if (!chunkRect.Contains(new Vector2Int(tileX, tileY)))
				return;

			ushort chunkTileX = (ushort)(tileX - chunk.ChunkX * 32);
			ushort chunkTileY = (ushort)(tileY - chunk.ChunkY * 32);

			string id = idTransl.GetId<FNEEntityData>(idCode);

			var entity = GenerateTileObject(
				new float2(chunk.ChunkX * 32 + chunkTileX, chunk.ChunkY * 32 + chunkTileY),
				chunkTileX + chunkTileY * 32,
				chunk,
				id
			);

			entity.RotationDegrees = rotation + (siteRotation * 90);
		}

		private void ReadTileEdgeObjectsMajorSite(
			int tileX, 
			int tileY, 
			NetBuffer br, 
			ServerWorldChunk chunk, 
			IdTranslator idTransl, 
			byte siteRotation
		)
		{
			var chunkRect = new RectInt(chunk.ChunkX * 32, chunk.ChunkY * 32, 32, 32);

			bool hasWestEdge = br.ReadBoolean();
			
			if (hasWestEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
                {
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				if (chunkRect.Contains(new Vector2Int(tileX, tileY)))
                {
					FNEEntity entity = null;
					switch (siteRotation)
					{
						// Still West
						case 0:
							entity = GenerateWestEdgeObject(
								new float2(tileX, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rotation
							);
							if (rotation == 180 || rotation == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes South
						case 1:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot90
							);
							if (rot90 == 180 || rot90 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes East
						case 2:
							entity = GenerateWestEdgeObject(
								new float2(tileX + 1, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot180
							);
							if (rot180 == 180 || rot180 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes North
						case 3:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY + 1),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot270
							);
							if (rot270 == 180 || rot270 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;
					}

					if (hasMountedObject)
					{
						var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
						var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

						entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
						entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
					}
				}
			}

			bool hasSouthEdge = br.ReadBoolean();
			
			if (hasSouthEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				if (chunkRect.Contains(new Vector2Int(tileX, tileY)))
                {
					FNEEntity entity = null;
					switch (siteRotation)
					{
						// Still South
						case 0:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rotation
							);
							if (rotation == 180 || rotation == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes East
						case 1:
							entity = GenerateWestEdgeObject(
								new float2(tileX + 1, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot90
							);
							if (rot90 == 180 || rot90 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes North
						case 2:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY + 1),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot180
							);
							if (rot180 == 180 || rot180 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes West
						case 3:
							entity = GenerateWestEdgeObject(
								new float2(tileX, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot270
							);
							if (rot270 == 180 || rot270 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;
					}

					if (hasMountedObject)
					{
						var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
						var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

						entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
						entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
					}
				}
					
			}

			bool hasEastEdge = br.ReadBoolean();
			

			if (hasEastEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				if (chunkRect.Contains(new Vector2Int(tileX, tileY)))
                {
					FNEEntity entity = null;
					switch (siteRotation)
					{
						// Still East
						case 0:
							entity = GenerateWestEdgeObject(
								new float2(tileX + 1, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rotation
							);
							if (rotation == 180 || rotation == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes North
						case 1:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY + 1),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot90
							);
							if (rot90 == 180 || rot90 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes West
						case 2:
							entity = GenerateWestEdgeObject(
								new float2(tileX, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot180
							);
							if (rot180 == 180 || rot180 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes South
						case 3:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot270
							);
							if (rot270 == 180 || rot270 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;
					}

					if (hasMountedObject)
					{
						var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
						var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

						entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
						entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
					}
				}
			}

			bool hasNorthEdge = br.ReadBoolean();

			if (hasNorthEdge)
			{
				ushort idCode = br.ReadUInt16();
				short rotation = br.ReadInt16();
				bool hasMountedObject = br.ReadBoolean();
				ushort mountedIdCode = ushort.MaxValue;
				bool mountedRotation = false;

				int rot90 = (rotation + 90) % 360;
				int rot180 = (rotation + 180) % 360;
				int rot270 = (rotation + 270) % 360;

				if (hasMountedObject)
				{
					mountedIdCode = br.ReadUInt16();
					mountedRotation = br.ReadBoolean();
				}

				if (chunkRect.Contains(new Vector2Int(tileX, tileY)))
                {
					FNEEntity entity = null;
					switch (siteRotation)
					{
						// Still North
						case 0:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY + 1),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rotation
							);
							if (rotation == 180 || rotation == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes West
						case 1:
							entity = GenerateWestEdgeObject(
								new float2(tileX, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot90
							);
							if (rot90 == 180 || rot90 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes South
						case 2:
							entity = GenerateSouthEdgeObject(
								new float2(tileX + 0.5f, tileY),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot180
							);
							if (rot180 == 180 || rot180 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;

						// Becomes East
						case 3:
							entity = GenerateWestEdgeObject(
								new float2(tileX + 1, tileY + 0.5f),
								chunk,
								idTransl.GetId<FNEEntityData>(idCode),
								rot270
							);
							if (rot270 == 180 || rot270 == 90)
							{
								mountedRotation = !mountedRotation;
							}
							break;
					}

					if (hasMountedObject)
					{
						var id = idTransl.GetId<MountedObjectData>(mountedIdCode);
						var mountedObjectData = DataBank.Instance.GetData<MountedObjectData>(id);

						entity.GetComponent<EdgeObjectComponentServer>().MountedObjectData = mountedObjectData;
						entity.GetComponent<EdgeObjectComponentServer>().OppositeMountedDirection = mountedRotation;
					}
				}	
			}
		}

		public FNEEntity GenerateTileObject(float2 position, int index, ServerWorldChunk chunk, string id)
		{
			var entity = GameServer.EntityAPI.CreateEntityImmediate(
				id,
				position,
				FNERandom.GetRandomIntInRange(0, 4) * 90
			);

			foreach (var comp in entity.Components)
			{
				if (comp is ITickable)
				{
					if (comp is CropComponentServer)
					{
						((CropComponentServer)comp).ForceMature();
					}
					break;
				}
			}

			AddTileObjectToGenerationMap(position, chunk.ChunkX, chunk.ChunkY);

			chunk.EntitiesToSync.Add(entity);
			
			//GameServer.EntityAPI.AddEntityToWorldStateImmediate(entity);
			//GameServer.NetConnector.SyncEntity(entity);

			return entity;
		}

		private bool HasTileObject(int tileX, int tileY)
		{
			return m_TileObjectGenerationMap[tileX % 32 + ((tileY % 32) * 32)];
		}

		private void AddTileObjectToGenerationMap(float2 position, byte chunkX, byte chunkY)
		{
			int index = ((int)position.x - chunkX * 32) + ((int)position.y - chunkY * 32) * 32;
			m_TileObjectGenerationMap[index] = true;
		}

		public FNEEntity GenerateSouthEdgeObject(float2 position, ServerWorldChunk chunk, string id, float rotation)
		{
			var entity = GameServer.EntityAPI.CreateEntityImmediate(
				id,
				position,
				rotation
			);

			chunk.EntitiesToSync.Add(entity);

			//chunk.AddSouthEdgeObject(entity);
			//GameServer.NetConnector.SyncEntity(entity);

			return entity;
		}

		public FNEEntity GenerateWestEdgeObject(float2 position, ServerWorldChunk chunk, string id, float rotation)
		{
			var entity = GameServer.EntityAPI.CreateEntityImmediate(
				id,
				position,
				rotation
			);

			chunk.EntitiesToSync.Add(entity);

			//chunk.AddWestEdgeObject(entity);
			//GameServer.NetConnector.SyncEntity(entity);

			return entity;
		}


		private float[] GenerateHeightMap(byte chunkX, byte chunkY, byte chunkSize, int seedX, int seedY)
		{
			float[] heightMap = new float[chunkSize * chunkSize];
			float layerFrequency = m_WorldGenData.layerFrequency;
			float layerWeight = m_WorldGenData.layerWeight;

			for (int octave = 0; octave < m_WorldGenData.octaves; octave++)
			{
				for (int y = 0; y < chunkSize; y++)
				{
					for (int x = 0; x < chunkSize; x++)
					{
						float inputX = chunkX * chunkSize + x + seedX;
						float inputY = chunkY * chunkSize + y + seedY;
						float noise = m_WorldGenData.layerWeight * SimplexNoise.Noise(inputX * layerFrequency, inputY * layerFrequency);
						heightMap[x + y * chunkSize] += noise;
					}
				}

				layerFrequency *= 2.0f;
				layerWeight *= m_WorldGenData.roughness;
			}

			return heightMap;
		}

		private void GenerateEnemies(ServerWorldChunk chunk)
		{
			var chunkX = chunk.ChunkX;
			var chunkY = chunk.ChunkY;

			var worldCenterChunkX = GameServer.World.WIDTH / 2;
			var worldCenterChunkY = GameServer.World.HEIGHT / 2;

			if (chunkX >= worldCenterChunkX - 2 && chunkX <= worldCenterChunkX + 2
			&& chunkY >= worldCenterChunkY - 2 && chunkY <= worldCenterChunkY + 2)
			{
				// chunk in safe zone
				return;
			}
			
			var generalDangerLevelWeight = math.distance(
				new int2(chunkX, chunkY), 
				new int2(worldCenterChunkX, worldCenterChunkY));
			
			var siteMetaData = GameServer.World.GetSiteMetaData();
			var isSite = false;
			if (siteMetaData.ContainsKey(chunkX * chunkY))
			{
				generalDangerLevelWeight += 25;
				isSite = true;
			}
			
			generalDangerLevelWeight += 1;

			generalDangerLevelWeight = generalDangerLevelWeight > 99 ? 99 : generalDangerLevelWeight;

			for (int i = 0; i < 1000; i++)
			{
				if (FNERandom.GetRandomIntInRange(0, 150 - (int) generalDangerLevelWeight) == 0)
				{
					var spawnRotation = FNERandom.GetRandomIntInRange(0, 360);

					var spawnPos = new float2(
						FNERandom.GetRandomIntInRange(chunkX * 32, chunkX * 32 + 32),
						FNERandom.GetRandomIntInRange(chunkY * 32, chunkY* 32 + 32)
						);

					var enemy = "shrubber";
					if (isSite)
					{
						var rand = FNERandom.GetRandomIntInRange(0, 100);
						if (rand <= 100 && rand > 90)
							enemy = "zombie_big";
						else if (rand <= 90 && rand >= 0)
							enemy = "default_zombie";
					}
					
					var chunkForSpawnPos = GameServer.World.GetWorldChunk<ServerWorldChunk>(spawnPos);
					if (chunkForSpawnPos == null)
						continue;
					
					var modelEntity = GameServer.EntityAPI.CreateEntityImmediate(enemy, spawnPos, spawnRotation);
					
					chunk.MovingEntitiesToSync.Add(modelEntity);

					if (FNERandom.GetRandomIntInRange(0, 20) == 0)
					{
						var clusterCount = FNERandom.GetRandomIntInRange(10, 50);

						for (int e = 0; e < clusterCount; e++)
						{
							var distance = FNERandom.GetRandomFloatInRange(0, 4);
							var v = new Vector2(distance, 0);
				
							var finalOffset = Quaternion.Euler(0, 0, FNERandom.GetRandomFloatInRange(0, 360)) * v;
					
							var spawnPosition = new float2(spawnPos.x + finalOffset.x, spawnPos.y + finalOffset.y);
							if (GameServer.World.GetWorldChunk<ServerWorldChunk>(spawnPosition) == null)
								continue;

							spawnRotation = FNERandom.GetRandomIntInRange(0, 360);

							modelEntity = GameServer.EntityAPI.CreateEntityImmediate(enemy, spawnPosition, spawnRotation);
							chunk.MovingEntitiesToSync.Add(modelEntity);
						}
					}
				}
			}
			// This not needed?
			//GameServer.NetAPI.Entity_SpawnHordeEntity_Batched_BAR(spawnDataList);
		}
	}
}

