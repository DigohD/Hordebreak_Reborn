using FNZ.Client.StaticData;
using FNZ.Client.View.Input;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Chat
{
	public class UI_Chat : MonoBehaviour
	{
		private string m_Current = string.Empty;

		private readonly byte MAX_MESSAGE_LENGTH = 255;
		private static readonly byte MAX_MESSAGE_COUNT = 255;
		private static readonly byte MAX_SAVED_MESSAGES_COUNT = 20;

		public GameObject UI_DebugWindow;

		public Text TXT_ChatWindow;
		public InputField INPUT_ChatInput;
		public ScrollRect SR_ScrollRect;

		public RectTransform RT_Content;
		public RectTransform RT_InputParent;

		private int sentMessageIndex = 0;

		private static bool Rerender = false;

		private static Queue<string> messageLog = new Queue<string>();
		private static List<string> sentMessages = new List<string>();

		private void Update()
		{
			if (Rerender)
			{
				StringBuilder toRenderBuilder = new StringBuilder("");
				foreach (var message in messageLog)
					toRenderBuilder.Append("\n" + message);
				TXT_ChatWindow.text = toRenderBuilder.ToString();
				Rerender = false;

				var textRT = (RectTransform)TXT_ChatWindow.transform;
				Canvas.ForceUpdateCanvases();
				RT_Content.sizeDelta = new Vector2(0, textRT.rect.height);
				SR_ScrollRect.normalizedPosition = new Vector2(0, 0);
			}
		}

		public void OnCycleMessages(bool up)
		{
			if (sentMessages.Count > 0)
			{
				INPUT_ChatInput.text = sentMessages.ElementAt(sentMessageIndex);

				if (up)
					sentMessageIndex = sentMessageIndex + 1 > sentMessages.Count - 1 ? 0 : sentMessageIndex + 1;
				else
					sentMessageIndex = sentMessageIndex - 1 < 0 ? sentMessages.Count - 1 : sentMessageIndex - 1;
				
				INPUT_ChatInput.caretPosition = INPUT_ChatInput.text.Length;
			}
		}

		public void OnTextChange(string current = "")
		{
			m_Current = current;
		}

		public void OnSendClick()
		{
			SendMessage();
			ToggleChat();
		}

		public static void NewMessage(string newMessage)
		{
			messageLog.Enqueue("<color=#aaaaaaff>[" + DateTime.Now.ToString("hh:mm:ss") + "]:</color> " + newMessage);
			if (messageLog.Count > MAX_MESSAGE_COUNT)
				messageLog.Dequeue();

			Rerender = true;
		}

		public void ToggleChat()
		{
			if (RT_InputParent.gameObject.activeInHierarchy)
			{
				SendMessage();

				RT_InputParent.gameObject.SetActive(false);
				transform.GetComponent<Image>().enabled = false;
				InputManager.Instance.PopInputLayer();

				UIManager.Instance.PlaySound("sfx_ui_close");
			}
			else
			{
				RT_InputParent.gameObject.SetActive(true);
				transform.GetComponent<Image>().enabled = true;
				INPUT_ChatInput.Select();
				INPUT_ChatInput.ActivateInputField();
				InputManager.Instance.PushInputLayer<ChatInputLayer>();

				UIManager.Instance.PlaySound("sfx_ui_open");
			}
		}

		public void CloseChat()
		{
			if (RT_InputParent.gameObject.activeInHierarchy)
			{
				INPUT_ChatInput.text = string.Empty;

				RT_InputParent.gameObject.SetActive(false);
				transform.GetComponent<Image>().enabled = false;
				InputManager.Instance.PopInputLayer();

				UIManager.Instance.PlaySound("sfx_ui_close");
			}
		}

		private void SendMessage()
		{
			if (m_Current.Length == 0 || m_Current.Length >= MAX_MESSAGE_LENGTH)
				return;

			if (m_Current.ToLower() == "/ping" || m_Current.ToLower() == "/ping ")
			{
				NetData.PING_TIMESTAMP = DateTime.Now;
			}
			if (m_Current.ToLower() == "/debug" || m_Current.ToLower() == "/debug ")
			{
				UI_DebugWindow.SetActive(!UI_DebugWindow.activeInHierarchy);
			}

#if UNITY_EDITOR
			if (m_Current.ToLower() == "/reloadxml")
			{
				DataBank.Instance.ReloadDataBank();
				return;
			}
#endif

			if (!sentMessages.Contains(m_Current))
			{
				sentMessages.Insert(0, m_Current);

				if (sentMessages.Count > MAX_SAVED_MESSAGES_COUNT)
					sentMessages.RemoveAt(sentMessages.Count - 1);
			}
			else
			{
				sentMessages.Remove(m_Current);
				sentMessages.Insert(0, m_Current);
			}

			sentMessageIndex = 0;

			GameClient.NetAPI.CMD_Chat_ChatMessage(m_Current);

			INPUT_ChatInput.text = string.Empty;
		}

	}
}