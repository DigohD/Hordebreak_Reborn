using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Model.Entity.Components.Health;
using FNZ.Client.View.UI.DamageEffect;
using FNZ.Shared.Model.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.Model.Entity.Components.Stat
{
	public class StatPanelClient : MonoBehaviour
	{
		private bool isInitialized = false;

		[SerializeField]
		private GameObject m_ShieldBar;

		[SerializeField]
		private Image m_HealthBarImage;
		[SerializeField]
		private Image m_DamageBarImage;
		[SerializeField]
		private Image m_FlashBarImage;
		[SerializeField]
		private Text m_HealthBarText;
		[SerializeField]
		private Text m_HealthBarTextShadow;
		//[SerializeField]
		//private Text m_ArmorText;

		[SerializeField]
		private Image m_ShieldsBarImage;
		[SerializeField]
		private Image m_ShieldsDamageBarImage;
		[SerializeField]
		private Image m_ShieldsFlashBarImage;
		[SerializeField]
		private Text m_ShieldsText;
		[SerializeField]
		private Text m_ShieldsTextShadow;

		[SerializeField]
		private Fullscreen_Damage_Effect FullscreenDamageEffect;

		//[SerializeField]
		//private Image m_FuelBarImage;
		//[SerializeField]
		//private Image m_FuelDrainBarImage;
		//[SerializeField]
		//private Image m_FuelFlashBarImage;

		private FNEEntity m_PlayerEntity;
		private bool m_HealthVisualsUpdating;
		private bool m_ShieldsVisualsUpdating;
		// private bool m_FuelVisualsUpdating;

		private float m_BarWidth, m_BarHeight, m_ShieldsFuelBarHeight;
		private float m_DamageBarWidth;
		private float m_ShieldsDamageBarWidth;
		// private float m_FuelDrainBarWidth;

		private float m_HealthDamageStallTimer;
		private float m_ShieldsDamageStallTimer;
		// private float m_FuelDrainStallTimer;

		private const float DAMAGE_SHRINK_SPEED = 150f;
		private const float FLASH_DECAY_SPEED = 3.4f;

		void Update()
		{
			if (!isInitialized && GameClient.LocalPlayerEntity != null)
				Init();

			if (m_HealthVisualsUpdating)
				UpdateHealthVisuals();

			if (m_ShieldsVisualsUpdating)
				UpdateShieldsVisuals();

			//if (m_FuelVisualsUpdating)
			//	UpdateFuelVisuals();
		}

		private void Init()
		{
			m_BarWidth = ((RectTransform)m_HealthBarImage.transform).rect.width;
			m_BarHeight = ((RectTransform)m_HealthBarImage.transform).rect.height;

			m_ShieldsFuelBarHeight = ((RectTransform)m_ShieldsBarImage.transform).rect.height;

			m_PlayerEntity = GameClient.LocalPlayerEntity;
			var statComp = m_PlayerEntity.GetComponent<StatComponentClient>();
			var excavatorComp = m_PlayerEntity.GetComponent<ExcavatorComponentClient>();

			UpdateHealthIndicators(statComp.CurrentHealth, statComp.CurrentHealth, statComp.Data.startHealth);
			UpdateShieldsIndicators(statComp.CurrentShields, statComp.CurrentShields, statComp.Data.startShields);
			// UpdateArmorIndicator(statComp.Armor);
			// UpdateFuelIndicators(excavatorComp.GetFuel(), excavatorComp.GetFuel(), excavatorComp.GetMaximumFuel());

			statComp.d_HealthChange += UpdateHealthIndicators;
			statComp.d_ShieldsChange += UpdateShieldsIndicators;
			// statComp.d_ArmorChange += UpdateArmorIndicator;
			// excavatorComp.d_FuelChange += UpdateFuelIndicators;

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

			isInitialized = true;
		}

		private void UpdateHealthIndicators(float health, float previousHealth, float maxHealth)
		{
			m_HealthVisualsUpdating = true;
			m_HealthDamageStallTimer = 0.3f;

			float percentHealth = health / maxHealth;
			float percentLost = Mathf.Abs((health - previousHealth) / maxHealth);

			if (previousHealth > health)
			{
				var flashColor = m_FlashBarImage.color;
				m_FlashBarImage.color = new Color(
					flashColor.r,
					flashColor.g,
					flashColor.b,
					1
				);

				var flashT = (RectTransform)m_FlashBarImage.transform;
				flashT.sizeDelta = new Vector2(percentLost * m_BarWidth, m_BarHeight);

				var damageT = (RectTransform)m_DamageBarImage.transform;
				m_DamageBarWidth = m_DamageBarWidth + (percentLost * m_BarWidth);
				damageT.sizeDelta = new Vector2(m_DamageBarWidth, m_BarHeight);

				if (FullscreenDamageEffect != null)
					FullscreenDamageEffect.TriggerFullscreenDamage();
			}

			var healthT = (RectTransform)m_HealthBarImage.transform;
			healthT.sizeDelta = new Vector2(percentHealth * m_BarWidth, m_BarHeight);

			if (health <= 0)
			{
				m_HealthBarText.text = "<color=#FF0000FF>You are dead!</color>";
				m_HealthBarTextShadow.text = "You are dead!";
			}
			else
			{
				m_HealthBarText.text = Mathf.Round(health).ToString() + " / " + maxHealth.ToString();
				m_HealthBarTextShadow.text = Mathf.Round(health).ToString() + " / " + maxHealth.ToString();
			}
		}

		private void UpdateHealthVisuals()
		{
			bool stillUpdatingDamage = true;
			bool stillUpdatingFlash = true;

			m_HealthDamageStallTimer -= Time.deltaTime;
			if (m_HealthDamageStallTimer <= 0)
			{
				m_HealthDamageStallTimer = 0;
				m_DamageBarWidth -= DAMAGE_SHRINK_SPEED * Time.deltaTime;

				if (m_DamageBarWidth <= 0)
				{
					m_DamageBarWidth = 0;
					stillUpdatingDamage = false;
				}

				var damageT = (RectTransform)m_DamageBarImage.transform;
				damageT.sizeDelta = new Vector2(m_DamageBarWidth, m_BarHeight);
			}

			var flashColor = m_FlashBarImage.color;
			if (flashColor.a > 0)
			{
				m_FlashBarImage.color = new Color(
					flashColor.r,
					flashColor.g,
					flashColor.b,
					flashColor.a - Time.deltaTime * FLASH_DECAY_SPEED
				);
			}
			else
			{
				stillUpdatingFlash = false;
			}

			if (!stillUpdatingDamage && !stillUpdatingFlash)
				m_HealthVisualsUpdating = false;
		}

		private void UpdateShieldsIndicators(float shields, float previousShields, float maxShields)
		{
			if (maxShields <= 0)
			{
				m_ShieldBar.SetActive(false);
				m_ShieldsText.gameObject.SetActive(false);

				LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

				return;
			}

			m_ShieldBar.SetActive(true);
			m_ShieldsText.gameObject.SetActive(true);

			((RectTransform)m_ShieldsDamageBarImage.transform).anchoredPosition = Vector3.zero;
			((RectTransform)m_ShieldsFlashBarImage.transform).anchoredPosition = Vector3.zero;

			m_ShieldsVisualsUpdating = true;
			m_ShieldsDamageStallTimer = 0.3f;

			float percentShields = shields / maxShields;
			float percentLost = Mathf.Abs((shields - previousShields) / maxShields);

			var shieldsT = (RectTransform)m_ShieldsBarImage.transform;
			shieldsT.sizeDelta = new Vector2(percentShields * m_BarWidth, m_ShieldsFuelBarHeight);

			if (shields < maxShields && previousShields > shields)
			{
				var flashColor = m_ShieldsFlashBarImage.color;
				m_ShieldsFlashBarImage.color = new Color(
					flashColor.r,
					flashColor.g,
					flashColor.b,
					1
				);

				var flashT = (RectTransform)m_ShieldsFlashBarImage.transform;
				flashT.sizeDelta = new Vector2(percentLost * m_BarWidth, m_ShieldsFuelBarHeight);

				var damageT = (RectTransform)m_ShieldsDamageBarImage.transform;
				m_ShieldsDamageBarWidth = m_ShieldsDamageBarWidth + (percentLost * m_BarWidth);
				damageT.sizeDelta = new Vector2(m_ShieldsDamageBarWidth, m_ShieldsFuelBarHeight);
			}

			if (shields <= 0)
			{
				m_ShieldsText.text = "-";
				m_ShieldsTextShadow.text = "-";
			}
			else
			{
				m_ShieldsText.text = (int)(percentShields * 100) + "%";
				m_ShieldsTextShadow.text = (int)(percentShields * 100) + "%";
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		}

		private void UpdateShieldsVisuals()
		{
			bool stillUpdatingDamage = true;
			bool stillUpdatingFlash = true;

			m_ShieldsDamageStallTimer -= Time.deltaTime;
			if (m_ShieldsDamageStallTimer <= 0)
			{
				m_ShieldsDamageStallTimer = 0;
				m_ShieldsDamageBarWidth -= DAMAGE_SHRINK_SPEED * Time.deltaTime;

				if (m_ShieldsDamageBarWidth <= 0)
				{
					m_ShieldsDamageBarWidth = 0;
					stillUpdatingDamage = false;
				}

				var damageT = (RectTransform)m_ShieldsDamageBarImage.transform;
				damageT.sizeDelta = new Vector2(m_ShieldsDamageBarWidth, m_ShieldsFuelBarHeight);
			}

			var flashColor = m_ShieldsFlashBarImage.color;
			if (flashColor.a > 0)
			{
				m_ShieldsFlashBarImage.color = new Color(
					flashColor.r,
					flashColor.g,
					flashColor.b,
					flashColor.a - Time.deltaTime * FLASH_DECAY_SPEED
				);
			}
			else
			{
				stillUpdatingFlash = false;
			}

			if (!stillUpdatingDamage && !stillUpdatingFlash)
				m_ShieldsVisualsUpdating = false;
		}

		//private void UpdateFuelIndicators(int fuel, int previousFuel, int maxFuel)
		//{
		//	// m_ShieldsText.gameObject.SetActive(true);

		//	((RectTransform)m_FuelDrainBarImage.transform).anchoredPosition = Vector3.zero;
		//	((RectTransform)m_FuelFlashBarImage.transform).anchoredPosition = Vector3.zero;

		//	m_FuelVisualsUpdating = true;
		//	m_FuelDrainStallTimer = 0.3f;

		//	float percentFuel = (float)fuel / (float)maxFuel;
		//	float percentLost = Mathf.Abs(((float)fuel - (float)previousFuel) / (float)maxFuel);

		//	var fuelT = (RectTransform)m_FuelBarImage.transform;
		//	fuelT.sizeDelta = new Vector2(percentFuel * m_BarWidth, m_ShieldsFuelBarHeight);

		//	if (fuel < maxFuel && previousFuel > fuel)
		//	{
		//		var flashColor = m_FuelFlashBarImage.color;
		//		m_FuelFlashBarImage.color = new Color(
		//			flashColor.r,
		//			flashColor.g,
		//			flashColor.b,
		//			1
		//		);

		//		var flashT = (RectTransform)m_FuelFlashBarImage.transform;
		//		flashT.sizeDelta = new Vector2(percentLost * m_BarWidth, m_ShieldsFuelBarHeight);

		//		var drainT = (RectTransform)m_FuelDrainBarImage.transform;
		//		m_FuelDrainBarWidth = m_FuelDrainBarWidth + (percentLost * m_BarWidth);
		//		drainT.sizeDelta = new Vector2(m_FuelDrainBarWidth, m_ShieldsFuelBarHeight);
		//	}

		//	/*if (shields <= 0)
  //          {
  //              m_ShieldsText.text = "-";
  //          }
  //          else
  //          {
  //              m_ShieldsText.text = (int)(percentShields * 100) + "%";
  //          }*/

		//	LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
		//}

		//private void UpdateFuelVisuals()
		//{
		//	bool stillUpdatingDrain = true;
		//	bool stillUpdatingFlash = true;

		//	m_FuelDrainStallTimer -= Time.deltaTime;
		//	if (m_FuelDrainStallTimer <= 0)
		//	{
		//		m_FuelDrainStallTimer = 0;
		//		m_FuelDrainBarWidth -= DAMAGE_SHRINK_SPEED * Time.deltaTime;

		//		if (m_FuelDrainBarWidth <= 0)
		//		{
		//			m_FuelDrainBarWidth = 0;
		//			stillUpdatingDrain = false;
		//		}

		//		var drainT = (RectTransform)m_FuelDrainBarImage.transform;
		//		drainT.sizeDelta = new Vector2(m_FuelDrainBarWidth, m_ShieldsFuelBarHeight);
		//	}

		//	var flashColor = m_FuelDrainBarImage.color;
		//	if (flashColor.a > 0)
		//	{
		//		m_FuelFlashBarImage.color = new Color(
		//			flashColor.r,
		//			flashColor.g,
		//			flashColor.b,
		//			flashColor.a - Time.deltaTime * FLASH_DECAY_SPEED
		//		);
		//	}
		//	else
		//	{
		//		stillUpdatingFlash = false;
		//	}

		//	if (!stillUpdatingDrain && !stillUpdatingFlash)
		//		m_FuelVisualsUpdating = false;
		//}

		//private void UpdateArmorIndicator(float armor)
		//{
		//	m_ArmorText.text = armor.ToString();
		//}
	}
}