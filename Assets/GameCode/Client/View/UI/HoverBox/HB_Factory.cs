using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.HoverBox
{
	public class HB_Factory
	{
		public static GameObject activeHoverBox;

		public static object sourceObject;

		private static HB_Pool m_HBPartsPool;

		public struct IconTextItem
		{
			public string text;
			public Sprite sprite;

			public IconTextItem(string text, Sprite sprite)
			{
				this.text = text;
				this.sprite = sprite;
			}
		}
		
		public struct IconTextIconItem
		{
			public string text;
			public Sprite startIcon;
			public Sprite endIcon;

			public IconTextIconItem(string text, Sprite startIcon, Sprite endIcon)
			{
				this.text = text;
				this.startIcon = startIcon;
				this.endIcon = endIcon;
			}
		}

		static HB_Factory()
		{
			m_HBPartsPool = GameObject.FindObjectOfType<HB_Pool>();
		}

		public static HB_Main CreateNewHoverBox(Canvas parentCanvas, object newSourceObject)
		{
			if (Object.ReferenceEquals(newSourceObject, sourceObject))
				return null;

			GameObject newBox = m_HBPartsPool.GetHoverBox();

			DestroyHoverbox();

			sourceObject = newSourceObject;

			activeHoverBox = newBox;
			activeHoverBox.transform.SetParent(parentCanvas.transform);

			return newBox.GetComponent<HB_Main>();
		}

		public static void DestroyHoverbox()
		{
			if (activeHoverBox != null)
			{
				foreach (var obj in activeHoverBox.GetComponentsInChildren<HB_Text>())
					m_HBPartsPool.RecycleText(obj.gameObject);
				foreach (var obj in activeHoverBox.GetComponentsInChildren<HB_DividerLine>())
					m_HBPartsPool.RecycleDividerLine(obj.gameObject);
				foreach (var obj in activeHoverBox.GetComponentsInChildren<HB_IconTextRow>())
					m_HBPartsPool.RecycleIconTextRow(obj.gameObject);
				foreach (var obj in activeHoverBox.GetComponentsInChildren<HB_IconTextIconRow>())
					m_HBPartsPool.RecycleIconTextIconRow(obj.gameObject);

				GameObject.Destroy(activeHoverBox);
			}

			sourceObject = null;
		}

		public static Transform BuildHBText(Transform parent, HB_Text.TextStyle style, Color color, string content)
		{
			GameObject title = m_HBPartsPool.GetText();

			title.transform.SetParent(parent, false);
			title.GetComponent<HB_Text>().Build(style, color, content);
			RectTransform rt = (RectTransform)title.transform;
			if (rt.GetComponent<TextMeshProUGUI>().preferredWidth > 300)
			{
				rt.sizeDelta = new Vector2(300, 0);
			}
			else
			{
				rt.sizeDelta = new Vector2(rt.GetComponent<TextMeshProUGUI>().preferredWidth, 0);
			}
			return title.transform;
		}

		public static Transform BuildHBDividerLine(RectTransform parent, Color color)
		{
			GameObject line = m_HBPartsPool.GetDividerLine();

			line.transform.SetParent(parent, false);
			line.GetComponent<HB_DividerLine>().Build(color);
			RectTransform rt = (RectTransform)line.transform;
			rt.sizeDelta = Vector2.zero;

			return line.transform;
		}

		public static Transform BuildIconTextRow(RectTransform parent, IconTextItem[] data, Color textColor)
		{
			GameObject row = m_HBPartsPool.GetIconTextRow();

			row.transform.SetParent(parent, false);
			row.GetComponent<HB_IconTextRow>().Build(data, textColor);
			RectTransform rt = (RectTransform)row.transform;

			return row.transform;
		}
		
		public static Transform BuildIconTextIconRow(RectTransform parent, IconTextIconItem[] data, Color textColor)
		{
			GameObject row = m_HBPartsPool.GetIconTextIconRow();

			row.transform.SetParent(parent, false);
			row.GetComponent<HB_IconTextIconRow>().Build(data, textColor);
			RectTransform rt = (RectTransform)row.transform;

			return row.transform;
		}
		
		public static Transform BuildRepeatingIconRow(RectTransform parent, Sprite icon, byte amount)
		{
			GameObject row = m_HBPartsPool.GetRepeatingIconRow();

			row.transform.SetParent(parent, false);
			row.GetComponent<HB_RepeatingIconRow>().Build(icon, amount);
			RectTransform rt = (RectTransform) row.transform;

			return row.transform;
		}

		/*public static Transform BuildLootInfo(
	        RectTransform parent,
	        Item data,
            InventoryComponentClient container,
	        Color textColor
	    )
	    {
	        GameObject lootInfo = GameObject.Instantiate(P_LootInfo);
	        lootInfo.transform.SetParent(parent);
	        lootInfo.GetComponent<HB_LootInfo>().Build(data, textColor);
	        lootInfo.GetComponent<HB_LootInfo>().onHoverBoxLootClick += container.OnHoverBoxLootClick;
	        RectTransform rt = (RectTransform) lootInfo.transform;
	
	        return lootInfo.transform;
	    }*/

		/*public static Transform BuildModInfo(
	        RectTransform parent,
	        Item data,
            InventoryComponentClient container,
	        Color textColor
	    )
	    {
	        GameObject modInfo = GameObject.Instantiate(P_ModInfo);
	        modInfo.transform.SetParent(parent);
	        modInfo.GetComponent<HB_ModInfo>().Build(data, textColor);
	
	        RectTransform rt = (RectTransform)modInfo.transform;
	
	        return modInfo.transform;
	    }*/
	}
}