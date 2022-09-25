using FNZ.Client.View.Audio;
using FNZ.Client.View.UI.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.IngameMenu
{
	public class UI_IngameMenu : MonoBehaviour
	{
		private AudioManager m_AudioManager;

		[SerializeField] private GameObject T_Main;
		[SerializeField] private GameObject T_Options;
		[SerializeField] private GameObject T_ExitToMenuConfirm;
		[SerializeField] private GameObject T_ExitGameConfirm;

		private void OnEnable()
		{
			T_Main.gameObject.SetActive(true);
			T_Options.gameObject.SetActive(false);
			T_ExitToMenuConfirm.gameObject.SetActive(false);
			T_ExitGameConfirm.gameObject.SetActive(false);
		}

		private void Start()
		{
			m_AudioManager = FindObjectOfType<AudioManager>();
		}

		public void ExitGame()
		{
			GameClient.NetAPI.CMD_Entity_ClientDisconnectFromServer();

#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
		}

		public void ExitGameBack()
		{
			T_ExitGameConfirm.SetActive(false);
			T_Main.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_close");
		}

		public void ExitGameOpen()
		{
			T_Main.SetActive(false);
			T_ExitGameConfirm.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_open");
		}

		public void ExitToMenu()
		{
			//TO_DO: Unload everything related to game scenes and load "StartMenu".

			GameClient.NetAPI.CMD_Entity_ClientDisconnectFromServer();
			AudioManager.Instance.PlayMusic("intro_music", 1);
			AudioManager.Instance.PlayAmbience("night", 1);
			SceneManager.LoadSceneAsync("StartMenu");
		}

		public void ExitToMenuBack()
		{
			T_ExitToMenuConfirm.SetActive(false);
			T_Main.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_close");
		}

		public void ExitToMenuOpen()
		{
			T_Main.SetActive(false);
			T_ExitToMenuConfirm.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_open");
		}

		public void OptionsMenuBack()
		{
			T_Options.SetActive(false);
			T_Main.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_close");
		}

		public void OptionsMenuOpen()
		{
			T_Main.SetActive(false);
			T_Options.SetActive(true);

			UIManager.Instance.PlaySound("sfx_ui_open");
		}

		public void SetVolumeAmbience(float value)
		{
			m_AudioManager.SetVolumeAmbience(value);
		}

		public void SetVolumeMaster(float value)
		{
			m_AudioManager.SetVolumeMaster(value);
		}

		public void SetVolumeMusic(float value)
		{
			m_AudioManager.SetVolumeMusic(value * 0.75f);
		}

		public void SetVolumeSFX(float value)
		{
			m_AudioManager.SetVolumeSFX(value);
		}

		public void ToggleAmbience(bool value)
		{
			m_AudioManager.ToggleAmbience(value);
		}

		public void ToggleMaster(bool value)
		{
			m_AudioManager.ToggleMaster(value);
		}

		public void ToggleMusic(bool value)
		{
			m_AudioManager.ToggleMusic(value);
		}

		public void ToggleSFX(bool value)
		{
			m_AudioManager.ToggleSFX(value);
		}

	}
}