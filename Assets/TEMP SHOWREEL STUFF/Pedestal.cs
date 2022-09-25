using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class Pedestal : MonoBehaviour
{
	public GameObject[] objectsToShow;
	private int currentObjectIndex = -1;

	private GameObject object1, object2;

	private readonly float OBJECT_TIME = 8;
	private float timer = 0;
	private float effectTimer = 0;

	public Text text;

	void Update()
	{
		if (currentObjectIndex >= objectsToShow.Length + 1)
		{
			return;
		}

		transform.Rotate(Vector3.back, 360f / OBJECT_TIME * 2 * Time.deltaTime);

		effectTimer += Time.deltaTime;
		timer += Time.deltaTime;
		if (timer > OBJECT_TIME)
		{
			nextObject();
			timer = 0;
		}

		if (timer < 1f)
			text.color = Color.Lerp(text.color, Color.clear, 2f * Time.deltaTime);
		else if (text.color.b < 1 && object1 != null)
		{
			if (currentObjectIndex % 2 == 0 && object1 != null)
				text.text = object1.name;
			else if (object2 != null)
				text.text = object2.name;

			text.color = Color.Lerp(text.color, Color.white, 2f * Time.deltaTime);
		}


		if (currentObjectIndex % 2 == 0)
		{
			if (object1 != null)
			{
				object1.transform.localPosition = Vector3.Lerp(object1.transform.localPosition, new Vector3(0, 0, object1.transform.localPosition.z), 2 * Time.deltaTime);

				if (effectTimer > 2 && object1.GetComponentInChildren<VisualEffect>())
				{
					object1.GetComponentInChildren<VisualEffect>().Play();
					effectTimer = 0;
				}
			}
			if (object2 != null)
			{
				object2.transform.localPosition = Vector3.Lerp(object2.transform.localPosition, Vector3.down * 10, 4 * Time.deltaTime);
				if (object2.transform.localPosition.y < -4.5f)
					object2.SetActive(false);
			}
		}
		else
		{
			if (object2 != null)
			{
				object2.transform.localPosition = Vector3.Lerp(object2.transform.localPosition, new Vector3(0, 0, object2.transform.localPosition.z), 2 * Time.deltaTime);

				if (effectTimer > 2 && object2.GetComponentInChildren<VisualEffect>())
				{
					object2.GetComponentInChildren<VisualEffect>().Play();
					effectTimer = 0;
				}
			}

			if (object1 != null)
			{
				object1.transform.localPosition = Vector3.Lerp(object1.transform.localPosition, Vector3.down * 10, 4 * Time.deltaTime);
				if (object1.transform.localPosition.y < -4.5f)
					object1.SetActive(false);
			}
		}
	}

	private void nextObject()
	{
		currentObjectIndex++;
		if (currentObjectIndex >= objectsToShow.Length)
		{
			return;
		}

		if (currentObjectIndex % 2 == 0)
		{
			if (object1 != null)
				object1.SetActive(false);
			object1 = objectsToShow[currentObjectIndex];
			object1.transform.localPosition = new Vector3(0, 10, object1.transform.localPosition.z);
			object1.SetActive(true);
		}
		else
		{
			if (object2 != null)
				object2.SetActive(false);
			object2 = objectsToShow[currentObjectIndex];
			object2.transform.localPosition = new Vector3(0, 10, object2.transform.localPosition.z);
			object2.SetActive(true);
		}
	}
}
