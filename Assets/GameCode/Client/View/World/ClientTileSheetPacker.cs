using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.World
{
	public class ClientTileSheetPacker
	{
		public static List<TileData> s_AllTilesData = DataBank.Instance.GetAllDataIdsOfType<TileData>();

		//Contains each category and tile id and the tile's respective texture maps. Ex: s_TileCategories[natural][dirt_outdoor][albedo] returns the albedo map for dirt_outdoor tile.
		private static Dictionary<string, Dictionary<string, Dictionary<string, Texture2D>>> s_TileCategories = new Dictionary<string, Dictionary<string, Dictionary<string, Texture2D>>>();

		//contains the atlases for albedos, normals etc.
		private static Dictionary<string, Texture2D> s_MapAtlas = new Dictionary<string, Texture2D>();

		//This one keeps track of indiviual tilemap's atlasindexes.
		private static Dictionary<string, int> s_TileAtlasIndexes = new Dictionary<string, int>();

		//temporary dicts used to build s_TileCategories.
		private static Dictionary<string, Texture2D> s_AlbedoMaps = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Texture2D> s_NormalMaps = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Texture2D> s_MaskMaps = new Dictionary<string, Texture2D>();
		private static Dictionary<string, Texture2D> s_EmissiveMaps = new Dictionary<string, Texture2D>();

		private const string DEFAULT_ALBEDO_PATH = "Data/Xml/World/Tile/TileMaps/DefaultAlbedo.png";
		private const string DEFAULT_NORMAL_PATH = "Data/Xml/World/Tile/TileMaps/DefaultNormal.png";
		private const string DEFAULT_MASKMAP_PATH = "Data/Xml/World/Tile/TileMaps/DefaultMask.png";
		public const string STRING_ALBEDO = "albedo";
		public const string STRING_NORMAL = "normal";
		public const string STRING_MASK = "mask";
		public const string STRING_EMISSIVE = "emissive";

		private const short ATLAS_SIZE = 1024;

		public static byte TILES_PER_ROW = 8;
		public static byte TILE_SIZE = 128;
		
		private const byte TEXTURE_PADDING = 0;

		public static bool AreTilesReady = false;

		static ClientTileSheetPacker()
		{
			//Add textures from paths to respective dictionaries
			foreach (var tile in s_AllTilesData)
			{
				//adds category and/or id if missing.
				if (!s_TileCategories.ContainsKey(tile.category))
					s_TileCategories.Add(tile.category, new Dictionary<string, Dictionary<string, Texture2D>>());
				if (!s_TileCategories[tile.category].ContainsKey(tile.Id))
					s_TileCategories[tile.category].Add(tile.Id, new Dictionary<string, Texture2D>());

				s_TileCategories[tile.category][tile.Id].Add(STRING_ALBEDO, GetMapFromPath(tile.albedopath, STRING_ALBEDO));

				if (!string.IsNullOrEmpty(tile.normalpath))
					s_TileCategories[tile.category][tile.Id].Add(STRING_NORMAL, GetMapFromPath(tile.normalpath, STRING_NORMAL));

				if (!string.IsNullOrEmpty(tile.maskmappath))
					s_TileCategories[tile.category][tile.Id].Add(STRING_MASK, GetMapFromPath(tile.maskmappath, STRING_MASK));

				if (!string.IsNullOrEmpty(tile.emissivepath))
					s_TileCategories[tile.category][tile.Id].Add(STRING_EMISSIVE, GetMapFromPath(tile.emissivepath, STRING_EMISSIVE));
			}

			foreach (var category in s_TileCategories)
			{
				var ids = category.Value;
				s_AlbedoMaps.Clear();
				s_NormalMaps.Clear();
				s_MaskMaps.Clear();
				s_EmissiveMaps.Clear();

				foreach (var id in ids)
				{
					var maps = id.Value;

					s_AlbedoMaps.Add(id.Key, maps[STRING_ALBEDO]);

					if (maps.ContainsKey(STRING_NORMAL))
						s_NormalMaps.Add(id.Key, maps[STRING_NORMAL]);

					if (maps.ContainsKey(STRING_MASK))
						s_MaskMaps.Add(id.Key, maps[STRING_MASK]);

					if (maps.ContainsKey(STRING_EMISSIVE))
						s_EmissiveMaps.Add(id.Key, maps[STRING_EMISSIVE]);
				}

				CreateNewSRGBAtlas(s_AlbedoMaps, category.Key + STRING_ALBEDO);

				//Add Albedo atlas index numbers
				int i = 0;
				foreach (var entry in s_AlbedoMaps)
				{
					s_TileAtlasIndexes.Add(entry.Key, i);
					i++;
				}

				if (s_NormalMaps.Count > 0)
					CreateNewAtlas(s_NormalMaps, category.Key + STRING_NORMAL);
				if (s_MaskMaps.Count > 0)
					CreateNewAtlas(s_MaskMaps, category.Key + STRING_MASK);
				if (s_EmissiveMaps.Count > 0)
					CreateNewSRGBAtlas(s_EmissiveMaps, category.Key + STRING_EMISSIVE);
			}
		}

		private static Texture2D GetMapFromPath(string path, string mapType)
		{
			Texture2D tempTexture = null;
			switch (mapType)
			{
				//Texture is in sRGB format
				case STRING_ALBEDO:
				case STRING_EMISSIVE:
					tempTexture = new Texture2D(0, 0);
					break;

				//Texture is in RGBA32 (linear color space) format
				case STRING_NORMAL:
				case STRING_MASK:
					tempTexture = new Texture2D(
						0,
						0,
						TextureFormat.RGBA32,
						true,
						true
					);
					break;

				//This shouldn't ever happen.
				default:
					break;
			}

			tempTexture = FNEFileLoader.TryLoadImage(path, tempTexture);

			if (tempTexture == null)
			{

				switch (mapType)
				{
					case STRING_ALBEDO:
						tempTexture = new Texture2D(0, 0);
						FNEFileLoader.TryLoadImage(DEFAULT_ALBEDO_PATH, tempTexture);
						return tempTexture;

					case STRING_NORMAL:
						tempTexture = new Texture2D(
							0,
							0,
							TextureFormat.RGBA32,
							true,
							true
						);
						FNEFileLoader.TryLoadImage(DEFAULT_NORMAL_PATH, tempTexture);
						return tempTexture;

					case STRING_MASK:
						tempTexture = new Texture2D(
							0,
							0,
							TextureFormat.RGBA32,
							true,
							true
						);
						FNEFileLoader.TryLoadImage(DEFAULT_MASKMAP_PATH, tempTexture);
						return tempTexture;

					case STRING_EMISSIVE:
						tempTexture = new Texture2D(0, 0);
						FNEFileLoader.TryLoadImage(DEFAULT_MASKMAP_PATH, tempTexture);
						return tempTexture;

					//This shouldn't ever happen.
					default:
						break;
				}
			}

			return tempTexture;
		}

		private static void CreateNewAtlas(Dictionary<string, Texture2D> dict, string name)
		{
			var textureArray = CreateTextureArray(dict, name);
			var atlasTexture = new Texture2D(ATLAS_SIZE, ATLAS_SIZE, TextureFormat.RGBA32, true, true);

			atlasTexture.filterMode = FilterMode.Point;
			atlasTexture.wrapMode = TextureWrapMode.Mirror;

			// atlasTexture.PackTextures(textureArray, TEXTURE_PADDING, ATLAS_SIZE);

			for (int i = 0; i < textureArray.Length; i++)
			{
				atlasTexture.SetPixels32((i % TILES_PER_ROW) * TILE_SIZE, (i / TILES_PER_ROW) * TILE_SIZE, TILE_SIZE, TILE_SIZE, textureArray[i].GetPixels32(0), 0);
			}

			atlasTexture.Apply();

			s_MapAtlas.Add(name.ToLower(), atlasTexture);
		}

		private static void CreateNewSRGBAtlas(Dictionary<string, Texture2D> dict, string name)
		{
			var textureArray = CreateTextureArray(dict, name);
			var atlasTexture = new Texture2D(ATLAS_SIZE, ATLAS_SIZE);

			atlasTexture.filterMode = FilterMode.Point;
			atlasTexture.wrapMode = TextureWrapMode.Mirror;

			// atlasTexture.PackTextures(textureArray, TEXTURE_PADDING, ATLAS_SIZE);

			for (int i = 0; i < textureArray.Length; i++)
			{
				atlasTexture.SetPixels32((i % TILES_PER_ROW) * TILE_SIZE, (i / TILES_PER_ROW) * TILE_SIZE, TILE_SIZE, TILE_SIZE, textureArray[i].GetPixels32(0), 0);
			}

			atlasTexture.Apply();

			s_MapAtlas.Add(name.ToLower(), atlasTexture);
		}

		private static Texture2D[] CreateTextureArray(Dictionary<string, Texture2D> dict, string name)
		{
			string tempName = string.Empty;

			if (name.Contains(STRING_ALBEDO))
				tempName = STRING_ALBEDO;
			if (name.Contains(STRING_NORMAL))
				tempName = STRING_NORMAL;
			if (name.Contains(STRING_MASK))
				tempName = STRING_MASK;
			if (name.Contains(STRING_EMISSIVE))
				tempName = STRING_EMISSIVE;

			var array = new Texture2D[TILES_PER_ROW * TILES_PER_ROW];

			for (int i = (TILES_PER_ROW * TILES_PER_ROW) - 1; i >= dict.Count; i--)
			{

				Texture2D tempTexture;

				switch (tempName)
				{
					case STRING_ALBEDO:
						tempTexture = new Texture2D(0, 0);
						tempTexture = FNEFileLoader.TryLoadImage(DEFAULT_ALBEDO_PATH, tempTexture);
						break;

					case STRING_NORMAL:
						tempTexture = new Texture2D(
							0,
							0,
							TextureFormat.RGBA32,
							true,
							true
						);
						tempTexture = FNEFileLoader.TryLoadImage(DEFAULT_NORMAL_PATH, tempTexture);
						break;

					case STRING_MASK:
						tempTexture = new Texture2D(
							0,
							0,
							TextureFormat.RGBA32,
							true,
							true
						);
						tempTexture = FNEFileLoader.TryLoadImage(DEFAULT_MASKMAP_PATH, tempTexture);
						break;

					case STRING_EMISSIVE:
						tempTexture = new Texture2D(0, 0);
						tempTexture = FNEFileLoader.TryLoadImage(DEFAULT_MASKMAP_PATH, tempTexture);
						break;

					//this should never happen
					default:
						tempTexture = null;
						break;
				}

				array[i] = tempTexture;
			}

			int index = 0;
			foreach (var texture in dict.Values)
			{
				array[index] = texture;
				index++;
			}

			return array;
		}

		public static Texture2D GetAtlas(string mapAtlasType)
		{
			return s_MapAtlas.Keys.Contains(mapAtlasType) ? s_MapAtlas[mapAtlasType.ToLower()] : null;
		}

		public static int GetAtlasIndex(string id)
		{
			return s_TileAtlasIndexes[id];
		}

		public static List<string> GetCategoryKeys()
		{
			return s_TileCategories.Keys.ToList();
		}

		public static Texture2D GetTileAlbedo(string category, string tileId)
		{
			return s_TileCategories[category][tileId][STRING_ALBEDO];
		}

	}
}