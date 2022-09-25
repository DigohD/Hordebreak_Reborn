using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Interaction
{

	public class InteractPrompt : MonoBehaviour
	{
		private Text m_Text;

		private readonly float m_PulseTime = 0.6f;

		private bool m_IsFading;
		private float m_Step;

		void Start()
		{
			m_IsFading = true;
			m_Step = 1f / m_PulseTime;
		}

		void Update()
		{
			if (m_Text == null)
				m_Text = GetComponent<Text>();

			float alpha = m_Text.color.a;
			var oldColor = m_Text.color;
			if (m_IsFading)
			{
				alpha -= m_Step * Time.deltaTime;
				if (alpha > 0)
				{
					m_Text.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
				}
				else
				{
					m_IsFading = false;
				}
			}
			else
			{
				alpha += m_Step * Time.deltaTime;
				if (alpha < 1)
				{
					m_Text.color = new Color(oldColor.r, oldColor.g, oldColor.b, alpha);
				}
				else
				{
					m_IsFading = true;
				}
			}
		}
	}
}