using FNZ.Client.Model.Entity.Components.Player;
using UnityEngine;

namespace FNZ.Client.View.Player.Systems
{

	public class PlayerAimingSystem
	{
		private Transform aimingChestBone;
		private Transform aimingSpineBone;
		private Transform rootSpineBone;

		public static Vector2 MouseHoverPosition;

		private Vector3 m_LastMouseImpactPoint;
		private PlayerController m_PlayerController;
		private PlayerComponentClient m_PlayerComponent;

		public PlayerAimingSystem(PlayerController playerController)
		{
			m_PlayerController = playerController;
			m_PlayerComponent = playerController.m_PlayerEntity.GetComponent<PlayerComponentClient>();

			rootSpineBone = m_PlayerController.GetComponentInChildren<Animator>().transform.Find("Spine1");

			aimingSpineBone = m_PlayerController.GetComponentInChildren<Animator>().transform.Find("Spine1/Spine2");

			aimingChestBone = m_PlayerController.GetComponentInChildren<Animator>().transform.Find("Spine1/Spine2/Spine3");
		}

		public void Update()
		{
			if (!m_PlayerComponent.IsSprinting())
			{
				AimUpperBody();
			}
		}

		public void AimUpperBody()
		{
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			m_LastMouseImpactPoint = Vector3.zero;
			int layerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Body");

			if (Physics.Raycast(ray, out RaycastHit hit, 10000f, layerMask))
			{
				m_LastMouseImpactPoint = hit.point;
				m_LastMouseImpactPoint = new Vector3(m_LastMouseImpactPoint.x, 0, m_LastMouseImpactPoint.z);
			}

			Vector3 dir = m_LastMouseImpactPoint - m_PlayerController.transform.position;
			MouseHoverPosition = m_LastMouseImpactPoint;
			float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

			m_PlayerController.m_PlayerEntity.RotationDegrees = angle;

			aimingSpineBone.rotation = Quaternion.AngleAxis(
				angle,
				Vector3.down
			) * aimingSpineBone.rotation;

			if (Quaternion.Angle(aimingSpineBone.rotation, rootSpineBone.rotation) >= 45)
			{
				Quaternion oldAimingRotation = aimingSpineBone.rotation;
				float rotationDifference = Quaternion.Angle(aimingSpineBone.rotation, rootSpineBone.rotation);
				float amountToRotate = rotationDifference - (rotationDifference - 45);
				Quaternion lowerBodyRotation = Quaternion.AngleAxis(
					amountToRotate,
					Vector3.down
				) * aimingSpineBone.rotation;

				rootSpineBone.rotation = Quaternion.Lerp(
					rootSpineBone.rotation,
					lowerBodyRotation,
					//10f * Time.time
					10
				   );

				//Reset upperbody rotation to its old rotation
				aimingSpineBone.rotation = oldAimingRotation;
			}
		}
	}
}