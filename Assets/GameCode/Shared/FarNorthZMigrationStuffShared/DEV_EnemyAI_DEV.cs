using UnityEngine;

namespace FNZ.Shared.FarNorthZMigrationStuff
{
	//DEV_ONLY
	public class DEV_EnemyAI_DEV : MonoBehaviour
	{
		private Vector3 m_TargetPosition;
		private Quaternion m_TargetRotation = Quaternion.identity;

		private float m_FloatValue;

		private void Start()
		{
			m_TargetPosition = transform.position;
		}

		private void Update()
		{
			m_FloatValue += Time.deltaTime;
			if (m_FloatValue > 1)
				m_FloatValue = 1;

			transform.position = Vector3.Lerp(transform.position, m_TargetPosition, m_FloatValue);
			transform.rotation = Quaternion.Lerp(transform.rotation, m_TargetRotation, m_FloatValue);
		}

		public void NewTargetPos(Vector2 tp, float rot)
		{
			m_TargetPosition.x = tp.x;
			m_TargetPosition.z = tp.y;
			m_TargetRotation = Quaternion.Euler(0, rot, 0);
			m_FloatValue = 0;
		}
	}
	//DEV_ONLY
}