using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Client.View.UI.StartMenu
{
	public class Lightflicker : MonoBehaviour
	{
		public Light lightSource;

		float baseIntensity;
		float minIntensity, maxIntensity;
		float targetIntensity = 3;

		private void Start()
		{
			baseIntensity = lightSource.intensity;
			minIntensity = baseIntensity * 0.5f;
			maxIntensity = baseIntensity * 1.2f;
		}

		float timer = 0;
		void Update()
		{
			timer += Time.deltaTime;
			if (timer > 0.05f)
			{
				targetIntensity = FNERandom.GetRandomFloatInRange(minIntensity, maxIntensity);
				timer = -FNERandom.GetRandomFloatInRange(0.02f, 0.07f);
			}

			lightSource.intensity = Mathf.Lerp(lightSource.intensity, targetIntensity, 10 * Time.deltaTime);
		}
	}

}
