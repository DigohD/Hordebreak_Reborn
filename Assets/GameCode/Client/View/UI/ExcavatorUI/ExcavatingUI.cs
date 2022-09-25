using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Model.Entity.Components.Player;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.ExcavatorUI 
{

	public class ExcavatingUI : MonoBehaviour
	{
		private ExcavatorComponentClient m_Excavator;
		private PlayerComponentClient m_Player;

		public Image IMG_ExcavatorFill;

		public void Init()
		{
			if (m_Player == null)
			{
				m_Player = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
				m_Excavator = m_Player.ParentEntity.GetComponent<ExcavatorComponentClient>();

				m_Excavator.d_OnExcavatorStopFiring += OnStopFire;
				m_Excavator.d_OnExcavatorProgressChange += OnProgressChange;
			}
		}

		void Update()
	    {
			if (m_Player == null && GameClient.LocalPlayerEntity != null)
			{
				m_Player = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
				m_Excavator = m_Player.ParentEntity.GetComponent<ExcavatorComponentClient>();

				m_Excavator.d_OnExcavatorStopFiring += OnStopFire;
				m_Excavator.d_OnExcavatorProgressChange += OnProgressChange;
			}

			if (m_Player == null)
				return;
		}


		void OnStopFire()
		{
			transform.GetChild(1).gameObject.SetActive(false);
		}

		void OnProgressChange(float progress)
		{
			if(progress < 0 && transform.GetChild(1).gameObject.activeInHierarchy)
			{
				transform.GetChild(1).gameObject.SetActive(false);
				return;
			}else if (progress >= 0 && !transform.GetChild(1).gameObject.activeInHierarchy)
			{
				transform.GetChild(1).gameObject.SetActive(true);
			}

			IMG_ExcavatorFill.fillAmount = Mathf.Clamp(progress, 0, 1);
		}
	}
}