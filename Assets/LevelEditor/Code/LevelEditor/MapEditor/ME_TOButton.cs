using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class ME_TOButton : MonoBehaviour
	{

		public ME_Control.SnapMode SnapMode;
		public string toType;

		public void Start()
		{
			var data = DataBank.Instance.GetData<FNEEntityData>(toType);
		}

		public void Update()
		{
			if (ME_Control.activePaintMode == ME_Control.PaintMode.TILE_OBJECT && ME_Control.SelectedTileObject == toType)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.activeSnapMode = SnapMode;
			ME_Control.activePaintMode = ME_Control.PaintMode.TILE_OBJECT;
			ME_Control.SelectedTileObject = toType;

			//if (ME_Control.SelectedTileObject == 21 || ME_Control.SelectedTileObject == 23 || ME_Control.SelectedTileObject == 24 || ME_Control.SelectedTileObject == 27 || ME_Control.SelectedTileObject == 26)
			//    ME_Control.G_PreviewCube.GetComponent<MeshRenderer>().material.color = Color.blue;
			//else if (ME_Control.SelectedTileObject == 22 || ME_Control.SelectedTileObject == 25 || ME_Control.SelectedTileObject == 28)
			//    ME_Control.G_PreviewCube.GetComponent<MeshRenderer>().material.color = new Color(1,1,0);
			//else
			//    ME_Control.G_PreviewCube.GetComponent<MeshRenderer>().material.color = Color.white;
		}
	}
}