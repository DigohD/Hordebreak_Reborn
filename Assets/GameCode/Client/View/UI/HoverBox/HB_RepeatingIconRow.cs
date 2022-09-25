using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.HoverBox 
{

	public class HB_RepeatingIconRow : MonoBehaviour
	{
		public GameObject P_RepeatingIconItem;

		public void Build(Sprite icon, byte amount)
		{
			if (amount > 5)
			{
				amount = 5;
			}
			else if (amount < 0)
			{
				amount = 0;
			}

			GameObject item = Instantiate(P_RepeatingIconItem);
			item.transform.SetParent(transform);
			for (int i = 0; i < 5; i++)
			{
				var child = item.transform.GetChild(i).gameObject;
				child.SetActive(i < amount);
				child.GetComponent<Image>().sprite = icon;
			}
			
			((RectTransform)transform).sizeDelta = new Vector2(120, 20);
		}
	}
}