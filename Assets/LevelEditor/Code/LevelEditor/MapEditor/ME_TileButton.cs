using FNZ.Client.Utils;
using FNZ.Client.View.World;
using FNZ.Shared.Model;
using FNZ.Shared.Model.World.Tile;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.LevelEditor
{
	public class ME_TileButton : MonoBehaviour
	{

		public string tileId;

		public void Start()
		{
			var tileData = DataBank.Instance.GetData<TileData>(tileId);
			string tileSheetId = tileData.textureSheet;

			Image i = GetComponent<Image>();

			Texture2D t = ClientTileSheetPacker.GetTileAlbedo(tileData.category, tileData.Id);

			Color[] Pixels = t.GetPixels();

			int xIndex = tileData.textureSheetIndex % 4;
			int yIndex = 3 - (tileData.textureSheetIndex / 4);

			i.sprite = Sprite.Create(
				t,
				new Rect(0, 0, 128, 128),
				new Vector2(0.5f, 0.5f)
			);
		}

		public void OnClick()
		{
			ME_Control.activeSnapMode = ME_Control.SnapMode.TILE;
			ME_Control.activePaintMode = ME_Control.PaintMode.TILE;
			ME_Control.SelectedTile = tileId;
		}
	}
}