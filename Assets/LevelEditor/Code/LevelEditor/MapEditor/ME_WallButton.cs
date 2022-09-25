using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.LevelEditor
{
	public class ME_WallButton : MonoBehaviour
	{

		public string WallType;

		public void Start()
		{
			var data = DataBank.Instance.GetData<FNEEntityData>(WallType);
		}

		public void Update()
		{
			if ((ME_Control.activePaintMode == ME_Control.PaintMode.WALL || ME_Control.activePaintMode == ME_Control.PaintMode.WALL_CONSTRUCT)
				&& ME_Control.SelectedWall == WallType)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.activePaintMode = ME_Control.PaintMode.WALL;
			ME_Control.SelectedWall = WallType;
		}
	}
}