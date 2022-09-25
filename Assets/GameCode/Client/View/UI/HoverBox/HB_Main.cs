using FNZ.Client.View.UI.StartMenu;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static FNZ.Client.View.UI.HoverBox.HB_Factory;

namespace FNZ.Client.View.UI.HoverBox
{
	public class HB_Main : MonoBehaviour
	{
		public enum AnchorMode
		{
			SCREEN_MOUSE,
			WORLD_OBJECT
		}

		public RectTransform T_BG;
		public RectTransform T_Borders;

		public GameObject P_Text;

		private Vector2 targetAnchors;
		private Vector3 position;

		private AnchorMode anchorMode = AnchorMode.SCREEN_MOUSE;

		void Update()
		{
			switch (anchorMode)
			{
				case AnchorMode.SCREEN_MOUSE:
					FollowMouse();
					break;
				case AnchorMode.WORLD_OBJECT:
					FollowWorldObject();
					break;
			}
		}

		void FollowMouse()
		{
			Vector2 mousePos = UnityEngine.Input.mousePosition;
			int width = Screen.width;
			int height = Screen.height;
			RectTransform rt = (RectTransform)transform;

			float anchorX = -0.1f;
			float anchorY = 0;

			if (mousePos.x > (width / 3) * 2)
			{
				anchorX = 1.05f;
			}
			if (mousePos.y > (height / 4))
			{
				anchorY = 1;
			}

			targetAnchors = new Vector2(anchorX, anchorY);

			T_BG.pivot = targetAnchors;
			T_Borders.pivot = targetAnchors;

			transform.position = mousePos;
		}

		void FollowWorldObject()
		{
			Vector2 objectPos = UnityEngine.Camera.main.WorldToScreenPoint(position);

			targetAnchors = new Vector2(0.5f, 0f);
			T_BG.pivot = targetAnchors;
			T_Borders.pivot = targetAnchors;
			transform.position = objectPos + (Vector2.up * 10);
		}

		/*void OnValidate()
	    {
	        foreach (RectTransform t in T_BG)
	            UnityEditor.EditorApplication.delayCall += () =>
	            {
	                DestroyImmediate(t.gameObject);
	            };
	
	        RebuildUI();
	
	        BuildTest();
	
	        RebuildUI();
	    }*/

		//private void BuildTest()
		//{
		//    AddTextItem(HB_Text.TextStyle.HEADER, new Color32(125, 125, 125, 255), "Created from code!");
		//    AddDividerLine(new Color32(80, 80, 80, 255));
		//    AddIconTextRow(
		//        new IconTextItem[]
		//        {
		//            new IconTextItem("100", (Sprite) Resources.Load("Granade_Inv", typeof(Sprite))),
		//            new IconTextItem("100", (Sprite) Resources.Load("HealthPack_Inv", typeof(Sprite))),
		//            new IconTextItem("100", (Sprite) Resources.Load("ScrapSuppressor_Inv", typeof(Sprite)))
		//        },
		//        Color.red
		//    );
		//    AddTextItem(HB_Text.TextStyle.BREAD_TEXT, new Color32(170, 170, 170, 255), "Detta äre n brödtext för att testa hoverbox varför inte testa den igen i morgon kväll! Nehe, okej, be like that then, mother fockah!");
		//    AddDividerLine(new Color32(80, 80, 80, 255));
		//    AddTextItem(HB_Text.TextStyle.BREAD_TEXT, new Color32(170, 170, 170, 255), "BYGGA HUS JAAAA!");
		//    AddDividerLine(new Color32(80, 80, 80, 255));

		//    AddLootInfo(
		//        Item.GenerateItem(7, 10),
		//        Color.white,
		//        new GridInventoryComponent()
		//    );

		//    FinishConstruction();
		//}

		private void RebuildUI()
		{
			foreach (RectTransform rt in GetComponentsInChildren<RectTransform>())
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
			}

			FollowMouse();
		}

		public void SetAnchorMode_WorldObject(Vector3 position)
		{
			anchorMode = AnchorMode.WORLD_OBJECT;
			this.position = position;

			FollowWorldObject();
		}

		public void FinishConstruction()
		{
			foreach (Transform t in T_BG)
			{
				t.GetComponent<HB_DividerLine>()?.PostProcess();
			}

			RebuildUI();
			StartCoroutine(SetBorder());

			transform.localScale = (Vector3.one * (UI_Options.GetUIScale() + 1.0f));
		}

		public void AddTextItem(HB_Text.TextStyle style, Color textColor, string text)
		{
			HB_Factory.BuildHBText(T_BG, style, textColor, text);

			RebuildUI();
		}

		public void AddDividerLine(Color linecolor)
		{
			GameObject divider = HB_Factory.BuildHBDividerLine(T_BG, linecolor).gameObject;

			RebuildUI();
		}

		public void AddIconTextRow(IconTextItem[] data, Color textColor)
		{
			HB_Factory.BuildIconTextRow(T_BG, data, textColor);

			RebuildUI();
		}
		
		public void AddIconTextIconRow(IconTextIconItem[] data, Color textColor)
		{
			HB_Factory.BuildIconTextIconRow(T_BG, data, textColor);

			RebuildUI();
		}
		
		public void AddRepeatingconRow(Sprite icon, byte amount)
		{
			HB_Factory.BuildRepeatingIconRow(T_BG, icon, amount);

			RebuildUI();
		}

		private IEnumerator SetBorder()
		{
			yield return new WaitForEndOfFrame();
			T_Borders.sizeDelta = new Vector2(T_BG.rect.width, T_BG.rect.height);
		}

		/*public void AddLootInfo(
	        Item data,
	        Color textColor,
	        InventoryComponentClient container
	    )
	    {
	        HB_Factory.BuildLootInfo(
	            T_BG,
	            data,
	            container,
	            textColor
	        );
	
	        RebuildUI();
	    }*/

		/*public void AddModInfo(
	        Item data,
	        Color textColor,
            InventoryComponentClient container
	    )
	    {
	        HB_Factory.BuildModInfo(
	            T_BG,
	            data,
	            container,
	            textColor
	        );
	
	        RebuildUI();
	    }*/
	}
}