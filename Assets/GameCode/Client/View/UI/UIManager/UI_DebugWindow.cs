using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Manager
{
	public class UI_DebugWindow : MonoBehaviour
	{
		private Text coordinates;

		void Start()
		{
			coordinates = GetComponentInChildren<Text>();
		}

		void Update()
		{
			if (Time.frameCount % 60 == 0)
				UpdateCoordinates();
		}

		public void UpdateCoordinates()
		{
			coordinates.text = $"Coordinates: {Mathf.Round(GameClient.LocalPlayerEntity.Position.x)}, {Mathf.Round(GameClient.LocalPlayerEntity.Position.y)}";
		}

	}
}