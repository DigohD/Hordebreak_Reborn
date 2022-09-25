using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Notifications
{

	public class UI_LocationText : MonoBehaviour
	{
		[SerializeField]
		private Text TXT_LocationName;

		[SerializeField]
		private Text TXT_LocationNameShadow;

		private Color m_TextColor;
		private Color m_TextShadowColor;

		private float m_DimValue = 0;
		private float m_DimDuration = 3.0f;
		private float m_FadeDuration = 0.8f;

		private FNEEntity m_Player;

		private void Start()
		{
			m_TextColor = TXT_LocationName.color;
			m_TextColor.a = 0f;
			TXT_LocationName.color = m_TextColor;

			m_TextShadowColor = TXT_LocationNameShadow.color;
			m_TextShadowColor.a = 0f;
			TXT_LocationNameShadow.color = m_TextShadowColor;
		}

		void Update()
		{
			if(m_Player == null && GameClient.LocalPlayerEntity == null)
            {
				return;
            } else if (m_Player == null)
            {
				m_Player = GameClient.LocalPlayerEntity;
				m_Player.GetComponent<PlayerComponentClient>().d_OnCurrentSiteChanged += OnEnterSite;
			}

			if (m_DimValue > 0)
            {
				m_DimValue -= Time.deltaTime;
				if (m_DimValue < 0)
					m_DimValue = 0;

				float newAlpha;

				// Fade out
				if (m_DimValue < m_FadeDuration)
                {
					newAlpha = m_DimValue / m_FadeDuration;
				}

				// Fade in
				else if (m_DimValue > (m_DimDuration - m_FadeDuration))
				{
					newAlpha = (m_DimDuration - m_DimValue) / m_FadeDuration;
				}

				else
                {
					newAlpha = 1.0f;
                }

				// change text color
				m_TextColor = TXT_LocationName.color;
				m_TextColor.a = newAlpha;
				TXT_LocationName.color = m_TextColor;

				// change background image color
				m_TextShadowColor = TXT_LocationNameShadow.color;
				m_TextShadowColor.a = newAlpha;
				TXT_LocationNameShadow.color = m_TextShadowColor;
			}
		}

		private void OnEnterSite (bool entered, string id)
        {
			if (!entered)
            {
				return;
            }

			var siteData = DataBank.Instance.GetData<SiteData>(id);

			if (!siteData.showOnMap)
			{
				return;
			}
			
			var typeTranslated = Localization.GetString(siteData.siteTypeRef);
			var nameTranslated = Localization.GetString(siteData.siteName);

			UpdateLocationText(nameTranslated + " " + typeTranslated);
			EnableText();
		}

		private void UpdateLocationText(string newText)
		{
			TXT_LocationName.text = newText;
			TXT_LocationNameShadow.text = newText;
		}

		private void EnableText()
        {
			m_DimValue = m_DimDuration;
        }
	}
}