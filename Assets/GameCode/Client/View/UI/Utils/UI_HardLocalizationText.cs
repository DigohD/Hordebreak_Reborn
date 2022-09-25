using FNZ.Shared.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Utils
{
	public class UI_HardLocalizationText : MonoBehaviour
	{
		[SerializeField]
		private string m_StringRef;

		[SerializeField]
		private bool m_ForceUpperCase;

		private Text textComponent;
		private TextMeshProUGUI textMeshProTextComponent;

		void Start()
		{
			textComponent = GetComponent<Text>();
			textMeshProTextComponent = GetComponent<TextMeshProUGUI>();

			if (m_ForceUpperCase)
            {
				if (textComponent != null)
                {
					textComponent.text = Localization.GetString(m_StringRef).ToUpper();
				} else
                {
					textMeshProTextComponent.text = Localization.GetString(m_StringRef).ToUpper();
				}
            }
            else
            {
				if (textComponent != null)
				{
					textComponent.text = Localization.GetString(m_StringRef);
				}
				else
				{
					textMeshProTextComponent.text = Localization.GetString(m_StringRef);
				}
			}
		}
	}
}