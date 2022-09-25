using FNZ.Client.View.UI.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.ScreenEffects
{
	public class UI_Fade : MonoBehaviour
	{
		private Image IMG_Fade;
		public Image IMG_Fade2;
		public Image IMG_Fade3;
		private bool m_FadeIn;

		public void Init(bool fadeIn)
		{
			m_FadeIn = fadeIn;
		}

		void Start()
		{
			IMG_Fade = GetComponent<Image>();
		}

		void Update()
		{
			if (m_FadeIn)
			{
				IMG_Fade.color = new Color(IMG_Fade.color.r, IMG_Fade.color.g, IMG_Fade.color.b, Mathf.Lerp(IMG_Fade.color.a, -0.1f, 1f * Time.deltaTime));
				IMG_Fade2.color = new Color(IMG_Fade2.color.r, IMG_Fade2.color.g, IMG_Fade2.color.b, Mathf.Lerp(IMG_Fade2.color.a, -0.1f, 1f * Time.deltaTime));
				IMG_Fade3.color = new Color(IMG_Fade3.color.r, IMG_Fade3.color.g, IMG_Fade3.color.b, Mathf.Lerp(IMG_Fade3.color.a, -0.1f, 3f * Time.deltaTime));

				if (IMG_Fade.color.a <= 0)
				{
					UIManager.Instance.InstantiateRoomUI();

					Destroy(transform.parent.gameObject);
				}
			}
			else
			{
				IMG_Fade3.color = new Color(Mathf.Lerp(IMG_Fade3.color.r, 1.1f, 0.005f), IMG_Fade3.color.g, IMG_Fade3.color.b, IMG_Fade3.color.a);
			}
		}
	}
}