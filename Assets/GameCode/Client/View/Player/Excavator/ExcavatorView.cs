using FNZ.Client.Model.Entity.Components.Excavatable;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.View.Audio;
using FNZ.Client.View.Camera;
using FNZ.Client.View.Manager;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Misc;
using FNZ.Shared.Utils;
using FNZ.Shared.Utils.CollisionUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace FNZ.Client.View.Player.Excavator 
{
	public class ExcavatorView : MonoBehaviour
	{
		private FNEEntity m_Player;
		private ExcavatableComponentClient m_ExcavatedComp;

		private ExcavatorComponentClient m_excavator;

		private GameObject m_ExcavatedGameObject;
		private Vector3[] m_ExcavatedVertices;

		private Vector3 m_TargetWorldPoint;
        public ParticleSystemForceField m_EndForceField;

        public Transform T_Muzzle;

		public ParticleSystem ExcavatedAnchorParticles;
		public ParticleSystem ExcavatedParticles;

		public ParticleSystem LoadupParticles;

        public ParticleSystem ChanneledParticles;

        private float m_MaxSizeMod = 0.01f;

		[SerializeField]
		private GameObject m_ForceField;

        private AudioSource m_LoopSource;

        [SerializeField]
        private GameObject P_HDRPOutline;
        private GameObject m_OutlineObject;

        [SerializeField]
        private Animator m_PlayerAnim;

        [SerializeField]
        private Light m_ExcavatorLight;

        private bool m_IsRemote;

        private Color m_TargetColor;
        private Color m_CurrentColor;

        private Tuple<byte, Vector2>[] m_BonusSpans;
        private float m_ExcavatingProgress;

        public void Init(FNEEntity player)
		{
			m_Player = player;
			m_excavator = player.GetComponent<ExcavatorComponentClient>();

            m_excavator.d_OnExcavatorStartFiring += OnFire;
			m_excavator.d_OnExcavatorStopFiring += OnStopFire;
            m_excavator.d_OnExcavatorProgressChange += OnProgressChange;
            m_excavator.d_OnExcavatorNewBonusSpans += OnNewBonusSpans;

            ExcavatedAnchorParticles.transform.SetParent(null);
			m_ForceField.transform.SetParent(null);
            m_ForceField.transform.position = transform.position;

            m_EndForceField.transform.SetParent(null);

            ExcavatedAnchorParticles.gameObject.SetActive(false);
            ChanneledParticles.Stop();

            if (m_Player == GameClient.LocalPlayerEntity)
            {
                m_OutlineObject = Instantiate(P_HDRPOutline);
                m_OutlineObject.SetActive(false);
            }
            else
            {
                m_IsRemote = true;
            }

            enabled = false;
        }

        float timeSinceFire = 0;
		float beamTimer = 0;
		void Update()
	    {

            if(m_ExcavatedComp == null || !m_ExcavatedComp.ParentEntity.Enabled)
            {
                LoadupParticles.Stop();
                ChanneledParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ExcavatedAnchorParticles.Stop(true);
                HighLightTarget();
            }
            else
            {
                if(!m_IsRemote)
                    CameraScript.shakeCamera(10f * Time.deltaTime, 2f);

                EffectData effectData = null;

                m_EndForceField.gravity = new ParticleSystem.MinMaxCurve(
                    Mathf.Lerp(m_EndForceField.gravity.constant, 0, Time.deltaTime * 20)
                );

                m_TargetColor = Color.gray;

                if (!m_IsRemote)
                {
                    for (int i = 0; i < m_BonusSpans.Length; i++)
                    {
                        var span = m_BonusSpans[i];
                        var bonusData = m_ExcavatedComp.Data.excavatableBonuses[span.Item1];

                        var baseTime = m_ExcavatedComp.Data.baseExcavateTime;
                        var spanStartPercent = span.Item2.x / baseTime;
                        var spanEndPercent = span.Item2.y / baseTime;

                        if (m_ExcavatingProgress > spanStartPercent && m_ExcavatingProgress < spanEndPercent)
                        {
                            var colorString = DataBank.Instance.GetData<ColorData>(bonusData.colorRef).colorCode;
                            var color = FNEUtil.ConvertHexStringToColor(colorString);

                            m_TargetColor = Color.Lerp(Color.gray, color, 0.5f);
                        }
                    }
                }

                m_CurrentColor = Color.Lerp(m_CurrentColor, m_TargetColor, Time.deltaTime * 100);

                ParticleSystemRenderer pr = (ParticleSystemRenderer)ChanneledParticles.GetComponent<Renderer>();
                var mat = pr.sharedMaterial;
                mat.SetColor("_EmissiveColor", m_CurrentColor * 500);
                pr.sharedMaterial = mat;

                var subEmitter = ChanneledParticles.transform.GetChild(0).GetComponent<ParticleSystem>();
                pr = (ParticleSystemRenderer)subEmitter.transform.GetComponent<Renderer>();
                mat = pr.sharedMaterial;
                mat.SetColor("_EmissiveColor", m_CurrentColor * 500);
                pr.sharedMaterial = mat;

                pr = (ParticleSystemRenderer)ExcavatedAnchorParticles.GetComponent<Renderer>();
                mat = pr.sharedMaterial;
                mat.SetColor("_EmissiveColor", m_CurrentColor * 500);
                pr.sharedMaterial = mat;

                pr = (ParticleSystemRenderer)LoadupParticles.GetComponent<Renderer>();
                mat = pr.sharedMaterial;
                mat.SetColor("_EmissiveColor", m_CurrentColor * 500);
                pr.sharedMaterial = mat;

                if (m_TargetColor.Equals(Color.gray))
                {
                    pr = (ParticleSystemRenderer) ExcavatedParticles.GetComponent<Renderer>();
                    mat = pr.sharedMaterial;
                    var color = Color.Lerp(mat.GetColor("_EmissiveColor") / 500, Color.gray, Time.deltaTime * 5);
                    mat.SetColor("_EmissiveColor", color * 500);
                    pr.sharedMaterial = mat;
                }
                else
                {
                    pr = (ParticleSystemRenderer)ExcavatedParticles.GetComponent<Renderer>();
                    mat = pr.sharedMaterial;
                    mat.SetColor("_EmissiveColor", m_CurrentColor * 500);
                    pr.sharedMaterial = mat;
                }

                m_ExcavatorLight.color = m_CurrentColor;

                beamTimer += Time.deltaTime;
                timeSinceFire += Time.deltaTime;
                if (beamTimer > 0.4f)
                {
                    beamTimer -= 0.4f;

                    var vertex = m_ExcavatedVertices[Random.Range(0, m_ExcavatedVertices.Length)];
                    m_TargetWorldPoint = m_ExcavatedGameObject.transform.TransformPoint(vertex);
                }

                if(m_ExcavatorLight.intensity <= 150)
                {
                    m_ExcavatorLight.intensity = Mathf.Lerp(m_ExcavatorLight.intensity, 150, Time.deltaTime * 2.5f);
                }

                if (ChanneledParticles.isStopped && timeSinceFire > 0.8f)
                {
                    ChanneledParticles.Stop();
                    ChanneledParticles.Play();

                    if (FNERandom.GetRandomIntInRange(0, 2) == 0)
                    {
                        effectData = DataBank.Instance.GetData<EffectData>("excavator_fire");
                    }
                    else
                    {
                        effectData = DataBank.Instance.GetData<EffectData>("excavator_fire2");
                    }

                    AudioManager.Instance.PlaySfx3dClip(effectData.sfxRef, new float2(T_Muzzle.position.x, T_Muzzle.position.z), 10);

                    m_LoopSource = AudioManager.Instance.PlaySfx3dClip(
                        "sfx_excavator_channel", 
                        new float2(T_Muzzle.position.x, T_Muzzle.position.z),
                        10
                    );
                    m_LoopSource.transform.SetParent(T_Muzzle);
                    m_LoopSource.loop = true;

                    if(!m_IsRemote)
                        CameraScript.shakeCamera(5f, 5f);

                    if(m_OutlineObject != null)
                        m_OutlineObject.SetActive(false);

                    m_ExcavatorLight.intensity = 150;

                    ExcavatedAnchorParticles.Play();
                }

                m_ForceField.transform.position = Vector3.Lerp(
                    m_ForceField.transform.position,
                    m_TargetWorldPoint,
                    5f * Time.deltaTime
                );

                m_MaxSizeMod = 1f / m_ExcavatedComp.GetHitsRemaining();
                var emissionMod = ExcavatedParticles.main;
                emissionMod.startSize = new ParticleSystem.MinMaxCurve(
                    0.01f,
                    Mathf.Lerp(emissionMod.startSize.constantMax, 0.2f * m_MaxSizeMod, 2 * Time.deltaTime)
                );
            }
		}

        private void HighLightTarget()
        {
            // Remote players
            if (m_OutlineObject == null)
                return;

            if (UIManager.Instance.IsBuilderOpen())
            {
                m_OutlineObject.SetActive(false);
                m_OutlineObject.transform.position = Vector3.zero;
                return;
            }

            var aimDirection = new float2(
                Mathf.Cos(math.radians(m_Player.RotationDegrees)),
                Mathf.Sin(math.radians(m_Player.RotationDegrees))
            );

            float2 endPoint = m_Player.Position + (aimDirection * m_excavator.Data.BaseRange * 0.75f);
            var hit = FNECollisionUtils.ExcavatorRayCastForWallsAndTileObjectsInModel(m_Player.Position, endPoint, GameClient.World);
            var hitEntity = hit.HitEntity;

            if (hit.IsHit && hitEntity.HasComponent<ExcavatableComponentClient>())
            {
                var viewRef = FNEEntity.GetEntityViewVariationId(hitEntity.Data, hitEntity.Position);
                var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewRef);
                var template = GameObjectPoolManager.GetObjectInstance(
                    viewRef,
                    PrefabType.FNEENTIY,
                    hitEntity.Position
                );
                m_OutlineObject.transform.localScale = Vector3.one * viewData.scaleMod;

                if (template != null)
                {
                    Quaternion rotation = Quaternion.AngleAxis(-hitEntity.RotationDegrees, Vector3.up);
                    var posVector = new Vector3(hitEntity.Position.x, -viewData.heightPos, hitEntity.Position.y);
                    if (hitEntity.Data.entityType == EntityType.TILE_OBJECT)
                    {
                        m_OutlineObject.transform.position = posVector + new Vector3(0.5f, 0.0f, 0.5f);
                        m_OutlineObject.transform.rotation = rotation;
                    }
                    else
                    {
                        m_OutlineObject.transform.position = posVector;
                        m_OutlineObject.transform.rotation = rotation;
                    }

                    m_OutlineObject.SetActive(true);

                    var meshFilter = template.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        m_OutlineObject.GetComponent<MeshFilter>().mesh = meshFilter.mesh;
                    }
                    else
                    {
                        var meshRenderer = template.GetComponentInChildren<SkinnedMeshRenderer>();
                        m_OutlineObject.GetComponentInChildren<MeshFilter>().mesh = meshRenderer.sharedMesh;
                    }

                    GameObjectPoolManager.DoRecycle(viewRef, template);
                }
            }
            else
            {
                m_OutlineObject.SetActive(false);
            }
        }

        private void OnNewBonusSpans(FNEEntity hitEntity, Tuple<byte, Vector2>[] bonusSpans)
        {
            if (m_ExcavatedComp != null)
            {
                m_EndForceField.gravity = -1;
                if(!m_IsRemote)
                    CameraScript.shakeCamera(5f, 5f);
                AudioManager.Instance.PlaySfx3dClip("sfx_excavator_complete", new float2(T_Muzzle.position.x, T_Muzzle.position.z), 10);
            }

            this.m_BonusSpans = bonusSpans;
        }

        private void OnProgressChange(float progress)
        {
            m_ExcavatingProgress = progress;
        }

        private void OnFire(FNEEntity hitEntity, Tuple<byte, Vector2>[] bonusSpans)
		{
            this.m_BonusSpans = bonusSpans;

            if (m_IsRemote)
                enabled = true;

			var entityData = hitEntity.Data;

			m_ExcavatedComp = hitEntity.GetComponent<ExcavatableComponentClient>();

			var viewRef = FNEEntity.GetEntityViewVariationId(hitEntity.Data, hitEntity.Position);
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewRef);
			m_ExcavatedGameObject = GameObjectPoolManager.GetObjectInstance(
				viewRef,
				PrefabType.FNEENTIY,
				hitEntity.Position
			);
			m_ExcavatedGameObject.transform.localScale = Vector3.one * viewData.scaleMod;

			if (m_ExcavatedGameObject != null)
			{
				Quaternion rotation = Quaternion.AngleAxis(hitEntity.RotationDegrees, Vector3.down);
				var posVector = new Vector3(hitEntity.Position.x, -viewData.heightPos, hitEntity.Position.y);
				if (entityData.entityType == EntityType.TILE_OBJECT)
				{
					m_ExcavatedGameObject.transform.position = posVector + new Vector3(0.5f, 0.0f, 0.5f);
					m_ExcavatedGameObject.transform.rotation = rotation;
				}
				else
				{
					m_ExcavatedGameObject.transform.position = posVector;
					m_ExcavatedGameObject.transform.rotation = rotation;
				}

                var shapeMod = ExcavatedAnchorParticles.shape;
                var meshFilter = m_ExcavatedGameObject.GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    m_ExcavatedVertices = meshFilter.mesh.vertices;
                    shapeMod.mesh = meshFilter.mesh;
                }
                else
                {
                    var meshRenderer = m_ExcavatedGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                    m_ExcavatedVertices = meshRenderer.sharedMesh.vertices;
                    shapeMod.mesh = meshRenderer.sharedMesh;
                }
                
                m_ExcavatedGameObject.SetActive(false);

				ExcavatedAnchorParticles.transform.position = m_ExcavatedGameObject.transform.position;
				ExcavatedAnchorParticles.gameObject.SetActive(true);
				ExcavatedAnchorParticles.transform.rotation = m_ExcavatedGameObject.transform.rotation;
				ExcavatedAnchorParticles.transform.localScale = Vector3.one * viewData.scaleMod;

                m_EndForceField.transform.position = m_ExcavatedGameObject.transform.position;

                ChanneledParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                LoadupParticles.Stop();
				LoadupParticles.Play();

                m_ExcavatorLight.intensity = 0;
                m_ExcavatorLight.enabled = true;

                timeSinceFire = 0;

                AudioManager.Instance.PlaySfx3dClip("sfx_excavator_loadup_fne", new float2(T_Muzzle.position.x, T_Muzzle.position.z), 10);

                var vertex = m_ExcavatedVertices[Random.Range(0, m_ExcavatedVertices.Length)];
                m_ForceField.transform.position = m_ExcavatedGameObject.transform.TransformPoint(vertex);
                m_TargetWorldPoint = m_ExcavatedGameObject.transform.TransformPoint(vertex);

                m_EndForceField.gravity = 0;

                m_PlayerAnim.SetBool("ContinuousFire", true);
                m_PlayerAnim.SetTrigger("Fire");
            }
		}

		private void OnStopFire()
		{
            if (m_LoopSource != null)
            {
                m_LoopSource.transform.SetParent(null);
                m_LoopSource.loop = false;
                m_LoopSource.Stop();
                m_LoopSource = null;
            }
            
            LoadupParticles.Stop();
            ChanneledParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ExcavatedAnchorParticles.Stop(true);

            m_PlayerAnim.SetBool("ContinuousFire", false);

            if (m_ExcavatedComp != null)
            {
                m_EndForceField.gravity = -1;
                m_ExcavatedComp = null;
                if(!m_IsRemote)
                    CameraScript.shakeCamera(5f, 5f);

                AudioManager.Instance.PlaySfx3dClip("sfx_excavator_complete", new float2(T_Muzzle.position.x, T_Muzzle.position.z), 10);
            }

            m_ExcavatorLight.enabled = false;
            
            if (m_IsRemote)
                enabled = false;
        }

        public void OnExcavatorActivation()
        {
            enabled = true;
        }

        public void OnExcavatorDeactivation()
        {
            enabled = false;
            m_OutlineObject.SetActive(false);
        }
    }
}