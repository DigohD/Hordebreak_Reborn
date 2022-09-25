using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.Utils.UI
{
	public class FlashSprite : MonoBehaviour
	{
		private float m_Alpha = 1;

		private Image m_Image;

		public void Init(Color color)
		{
			m_Image = GetComponent<Image>();
			m_Image.color = new Color(color.r, color.g, color.b, m_Alpha);
		}

		void Update()
		{
			m_Alpha = Mathf.Lerp(m_Alpha, -0.05f, 7.5f * Time.deltaTime);
			Color imgColor = m_Image.color;
			if (m_Alpha <= 0)
			{
				m_Image.color = new Color(imgColor.r, imgColor.g, imgColor.b, 0);
				Destroy(this);
			}
			m_Image.color = new Color(imgColor.r, imgColor.g, imgColor.b, m_Alpha);
		}
	}
}