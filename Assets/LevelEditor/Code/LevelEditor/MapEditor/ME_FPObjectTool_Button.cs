using UnityEngine;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class ME_FPObjectTool_Button : MonoBehaviour
	{
		public ME_FPObjectHandler.HandleType handle;

		public void Update()
		{
			if (ME_FPObjectHandler.ActiveHandle == handle)
				GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f);
			else
				GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
		}

		public void OnClick()
		{
			ME_Control.ActivePaintMode = ME_Control.PaintMode.FPOBJECT_MANIPULATOR;
			ME_FPObjectHandler.ActiveHandle = handle;
		}
	}
}