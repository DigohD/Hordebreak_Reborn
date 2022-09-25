using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.DamageEffect 
{

	public class Fullscreen_Damage_Effect : MonoBehaviour
	{

		private RawImage FullscreenDamageEffectImage;
		private float fadeValue = 0.0f;
		private float fadeDuration = 2.0f;

	    void Start()
	    {
			FullscreenDamageEffectImage = gameObject.GetComponent<RawImage>();
			UpdateImageOpacity();
		}

	    void Update()
	    {
			if (fadeValue > 0)
			{
				fadeValue -= Time.deltaTime;
				UpdateImageOpacity();
			}
			else if (fadeValue < 0)
				fadeValue = 0;
	    }

		private void UpdateImageOpacity ()
        {
			Color oldColor = FullscreenDamageEffectImage.color;
			float newAlpha = fadeValue / fadeDuration;
			oldColor.a = newAlpha;
			FullscreenDamageEffectImage.color = oldColor;
        }
		public void TriggerFullscreenDamage ()
        {
			fadeValue = fadeDuration;
        }
	}
}