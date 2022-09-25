using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.HoverBox 
{

	public class HB_IconTextIconRow : MonoBehaviour
	{
		public GameObject P_IconTextIconItem;

		public void Build(HB_Factory.IconTextIconItem[] data, Color textColor)
		{
			foreach (HB_Factory.IconTextIconItem itii in data)
			{
				GameObject item = Instantiate(P_IconTextIconItem);
				item.transform.SetParent(transform);
				item.transform.GetChild(0).GetComponentInChildren<Image>().sprite = itii.startIcon;
				item.GetComponentInChildren<TextMeshProUGUI>().text = itii.text;
				item.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
				item.transform.GetChild(2).GetComponentInChildren<Image>().sprite = itii.endIcon;
			}

			((RectTransform)transform).sizeDelta = new Vector2(data.Length * 80, 20);
		}
	}
}