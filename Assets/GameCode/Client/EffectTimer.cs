using FNZ.Client.View.Manager;
using FNZ.Shared.Model.VFX;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace FNZ.Client
{
	public class EffectTimer : MonoBehaviour
	{
		private static GameObject s_Parent;

		[SerializeField]
		private float m_OriginalDuration;
		private float m_Timer;
		private float m_LightTimer;

		private HDAdditionalLightData m_Light;
		private float m_LightFadeTime;
		private float m_StartIntensity;
		private Color m_StartColor;
		private Color m_TargetColor;

		private bool m_RecycleWhenDead = true;

		private void OnEnable()
		{
			m_Timer = m_OriginalDuration;

			
			if (m_Light != null)
			{
				m_Light.gameObject.SetActive(true);
				m_LightTimer = m_LightFadeTime;
				m_Light.color = m_StartColor;
				m_Light.intensity = m_StartIntensity;
			}
		}

		private void Start()
		{
			if (s_Parent)
				gameObject.transform.SetParent(s_Parent.transform);
			else
			{
				s_Parent = GameObject.Find("Effects");
				s_Parent = new GameObject();
				s_Parent.transform.name = "Effects";
				gameObject.transform.SetParent(s_Parent.transform);
			}
		}

		void Update()
		{
			m_Timer -= Time.deltaTime;
			m_LightTimer -= Time.deltaTime;

			if (m_Timer <= 0 && m_RecycleWhenDead)
				GameObjectPoolManager.RecycleObject(gameObject.name, gameObject);

			if(m_Light != null && m_Light.gameObject.activeInHierarchy && m_LightTimer >= 0)
            {
				var lerpVal = (m_LightFadeTime - m_LightTimer) / m_LightFadeTime;
				var color = Color.Lerp(m_StartColor, m_TargetColor, lerpVal);
				m_Light.color = color;
				m_Light.intensity = Mathf.Lerp(m_StartIntensity, 0, lerpVal);
            }
            else if(m_Light != null)
            {
				m_Light.gameObject.SetActive(false);
			}
		}

		public void Init(float duration, VFXLightData lightData, bool recycleWhenDead = true)
		{
			m_OriginalDuration = duration;
			m_Timer = duration;

			if(lightData != null)
            {
				m_LightFadeTime = lightData.fadeTime;
				m_LightTimer = m_LightFadeTime;
				ColorUtility.TryParseHtmlString(lightData.startColor, out m_StartColor);
				ColorUtility.TryParseHtmlString(lightData.endColor, out m_TargetColor);
				m_StartIntensity = lightData.intensity;
				m_Light = GetComponentInChildren<HDAdditionalLightData>();
			}
		}

		public void BlockRecyclingWhenDead()
        {
            m_RecycleWhenDead = false;
        }

	}
}