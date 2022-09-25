using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Rooms
{

	public class RoomPropertyEntry : MonoBehaviour
	{
		public Text TXT_Name;
		public Image IMG_Icon;

		public void Render(RoomPropertyData data, int level)
		{
			IMG_Icon.sprite = SpriteBank.GetSprite(data.iconRef);
			TXT_Name.color = Color.cyan;

			if (level == 1)
			{
				TXT_Name.text = Localization.GetString("room_property_half") + " " + Localization.GetString(data.displayNameRef);
			}
			else if(level > 1)
			{
				TXT_Name.text = Localization.GetString("room_property_full") + " " + Localization.GetString(data.displayNameRef);
			}

			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
		}
	}
}