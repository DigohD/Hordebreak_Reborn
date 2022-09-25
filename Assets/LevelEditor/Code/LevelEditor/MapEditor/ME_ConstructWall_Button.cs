using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class ME_ConstructWall_Button : MonoBehaviour
	{
		public void Update()
		{
			if (ME_Control.activePaintMode == ME_Control.PaintMode.WALL_CONSTRUCT)
				GetComponent<Image>().color = new Color(0.6f, 1f, 0.6f);
			else
				GetComponent<Image>().color = new Color(0.65f, 0.65f, 0.65f);
		}

		public void OnClick()
		{
			ME_Control.activePaintMode = ME_Control.PaintMode.WALL_CONSTRUCT;
			ME_Control.activeSnapMode = ME_Control.SnapMode.CORNER;
			if (string.IsNullOrEmpty(ME_Control.SelectedWall))
			{
				ME_Control.SelectedWall = "eo_concrete_wall";
			}
		}
	}
}