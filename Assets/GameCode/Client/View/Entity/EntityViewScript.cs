using System;
using FNZ.Client.Model.Entity.Components.Refinement;
using FNZ.Client.View.Audio;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Entity;
using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.RoomRequirements;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace FNZ.Client.View.EntityView 
{
	public class EntityViewScript : MonoBehaviour
	{
		private FNEEntity m_ParentEntity;

		private VisualEffect m_ViewEffect;
		private Light m_ViewLight;

		private RoomRequirementsComponentClient m_RoomReqsComp;
		private RefinementComponentClient m_RefineComp;

		private AudioSource m_LoopSource;

		private bool fulfillsRoomReqs;

		private EntityLightSourceData m_LightData;
		private float m_TargetLightIntensity;
		private float m_FlickerTimer = 0;
		private bool m_FlickerPeriod = false;

		private bool m_EffectsActive;
		
		public void Init(FNEEntity parentEntity)
		{
			m_ParentEntity = parentEntity;

			m_ViewEffect = GetComponentInChildren<VisualEffect>();
			m_ViewLight = GetComponentInChildren<Light>();

			var viewId = FNEEntity.GetEntityViewVariationId(
				m_ParentEntity.Data, 
				new float2(parentEntity.Position.x, parentEntity.Position.y));

			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewId);
			if (viewData.entityLightSourceData != null)
			{
				m_LightData = viewData.entityLightSourceData;
			}

			ConnectComponents();

			DeactivateEffects();
		}

		private void ConnectComponents()
		{
			if (m_ParentEntity.HasComponent<RoomRequirementsComponentClient>())
			{
				m_RoomReqsComp = m_ParentEntity.GetComponent<RoomRequirementsComponentClient>();
			}
			if (m_ParentEntity.HasComponent<RefinementComponentClient>())
			{
				m_RefineComp = m_ParentEntity.GetComponent<RefinementComponentClient>();
				m_RefineComp.d_OnRefinementWorkingUpdate += RefinementUpdate;
			}
		}

		void Update()
		{
			var roomId = GameClient.World.GetTileRoom(new float2(
				transform.position.x,
				transform.position.z));
			var room = GameClient.RoomManager.GetRoom(roomId);
			
			var wasFulfilled = fulfillsRoomReqs;
			
			if (m_RoomReqsComp != null && m_RefineComp == null)
			{
				if (room != null)
				{
					var baseOnline = GameClient.RoomManager.IsBaseOnline(room.ParentBaseId);
					if (!baseOnline || !room.DoesRoomFulfillRequirements(
						m_RoomReqsComp.Data.propertyRequirements))
					{
						fulfillsRoomReqs = false;
					}else
					{
						fulfillsRoomReqs = true;
					}
				}
				else
				{
					fulfillsRoomReqs = false;
				}

				if (wasFulfilled != fulfillsRoomReqs)
				{
					if (fulfillsRoomReqs)
					{
						ActivateEffects();
					}
					else
					{
						DeactivateEffects();
					}
				}
			}else if (!m_EffectsActive && m_RoomReqsComp == null && m_RefineComp == null)
			{
				ActivateEffects();
			}
			
			UpdateLight();
		}

		private void UpdateLight()
		{
			if (m_ViewLight != null && m_ViewLight.enabled && m_LightData != null && m_LightData.flickerIntensity > 0)
			{
				m_FlickerTimer -= Time.deltaTime;

				if (m_FlickerTimer <= 0)
				{
					m_FlickerPeriod = !m_FlickerPeriod;
					m_FlickerTimer = FNERandom.GetRandomFloatInRange(m_LightData.flickerIntervalMin, m_LightData.flickerIntervalMax);
					var flickerIntensity = Mathf.Clamp(m_LightData.flickerIntensity, 0, 1);
					var diff = m_LightData.flickerIntensity / 2f;
					float target = 0;
					if (m_FlickerPeriod)
					{
						target = FNERandom.GetRandomFloatInRange(flickerIntensity, 1);
					}
					else
					{
						target = FNERandom.GetRandomFloatInRange(0, 1 - flickerIntensity);
					}
					m_TargetLightIntensity = m_LightData.intensity * target;
				}

				m_ViewLight.intensity = Mathf.Lerp(m_ViewLight.intensity, m_TargetLightIntensity, (1 - m_LightData.flickerSmoothness));
			}
		}
		
		private void RefinementUpdate(bool isWorking)
		{
			if (isWorking)
			{
				ActivateEffects();
				RefinementActivationSFX();
			}
			else
			{
				DeactivateEffects();
				RefinementDeactivationSFX();
			}
		}

		private void ActivateEffects()
		{
			if (m_ViewEffect != null)
			{
				m_ViewEffect.Play();
			}
			if (m_ViewLight != null)
			{
				m_ViewLight.enabled = true;
			}
		}

		private void DeactivateEffects()
		{
			if (m_ViewEffect != null)
			{
				m_ViewEffect.Stop();
			}
			if (m_ViewLight != null)
			{
				m_ViewLight.enabled = false;
			}
		}

		private void RefinementActivationSFX()
		{
			var startSfxRef = m_RefineComp.Data.startSFXRef;
			if (!string.IsNullOrEmpty(startSfxRef))
			{
				AudioManager.Instance.PlaySfx3dClip(startSfxRef, new float2(transform.position.x, transform.position.z));
			}

			var loopingSfxRef = m_RefineComp.Data.activeSFXLoopRef;
			if (!string.IsNullOrEmpty(loopingSfxRef))
			{
				m_LoopSource = AudioManager.Instance.PlaySfx3dClip(loopingSfxRef, new float2(transform.position.x, transform.position.z));
				m_LoopSource.loop = true;
			}
		}

		private void RefinementDeactivationSFX()
		{
			var sfxRef = m_RefineComp.Data.stopSFXRef;
			if (!string.IsNullOrEmpty(sfxRef))
			{
				AudioManager.Instance.PlaySfx3dClip(sfxRef, new float2(transform.position.x, transform.position.z));
			}
			if (m_LoopSource != null)
			{
				m_LoopSource.loop = false;
				m_LoopSource.Stop();
			}
		}
	}
}