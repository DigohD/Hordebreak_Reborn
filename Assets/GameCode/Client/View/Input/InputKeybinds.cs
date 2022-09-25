using FNZ.Client.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.Input
{
	public class InputKeybinds : FNESingleton<InputKeybinds>
	{
		public Dictionary<string, KeyCode> Keybinds;
		public Dictionary<string, MouseButton> Mousebinds;
		private Dictionary<string, Text> m_ButtonTexts;

		private Text m_CurrentButtonText;
		private string m_CurrentKeybindName;
		private string m_PreviousButtonText;

		private KeyCode[] m_AllKeyCodes;

		private bool m_Listening = false;

		private void Awake()
		{
			if (Instance != this) Destroy(gameObject);
		}

		private void Start()
		{
			Keybinds = new Dictionary<string, KeyCode>();
			Mousebinds = new Dictionary<string, MouseButton>();
			m_ButtonTexts = new Dictionary<string, Text>();

			Keybinds.Add(ActionIdentifiers.MOVE_FORWARD, KeyCode.W);
			Keybinds.Add(ActionIdentifiers.MOVE_BACK, KeyCode.S);
			Keybinds.Add(ActionIdentifiers.MOVE_LEFT, KeyCode.A);
			Keybinds.Add(ActionIdentifiers.MOVE_RIGHT, KeyCode.D);
			Keybinds.Add(ActionIdentifiers.ACTION_SPRINT, KeyCode.LeftShift);
			Keybinds.Add(ActionIdentifiers.ACTION_INTERACT, KeyCode.E);
			Keybinds.Add(ActionIdentifiers.ACTION_RELOAD, KeyCode.R);
			Keybinds.Add(ActionIdentifiers.ACTION_TOGGLE_BUILDING_MENU, KeyCode.B);
			Keybinds.Add(ActionIdentifiers.ACTION_TOGGLE_INVENTORY, KeyCode.I);
			Keybinds.Add(ActionIdentifiers.ACTION_EXCAVATOR_SLOT, KeyCode.Q);
			Keybinds.Add(ActionIdentifiers.ACTION_WEAPON1_SLOT, KeyCode.Alpha1);
			Keybinds.Add(ActionIdentifiers.ACTION_WEAPON2_SLOT, KeyCode.Alpha2);
			Keybinds.Add(ActionIdentifiers.ACTION_HOTBAR_SLOT1, KeyCode.Alpha3);
			Keybinds.Add(ActionIdentifiers.ACTION_HOTBAR_SLOT2, KeyCode.Alpha4);
			Keybinds.Add(ActionIdentifiers.ACTION_HOTBAR_SLOT3, KeyCode.Alpha5);
			Mousebinds.Add(ActionIdentifiers.ACTION_SHOOT_PRIMARY, MouseButton.LEFT);
			Mousebinds.Add(ActionIdentifiers.ACTION_SHOOT_SECONDARY, MouseButton.RIGHT);

			m_AllKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
			
			DontDestroyOnLoad(this);
		}

		private void Update()
		{
			if (m_Listening)
			{
				if (UnityEngine.Input.anyKeyDown)
				{
					foreach (KeyCode key in m_AllKeyCodes)
					{
						if (UnityEngine.Input.GetKeyDown(key))
						{
							if (key == KeyCode.Escape)
							{
								m_CurrentButtonText.text = m_PreviousButtonText;
								break;
							}

							if (Keybinds.ContainsValue(key))
							{
								var entry = Keybinds.First(kvp => kvp.Value == key);
								Keybinds[entry.Key] = KeyCode.None;
								m_ButtonTexts[entry.Key].text = Keybinds[entry.Key].ToString();
							}

							Keybinds[m_CurrentKeybindName] = key;
							m_CurrentButtonText.text = CheckSpecialCases(key.ToString());
						}
					}

					m_CurrentKeybindName = null;
					m_CurrentButtonText = null;
					m_PreviousButtonText = string.Empty;
					m_Listening = false;
				}
			}
		}

		public void OnNewKeybind(Button button)
		{
			m_CurrentKeybindName = Keybinds.Keys.First(key => key.Contains(button.transform.parent.name));
			m_CurrentButtonText = button.GetComponentInChildren<Text>();

			m_PreviousButtonText = m_CurrentButtonText.text;
			m_CurrentButtonText.text = "Press any key...";

			m_Listening = true;
		}

		public string CheckSpecialCases(string str)
		{
			if (str.ToLower().Contains("alpha"))
				str = str.Last().ToString();
			else if (str.ToLower().Contains("mouse"))
			{
				if (int.TryParse(str.Last().ToString(), out int number))
				{
					number++;
					str = $"Mouse{number}";
				}
			}

			return str;
		}

		public void SetButtonText(Transform keybindParent)
		{
			foreach (var key in Keybinds.Keys)
			{
				if (key.Contains(keybindParent.name))
				{
					var actionName = Keybinds.Keys.First(entry => entry.Contains(keybindParent.name));
					var buttonText = keybindParent.GetComponentInChildren<Button>().GetComponentInChildren<Text>();

					if (!m_ButtonTexts.ContainsKey(actionName))
						m_ButtonTexts.Add(actionName, buttonText);
					else
						m_ButtonTexts[actionName] = buttonText;
					
					buttonText.text = CheckSpecialCases(Keybinds[key].ToString());
					break;
				}
			}
		}

	}
}