using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.ScreenEffects
{
	public class UI_Arrow : MonoBehaviour
	{
		public Text nameText;

		private Image m_Img;

		private Vector3 m_Pos;

		public void Init(string name, float2 pos)
		{
			m_Pos = new Vector3(pos.x, 0, pos.y);
			nameText.text = name;
			m_Img = GetComponent<Image>();
		}

		public void UpdatePosition(float2 pos)
		{
			m_Pos = new Vector3(pos.x, 0, pos.y);
		}

		void Update()
		{
			if (GameClient.LocalPlayerEntity == null)
				return;

			Vector2 localPlayerCameraSpacePos = UnityEngine.Camera.main.WorldToScreenPoint(new Vector3(GameClient.LocalPlayerEntity.Position.x, 0, GameClient.LocalPlayerEntity.Position.y));
			Vector2 observedTargetCameraSpacePos = UnityEngine.Camera.main.WorldToScreenPoint(m_Pos);

			if (Vector3.Dot(UnityEngine.Camera.main.transform.forward, m_Pos - UnityEngine.Camera.main.transform.position) < 0)
				observedTargetCameraSpacePos *= -1;

			Vector2 PlayerDiff = observedTargetCameraSpacePos - localPlayerCameraSpacePos;
			if (observedTargetCameraSpacePos.x <= 0 || observedTargetCameraSpacePos.x >= Screen.width ||
				observedTargetCameraSpacePos.y <= 0 || observedTargetCameraSpacePos.y >= Screen.height)
			{
				m_Img.enabled = true;
				nameText.enabled = true;

				float relativeX = PlayerDiff.x * Screen.height;
				float relativeY = PlayerDiff.y * Screen.width;

				Vector2 straightToEdgeVector;

				if (Mathf.Abs(relativeX) > Mathf.Abs(relativeY))
				{
					if (PlayerDiff.x > 0) straightToEdgeVector = new Vector2(Screen.width / 2, 0);
					else straightToEdgeVector = new Vector2(-Screen.width / 2, 0);
				}
				else
				{
					if (PlayerDiff.y > 0) straightToEdgeVector = new Vector2(0, Screen.height / 2);
					else straightToEdgeVector = new Vector2(0, -Screen.height / 2);
				}

				float hypLength = Mathf.Abs((straightToEdgeVector.magnitude - 10) / Vector2.Dot(PlayerDiff.normalized, straightToEdgeVector.normalized));

				transform.position = (PlayerDiff.normalized * hypLength) + new Vector2(Screen.width / 2, Screen.height / 2);

				float angle = Vector2.Angle(Vector2.right, PlayerDiff);

				if (PlayerDiff.y < 0)
				{
					angle *= -1;
				}

				angle -= 90; //This line corrects for the currently used sprite (building_direction_arrow) and should be removed when correct asset is used. /Johan

				transform.localRotation = Quaternion.Euler(0, 0, angle);
				transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, -angle);
			}
			else
			{
				GetComponent<Image>().enabled = false;
				nameText.enabled = false;
			}
		}

	}
}