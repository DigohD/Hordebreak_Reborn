using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Manager
{
	public class UI_Error : MonoBehaviour
	{
		[SerializeField] Text m_ErrorTitle;
		[SerializeField] Text m_ErrorInfo;

		public void Init(string errorTitle, string errorInfo)
		{
			m_ErrorTitle.text = errorTitle;
			m_ErrorInfo.text = errorInfo;
		}
	}
}