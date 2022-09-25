using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.HoverBox
{

	public class HB_DividerLine : MonoBehaviour
	{

		public void Build(Color color)
		{
			Image line = GetComponent<Image>();

			line.color = color;
		}

		public void PostProcess()
		{
			RectTransform rt = (RectTransform)transform;
			GameObject HBMain = rt.GetComponentInParent<HB_Main>().gameObject;
			RectTransform BGRect = (RectTransform)HBMain.transform.GetChild(0);
			VerticalLayoutGroup VLG = BGRect.GetComponent<VerticalLayoutGroup>();
			rt.sizeDelta = new Vector2(VLG.preferredWidth - (VLG.padding.left + VLG.padding.right), 0);
		}
	}
}