using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.World.Environment;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Environment
{
	public class EnvironmentEntry : MonoBehaviour
	{
		public Text TXT_Name;
		public Text TXT_Amount;
		public Image IMG_Icon;

		public void Render(EnvironmentData data, int value, int amount)
		{
			//TXT_Name.text = Localization.GetString(data.nameRef);
			TXT_Amount.text = "" + value;
			IMG_Icon.sprite = SpriteBank.GetSprite(data.iconRef);
		}
	}
}