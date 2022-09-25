using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class TileSheetUI : MonoBehaviour
	{
		public GameObject P_TileButton;
		public Transform T_TileGrid;
		public Text TXT_TileSheetName;

		public void GenerateButtons(List<TileData> tiles)
		{
			TXT_TileSheetName.text = "TILES";

			foreach (var tile in tiles)
			{
				var newButton = Instantiate(P_TileButton);

				int tileIndex = tile.textureSheetIndex;

				Image img = newButton.GetComponent<Image>();

				Texture2D texture = new Texture2D(0, 0);
					
				if (string.IsNullOrEmpty(tile.albedopath))
                {
					FNEFileLoader.TryLoadImage("Data/Xml/World/Tile/TileMaps/DefaultAlbedo.png", texture);
				} else
				{
					FNEFileLoader.TryLoadImage(tile.albedopath, texture);
				}

				Color[] Pixels = texture.GetPixels();

				int xIndex = tileIndex % 4;
				int yIndex = 3 - (tileIndex / 4);

				img.sprite = Sprite.Create(
					texture,
					new Rect(0, 0, 128, 128),
					new Vector2(0.5f, 0.5f)
				);

				newButton.GetComponent<ME_TileButton>().tileId = tile.Id;

				newButton.transform.SetParent(T_TileGrid);
			}
		}
	}
}