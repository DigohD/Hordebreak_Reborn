using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.Model.Entity.Components.Player;
using FNZ.Shared.Model.Items.Components;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{
	// Used as bitmask. All enum values must be powers of two.
	public enum PlayerAnimationState
	{
		IDLE = 1,
		WALKING = 2,
		SPRINTING = 4,
		REVERSE_WALKING = 8,
	}

	public enum OneShotAnimationType
	{
		Punch = 1,
		Fire = 2,
		Death = 3,
		Revive = 4,
		Throw_Overhead = 5
	}

	public class PlayerAnimationSystem
	{
		private Animator m_Animator;
		private PlayerController m_PlayerController;
		private float currentLayerWeight;
		private bool changeWeapon = false;

		private WeaponPosture currentWeaponPosture = WeaponPosture.HEAVY;
		private WeaponPosture previousWeaponPosture = WeaponPosture.HEAVY;

		private float transitionSpeed = 0.07f; //Lower is faster

		private float m_AnimNetTimer;
		private readonly float ANIMATION_SYNCHRONIZATION_INTERVAL = 0.2f;

		public PlayerAnimationSystem(PlayerController playerController)
		{
			m_PlayerController = playerController;
			m_Animator = m_PlayerController.GetComponentInChildren<Animator>();
			currentLayerWeight = m_Animator.GetLayerWeight(1);
		}

		public void Update()
		{
			m_AnimNetTimer += Time.deltaTime;
			if (m_AnimNetTimer >= ANIMATION_SYNCHRONIZATION_INTERVAL)
			{
				NetUpdateAnimator();
				m_AnimNetTimer = 0;
			}

			if (changeWeapon)
			{
				SetWeaponAnimationLayer(currentWeaponPosture, previousWeaponPosture);
			}
		}

		private void NetUpdateAnimator()
		{
			m_PlayerController.m_PlayerEntity.GetComponent<PlayerComponentClient>().NE_Send_AnimatorData(
				new PlayerAnimatorData
				{
					WeaponPosture = currentWeaponPosture,
					MovementX = m_Animator.GetFloat("MovementX"),
					MovementY = m_Animator.GetFloat("MovementY"),
					Sprinting = m_Animator.GetBool("Sprinting"),
					Idle = m_Animator.GetBool("Idle")
				}
			);
		}

		public void PlayOneShotAnimation(OneShotAnimationType animType)
		{
			m_Animator.SetTrigger(animType.ToString());
			GameClient.NetAPI.CMD_Player_animationEvent(m_PlayerController.m_PlayerEntity, animType);
		}

		public void SetAnimatorBool(string name, bool value)
		{
			m_Animator.SetBool(name, value);
		}

		public void SetAnimatorFloat(string name, float value)
		{
			m_Animator.SetFloat(name, value);
		}

		public void SetChangeWeapon(bool change)
		{
			changeWeapon = change;
		}

		public void SetCurrentWeaponType(WeaponPosture weaponPosture)
		{
			previousWeaponPosture = currentWeaponPosture;
			currentWeaponPosture = weaponPosture;
		}

		private void SetWeaponAnimationLayer(WeaponPosture weaponPosture, WeaponPosture prevWeaponPosture)
		{
			float velocity = 0;

			float weight1 = 0;
			float weight2 = 0;
			float weight3 = 0;
			float weight4 = 0;

			switch (weaponPosture)
			{
				case WeaponPosture.UNARMED:
					m_Animator.SetBool("WeaponEquipped", false);

					weight1 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(1), 0, ref velocity, transitionSpeed);
					weight2 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(2), 0, ref velocity, transitionSpeed);
					weight3 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(3), 0, ref velocity, transitionSpeed);
					weight4 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(4), 0, ref velocity, transitionSpeed);

					if (weight1 == 0 && weight2 == 0 && weight3 == 0 && weight4 == 0)
						changeWeapon = false;
					break;

				case WeaponPosture.RIFLE:
					m_Animator.SetBool("WeaponEquipped", true);

					weight1 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(1), 1, ref velocity, transitionSpeed);
					weight2 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(2), 0, ref velocity, transitionSpeed);
					weight3 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(3), 0, ref velocity, transitionSpeed);
					weight4 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(4), 0, ref velocity, transitionSpeed);

					if (weight1 == 1 && weight2 == 0 && weight3 == 0 && weight4 == 0)
						changeWeapon = false;
					break;

				case WeaponPosture.HEAVY:
					m_Animator.SetBool("WeaponEquipped", true);

					weight1 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(1), 0, ref velocity, transitionSpeed);
					weight2 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(2), 1, ref velocity, transitionSpeed);
					weight3 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(3), 0, ref velocity, transitionSpeed);
					weight4 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(4), 0, ref velocity, transitionSpeed);

					if (weight1 == 0 && weight2 == 1 && weight3 == 0 && weight4 == 0)
						changeWeapon = false;
					break;

				case WeaponPosture.LIGHT:
					m_Animator.SetBool("WeaponEquipped", true);

					weight1 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(1), 0, ref velocity, transitionSpeed);
					weight2 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(2), 0, ref velocity, transitionSpeed);
					weight3 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(3), 1, ref velocity, transitionSpeed);
					weight4 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(4), 0, ref velocity, transitionSpeed);

					if (weight1 == 0 && weight2 == 0 && weight3 == 1 && weight4 == 0)
						changeWeapon = false;
					break;

				case WeaponPosture.THROW:
					m_Animator.SetBool("WeaponEquipped", true);

					weight1 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(1), 0, ref velocity, transitionSpeed);
					weight2 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(2), 0, ref velocity, transitionSpeed);
					weight3 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(3), 0, ref velocity, transitionSpeed);
					weight4 = Mathf.SmoothDamp(m_Animator.GetLayerWeight(4), 1, ref velocity, transitionSpeed);

					if (weight1 == 0 && weight2 == 0 && weight3 == 0 && weight4 == 1)
						changeWeapon = false;
					break;
			}

			m_Animator.SetLayerWeight(1, weight1);
			m_Animator.SetLayerWeight(2, weight2);
			m_Animator.SetLayerWeight(3, weight3);
			m_Animator.SetLayerWeight(4, weight4);
		}

	}
}