using UnityEngine;
using UnityEngine.UI;

namespace FNZ.LevelEditor
{
	public class ME_FPObjectManipulation_Button : MonoBehaviour
	{
		public void Update()
		{
			if (ME_Control.activePaintMode == ME_Control.PaintMode.FPOBJECT_MANIPULATOR)
				GetComponent<Image>().color = new Color(0.6f, 1f, 0.6f);
			else
				GetComponent<Image>().color = new Color(0.65f, 0.65f, 0.65f);
		}

		public void OnClick()
		{
			ME_Control.activePaintMode = ME_Control.PaintMode.FPOBJECT_MANIPULATOR;
		}
	}
}