using UnityEngine;
using UnityEngine.UI;

namespace FNZ.LevelEditor.LevelEditorV2
{
	public class CameraSettingsUI : MonoBehaviour
	{
		public Image IMG_EditorIcon;
		public Image IMG_GameIcon;

		public GameObject GameSetup;
		public GameObject EditorSetup;

		public void OnEditorClick()
		{
			IMG_EditorIcon.color = new Color(0.4f, 0.65f, 0.4f);
			IMG_GameIcon.color = new Color(0.35f, 0.35f, 0.35f);

			GameSetup.SetActive(false);
			EditorSetup.SetActive(true);
		}

		public void OnGameclick()
		{
			IMG_GameIcon.color = new Color(0.4f, 0.65f, 0.4f);
			IMG_EditorIcon.color = new Color(0.35f, 0.35f, 0.35f);

			GameSetup.SetActive(true);
			EditorSetup.SetActive(false);
		}
	}
}