using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.RespawnPrompt 
{
	public class RespawnPrompt : MonoBehaviour
	{
		// public GameObject G_RespawnPrompt;

		private PlayerComponentClient playerComp;

		[SerializeField] private Button BTN_RespawnButton;
		[SerializeField] private Text TXT_RespawnText;
		
        private void Start()
        {
			// AddButtonEvents();
		}

        private float buttonDelay = 1.5f;
        
        void Update()
        {
			if (playerComp == null && GameClient.LocalPlayerEntity != null)
            {
				playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
				if(playerComp != null)
                {
					Init();
				}
			}

			buttonDelay -= Time.deltaTime;
			if (buttonDelay < 0)
				buttonDelay = 0;

			if (buttonDelay == 0 && !BTN_RespawnButton.interactable)
			{
				TXT_RespawnText.color = Color.white;
				BTN_RespawnButton.interactable = true;
			}
        }

		private void Init()
        {
			playerComp.d_OnPlayerDeath += OnPlayerDeath;
			playerComp.d_OnPlayerRevival += OnPlayerRevival;
            if (!playerComp.IsDead)
            {
	            transform.GetChild(0).gameObject.SetActive(false);
            }
		}

		private void OnPlayerDeath()
        {
	        BTN_RespawnButton.interactable = false;
	        buttonDelay = 1.5f;
	        TXT_RespawnText.color = new Color(1, 1, 1, 0.3f);
			transform.GetChild(0).gameObject.SetActive(true);
        }

		private void OnPlayerRevival()
		{
			transform.GetChild(0).gameObject.SetActive(false);
			BTN_RespawnButton.interactable = false;
		}

		public void OnRespawnClick()
        {
			// G_RespawnPrompt.GetComponent<Image>().color = new Color(0.58f, 0.21f, 0.21f, 1);

			playerComp.NE_Send_RespawnRequest();
		}

  //      private void AddButtonEvents()
		//{
		//	EventTrigger eventTrigger = G_RespawnPrompt.AddComponent<EventTrigger>();

		//	EventTrigger.Entry entry = new EventTrigger.Entry();
		//	entry.eventID = EventTriggerType.PointerDown;
		//	entry.callback.AddListener((eventData) =>
		//	{
		//		OnRespawnClick();

		//		var flash = G_RespawnPrompt.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
		//		flash.Init(Color.white);
		//	});
		//	eventTrigger.triggers.Add(entry);

		//	entry = new EventTrigger.Entry();
		//	entry.eventID = EventTriggerType.PointerEnter;
		//	entry.callback.AddListener((eventData) =>
		//	{
		//		G_RespawnPrompt.GetComponent<Image>().color = new Color(0.8f, 0.65f, 0.65f, 1);

		//		UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
		//	});
		//	eventTrigger.triggers.Add(entry);

		//	entry = new EventTrigger.Entry();
		//	entry.eventID = EventTriggerType.PointerExit;
		//	entry.callback.AddListener((eventData) =>
		//	{
		//		G_RespawnPrompt.GetComponent<Image>().color = new Color(0.58f, 0.21f, 0.21f, 1);
		//	});
		//	eventTrigger.triggers.Add(entry);
		//}
	}
}