using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.World.Atmosphere;
using FNZ.Shared.Model.World.Site;
using FNZ.Shared.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace FNZ.Client.View.Atmosphere
{


	public struct AtmosphereLayer
	{
		public Color FogColor;
		public Color SunTint;
		public Color MoonTint;
		public Color WaterTint;
		public FogDensity FogDense;

		public AtmosphereLayer(
			Color FogColor,
			Color SunTint,
			Color MoonTint,
			Color WaterTint,
			FogDensity FogDense
		)
		{
			this.FogColor = FogColor;
			this.SunTint = SunTint;
			this.MoonTint = MoonTint;
			this.WaterTint = WaterTint;
			this.FogDense = FogDense;
		}

		public bool IsDefault()
		{
			return FogColor == Color.white
				   && SunTint == Color.white
				   && MoonTint == Color.white
				   && WaterTint == Color.white
				   && FogDense == FogDensity.LIGHTEST;
		}
	}

	public class AtmosphereVolume : MonoBehaviour
	{
		[SerializeField]
		private Volume m_AtmosphereVolume;

		[SerializeField]
		private DensityVolume m_DensityVolume;

		private Fog m_Fog;

		[SerializeField]
		private HDAdditionalLightData m_SunLight;
		[SerializeField]
		private HDAdditionalLightData m_MoonLight;

		private FNEEntity m_Player;

		private AtmosphereLayer m_BiomeAtmosphere = new AtmosphereLayer(
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			FogDensity.LIGHTEST
		);

		private AtmosphereLayer m_SiteAtmosphere = new AtmosphereLayer(
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			FogDensity.LIGHTEST
		);

		private AtmosphereLayer m_BaseAtmopshere = new AtmosphereLayer(
			Color.white,
			Color.white,
			Color.white,
			Color.white,
			FogDensity.LIGHTEST
		);


		void Start()
		{
			m_AtmosphereVolume.profile.TryGet<Fog>(out m_Fog);
		}

		void Update()
		{
			if (m_Player == null && GameClient.LocalPlayerEntity == null)
				return;
			else if (m_Player == null)
			{
				m_Player = GameClient.LocalPlayerEntity;
				m_Player.GetComponent<PlayerComponentClient>().d_OnCurrentSiteChanged += OnEnterSite;
			}

			var targetLayer = m_BaseAtmopshere;

			var playerPos = m_Player.Position;
			var tileData = GameClient.World.GetTileData((int)playerPos.x, (int)playerPos.y);
			if (tileData == null) return;
			var atmosphereRef = tileData.atmosphereRef;
			if (!string.IsNullOrEmpty(atmosphereRef))
			{
				var atmosphereData = DataBank.Instance.GetData<AtmosphereData>(atmosphereRef);

				m_BiomeAtmosphere = new AtmosphereLayer(
					FNEUtil.ConvertHexStringToColor(atmosphereData.fogTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.sunTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.moonTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.waterTint),
					atmosphereData.fogThickness
				);
			}
			else
			{
				m_BiomeAtmosphere = new AtmosphereLayer(
					Color.white,
					Color.white,
					Color.white,
					Color.white,
					FogDensity.LIGHTEST
				);
			}

			if (!m_BiomeAtmosphere.IsDefault())
			{
				targetLayer = m_BiomeAtmosphere;
			}

			if (!m_SiteAtmosphere.IsDefault())
			{
				targetLayer = m_SiteAtmosphere;
			}

			//m_Fog.meanFreePath.value = Mathf.Lerp(m_Fog.meanFreePath.value, (float) targetLayer.FogDense, Time.deltaTime);
			m_Fog.albedo.value = Color.Lerp(m_Fog.albedo.value, targetLayer.FogColor, Time.deltaTime);
			m_DensityVolume.parameters.albedo = Color.Lerp(m_DensityVolume.parameters.albedo, targetLayer.FogColor, Time.deltaTime);
			m_SunLight.color = Color.Lerp(m_SunLight.color, targetLayer.SunTint, Time.deltaTime);
			m_MoonLight.color = Color.Lerp(m_MoonLight.color, targetLayer.MoonTint, Time.deltaTime);
		}

		private void OnEnterSite(bool entered, string id)
		{
			if (!entered)
			{
				m_SiteAtmosphere = new AtmosphereLayer(
					Color.white,
					Color.white,
					Color.white,
					Color.white,
					FogDensity.LIGHTEST
				);
				return;
			}

			var siteData = DataBank.Instance.GetData<SiteData>(id);
			if (!string.IsNullOrEmpty(siteData.atmosphereRef))
			{
				var atmosphereData = DataBank.Instance.GetData<AtmosphereData>(siteData.atmosphereRef);

				m_SiteAtmosphere = new AtmosphereLayer(
					FNEUtil.ConvertHexStringToColor(atmosphereData.fogTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.sunTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.moonTint),
					FNEUtil.ConvertHexStringToColor(atmosphereData.waterTint),
					atmosphereData.fogThickness
				);
			}
			else
			{
				m_SiteAtmosphere = new AtmosphereLayer(
					Color.white,
					Color.white,
					Color.white,
					Color.white,
					FogDensity.LIGHTEST
				);
			}
		}
	}
}