using UnityEngine;

namespace FNZ.LevelEditor
{
	public class ME_SizeButton : MonoBehaviour
	{

		public ME_Control.BrushSize size;

		public void OnClick()
		{
			ME_Control.activeBrushSize = size;
		}
	}
}
