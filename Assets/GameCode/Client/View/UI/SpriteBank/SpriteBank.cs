using FNZ.Shared.Model;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Sprites;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.UI.Sprites
{
	public class SpriteBank
	{
		private static Dictionary<string, Dictionary<string, Texture2D>> s_Categories = new Dictionary<string, Dictionary<string, Texture2D>>();

		private static List<SpriteData> s_AllSpritesData = DataBank.Instance.GetAllDataIdsOfType<SpriteData>();
		private static List<Texture2D> s_AtlasList = new List<Texture2D>();
		private static List<Rect[]> s_RectArrayList = new List<Rect[]>();

		private const short m_AtlasSize = 4096;

		private const byte m_TexturePadding = 1;

		public static bool Generated_Icons_Done = false;

		static SpriteBank()
		{
			var generatedIcons = ItemPhotoBooth.GeneratedIcons;

			//foreach dataDef in m_AllSpritesData, add categories to m_Categories and entries to it's corresponding dictionary.
			for (int i = 0; i < s_AllSpritesData.Count; i++)
			{
				var dataDef = s_AllSpritesData.ElementAt(i);

				if (!s_Categories.ContainsKey(dataDef.spriteCategory))
					s_Categories.Add(dataDef.spriteCategory, new Dictionary<string, Texture2D>());

				var categoryDict = s_Categories[dataDef.spriteCategory];
				categoryDict.Add(dataDef.Id, GetTextureFromDef(dataDef.spritePath));
			}

			s_Categories.Add("FNE_GENERATED_ICONS", new Dictionary<string, Texture2D>());
			foreach (var key in generatedIcons.Keys)
			{
				var png = generatedIcons[key].EncodeToPNG();

				Texture2D tempTexture = new Texture2D(0, 0);
				tempTexture.LoadImage(png);

				s_Categories["FNE_GENERATED_ICONS"].Add(key + "_icon", tempTexture);
                DataBank.Instance.GetData<ItemData>(key).iconRef = key + "_icon";

				// File.WriteAllBytes(Application.dataPath + "/" + key + ".png", png);
			}

			//foreach category in m_Categories, extract dictionary and add their textures to array before creating an atlas from them.
			for (int i = 0; i < s_Categories.Count; i++)
			{
				var category = s_Categories.ElementAt(i);
				var dict = category.Value;

				Texture2D[] textureArray = new Texture2D[dict.Count];

				for (int j = 0; j < dict.Count; j++)
				{
					textureArray[j] = dict.ElementAt(j).Value;
				}

				CreateNewAtlas(textureArray);
			}
		}



		private static Texture2D GetTextureFromDef(string path)
		{
			Texture2D tempTexture = new Texture2D(0, 0);
			tempTexture.LoadImage(File.ReadAllBytes(Application.streamingAssetsPath + "/" + path));

			return tempTexture;
		}

		static int count = 0;
		private static void CreateNewAtlas(Texture2D[] textures)
		{
			var atlasTexture = new Texture2D(m_AtlasSize, m_AtlasSize);
			Rect[] rects = atlasTexture.PackTextures(textures, m_TexturePadding, m_AtlasSize);

			var oldArray = atlasTexture.GetPixels32();
			var newPixelArray = new Color32[oldArray.Length];
			for (int i = 0; i < newPixelArray.Length; i++)
			{
				if (oldArray[i].a == 0)
				{
					newPixelArray[i] = new Color32(0, 0, 0, 0);
				}
				else
				{
					newPixelArray[i] = oldArray[i];
				}
			}
			atlasTexture.SetPixels32(newPixelArray);
			atlasTexture.Apply();

			s_AtlasList.Add(atlasTexture);
			s_RectArrayList.Add(rects);

			File.WriteAllBytes(Application.dataPath + "/" + count++ + ".png", atlasTexture.EncodeToPNG());
		}

		public static Sprite GetSprite(string spriteId)
		{
			//foreach category in m_Categories, extract dictionary and find out if id matches a stored one.
			//if it does, create a sprite and return it, otherwise return null.
			for (int i = 0; i < s_Categories.Count; i++)
			{
				var category = s_Categories.ElementAt(i);
				var dict = category.Value;

				for (int j = 0; j < dict.Count; j++)
				{
					if (dict.ElementAt(j).Key == spriteId)
					{
						UnityEngine.Sprite sprite = UnityEngine.Sprite.Create(s_AtlasList[i], s_RectArrayList[i][j], new Vector2(0, 0));

						var w = sprite.texture.width;
						var h = sprite.texture.height;

						return UnityEngine.Sprite.Create(
							s_AtlasList[i],
							new Rect(
								sprite.rect.x * w,
								sprite.rect.y * h,
								sprite.rect.width * w,
								sprite.rect.height * h
							),
							new Vector2(0, 0)
						);
					}
				}
			}

			return null;
		}
	}
}