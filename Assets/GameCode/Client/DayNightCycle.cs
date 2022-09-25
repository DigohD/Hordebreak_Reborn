using FNZ.Client.Net.NetworkManager;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Atmosphere;
using FNZ.Shared.Model.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FNZ.Client
{
	// THIS SCRIPT IS DEPRECATED!
	public class DayNightCycle : MonoBehaviour
	{
		public HDAdditionalLightData SunLight;
		public HDAdditionalLightData MoonLight;
		public HDAdditionalLightData RimLight;
		public Light SunLightColorTemp;
		public const int SUNRISE = 6;
		public const int SUNSET = 22;
		public float[] TimeOfDayIntensityValues;
		public float[] TimeOfNightIntensityValues;
		public bool[] SunVsMoonShadows;
		private Color[] m_TimeOfDayColorValues;
		[Space]

		//DEV_ONLY VARIABLES STARTS HERE
		[Range(0, 23)]
		public byte SliderTimeOfDay;
		private float LerpTimeInSeconds;

		private byte m_TimeOfDay = 12;
		private float m_MaxLightIntensity;

		[SerializeField]
		private Volume m_PPVolume;
		private FilmGrain m_FilmGrain;

		private void Start()
		{
			var colorTexture = (Texture2D)Resources.Load("DayStrip");
			if (colorTexture == null)
				Debug.LogError("DAYSTRIP FAILED TO LOAD");
			m_TimeOfDayColorValues = colorTexture.GetPixels();

			ClientWorldNetworkManager.d_NewHour += UpdateValues;

			for (int i = 0; i < TimeOfDayIntensityValues.Length; i++)
				m_MaxLightIntensity = m_MaxLightIntensity < TimeOfDayIntensityValues[i] ? TimeOfDayIntensityValues[i] : m_MaxLightIntensity;

			m_PPVolume.profile.TryGet<FilmGrain>(out m_FilmGrain);
		}

		private void Update()
		{
			if (!FindObjectOfType<PlayerController>())
				return;

			m_TimeSinceStart += Time.deltaTime;
			UpdateLights();
			UpdateShadows();
			UpdateRotation();
		}

		private float m_CalculationStartTime;
		private float m_TimeSinceStart;
		private void UpdateValues(byte timeOfDay)
		{
			PlayerAtmosphereSFX.PlayAmbienceBasedOnTimeOfDay(timeOfDay);

			m_CalculationStartTime = Time.time;
			m_TimeSinceStart = 0;
			m_TimeOfDay = timeOfDay;

			CalculateLightValues();
			CalculateRotationValues();
			CalculateShadowValues();
		}

		private void UpdateValuesDev(byte timeOfDay)
		{
			SunLight.color = m_TimeOfDayColorValues[timeOfDay];
			SunLight.intensity = TimeOfDayIntensityValues[timeOfDay];

			float sunDimValue = (SunLight.intensity - (m_MaxLightIntensity / 2f)) / m_MaxLightIntensity;
			sunDimValue = sunDimValue * 2;
			sunDimValue = sunDimValue > 1 ? 1 : sunDimValue;
			SunLight.shadowDimmer = sunDimValue < 0 ? 0 : sunDimValue;

			if (SunLight.intensity < 275.0f)
			{
				MoonLight.intensity = (SunLight.intensity);
				MoonLight.color = m_TimeOfDayColorValues[timeOfDay];
				MoonLight.shadowDimmer = 0.7f;
			}
			else
			{
				MoonLight.intensity = 0;
				MoonLight.shadowDimmer = 0;
			}

			SunLight.transform.localEulerAngles = new Vector3(SunLight.transform.eulerAngles.x, timeOfDay * 15);
		}


		private float m_IntensityChangeValue;
		private float m_IntensityStartIntensity;
		private float m_MoonIntensityStartIntensity;
		private Color m_startColor;
		private Color m_targetColor;
		private void CalculateLightValues()
		{
			m_IntensityChangeValue = 0;
			m_IntensityStartIntensity = SunLight.intensity;
			m_MoonIntensityStartIntensity = MoonLight.intensity;
			SunLightColorTemp.shadows = LightShadows.Soft;

			m_startColor = SunLight.color;
			m_targetColor = m_TimeOfDayColorValues[m_TimeOfDay];
		}

		private void UpdateLights()
		{
			LerpTimeInSeconds = EnvironmentShared.SECONDS_PER_HOUR;

			m_IntensityChangeValue = m_TimeSinceStart / LerpTimeInSeconds;
			SunLight.intensity = Mathf.Lerp(m_IntensityStartIntensity, TimeOfDayIntensityValues[m_TimeOfDay], m_IntensityChangeValue);

			/*float sunDimValue = (SunLight.intensity - (m_MaxLightIntensity / 2f)) / m_MaxLightIntensity;
			sunDimValue = sunDimValue * 2;
			sunDimValue = sunDimValue > 1 ? 1 : sunDimValue;
			SunLight.shadowDimmer = sunDimValue < 0 ? 0 : sunDimValue;

			SunLight.color = Color.Lerp(m_startColor, m_targetColor, m_IntensityChangeValue);
            */

			MoonLight.intensity = Mathf.Lerp(m_MoonIntensityStartIntensity, TimeOfNightIntensityValues[m_TimeOfDay], m_IntensityChangeValue);
			RimLight.intensity = MoonLight.intensity * 0.33f;
			//m_FilmGrain.intensity.value = Mathf.Lerp(m_FilmGrain.intensity.value, 0.5f * (MoonLight.intensity / 6000f), Time.deltaTime / 20f);
			//MoonLight.color = Color.Lerp(m_startColor, m_targetColor, m_IntensityChangeValue);
			//MoonLight.shadowDimmer = 0.7f;
		}

		private float m_TargetSunDimmer;
		private float m_TargetMoonDimmer;
		private void UpdateShadows()
		{
			LerpTimeInSeconds = EnvironmentShared.SECONDS_PER_HOUR;

			float sunLerp = m_TargetSunDimmer == 0 ? 1.5f : 1;
			float moonLerp = m_TargetMoonDimmer == 0 ? 1.5f : 1;
			SunLight.shadowDimmer = Mathf.Lerp(SunLight.shadowDimmer, m_TargetSunDimmer, (Time.deltaTime * sunLerp) / LerpTimeInSeconds);
			MoonLight.shadowDimmer = Mathf.Lerp(MoonLight.shadowDimmer, m_TargetMoonDimmer, (Time.deltaTime * moonLerp) / LerpTimeInSeconds);
		}

		private Vector3 m_RotationStartVector;
		private float m_VectorDistance;
		private float m_RotationTargetYValue;
		private Vector3 m_RotationTargetVector;
		private float m_RotationPercent;
		private void CalculateRotationValues()
		{
			/*m_RotationTargetYValue = m_TimeOfDay * 15f;
			if (m_TimeOfDay > 0)
				m_RotationStartVector = new Vector3(SunLight.transform.eulerAngles.x, m_RotationTargetYValue - 15);
			else
				m_RotationStartVector = new Vector3(SunLight.transform.eulerAngles.x, 345);
			m_RotationTargetVector = new Vector3(SunLight.transform.eulerAngles.x, m_RotationTargetYValue);

			m_VectorDistance = (m_RotationTargetVector - m_RotationStartVector).magnitude;*/
		}

		private void UpdateRotation()
		{
			/*float time = m_TimeSinceStart / LerpTimeInSeconds;
			SunLight.transform.localEulerAngles = AngleLerp(
				m_RotationStartVector,
				m_RotationTargetVector,
				time
			);*/
		}

		private void CalculateShadowValues()
		{
			bool sunDown;
			bool moonDown;
			if (m_TimeOfDay + 1 >= SunVsMoonShadows.Length)
			{
				sunDown = SunVsMoonShadows[m_TimeOfDay] != SunVsMoonShadows[0] && SunVsMoonShadows[m_TimeOfDay];
				moonDown = SunVsMoonShadows[m_TimeOfDay] != SunVsMoonShadows[0] && !SunVsMoonShadows[m_TimeOfDay];
			}
			else
			{
				sunDown = SunVsMoonShadows[m_TimeOfDay] != SunVsMoonShadows[m_TimeOfDay + 1] && SunVsMoonShadows[m_TimeOfDay];
				moonDown = SunVsMoonShadows[m_TimeOfDay] != SunVsMoonShadows[m_TimeOfDay + 1] && !SunVsMoonShadows[m_TimeOfDay];
			}

			if (SunVsMoonShadows[m_TimeOfDay])
			{
				SunLight.EnableShadows(true);
				MoonLight.EnableShadows(false);
				m_TargetSunDimmer = 1;
				m_TargetMoonDimmer = 0;
			}
			else
			{
				MoonLight.EnableShadows(true);
				SunLight.EnableShadows(false);
				m_TargetMoonDimmer = 0.4f;
				m_TargetSunDimmer = 0;
			}

			if (sunDown)
				m_TargetSunDimmer = 0;
			if (moonDown)
				m_TargetMoonDimmer = 0;
		}

		private Vector3 AngleLerp(Vector3 StartAngle, Vector3 FinishAngle, float t)
		{
			float xLerp = Mathf.LerpAngle(StartAngle.x, FinishAngle.x, t);
			float yLerp = Mathf.LerpAngle(StartAngle.y, FinishAngle.y, t);
			float zLerp = Mathf.LerpAngle(StartAngle.z, FinishAngle.z, t);
			Vector3 Lerped = new Vector3(xLerp, yLerp, zLerp);
			return Lerped;
		}

		private void OnDestroy()
		{
			ClientWorldNetworkManager.d_NewHour -= UpdateValues;
		}

	}
}