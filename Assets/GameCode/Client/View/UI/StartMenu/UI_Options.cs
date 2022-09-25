using FNZ.Client.View.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.StartMenu
{
	public class UI_Options : MonoBehaviour
	{
		private List<Resolution> m_Resolutions = new List<Resolution>();

		public Dropdown ResolutionDropdown;
		public Dropdown QualityDropdown;
		public Dropdown FullscreenDropdown;

		public Slider MasterSlider;
		public Toggle MasterToggle;
		public Slider AmbienceSlider;
		public Toggle AmbienceToggle;
		public Slider MusicSlider;
		public Toggle MusicToggle;
		public Slider SfxSlider;
		public Toggle SfxToggle;
		public Slider UIScaleSlider;

		public GameObject Resolution;
		public GameObject Audio;
		public GameObject Gameplay;

		public GameObject Canvas;

		private AudioManager am;

		private float UIScaleWidthRef;
		private float UIScaleHeightRef;
		private static float UIScale = 1;

		private void OnEnable()
		{
			CloseAllChildren();
			Resolution.SetActive(true);
		}

		private void Start()
		{
			am = FindObjectOfType<AudioManager>();

			//Screen Resolution
			foreach (var res in Screen.resolutions)
			{
				if (res.width >= 1280 && res.height >= 720)
					m_Resolutions.Add(res);
			}

			m_Resolutions.Reverse(); //This is in order to have higher resolutions at the top of the list.

			ResolutionDropdown.ClearOptions();

			var resolutionStrings = new List<string>();
			int currentResolutionIndex = 0;
			for (int i = 0; i < m_Resolutions.Count; i++)
			{
				resolutionStrings.Add($"{m_Resolutions[i].width} x {m_Resolutions[i].height}, {m_Resolutions[i].refreshRate}Hz");

				if (m_Resolutions[i].width == Screen.currentResolution.width && m_Resolutions[i].height == Screen.currentResolution.height)
				{
					if (m_Resolutions[i].refreshRate >= Screen.currentResolution.refreshRate)
						currentResolutionIndex = i;
				}
			}

			ResolutionDropdown.AddOptions(resolutionStrings);
			ResolutionDropdown.value = currentResolutionIndex;
			ResolutionDropdown.RefreshShownValue();

			//Graphics Quality
			QualitySettings.SetQualityLevel(3);
			QualityDropdown.value = 3;
			QualityDropdown.RefreshShownValue();

			//Fullscreen
			FullscreenDropdown.value = (int)Screen.fullScreenMode;
			FullscreenDropdown.RefreshShownValue();

			//Audio
			MasterSlider.value = am.GetVolumeMaster();
			AmbienceSlider.value = am.GetVolumeAmbience();
			MusicSlider.value = am.GetVolumeMusic();
			SfxSlider.value = am.GetVolumeSfx();
			MasterToggle.isOn = am.GetToggleMaster();
			AmbienceToggle.isOn = am.GetToggleAmbience();
			MusicToggle.isOn = am.GetToggleMusic();
			SfxToggle.isOn = am.GetToggleSfx();

		}

		public void CloseAllChildren()
		{
			Resolution.SetActive(false);
			Audio.SetActive(false);
			Gameplay.SetActive(false);
		}

		public void SetGraphicsQuality(int index)
		{
			QualitySettings.SetQualityLevel(index);
		}

		public void SetResolution(int resolutionIndex)
		{
			Screen.SetResolution(
				m_Resolutions[resolutionIndex].width,
				m_Resolutions[resolutionIndex].height,
				Screen.fullScreen
				);
		}

		public void SetFullscreenMode(int fullScreenMode)
		{
			switch (fullScreenMode)
			{
				case 0:
					Cursor.lockState = CursorLockMode.Confined;
					Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
					break;

				case 1:
					Cursor.lockState = CursorLockMode.None;
					Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
					break;

				case 2:
					Cursor.lockState = CursorLockMode.None;
					Screen.fullScreenMode = FullScreenMode.Windowed;
					break;

				default:
					Cursor.lockState = CursorLockMode.Confined;
					Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
					break;
			}
		}

		public void SetMasterVolume(float vol)
		{
			am.SetVolumeMaster(vol);
		}

		public void SetMusicVolume(float vol)
		{
			am.SetVolumeMusic(vol * 0.75f);
		}

		public void SetSFXVolume(float vol)
		{
			am.SetVolumeSFX(vol);
		}

		public void SetAmbienceVolume(float vol)
		{
			am.SetVolumeAmbience(vol);
		}

		public void ToggleMasterVolume(bool value)
		{
			am.ToggleMaster(value);
		}

		public void ToggleMusicVolume(bool value)
		{
			am.ToggleMusic(value);
		}

		public void ToggleSFXVolume(bool value)
		{
			am.ToggleSFX(value);
		}

		public void ToggleAmbienceVolume(bool value)
		{
			am.ToggleAmbience(value);
		}
		public void UpdateUIScaleSliderValue()
		{
			// 50% scale
			if (UIScale == 0.8f) { UIScaleSlider.value = 0; }

			// 100% scale
			else if (UIScale == 1.0f) { UIScaleSlider.value = 1; }

			// 150% scale
			else if (UIScale == 1.2f) { UIScaleSlider.value = 2; }

			// 200% scale
			else if (UIScale == 1.4f) { UIScaleSlider.value = 3; }
		}

		public void SetUIScaleValueChanged ()
        {
			// 50% scale
			if (UIScaleSlider.value == 0) { UIScale = 0.8f; }

			// 100% scale
			else if (UIScaleSlider.value == 1) { UIScale = 1.0f; }

			// 150% scale
			else if (UIScaleSlider.value == 2) { UIScale = 1.2f; ; }

			// 200% scale
			else if (UIScaleSlider.value == 3) { UIScale = 1.4f; ; }
		}

		public void SetUIScalePointerUp()
		{
			CanvasScaler CanvasScaler = Canvas.GetComponent<CanvasScaler>();
			CanvasScaler.referenceResolution = new Vector2((UIScaleWidthRef / UIScale), (UIScaleHeightRef / UIScale));
		}

		public static float GetUIScale() { return UIScale; }
		public static void SetUIScale( float newValue) { UIScale = newValue; }

		public float GetUIScaleWidthRef() { return UIScaleWidthRef; }
		public void SetUIScaleWidthRef(float newValue) { UIScaleWidthRef = newValue; }

		public float GetUIScaleHeightRef() { return UIScaleHeightRef; }
		public void SetUIScaleHeightRef(float newValue) { UIScaleHeightRef = newValue; }
	}
}