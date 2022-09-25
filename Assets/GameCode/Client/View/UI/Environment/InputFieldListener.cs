using FNZ.Client.View.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FNZ.Client.View.UI.Environment
{

	public class InputFieldListener : MonoBehaviour, ISelectHandler
	{
		private bool m_IsActive = false;
		private bool m_IsPushed = false;

		public void OnSelect(BaseEventData eventData)
		{
			m_IsActive = true;
			m_IsPushed = true;
			InputManager.Instance.PushInputLayer<InputFieldInputLayer>();
		}

		public void OnInputDone()
		{
			m_IsActive = false;
		}

		void Start()
		{

		}

		void Update()
		{

		}

		void LateUpdate()
		{
			if (!m_IsActive && m_IsPushed)
			{
				InputManager.Instance.PopInputLayer();
				m_IsPushed = false;
			}
		}

		void OnDestroy()
		{
			if (m_IsPushed)
				InputManager.Instance.PopInputLayer();
		}
	}
}