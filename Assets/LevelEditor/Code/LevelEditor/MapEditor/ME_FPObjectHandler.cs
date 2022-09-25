using UnityEngine;
namespace FNZ.LevelEditor
{
	public class ME_FPObjectHandler : MonoBehaviour
	{

		public enum HandleType
		{
			NONE,
			TRANSLATION,
			SCALE,
			ROTATION
		}

		public enum Axis
		{
			NULL,
			X,
			Y,
			Z
		}

		public GameObject G_Translation;
		public GameObject G_Scale;
		public GameObject G_Rotation;

		public GameObject G_Translation_ZPlane;
		public GameObject G_Translation_XYPlane;

		public GameObject G_Scale_ZPlane;
		public GameObject G_Scale_XYPlane;

		public GameObject G_Rotation_YZPlane;
		public GameObject G_Rotation_XYPlane;
		public GameObject G_Rotation_XZPlane;

		private Vector3 M_StartOffset;
		private Vector3 M_StartPosition;
		private float startScaleX, startScaleY, startScaleZ;
		private Quaternion startRotation;

		public static HandleType ActiveHandle = HandleType.NONE;
		private Axis ActiveAxis;

		private GameObject ToManipulate;

		public void SetObservedObject(GameObject ToManipulate)
		{
			this.ToManipulate = ToManipulate;

			transform.position = ToManipulate.transform.position;
		}

		void Update()
		{
			G_Translation.SetActive(false);
			G_Scale.SetActive(false);
			G_Rotation.SetActive(false);

			if (ME_Control.activePaintMode != ME_Control.PaintMode.FPOBJECT_MANIPULATOR)
			{
				ToManipulate = null;
			}

			if (ToManipulate != null && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
			{
				GameObject duplicate = Instantiate(ToManipulate, ToManipulate.transform.position, ToManipulate.transform.rotation);
				ToManipulate = duplicate;
			}

			if (ToManipulate == null)
			{
				return;
			}

			if (ActiveHandle == HandleType.TRANSLATION)
			{
				G_Translation.SetActive(true);
				TranslationUpdate();
				transform.rotation = Quaternion.identity;
				transform.position = ToManipulate.transform.position;
			}
			else if (ActiveHandle == HandleType.SCALE)
			{
				G_Scale.SetActive(true);
				ScaleUpdate();
				G_Scale.transform.rotation = ToManipulate.transform.rotation;
				G_Scale.transform.Rotate(Vector3.up, 180);
				transform.position = ToManipulate.transform.position;
			}
			else if (ActiveHandle == HandleType.ROTATION)
			{
				G_Rotation.SetActive(true);
				RotationUpdate();
				G_Rotation.transform.rotation = ToManipulate.transform.rotation;
				G_Rotation.transform.Rotate(Vector3.up, 180);
				transform.position = ToManipulate.transform.position;
			}
		}

		private void TranslationUpdate()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			int layerMask = 1 << LayerMask.NameToLayer("EditorHandle");
			if (Physics.Raycast(ray, out hit, 1000f, layerMask))
			{
				if (Input.GetMouseButtonDown(0))
				{
					ResetPlanes();

					ActiveAxis = Axis.NULL;

					if (hit.collider.name.Equals("XAxis"))
					{
						ActiveAxis = Axis.X;
						G_Translation_XYPlane.SetActive(true);
						G_Translation_XYPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
					}
					else if (hit.collider.name.Equals("YAxis"))
					{
						ActiveAxis = Axis.Y;
						G_Translation_XYPlane.SetActive(true);
						G_Translation_XYPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
					}
					else if (hit.collider.name.Equals("ZAxis"))
					{
						ActiveAxis = Axis.Z;
						G_Translation_ZPlane.SetActive(true);
						G_Translation_ZPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
					}
				}
			}

			if (Input.GetMouseButton(0))
			{
				layerMask = 1 << LayerMask.NameToLayer("EditorPlane");
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					if (ActiveAxis != Axis.NULL)
					{
						Vector3 diff = hit.point - (transform.position + M_StartOffset);
						switch (ActiveAxis)
						{
							case Axis.X:
								ToManipulate.transform.position += Vector3.right * diff.x;
								break;
							case Axis.Y:
								ToManipulate.transform.position += Vector3.up * diff.y;
								break;
							case Axis.Z:
								ToManipulate.transform.position += Vector3.forward * diff.z;
								break;
						}
					}
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				ResetPlanes();
			}
		}

		private void ScaleUpdate()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			int layerMask = 1 << LayerMask.NameToLayer("EditorHandle");
			if (Physics.Raycast(ray, out hit, 1000f, layerMask))
			{
				if (Input.GetMouseButtonDown(0))
				{
					ResetPlanes();

					ActiveAxis = Axis.NULL;

					if (hit.collider.name.Equals("XAxis"))
					{
						ActiveAxis = Axis.X;
						G_Scale_XYPlane.SetActive(true);
						G_Scale_XYPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
						M_StartPosition = hit.point;
					}
					else if (hit.collider.name.Equals("YAxis"))
					{
						ActiveAxis = Axis.Y;
						G_Scale_XYPlane.SetActive(true);
						G_Scale_XYPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
						M_StartPosition = hit.point;
					}
					else if (hit.collider.name.Equals("ZAxis"))
					{
						ActiveAxis = Axis.Z;
						G_Scale_ZPlane.SetActive(true);
						G_Scale_ZPlane.transform.position = hit.point;
						M_StartOffset = hit.point - transform.position;
						M_StartPosition = hit.point;
					}

					startScaleX = ToManipulate.transform.localScale.x;
					startScaleY = ToManipulate.transform.localScale.y;
					startScaleZ = ToManipulate.transform.localScale.z;
				}
			}

			/*
             * I don't know why, but X and Y had to be inverted in certain ways for it to work 
             */
			if (Input.GetMouseButton(0))
			{
				layerMask = 1 << LayerMask.NameToLayer("EditorPlane");
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					if (ActiveAxis != Axis.NULL)
					{
						Vector3 refDiff = M_StartPosition - ToManipulate.transform.position;
						Vector3 diff = hit.point - M_StartPosition;

						switch (ActiveAxis)
						{
							case Axis.Y:
								float deltaScale = (refDiff.x + diff.x) / refDiff.x;
								ToManipulate.transform.localScale = new Vector3(
									ToManipulate.transform.localScale.x,
									startScaleY * deltaScale,
									ToManipulate.transform.localScale.z
								);

								if (Input.GetKey(KeyCode.LeftShift))
								{
									ToManipulate.transform.localScale = new Vector3(
										startScaleX * deltaScale,
										startScaleY * deltaScale,
										startScaleZ * deltaScale
									);
								}
								break;

							case Axis.X:
								deltaScale = (refDiff.y + diff.y) / refDiff.y;
								ToManipulate.transform.localScale = new Vector3(
									startScaleX * deltaScale,
									ToManipulate.transform.localScale.y,
									ToManipulate.transform.localScale.z
								);

								if (Input.GetKey(KeyCode.LeftShift))
								{
									ToManipulate.transform.localScale = new Vector3(
										startScaleX * deltaScale,
										startScaleY * deltaScale,
										startScaleZ * deltaScale
									);
								}
								break;

							case Axis.Z:
								deltaScale = (refDiff.z + diff.z) / refDiff.z;
								ToManipulate.transform.localScale = new Vector3(
									ToManipulate.transform.localScale.x,
									ToManipulate.transform.localScale.y,
									startScaleZ * deltaScale
								);

								if (Input.GetKey(KeyCode.LeftShift))
								{
									ToManipulate.transform.localScale = new Vector3(
										startScaleX * deltaScale,
										startScaleY * deltaScale,
										startScaleZ * deltaScale
									);
								}
								break;
						}
					}
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				ResetPlanes();
			}
		}

		private void RotationUpdate()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			int layerMask = 1 << LayerMask.NameToLayer("EditorHandle");
			if (Physics.Raycast(ray, out hit, 1000f, layerMask))
			{
				if (Input.GetMouseButtonDown(0))
				{
					ResetPlanes();

					ActiveAxis = Axis.NULL;

					if (hit.collider.name.Equals("XAxis"))
					{
						ActiveAxis = Axis.X;
						G_Rotation_YZPlane.SetActive(true);
						G_Rotation_YZPlane.transform.position = hit.point;
						M_StartPosition = hit.point;
						startRotation = ToManipulate.transform.rotation;
					}
					else if (hit.collider.name.Equals("YAxis"))
					{
						ActiveAxis = Axis.Y;
						G_Rotation_XZPlane.SetActive(true);
						G_Rotation_XZPlane.transform.position = hit.point;
						M_StartPosition = hit.point;
						startRotation = ToManipulate.transform.rotation;
					}
					else if (hit.collider.name.Equals("ZAxis"))
					{
						ActiveAxis = Axis.Z;
						G_Rotation_XYPlane.SetActive(true);
						G_Rotation_XYPlane.transform.position = hit.point;
						M_StartPosition = hit.point;
						startRotation = ToManipulate.transform.rotation;
					}
				}
			}

			if (Input.GetMouseButton(0))
			{
				layerMask = 1 << LayerMask.NameToLayer("EditorPlane");
				if (Physics.Raycast(ray, out hit, 1000f, layerMask))
				{
					if (ActiveAxis != Axis.NULL)
					{
						Vector3 newPoint = hit.point;

						Vector3 from = M_StartPosition - ToManipulate.transform.position;
						Vector3 to = newPoint - ToManipulate.transform.position;

						float angleDiff = Vector3.Angle(from, to);
						Vector3 crossProduct = Vector3.Cross(from, to);

						switch (ActiveAxis)
						{
							case Axis.X:
								if (Vector3.Dot(ToManipulate.transform.right, crossProduct.normalized) > 0)
									angleDiff = -angleDiff;

								if (Input.GetKey(KeyCode.LeftControl))
								{
									if (angleDiff < 45 && angleDiff >= -45)
										angleDiff = 0;
									else if (angleDiff < 135 && angleDiff >= 45)
										angleDiff = 90;
									else if (angleDiff < -45 && angleDiff >= -135)
										angleDiff = -90;
									else
										angleDiff = 180;
								}

								ToManipulate.transform.rotation = startRotation;
								ToManipulate.transform.RotateAround(
									ToManipulate.transform.position,
									ToManipulate.transform.right,
									-angleDiff
								);
								break;

							case Axis.Y:
								if (Vector3.Dot(ToManipulate.transform.up, crossProduct.normalized) > 0)
									angleDiff = -angleDiff;

								if (Input.GetKey(KeyCode.LeftControl))
								{
									if (angleDiff < 45 && angleDiff >= -45)
										angleDiff = 0;
									else if (angleDiff < 135 && angleDiff >= 45)
										angleDiff = 90;
									else if (angleDiff < -45 && angleDiff >= -135)
										angleDiff = -90;
									else
										angleDiff = 180;
								}

								ToManipulate.transform.rotation = startRotation;
								ToManipulate.transform.RotateAround(
									ToManipulate.transform.position,
									ToManipulate.transform.up,
									-angleDiff
								);
								break;

							case Axis.Z:
								if (Vector3.Dot(ToManipulate.transform.forward, crossProduct.normalized) > 0)
									angleDiff = -angleDiff;

								if (Input.GetKey(KeyCode.LeftControl))
								{
									if (angleDiff < 45 && angleDiff >= -45)
										angleDiff = 0;
									else if (angleDiff < 135 && angleDiff >= 45)
										angleDiff = 90;
									else if (angleDiff < -45 && angleDiff >= -135)
										angleDiff = -90;
									else
										angleDiff = 180;
								}

								ToManipulate.transform.rotation = startRotation;
								ToManipulate.transform.RotateAround(
									ToManipulate.transform.position,
									ToManipulate.transform.forward,
									-angleDiff
								);
								break;
						}
					}
				}
			}

			if (Input.GetMouseButtonUp(0))
			{
				ResetPlanes();
			}
		}

		private void ResetPlanes()
		{
			G_Translation_XYPlane.SetActive(false);
			G_Translation_ZPlane.SetActive(false);

			G_Scale_XYPlane.SetActive(false);
			G_Scale_ZPlane.SetActive(false);

			G_Rotation_XYPlane.SetActive(false);
			G_Rotation_YZPlane.SetActive(false);
			G_Rotation_XZPlane.SetActive(false);
		}
	}
}