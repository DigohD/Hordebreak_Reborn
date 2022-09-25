using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using UnityEngine;

namespace FNZ.Client.View.UI.ScreenEffects
{
	public class UI_ScreenEffects : MonoBehaviour
	{
		public GameObject GO_Fade;

		void Start()
		{
			var xmlErrors = DataBank.Instance.GetDataBankErrors();

			if (xmlErrors.Count > 0)
			{
				var UiErrorHandler = UIManager.Instance.GetComponentInChildren<UI_ErrorMessage>();

				foreach (var error in xmlErrors)
				{
					Debug.LogError(error.Item1 + " | " + error.Item2);
					//UiErrorHandler.NewErrorMessage(error.Item1, error.Item2);
				}
			}

			Invoke("FadeInGame", 4);
		}

		private void FadeInGame()
		{
			GO_Fade.GetComponent<UI_Fade>().Init(true);
		}
	}
}