using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.Input
{
	public class InputKeybindsConnector : MonoBehaviour
	{
		private InputKeybinds m_Keybinds;
		private Button m_Button;

		private void Start()
		{
			m_Keybinds = FindObjectOfType<InputKeybinds>();
			m_Button = GetComponentInChildren<Button>();

			m_Keybinds.SetButtonText(transform);
		}

		public void NewKeyBind()
		{
			m_Keybinds.OnNewKeybind(m_Button);
		}
	}
}