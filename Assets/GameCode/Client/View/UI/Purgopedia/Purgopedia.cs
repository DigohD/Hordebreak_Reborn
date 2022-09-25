using FNZ.Client.Utils.UI;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Purgopedia 
{

	public class Purgopedia : MonoBehaviour
	{
		private bool[] chapterUnlocks;
		[SerializeField]
		private GameObject[] P_ChapterSections;

		[SerializeField]
		private GameObject P_SectionButton;
		[SerializeField]
		private Transform T_ButtonParent;
		[SerializeField]
		private Transform T_ContentParent;

        private static GameObject m_ClickedButton;

        private Color m_BaseColor, m_SelectColor;

        private void Start()
		{
            m_BaseColor = new Color(0.3f, 0.3f, 0.3f);
            m_SelectColor = new Color(0.2f, 0.2f, 0.0941f);

            chapterUnlocks = new bool[P_ChapterSections.Length];
			chapterUnlocks[0] = true;

            for (int i = 0; i < chapterUnlocks.Length; i++)
            {
                chapterUnlocks[i] = true;
            }

			RenderButtons();
		}

		private void RenderButtons()
		{
			foreach (Transform child in T_ButtonParent)
				Destroy(child.gameObject);

            int x = 0;
			for (int i = 0; i < P_ChapterSections.Length; i++)
			{
                x = i - 1;
				if (chapterUnlocks[i])
				{
					var newButton = Instantiate(P_SectionButton);
					newButton.transform.SetParent(T_ButtonParent);
					newButton.GetComponentInChildren<Text>().text = P_ChapterSections[i].name;
					newButton.transform.localScale = Vector3.one;

                    AddEventToButton(newButton, i);
                }
			}
		}

        private void AddEventToButton(GameObject button, int i)
        {
            EventTrigger eventTrigger = button.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) =>
            {
                OnButtonClick(i, button);
            });
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) =>
            {
                OnButtonHover(button);
            });
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) =>
            {
                OnButtonExit(button);
            });
            eventTrigger.triggers.Add(entry);
        }

        public void OnButtonClick(int i, GameObject button)
        {
            foreach (Transform child in T_ContentParent)
                Destroy(child.gameObject);

            var newContent = Instantiate(P_ChapterSections[i]);
            newContent.transform.SetParent(T_ContentParent, false);
            newContent.transform.localPosition = Vector3.zero;

            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);

			if (m_ClickedButton != null) m_ClickedButton.GetComponent<Image>().color = m_BaseColor;
            m_ClickedButton = button;

            var flash = button.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
            flash.Init(Color.white);
        }

        public void OnButtonHover(GameObject button)
        {
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            button.GetComponent<Image>().color = m_SelectColor;
        }

        public void OnButtonExit(GameObject button)
        {
			if (m_ClickedButton != button)
                button.GetComponent<Image>().color = m_BaseColor;
        }
    }
}