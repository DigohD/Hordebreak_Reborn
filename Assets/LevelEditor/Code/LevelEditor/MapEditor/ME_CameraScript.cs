using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FNZ.LevelEditor
{
	public class ME_CameraScript : MonoBehaviour
	{

		private float zoomValue = -20;

		private float totalRot = 0;

		EventSystem m_EventSystem;
		GraphicRaycaster m_Raycaster;

		void Start()
		{
			m_Raycaster = GameObject.FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
			m_EventSystem = GameObject.FindObjectOfType<EventSystem>();
		}

		Vector2 lastMousePos;
		void Update()
		{
			Vector2 moveVector = Vector3.zero;
			if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
			{
				moveVector = new Vector3(0, 1, 0);
			}
			else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
			{
				moveVector = new Vector3(1, 0, 0);
			}
			else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
			{
				moveVector = new Vector3(0, -1, 0);
			}
			else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
			{
				moveVector = new Vector3(-1, 0, 0);
			}
			else if (Input.GetKey(KeyCode.S))
			{
				moveVector = new Vector3(-1, -1, 0).normalized;
			}
			else if (Input.GetKey(KeyCode.A))
			{
				moveVector = new Vector3(-1, 1, 0).normalized;
			}
			else if (Input.GetKey(KeyCode.W))
			{
				moveVector = new Vector3(1, 1, 0).normalized;
			}
			else if (Input.GetKey(KeyCode.D))
			{
				moveVector = new Vector3(1, -1, 0).normalized;
			}

			if (Input.GetKey(KeyCode.LeftShift))
				transform.position += (Vector3)moveVector * Time.deltaTime * 50f;
			else
				transform.position += (Vector3)moveVector * Time.deltaTime * 10f;

			transform.position = new Vector3(
				transform.position.x,
				transform.position.y,
				zoomValue
			);

			if (Input.GetAxis("Mouse ScrollWheel") > 0 && !DidRaycastHitUI())
			{
				zoomValue += 1f;
				if (zoomValue > -6)
					zoomValue = -6;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0 && !DidRaycastHitUI())
			{
				zoomValue += -1f;
				if (zoomValue < -100)
					zoomValue = -100;
			}

			if (Input.GetMouseButton(2))
			{
				Vector2 diff = (Vector2)Input.mousePosition - lastMousePos;
				transform.Rotate(Vector3.right, -diff.y * 0.3f);
				totalRot += (-diff.y * 0.3f);
				if (totalRot <= -40 || totalRot >= 30)
				{
					transform.Rotate(Vector3.right, diff.y * 0.3f);
					totalRot += (diff.y * 0.3f);
				}
			}

			if (Input.GetKeyDown(KeyCode.V))
				Camera.main.orthographic = !Camera.main.orthographic;

			lastMousePos = Input.mousePosition;
		}

		public bool DidRaycastHitUI()
		{
			PointerEventData m_PointerEventData = new PointerEventData(m_EventSystem);
			//Set the Pointer Event Position to that of the mouse position
			m_PointerEventData.position = Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			//Raycast using the Graphics Raycaster and mouse click position
			m_Raycaster.Raycast(m_PointerEventData, results);

			if (results.Count > 0)
			{
				return true;
			}

			return false;
		}
	}
}