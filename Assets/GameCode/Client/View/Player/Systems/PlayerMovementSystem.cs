using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{
	public class PlayerMovementSystem
	{
		public static Vector2 MouseHoverPosition;

		public Vector3 m_InputDirection;
		public Vector3 m_DesiredDirection;

		public float2 Velocity { get; set; }

		private float m_BaseSpeed;
		private float m_BaseSprintMultiplier = 1.75f;

		private PlayerController m_PlayerController;

		private PlayerAnimationSystem m_PlayerAnimationSystem;
		private PlayerComponentClient m_PlayerComponent;

		private Vector3 m_LastMouseImpactPoint;

		public PlayerMovementSystem(PlayerController playerController)
		{
			m_PlayerController = playerController;
			m_PlayerComponent = playerController.m_PlayerEntity.GetComponent<PlayerComponentClient>();

			m_BaseSpeed = m_PlayerComponent.m_Data.baseSpeed;
		}

		public void Update()
		{
			if (m_PlayerAnimationSystem == null)
			{
				m_PlayerAnimationSystem = m_PlayerController.GetPlayerAnimationSystem();
			}

			UpdateOrientation();
			UpdatePlayerPosition();
			Velocity = GetVelocity();
			DetermineAnimation();
		}

		public void UpdateOrientation()
		{
			if (!m_PlayerComponent.IsSprinting() || math.length(Velocity) == 0)
			{
				AimToMouse();
			}
			else
			{
				AimWithVelocity();
			}

		}

		public void UpdateTargetVelocity(Vector3 direction)
		{
			m_InputDirection += direction;
		}

		public void UpdatePlayerPosition()
		{
			float2 desiredVelocity = Velocity;

			m_PlayerController.m_PlayerEntity.Position += desiredVelocity;


			m_PlayerController.transform.position = Vector3.Lerp(
				m_PlayerController.transform.position,
				new Vector3(
					m_PlayerController.m_PlayerEntity.Position.x,
					0,
					m_PlayerController.m_PlayerEntity.Position.y
				),
				15f * Time.deltaTime
			);

			UIManager.Instance.UpdatePlayerRoom();

			if (math.length(desiredVelocity) > 0 && !m_PlayerComponent.IsSprinting())
			{
				Vector3 move = new Vector3(desiredVelocity.x, 0, desiredVelocity.y);
				Vector3 aim = m_LastMouseImpactPoint - m_PlayerController.transform.position;

				float angle = Vector2.Angle(move, aim);
			}
		}

		public float GetSpeed()
		{
			float speed = Time.deltaTime * m_BaseSpeed;

			if (m_PlayerComponent.IsSprinting())
				speed *= m_BaseSprintMultiplier;

			return speed;
		}

		public void ResetVelocityVector()
		{
			m_InputDirection = Vector3.zero;
		}

		public Vector2 GetVelocity()
		{
			return m_InputDirection.normalized * GetSpeed();
		}

		public Vector2 GetBaseVelocity()
		{
			return m_InputDirection.normalized * Time.deltaTime * m_BaseSpeed;
		}

		public Vector2 GetInputDirection()
		{
			return m_InputDirection.normalized;
		}

		public Vector2 GetDesiredDirection()
		{
			return m_DesiredDirection.normalized;
		}

		private void AimToMouse()
		{
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			m_LastMouseImpactPoint = Vector3.zero;
			int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Body");

			if (Physics.Raycast(ray, out RaycastHit hit, 10000f, layerMask))
			{
				Transform objectHit = hit.transform;
				m_LastMouseImpactPoint = new Vector3(hit.point.x, 0, hit.point.z);
			}

			Vector3 dir = m_LastMouseImpactPoint - m_PlayerController.transform.position;
			MouseHoverPosition = m_LastMouseImpactPoint;
			float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

			m_PlayerController.m_PlayerEntity.RotationDegrees = angle;

			m_PlayerController.transform.rotation = Quaternion.AngleAxis(
				m_PlayerController.m_PlayerEntity.RotationDegrees,
				Vector3.down
			);
		}

		private void AimWithVelocity()
		{
			Vector3 target = m_PlayerController.transform.position + new Vector3(Velocity.x, 0, Velocity.y);
			Vector3 dir = target - m_PlayerController.transform.position;
			MouseHoverPosition = target;

			float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

			m_PlayerController.m_PlayerEntity.RotationDegrees = angle;

			Quaternion newRot = Quaternion.Lerp(
				m_PlayerController.transform.rotation,
				Quaternion.AngleAxis(
					m_PlayerController.m_PlayerEntity.RotationDegrees,
					Vector3.down
				),
				10f * Time.deltaTime
			);

			m_PlayerController.transform.rotation = newRot;
		}

		private void DetermineAnimation()
		{
			//If you're moving.
			if (m_InputDirection != Vector3.zero)
			{
				var viewDirection = new Vector3(m_InputDirection.x, 0, m_InputDirection.y);
				
				m_PlayerAnimationSystem.SetAnimatorBool("Idle", false);

				if (m_PlayerComponent.IsSprinting())
				{
					m_PlayerAnimationSystem.SetAnimatorBool("Sprinting", true);
				}
				else
				{
					m_PlayerAnimationSystem.SetAnimatorBool("Sprinting", false);
				}

				//Angle difference between where you are moving towards and where you are aiming.
				var angleDifference = Vector3.Angle(viewDirection.normalized, (m_LastMouseImpactPoint - m_PlayerController.transform.position).normalized);
				
				//Angle difference for strafing.
				var strafeAngle = Quaternion.AngleAxis(90, Vector3.down) * viewDirection;
				var strafeAngleDifference = Vector3.Angle(strafeAngle.normalized, (m_LastMouseImpactPoint - m_PlayerController.transform.position).normalized);

				//This sets forward / backpedal values for the animator.
				if (angleDifference <= 45) //moving towards mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementY", 1);
				else if (angleDifference > 45 && angleDifference < 80) //moving beside mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementY", FNEUtil.ScaleValueFloat(angleDifference, 45, 80, 1, 0));
				else if (angleDifference > 80 && angleDifference < 100)
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementY", 0);
				else if (angleDifference > 100 && angleDifference < 135)
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementY", FNEUtil.ScaleValueFloat(angleDifference, 100, 135, 0, -1));
				else //moving away from mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementY", -1);

				//This sets strafe left/right values for the animator.
				if (strafeAngleDifference <= 45) //moving towards mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementX", -1);
				else if (strafeAngleDifference > 45 && strafeAngleDifference < 80) //moving beside mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementX", FNEUtil.ScaleValueFloat(strafeAngleDifference, 45, 80, -1, 0));
				else if (strafeAngleDifference > 80 && strafeAngleDifference < 100)
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementX", 0);
				else if (strafeAngleDifference > 100 && strafeAngleDifference < 135)
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementX", FNEUtil.ScaleValueFloat(strafeAngleDifference, 100, 135, 0, 1));
				else //moving away from mouse pointer
					m_PlayerAnimationSystem.SetAnimatorFloat("MovementX", 1);
			}
			else //If you're not moving.
			{
				m_PlayerAnimationSystem.SetAnimatorBool("Idle", true);
				m_PlayerAnimationSystem.SetAnimatorBool("Sprinting", false);
			}

		}
	}
}