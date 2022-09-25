using FNZ.Client.View.UI.Component.Inventory;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model.Items;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.FNECursor
{

	public class Cursor : MonoBehaviour
	{
		public static Vector2 MousePosition;

		private Vector2 m_Delta;
		private UnityEngine.Sprite m_DefaultCursorSprite;

		public Image IMG_ItemImage;
		public Image IMG_PointerImage;

		private void Start()
		{
			Init();
		}

		public void Init()
		{
			m_DefaultCursorSprite = Resources.Load<UnityEngine.Sprite>("UISprite/Cursor/default");
			UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
			UnityEngine.Cursor.visible = false;
			gameObject.SetActive(true);
			EnableDefaultCursor();
			lastPos = UnityEngine.Input.mousePosition;
		}

		public void SetItemCursor(Item item)
		{
			if (item != null)
			{
				IMG_ItemImage.enabled = true;
				IMG_ItemImage.sprite = SpriteBank.GetSprite(item.Data.iconRef);
				IMG_ItemImage.preserveAspect = true;
				var rectTransform = (RectTransform)IMG_ItemImage.transform;
				rectTransform.sizeDelta = new Vector2(
					item.Data.width * InventoryUI.CELL_WIDTH,
					item.Data.height * InventoryUI.CELL_WIDTH
				);
			}
			else
			{
				IMG_ItemImage.enabled = false;
			}
		}

		public void EnableDefaultCursor()
		{
			IMG_PointerImage.sprite = m_DefaultCursorSprite;
			IMG_PointerImage.preserveAspect = true;
			var rectTransform = (RectTransform)IMG_PointerImage.transform;
			rectTransform.sizeDelta = new Vector2(
				InventoryUI.CELL_WIDTH,
				InventoryUI.CELL_WIDTH
			);
		}

		Vector2 lastPos;
		void Update()
		{
			Vector2 mouseDelta = (Vector2)UnityEngine.Input.mousePosition - lastPos;

			transform.position += (Vector3)mouseDelta;
			if (((Vector2)transform.position - (Vector2)UnityEngine.Input.mousePosition).magnitude > 10)
			{
				transform.position = (Vector2)UnityEngine.Input.mousePosition;
			}
			MousePosition = (Vector2)transform.position;
			lastPos = UnityEngine.Input.mousePosition;
		}
	}
}