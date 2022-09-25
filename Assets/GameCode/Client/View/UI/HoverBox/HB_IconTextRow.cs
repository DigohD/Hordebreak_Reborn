using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox
{

	public class HB_IconTextRow : MonoBehaviour
	{

		public GameObject P_IcontextItem;

		public void Build(IconTextItem[] data, Color textColor)
		{
			foreach (IconTextItem iti in data)
			{
				GameObject item = Instantiate(P_IcontextItem);
				item.transform.SetParent(transform, false);
				item.GetComponentInChildren<Image>().sprite = iti.sprite;
				item.GetComponentInChildren<TextMeshProUGUI>().text = iti.text;
				item.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
			}

			((RectTransform)transform).sizeDelta = new Vector2(data.Length * 80, 20);
		}
	}
}