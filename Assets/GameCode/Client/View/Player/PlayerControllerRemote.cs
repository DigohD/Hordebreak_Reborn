using FNZ.Client.Model.Entity.Components.Health;
using FNZ.Client.Model.Entity.Components.Name;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.Player.Excavator;
using FNZ.Client.View.Player.PlayerStitching;
using FNZ.Client.View.Player.Systems;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.Player;
using FNZ.Shared.Model.Items.Components;
using System;
using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.Player
{
	public class PlayerControllerRemote : MonoBehaviour
	{
		public FNEEntity m_PlayerEntity;
		private PlayerComponentClient m_PlayerComponentClient;

		private Animator m_Animator;

		private Transform T_NamePlate;

		private PlayerMeshStitcher m_PlayerMeshStitcher;

        private ExcavatorView m_ExcavatorView;

        void Start()
		{
			m_PlayerComponentClient = m_PlayerEntity.GetComponent<PlayerComponentClient>();
			m_Animator = GetComponentInChildren<Animator>();
			
			m_PlayerMeshStitcher.SetDefaultMeshes(m_PlayerComponentClient.GetPlayerViewSetup(), 0);
			m_PlayerMeshStitcher.InitStitch(m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>());
		}

		public void Init(FNEEntity playerEntity)
		{
			m_PlayerEntity = playerEntity;

			GameObject namePlate = Instantiate(Resources.Load<GameObject>("Prefab/Entity/Player/PlayerName"));
			namePlate.transform.SetParent(UIManager.Instance.PlayerNames);
			namePlate.transform.localPosition = new Vector3(0, 0, -2);
			T_NamePlate = namePlate.transform;
			UpdateNamePlateText(false);

			var statComp = m_PlayerEntity.GetComponent<StatComponentClient>();

			m_PlayerEntity.GetComponent<PlayerComponentClient>().d_AfkChange += UpdateNamePlateText;
			
			m_PlayerMeshStitcher = gameObject.GetComponentInChildren<PlayerMeshStitcher>();
			m_PlayerMeshStitcher.Init(m_PlayerEntity);
			
            m_ExcavatorView = GetComponent<ExcavatorView>();
            m_ExcavatorView.Init(m_PlayerEntity);
        }

		private void Update()
		{
			transform.position = Vector3.Lerp(
				transform.position,
				new Vector3(
					m_PlayerEntity.Position.x,
					0,
					m_PlayerEntity.Position.y
				),
				Time.deltaTime * 14f
			);

			transform.rotation = Quaternion.Lerp(
				transform.rotation, 
				Quaternion.AngleAxis(m_PlayerEntity.RotationDegrees, Vector3.down), 
				Time.deltaTime * 14f
			);

			UpdateNamePlateTransform();
		}

		private void UpdateNamePlateTransform()
		{
            //T_NamePlate.LookAt(T_NamePlate.position + UnityEngine.Camera.main.transform.rotation * Vector3.forward,
            //	UnityEngine.Camera.main.transform.rotation * Vector3.up);

            var pos = UnityEngine.Camera.main.WorldToScreenPoint(transform.position + (Vector3.up * 2f));
            T_NamePlate.position = pos;
		}

		private void UpdateNamePlateText(bool afk)
		{
			string name;

			foreach (var tm in T_NamePlate.GetComponentsInChildren<Text>())
			{
				name = m_PlayerEntity.GetComponent<NameComponentClient>().entityName;

				tm.text = afk ? "<AFK> " + name : name;
			}
		}

		public void NetUpdateAnimator(PlayerAnimatorData data)
		{
			if (!m_Animator)
				return;

			m_Animator.SetLayerWeight(1, data.WeaponPosture == WeaponPosture.RIFLE ? 1 : 0);
			m_Animator.SetLayerWeight(2, data.WeaponPosture == WeaponPosture.HEAVY ? 1 : 0);
			m_Animator.SetLayerWeight(3, data.WeaponPosture == WeaponPosture.LIGHT ? 1 : 0);
			m_Animator.SetLayerWeight(4, data.WeaponPosture == WeaponPosture.THROW ? 1 : 0);

			m_Animator.SetBool("Idle", data.Idle);
			m_Animator.SetBool("Sprinting", data.Sprinting);

			m_Animator.SetFloat("MovementX", data.MovementX);
			m_Animator.SetFloat("MovementY", data.MovementY);

			/*foreach (var animationEnum in Enum.GetValues(typeof(PlayerAnimationState)).Cast<PlayerAnimationState>())
			{
				int flagBit = (int)animationEnum;

				// Enum names must match animation bools in animator
				m_Animator.SetBool(animationEnum.ToString(), (newMask & flagBit) > 0);
			}*/
		}

		public void PlayOneShotAnimation(OneShotAnimationType animType)
		{
			Debug.LogWarning("PlayOneShotAnimation Where is it called?");
			m_Animator.SetTrigger(animType.ToString());
		}

		public FNEEntity GetPlayerEntity()
		{
			return m_PlayerEntity;
		}

		private void OnDestroy()
		{
			GameObject.Destroy(T_NamePlate.gameObject);
		}
	}
}