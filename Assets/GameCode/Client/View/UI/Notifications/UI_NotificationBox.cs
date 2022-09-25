using FNZ.Client.Utils.UI;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Notifications
{
	public delegate void OnExpire(GameObject notBox);

	public class UI_NotificationBox : MonoBehaviour
	{
		public OnExpire d_OnExpire;

		private List<GameObject> m_ParentList;
		public Image img_NotificationIcon;
		public Text txt_TextBox;
		public Image img_Background;

		public float Identifier;
		public float FadeOutTime = 1.5f;
		public float FadeInTime = 0.3f;
		public float LerpSpeed = 0.1f;

		private float m_BoxTimer;
		private float m_InitTime = 0;
		private float m_MaxDurationTime;

		private int m_ParentIndex;

		public bool IsPermanent;

		private byte m_MaxBoxes = 8;

		private void Start()
		{
			m_ParentList = GetComponentInParent<UI_Notification>().NotificationBoxList;
			d_OnExpire += GetComponentInParent<UI_Notification>().DeleteFromList;
			m_MaxDurationTime = m_BoxTimer;
		}

		private void Update()
		{
			if (!IsPermanent)
				m_BoxTimer -= Time.deltaTime;

			if (m_BoxTimer > 0)
			{
				m_ParentIndex = m_ParentList.IndexOf(gameObject);

				if (gameObject.transform.localPosition != new Vector3(0, 0, 0) + (Vector3.up * UI_Notification.s_Spacing) * m_ParentIndex)
				{
					gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, new Vector3(0, 0, 0) + (Vector3.up * UI_Notification.s_Spacing) * m_ParentIndex, LerpSpeed);
				}

				if (m_BoxTimer > m_MaxDurationTime - FadeInTime)
					FadeInNotification(1);
				else
					FadeOutNotification(m_BoxTimer);

				if (m_ParentIndex > m_MaxBoxes && m_BoxTimer > FadeOutTime)
					SetPermanentFalse();
			}

			if (m_BoxTimer <= 0)
			{
				d_OnExpire?.Invoke(gameObject);
				gameObject.SetActive(false);
			}
		}

		private void FadeInNotification(float timer)
		{
			m_InitTime += Time.deltaTime;

			img_NotificationIcon.color = new Color(img_NotificationIcon.color.r,
													img_NotificationIcon.color.g,
													img_NotificationIcon.color.b,
													1
													);

			img_Background.color = new Color(img_Background.color.r,
												img_Background.color.g,
												img_Background.color.b,
												1
												);

			txt_TextBox.color = new Color(txt_TextBox.color.r,
										txt_TextBox.color.g,
										txt_TextBox.color.b,
										1
										);
		}

		private void FadeOutNotification(float timer)
		{
			img_NotificationIcon.color = new Color(img_NotificationIcon.color.r,
													img_NotificationIcon.color.g,
													img_NotificationIcon.color.b,
													FNEUtil.ScaleValueFloat(timer, 0, FadeOutTime, 0, 1)
													);

			img_Background.color = new Color(img_Background.color.r,
												img_Background.color.g,
												img_Background.color.b,
												FNEUtil.ScaleValueFloat(timer, 0, FadeOutTime, 0, 1)
												);

			txt_TextBox.color = new Color(txt_TextBox.color.r,
										txt_TextBox.color.g,
										txt_TextBox.color.b,
										FNEUtil.ScaleValueFloat(timer, 0, FadeOutTime, 0, 1)
										);
		}

		public void Init(string spriteId, string message, string color)
		{
			var sprite = SpriteBank.GetSprite(spriteId);
			img_NotificationIcon.sprite = sprite;
			img_Background.color = FNEUtil.ConvertHexStringToColor(color);
			txt_TextBox.text = message;
        }

		public void SetTimer(float timer)
		{
			m_BoxTimer = timer;
			m_InitTime = 0;
		}

		public void SetPermanentFalse()
		{
			IsPermanent = false;
			m_BoxTimer = FadeOutTime;
		}
	}
}