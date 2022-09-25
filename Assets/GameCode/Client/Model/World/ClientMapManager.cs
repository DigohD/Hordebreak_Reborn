using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World;
using FNZ.Shared.Model.World.Tile;
using FNZ.Shared.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace FNZ.Client.Model.World 
{

	public class ClientMapManager : MapManager
	{
		private static Texture2D mapImage;

		private static Texture2D maskImage;

		private Dictionary<ushort, Color32> colorDict = new Dictionary<ushort, Color32>();

		private Color32[] chunkPixels = new Color32[32 * 32];

		private static byte minX, maxX, minY, maxY;
		
		private const int c_WorldMapPartSize = 8;
		
		private static Texture2D[,] s_WorldMapParts = new Texture2D[c_WorldMapPartSize, c_WorldMapPartSize];

		public ClientMapManager(int widthInChunks, int heightInChunks) : base(widthInChunks, heightInChunks)
		{
			for (var y = 0; y < c_WorldMapPartSize; y++)
			{
				for (var x = 0; x < c_WorldMapPartSize; x++)
				{
					var dimension = (GameClient.World.WIDTH_IN_CHUNKS / c_WorldMapPartSize) *
					                GameClient.World.CHUNK_SIZE;
					
					var mapPartImage = new Texture2D(dimension, dimension, TextureFormat.RGBA32, 1, false)
					{
						filterMode = FilterMode.Point
					};

					s_WorldMapParts[x, y] = mapPartImage;

					var pixels = mapPartImage.GetRawTextureData<Color32>();

					for (var i = 0; i < dimension; i++)
					{
						for (var j = 0; j < dimension; j++)
						{
							pixels[j + i * dimension] = new Color32(20, 20, 20, 255);
						}
					}
					
					mapPartImage.Apply();
				}
			}
			
			// mapImage = new Texture2D(widthInChunks * 32, heightInChunks * 32, TextureFormat.RGBA32, 1, false);
			// mapImage.filterMode = FilterMode.Point;
			// mapPixelData = mapImage.GetRawTextureData<Color32>();
			//
			// maskImage = new Texture2D(widthInChunks * 32, heightInChunks * 32, TextureFormat.RGBA32, 1, false);
			// maskImage.filterMode = FilterMode.Point;
			// maskPixelData = maskImage.GetRawTextureData<Color32>();
			//
			// for (int i = 0; i < widthInChunks * 32; i++)
			// {
			// 	for (int j = 0; j < widthInChunks * 32; j++)
			// 	{
			// 		mapPixelData[i + j * maskImage.width] = new Color32(0, 0, 0, 0);
			// 		maskPixelData[i + j * maskImage.width] = new Color32(0, 0, 0, 255);
			// 	}
			// }
			// mapImage.Apply();
			// //maskImage.Apply();
		}
		
		private Texture2D GetWorldMapPartTexture(int chunkX, int chunkY)
		{
			var indices = GetWorldMapPartChunkIndicesFromChunk(chunkX, chunkY);
			return s_WorldMapParts[indices.x, indices.y];
		}

		private int2 GetWorldMapPartChunkIndicesFromChunk(int chunkX, int chunkY)
		{
			var size = 32;
			var chunkYpos = chunkY % size;
			var chunkYnr = (chunkY - chunkYpos) / size;
			var chunkXpos = chunkX % size;
			var chunkXnr = (chunkX - chunkXpos) / size;
			return new int2(chunkXnr, chunkYnr);
		}

		public void HandleRevealedChunk(byte chunkX, byte chunkY, ushort[] tileIdCodes)
		{
			minX = minX < chunkX ? chunkX : minX;
			maxX = maxX > chunkX ? chunkX : maxX;
			minY = minY < chunkY ? chunkY : minY;
			maxY = maxY > chunkY ? chunkY : maxY;

			Profiler.BeginSample("CALC MAP");

			// int centerX = chunkX * 32 + 16;
			// int centerY = chunkY * 32 + 16;
			//
			// bool rightRevealed = mapPixelData[(centerX + 32) + centerY * mapImage.width].a > 0;
			// bool leftRevealed = mapPixelData[(centerX - 32) + centerY * mapImage.width].a > 0;
			// bool topRevealed = mapPixelData[centerX + (centerY + 32) * mapImage.width].a > 0;
			// bool bottomRevealed = mapPixelData[centerX + (centerY - 32) * mapImage.width].a > 0;
			
			var mapPart = GetWorldMapPartTexture(chunkX, chunkY);
			var pixelData = mapPart.GetRawTextureData<Color32>();

			int i = 0;
			for (int y = chunkY * 32; y < chunkY * 32 + 32; y++)
			{
				for (int x = chunkX * 32; x < chunkX * 32 + 32; x++)
				{
					if (!colorDict.ContainsKey(tileIdCodes[i]))
					{
						var tileId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[i]);
						var tileData = DataBank.Instance.GetData<TileData>(tileId);
						var tileColor = FNEUtil.HexStringToColor(tileData.mapColor);
						colorDict.Add(tileIdCodes[i], tileColor);
					}
			
					pixelData[x % 1024 + y % 1024 * 1024] = colorDict[tileIdCodes[i]];
					
					i++;
				}
			}

			// DeterminePixelMasking(centerX, centerY);
			//
			// if (rightRevealed)
			// 	DeterminePixelMasking(centerX + 32, centerY);
			//
			// if (leftRevealed)
			// 	DeterminePixelMasking(centerX - 32, centerY);
			//
			// if (topRevealed)
			// 	DeterminePixelMasking(centerX, centerY + 32);
			//
			// if (bottomRevealed)
			// 	DeterminePixelMasking(centerX, centerY - 32);


			/*for (int i = 0; i < tileIdCodes.Length; i++)
			{
				if (!colorDict.ContainsKey(tileIdCodes[i]))
				{
					var tileId = IdTranslator.Instance.GetId<TileData>(tileIdCodes[i]);
					var tileData = DataBank.Instance.GetData<TileData>(tileId);
					var tileColor = FNEUtil.HexStringToColor(tileData.mapColor);
					colorDict.Add(tileIdCodes[i], tileColor);
				}

				chunkPixels[i] = colorDict[tileIdCodes[i]];
			}*/
			Profiler.EndSample();
			
			/*Profiler.BeginSample("SET PIXELS");
			mapImage.SetPixels32(chunkX * 32, chunkY * 32, 32, 32, chunkPixels, 0);
			Profiler.EndSample();*/

			Profiler.BeginSample("APPLY MAP");
			
			mapPart.Apply();
			//maskImage.Apply();
			Profiler.EndSample();

			// Save map to png (Debug purposes)
			// byte[] bytes = mapImage.EncodeToPNG();
			// File.WriteAllBytes(Application.dataPath + "/World_Map.png", bytes);
		}
		
		public static Texture2D GetTileMap()
		{
			return mapImage;
		}

		public static Texture2D GetMapPartTexture(int x, int y)
		{
			return s_WorldMapParts[x, y];
		}

		public static Texture2D GetMaskedTileMap()
		{
			return maskImage;
		}

		public static Rect GetRevealedBoundingRect()
		{
			return new Rect(minX, minY, maxX - minX, maxY - minY);
		}
		
		// private void DeterminePixelMasking(int centerX, int centerY)
		// {
		// 	bool rightRevealed = mapPixelData[(centerX + 32) + centerY * mapImage.width].a > 0;
		// 	bool leftRevealed = mapPixelData[(centerX - 32) + centerY * mapImage.width].a > 0;
		// 	bool topRevealed = mapPixelData[centerX + (centerY + 32) * mapImage.width].a > 0;
		// 	bool bottomRevealed = mapPixelData[centerX + (centerY - 32) * mapImage.width].a > 0;
		//
		// 	for (int y = centerY - 16; y < centerY + 16; y++)
		// 	{
		// 		for (int x = centerX - 16; x < centerX + 16; x++)
		// 		{
		// 			maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 0);
		//
		// 			if (!rightRevealed)
		// 			{
		// 				if (x - centerX >= 10)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 10);
		// 				}
		// 				if (x - centerX >= 11)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 30);
		// 				}
		// 				if (x - centerX >= 12)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 60);
		// 				}
		// 				if (x - centerX >= 13)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 100);
		// 				}
		// 				if (x - centerX >= 14)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 150);
		// 				}
		// 				if (x - centerX >= 15)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 200);
		// 				}
		// 				if (x - centerX >= 16)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 250);
		// 				}
		// 			}
		//
		// 			if (!leftRevealed)
		// 			{
		// 				if (x - centerX <= -10)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 10);
		// 				}
		// 				if (x - centerX <= -11)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 30);
		// 				}
		// 				if (x - centerX <= -12)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 60);
		// 				}
		// 				if (x - centerX <= -13)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 100);
		// 				}
		// 				if (x - centerX <= -14)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 150);
		// 				}
		// 				if (x - centerX <= -15)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 200);
		// 				}
		// 				if (x - centerX <= -16)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 250);
		// 				}
		// 			}
		//
		// 			if (!topRevealed)
		// 			{
		// 				if (y - centerY >= 10)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 10);
		// 				}
		// 				if (y - centerY >= 11)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 30);
		// 				}
		// 				if (y - centerY >= 12)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 60);
		// 				}
		// 				if (y - centerY >= 13)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 100);
		// 				}
		// 				if (y - centerY >= 14)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 150);
		// 				}
		// 				if (y - centerY >= 15)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 200);
		// 				}
		// 				if (y - centerY >= 16)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 250);
		// 				}
		// 			}
		//
		// 			if (!bottomRevealed)
		// 			{
		// 				if (y - centerY <= -10)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 10);
		// 				}
		// 				if (y - centerY <= -11)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 30);
		// 				}
		// 				if (y - centerY <= -12)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 60);
		// 				}
		// 				if (y - centerY <= -13)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 100);
		// 				}
		// 				if (y - centerY <= -14)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 150);
		// 				}
		// 				if (y - centerY <= -15)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 200);
		// 				}
		// 				if (y - centerY <= -16)
		// 				{
		// 					maskPixelData[x + y * maskImage.width] = new Color32(0, 0, 0, 250);
		// 				}
		// 			}
		// 		}
		// 	}
		// }
	}
}