using UnityEngine;

namespace FNZ.Client.View.UI.Utils
{
	public class UI_ToggleGameObject : MonoBehaviour
	{
		public GameObject ToToggle;

		public void OnClick()
		{
			ToToggle.SetActive(!ToToggle.activeInHierarchy);
		}
	}
}