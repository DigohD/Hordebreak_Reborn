using FNZ.Client.Net.NetworkManager;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.QuestType;
using FNZ.Shared.Utils;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Objective 
{

	public class ObjectiveScript : MonoBehaviour
	{

		[SerializeField]
		private TextMeshProUGUI TXT_ObjectiveTitle;
		[SerializeField]
		private TextMeshProUGUI TXT_ObjectiveDesc;

		[SerializeField]
		private GameObject RT_Description;

		[SerializeField]
		private Image IMG_Arrow;

		[SerializeField]
		private GameObject GO_Flash;

		private bool showDescription = true;
		private bool hasQuest = false;
		
		void Start()
		{
			ClientQuestNetworkManager.d_OnQuestUpdateReceived += SetObjective;
		}
		

		private void SetObjective(string id, int progress, float2[] mapHighlights)
        {
	        if(hasQuest)
		        UIManager.Instance.PlaySound("sfx_quest_success");
	        
	        if (string.IsNullOrEmpty(id))
	        {
		        gameObject.SetActive(false);
		        hasQuest = false;
		        return;
	        }

	        hasQuest = true;
	        
			QuestData Objective = DataBank.Instance.GetData<QuestData>(id);

			TXT_ObjectiveTitle.text = Localization.GetString(Objective.titleRef);

			var desc = Localization.GetString(Objective.descriptionRef);

			desc = desc.Replace('{', '<').Replace('}', '>');

			TXT_ObjectiveDesc.text = desc;

			

			RectTransform RT = RT_Description.GetComponent<RectTransform>();

			LayoutRebuilder.ForceRebuildLayoutImmediate(RT);

			var flash = GO_Flash.AddComponent<FlashSprite>();
			flash.Init(Color.white);
		}

		// Called from UI Button
		public void ToggleDescription ()
        {
			if (showDescription)
			{
				RT_Description.SetActive(false);
				showDescription = false;
				IMG_Arrow.transform.Rotate(Vector3.forward * 180);
			} else
			{
				RT_Description.SetActive(true);
				showDescription = true;
				IMG_Arrow.transform.Rotate(Vector3.forward * 180);
			}
        }
	}
}