using FNZ.Client.View.Prefab;
using FNZ.Shared.Model.Entity;
using System.Collections.Generic;
using FNZ.Client.Utils;
using FNZ.Client.View.UI.Chat;
using FNZ.Client.View.UI.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.Camera
{
	public class CameraScript : MonoBehaviour
	{
		private GameObject m_AudioListener;
		private static float shakeIntensity;
		private float frequencyX = 40, frequencyY = 40, amplitude = 0.06f;

		public static float zoomValue = 16;
		public float MaxZoomOut;
		public static float ScoutZoom = -24;

		public Text observedText;

		private static EventSystem s_EventSystem;
		private static GraphicRaycaster s_Raycaster;

		[SerializeField]
		private GameObject GO_DensityVolume;

		void Start()
		{
			s_Raycaster = GameObject.FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
			s_EventSystem = GameObject.FindObjectOfType<EventSystem>();

			m_AudioListener = Instantiate(PrefabBank.GetPrefab("Prefab/Audio/AudioListener"), transform);
		}

		public GameObject observedPlayer;
		public FNEEntity observedPlayerEntity;

		float counterX = 0;
		float counterY = 0;

		private static float LeftUIPixelWidth;
		private static float rightUIPixelWidth;

		private static float LeftUIOffset;
		private static float rightUIOffset;

		private static bool snapToNewUIOffsets = false;

		void Update()
		{
			if (observedPlayer == null)
			{
				observedPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
				return;
			}

			GO_DensityVolume.transform.position = new Vector3(transform.position.x, -6f, transform.position.z);

			if (UnityEngine.Input.GetKeyDown(KeyCode.F5))
			{
				TakeScreenShot();
			}else if (UnityEngine.Input.GetKeyDown(KeyCode.F6))
			{
				TakeScreenShotWithUI();
			}

			Vector3 aimOffset = Vector3.zero;
			CalculateUIOffsets();
			Vector3 shakeOffset = Vector3.zero;
			counterX = counterX + Time.deltaTime + Random.Range(0.0f, Time.deltaTime);
			counterY = counterY + Time.deltaTime + Random.Range(0.0f, Time.deltaTime);
			if (shakeIntensity < 0)
				shakeIntensity = 0;
			if (shakeIntensity > 0)
			{
				shakeIntensity -= ((Time.deltaTime * shakeIntensity) + (4f * Time.deltaTime));
				shakeOffset = new Vector3(Mathf.Sin(counterX * frequencyX) * amplitude * shakeIntensity * 0.03f * zoomValue, Mathf.Sin(counterY * frequencyY) * amplitude * shakeIntensity * 0.04f * zoomValue, 0);
			}

			if (false && UnityEngine.Input.GetKey(KeyCode.LeftControl))
			{
				Vector3 playerFinal = observedPlayer.transform.position + new Vector3(0.75f, 0, 0.75f);
				Vector3 goal = playerFinal + (gameObject.transform.forward * ScoutZoom) + shakeOffset;
				transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * 10.0f);
				m_AudioListener.transform.position = observedPlayer.transform.position + new Vector3(0, 3 + (zoomValue * 2 / MaxZoomOut), 0);

				if ((goal - transform.position).magnitude > 30 || snapToNewUIOffsets)
				{
					transform.position = goal;
					snapToNewUIOffsets = false;
				}
			}
			else
			{
				Vector3 playerFinal = observedPlayer.transform.position + new Vector3(0.75f, 0, 0.75f);
				Vector3 goal = playerFinal + new Vector3(0.2f, 0, 0.2f) + (-gameObject.transform.forward * zoomValue) + shakeOffset + aimOffset + (-gameObject.transform.up * zoomValue * 0.06f);
				transform.position = Vector3.Lerp(transform.position, goal, Time.deltaTime * (200.0f / zoomValue));

				m_AudioListener.transform.position = observedPlayer.transform.position + new Vector3(0,  3 + (zoomValue * 2 / MaxZoomOut), 0);

				if ((goal - transform.position).magnitude > 30 || snapToNewUIOffsets)
				{
					transform.position = goal;
					snapToNewUIOffsets = false;
				}

				if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") < 0 && !DidRaycastHitUI())
				{
					zoomValue += 2f;
					if (zoomValue > MaxZoomOut)
						zoomValue = MaxZoomOut;
				}
				else if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") > 0 && !DidRaycastHitUI())
				{
					zoomValue -= 2f;
					if (zoomValue < 6)
						zoomValue = 6;
				}
			}
		}

		public static void shakeCamera(float amount)
		{
			shakeIntensity += amount;
		}

		public static void shakeCamera(float amount, float max)
		{
			if (shakeIntensity < max)
				shakeIntensity += amount;
		}

		public static bool DidRaycastHitUI()
		{
			PointerEventData m_PointerEventData = new PointerEventData(s_EventSystem);
			//Set the Pointer Event Position to that of the mouse position
			m_PointerEventData.position = UnityEngine.Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			//Raycast using the Graphics Raycaster and mouse click position
			s_Raycaster.Raycast(m_PointerEventData, results);

			if (results.Count > 0)
			{
				return true;
			}

			return false;
		}

		public static void SetLeftUIPixelWidth(float offset)
		{
			LeftUIPixelWidth = offset;
			snapToNewUIOffsets = true;
		}

		public static void SetRightUIPixelWidth(float offset)
		{
			rightUIPixelWidth = offset;
			snapToNewUIOffsets = true;
		}

		private void CalculateUIOffsets()
		{
			rightUIOffset = rightUIPixelWidth / Screen.width;
			LeftUIOffset = LeftUIPixelWidth / Screen.width;

			GetComponent<UnityEngine.Camera>().rect = new Rect(LeftUIOffset - rightUIOffset, 0, 1, 1);
		}

		private void TakeScreenShot()
		{
			RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
			UnityEngine.Camera.main.targetTexture = rt;
			Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			UnityEngine.Camera.main.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			UnityEngine.Camera.main.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			string filename = ViewUtils.ScreenShotName(Screen.width, Screen.height);
			System.IO.Directory.CreateDirectory(string.Format("{0}/screenshots/", Application.dataPath));
			System.IO.File.WriteAllBytes(filename, bytes);
			UI_Chat.NewMessage(string.Format("Took screenshot to: {0}", filename));
		}
		
		private void TakeScreenShotWithUI()
		{
			System.IO.Directory.CreateDirectory(string.Format("{0}/screenshots/", Application.dataPath));
			string filename = ViewUtils.ScreenShotName(Screen.width, Screen.height);
			ScreenCapture.CaptureScreenshot(filename);
			UI_Chat.NewMessage(string.Format("Took screenshot with UI to: {0}", filename));
		}
	}
}