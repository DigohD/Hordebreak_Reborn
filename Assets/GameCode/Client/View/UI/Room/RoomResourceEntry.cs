using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.World;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Rooms
{
	public class RoomResourceEntry : MonoBehaviour
	{
		public Text TXT_Name;
		public Text TXT_Amount;
		public Image IMG_Icon;

		public void Render(RoomResourceData data, int amount)
		{
			// TXT_Name.text = Localization.GetString(data.nameRef);
			IMG_Icon.sprite = SpriteBank.GetSprite(data.iconRef);

			TXT_Amount.text = "" + amount;
			TXT_Amount.color = Color.yellow;

			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
		}
	}
}