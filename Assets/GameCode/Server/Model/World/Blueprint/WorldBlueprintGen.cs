using FNZ.Server.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace FNZ.Server.Model.World.Blueprint
{
	public partial class WorldBlueprintGen
	{
		private readonly WorldGenData m_WorldGenData;

		public WorldBlueprintGen(WorldGenData data)
		{
			m_WorldGenData = data;
		}

		Dictionary<string, TileData> m_TileDataDict = new Dictionary<string, TileData>();
		Dictionary<string, ushort> m_TileCodeDict = new Dictionary<string, ushort>();

		Semaphore s;

		ushort[] tileIds;
		Color32[] pixels;

		Semaphore arrayBlock;

		private const byte sitePadding = 3;
		private const byte safeZonePadding = 1;
		
		// TEMP DEMO KOD
		public static List<int2> event1Sites = new List<int2>();
		public static int2 event2Site;

		public int worldChunkSize = 256;
		public int chunkSize = 32;

		private int[] m_RoadDistances;
		
		public struct SiteMetaData
        {
			public int centerWorldX;
			public int centerWorldY;
			public ushort chunkX;
			public ushort chunkY;
			public byte width;
			public byte height;
			public string siteId;
			public byte rotation;

			public void Serialize(NetBuffer bw)
            {
				bw.Write(centerWorldX);
				bw.Write(centerWorldY);
				bw.Write(chunkX);
				bw.Write(chunkY);
				bw.Write(width);
				bw.Write(height);
				bw.Write(siteId);
				bw.Write(rotation);
			}

			public void Deserialize(NetBuffer br)
			{
				centerWorldX = br.ReadInt32();
				centerWorldY = br.ReadInt32();
				chunkX = br.ReadUInt16();
				chunkY = br.ReadUInt16();
				width = br.ReadByte();
				height = br.ReadByte();
				siteId = br.ReadString();
				rotation = br.ReadByte();
			}

			public int GetBufferSize()
            {
				return 11 + (siteId.Length * 4);
			}
        }

		public void GenerateSiteMapBlueprint(int seedX, int seedY)
        {
			var starttime = DateTime.Now.Ticks;
			
			var worldTileSize = worldChunkSize * chunkSize;

			bool[] occupiedChunks = new bool[worldChunkSize * worldChunkSize];
			Dictionary<int, bool> majorChunkCenters = new Dictionary<int, bool>();
			Dictionary<int, bool> landmarkChunks = new Dictionary<int, bool>();
			Dictionary<int, bool> plotChunks = new Dictionary<int, bool>();
			Dictionary<int, bool> vertHighwayChunks = new Dictionary<int, bool>();
			Dictionary<int, bool> horHighwayChunks = new Dictionary<int, bool>();
			Dictionary<int, bool> majorRoadChunks = new Dictionary<int, bool>();
			Dictionary<int, bool> minorRoadChunks = new Dictionary<int, bool>();
			
			pixels = new Color32[worldChunkSize * worldChunkSize];
			
			// Spawn hard-coded sites in starting area. Generate safe zone.
            InitBlueprintGen(occupiedChunks, majorChunkCenters, landmarkChunks);
            
            // Generate highway grid
            GenerateHighways(occupiedChunks, vertHighwayChunks, horHighwayChunks);
            
            // Reserve space for MAJOR sites (bigger than 1 chunk in any direction)
			ReserveMajorSiteChunks(occupiedChunks, majorChunkCenters);
			
			// Place sites within reserved MAJOR site spaces
			Dictionary<int, SiteMetaData> majorOccupiedChunks;
			majorOccupiedChunks = PlaceMajorSites(majorChunkCenters, pixels);

			foreach(var key in majorOccupiedChunks.Keys)
			{
				pixels[(key % worldChunkSize) + (key / worldChunkSize) * 256] = new Color32(255, 0, 255, 255);
			}
			
			List<SiteMetaData> toWrite = new List<SiteMetaData>();

			foreach (var entry in majorOccupiedChunks)
            {
				toWrite.Add(entry.Value);
			}
			
			// Calculate initial road distances to highways
			m_RoadDistances = new int[worldChunkSize * worldChunkSize];
			for (int i = 0; i < m_RoadDistances.Length; i++)
				m_RoadDistances[i] = int.MaxValue;
			
			FloodFillHighways(occupiedChunks, vertHighwayChunks, horHighwayChunks);
			
			PlaceMajorRoads(
				occupiedChunks,
				toWrite,
				majorOccupiedChunks,
				majorRoadChunks
			);
			
			// Reserve chunks for Landmark sites (Smaller than 1 chunk in both directions)
			ReserveLandmarkChunks(occupiedChunks, landmarkChunks);
			
			// Place sites within reserved Landmark site spaces
			Dictionary<int, SiteMetaData> landmarkOccupiedChunks;
			landmarkOccupiedChunks = PlaceLandmarks(landmarkChunks, pixels);

			foreach (var entry in landmarkOccupiedChunks)
			{
				toWrite.Add(entry.Value);
			}
			
			PlaceMinorRoads(occupiedChunks, landmarkOccupiedChunks, minorRoadChunks);
			
			// Reserve chunks for Plot sites (Smaller than 1 chunk in both directions)
			ReservePlotChunks(occupiedChunks, plotChunks);
			
			// Place sites within reserved plot site spaces
			Dictionary<int, SiteMetaData> plotOccupiedChunks;
			plotOccupiedChunks = PlacePlots(plotChunks, pixels);
			
			foreach (var entry in plotOccupiedChunks)
			{
				toWrite.Add(entry.Value);
			}

			var endTime = DateTime.Now.Ticks;

			var span = new TimeSpan(endTime - starttime);

			var netBuffer = new NetBuffer();

			netBuffer.Write(toWrite.Count);

			var size = 0;
			for (int i = 0; i < toWrite.Count; i++)
			{
				size += toWrite[i].GetBufferSize();
			}

			netBuffer.EnsureBufferSize(size);

			for (int i = 0; i < toWrite.Count; i++)
            {
				toWrite[i].Serialize(netBuffer);
			}

			FileUtils.WriteFile(
				GameServer.FilePaths.GetOrCreateChunkSiteMetaFilePath(),
				netBuffer.Data
			);

			var texture = new Texture2D(worldChunkSize, worldChunkSize, TextureFormat.RGBA32, false);
			texture.SetPixels32(0, 0, worldChunkSize, worldChunkSize, pixels);
			texture.Apply();

			byte[] bytes = texture.EncodeToPNG();

			File.WriteAllBytes(Application.dataPath + "/NEWWORLD.png", bytes);
		}
		
		#region old testing stuff
		
		public void GenerateHeightMapBlueprint(int seedX, int seedY)
		{
			var tilesInBiome = m_WorldGenData.tilesInBiome;

			var worldChunkSize = 256;
			var chunkSize = 32;

			var worldTileSize = worldChunkSize * chunkSize;

			tileIds = new ushort[worldTileSize * worldTileSize];
			pixels = new Color32[worldTileSize * worldTileSize];

			tilesInBiome.ForEach(tib =>
			{
				m_TileDataDict.Add(tib.tileRef, DataBank.Instance.GetData<TileData>(tib.tileRef));
				m_TileCodeDict.Add(tib.tileRef, IdTranslator.Instance.GetIdCode<TileData>(tib.tileRef));
			});

			Debug.LogWarning("GENERATE WOLR DBLUEPRINT!");

			var starttime = DateTime.Now.Ticks;

			int worker = 0;
			worker = Environment.ProcessorCount - 2;

			Debug.LogWarning("AVAILABLE THREADS! " + worker);

			var threads = new Thread[worker];

			s = new Semaphore(worker, worker);
			arrayBlock = new Semaphore(1, 1);

			for (int i = 0; i < worker; i++)
			{
				new Thread(() => GenThread(i, seedX, seedY, worker))
				{
					Name = "WorldGen-Thread"
				}.Start();

				Thread.Sleep(50);
			}

			Thread.Sleep(1000);

			for(int i = 0; i < worker; i++)
				s.WaitOne();

			var endTime = DateTime.Now.Ticks;

			var span = new TimeSpan(endTime - starttime);

			Debug.LogWarning("GENERATE WORLD BLUEPRINT DONE!");
			Debug.LogWarning("Took Seconds: " + span.TotalSeconds);

			/*var texture = new Texture2D(worldTileSize, worldTileSize, TextureFormat.RGBA32, false);
			texture.SetPixels32(0, 0, worldTileSize, worldTileSize, pixels);
			texture.Apply();

			byte[] bytes = texture.EncodeToPNG();

			Debug.LogWarning("SAVE TO: " + Application.dataPath + "/NEWWORLD.png");

			File.WriteAllBytes(Application.dataPath + "/NEWWORLD.png", bytes);*/
		}

		private void GenThread(int threadIndex, int seedX, int seedY, int threadCount)
		{
			s.WaitOne();

			var tilesInBiome = m_WorldGenData.tilesInBiome;

			var worldChunkSize = 256;
			var chunkSize = 32;

			var worldTileSize = worldChunkSize * chunkSize;

			var frequency = m_WorldGenData.layerFrequency;
			var weight = m_WorldGenData.layerWeight;
			var roughness = m_WorldGenData.roughness;
			for(int cY = threadIndex; cY < worldChunkSize; cY += threadCount)
            {
				for (int cX = 0; cX < worldChunkSize; cX++)
				{
					for (int y = 0; y < chunkSize; y++)
					{
						var worldY = cY * 32 + y;
						for (int x = 0; x < chunkSize; x++)
						{
							var worldX = cX * 32 + x;

							float layerFrequency = frequency;
							float layerWeight = weight;

							var heightValue = 0f;

							for (int octave = 0; octave < m_WorldGenData.octaves; octave++)
							{
								float inputX = worldX + seedX;
								float inputY = worldY + seedY;
								float noise = layerWeight * SimplexNoise.Noise(inputX * layerFrequency, inputY * layerFrequency);
								heightValue += noise;

								layerFrequency *= 2.0f;
								layerWeight *= roughness;
							}

							float height = Mathf.Clamp(heightValue, -1.0f, 1.0f);

							for (int t = 0; t < tilesInBiome.Count; t++)
							{
								if (tilesInBiome[t].height >= height)
								{
									tileIds[worldX + worldY * worldTileSize] = m_TileCodeDict[tilesInBiome[t].tileRef];
									//pixels[worldX + worldY * worldTileSize] = FNEUtil.ConvertHexStringToColor32(m_TileDataDict[tilesInBiome[t].tileRef].mapColor);
									break;
								}
							}
						}
					}
				}
			}

			s.Release(1);
		}
		
		#endregion
	}
}