using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.HoverBox
{

	public class HB_Text : MonoBehaviour
	{

		public enum TextStyle
		{
			HEADER,
			SUB_HEADER,
			BREAD_TEXT
		}

		public void Build(TextStyle style, Color color, string content)
		{
			var text = GetComponent<TextMeshProUGUI>();

			int fontSize = 12;
			switch (style)
			{
				case TextStyle.HEADER:
					fontSize = 18;
					break;
				case TextStyle.SUB_HEADER:
					fontSize = 16;
					break;
				case TextStyle.BREAD_TEXT:
					fontSize = 12;
					break;
				default:
					break;
			}

			text.fontSize = fontSize;
			text.color = color;
			text.text = content;
		}
	}
}