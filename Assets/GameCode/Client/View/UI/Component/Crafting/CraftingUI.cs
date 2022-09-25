using System;
using FNZ.Client.Model.Entity.Components.Crafting;
using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components.Crafting;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace FNZ.Client.View.UI.Component.Crafting
{
	public class CraftingUI : MonoBehaviour
	{
		private GameObject P_RecipeItem;
		private GameObject P_MAterialItem;

		[SerializeField]	
		private TextMeshProUGUI TXT_PopupHeader;

		[SerializeField]
		private Image IMG_ItemPreviewIcon;

		[SerializeField]
		private TextMeshProUGUI TXT_ItemName;
		[SerializeField]
		private TextMeshProUGUI TXT_ItemDescription;

		[SerializeField]
		private Button BTN_MinusOne;
		[SerializeField]
		private InputField INPUT_CraftAmount;
		[SerializeField]
		private Button BTN_PlusOne;
		[SerializeField]
		private GameObject G_SetMaximum;
		[SerializeField]
		private GameObject G_Craft;

		[SerializeField]
		private Transform T_RecipeParent;
		[SerializeField]
		private Transform T_MaterialParent;

		[SerializeField]
		private ScrollRect SV_RecipeScroll;
		
		private CraftingComponentClient m_CraftingComp;
		private InventoryComponentClient m_InventoryComponent;

		public void Init(CraftingComponentClient craftingComp)
		{
			m_CraftingComp = craftingComp;
			m_InventoryComponent = GameClient.LocalPlayerEntity.GetComponent<InventoryComponentClient>();
			m_CraftingComp.d_OnCraftUpdate += ReRenderChangingUI;
			m_InventoryComponent.d_OnInventoryUpdate += ReRenderChangingUI;

			INPUT_CraftAmount.caretColor = Color.white;
		}

		// Start is called before the first frame update
		void Start()
		{
			P_RecipeItem = PrefabBank.GetPrefab("Prefab/ComponentUI/CraftingUI/CraftingListItem");
			P_MAterialItem = PrefabBank.GetPrefab("Prefab/ComponentUI/CraftingUI/CraftingMaterialItem");

			BuildUI();
		}

		void Update()
		{
			var mouseScrollY = UnityEngine.Input.mouseScrollDelta.y;
			if (mouseScrollY != 0 &&
			    RectTransformUtility.RectangleContainsScreenPoint(
				    (RectTransform) T_RecipeParent,
				    UnityEngine.Input.mousePosition
			    )
			)
			{
				SV_RecipeScroll.normalizedPosition = new Vector2(
					SV_RecipeScroll.normalizedPosition.x,
					SV_RecipeScroll.normalizedPosition.y + (mouseScrollY * 0.1f)
				);
			}
		}

		private void BuildUI()
		{
			foreach (Transform t in T_RecipeParent)
				Destroy(t.gameObject);

			TXT_PopupHeader.text = m_CraftingComp.ParentEntity.Data.editorName;

			foreach (var recipeRef in m_CraftingComp.Data.recipes)
			{
				var recipe = DataBank.Instance.GetData<CraftingRecipeData>(recipeRef);
				var productData = DataBank.Instance.GetData<ItemData>(recipe.productRef);
				var newItem = Instantiate(P_RecipeItem);

				newItem.transform.SetParent(T_RecipeParent);

				newItem.transform.localScale = Vector3.one;
				newItem.name = recipe.Id;

				var icon = newItem.transform.GetChild(1).GetChild(0).GetComponent<Image>();
				var text = newItem.GetComponentInChildren<Text>();

				icon.sprite = SpriteBank.GetSprite(productData.iconRef);
				text.text = Localization.GetString(productData.nameRef);

				AddRecipeItemEvents(newItem, recipe);
			}

			AddPreviewIconEvents();
			AddCraftButtonEvents();
			AddSetMaximumButtonEvents();

			BTN_MinusOne.onClick.AddListener(() =>
			{
				m_CraftingComp.AmountSubtractOne();
				INPUT_CraftAmount.text = m_CraftingComp.GetCraftAmount().ToString();
			});

			BTN_PlusOne.onClick.AddListener(() =>
			{
				m_CraftingComp.AmountAddOne();
				INPUT_CraftAmount.text = m_CraftingComp.GetCraftAmount().ToString();
			});

			ReRenderChangingUI();
		}

		public CraftingComponentClient GetCraftingComp()
		{
			return m_CraftingComp;
		}

		private void ReRenderChangingUI()
		{
			ReRenderChangingUI(null);
		}

		private void ReRenderChangingUI(List<int2> changedCells = null)
		{
			foreach (Transform t in T_MaterialParent)
				Destroy(t.gameObject);

			var activeRecipe = m_CraftingComp.GetActiveRecipe();
			var activeProduct = DataBank.Instance.GetData<ItemData>(activeRecipe.productRef);

			IMG_ItemPreviewIcon.sprite = SpriteBank.GetSprite(activeProduct.iconRef);

			TXT_ItemName.text = "<color=#ffff00>" + activeRecipe.productAmount + "x</color> " + Localization.GetString(activeProduct.nameRef);
			TXT_ItemDescription.text = Localization.GetString(activeProduct.infoRef);

			foreach (var materialDef in activeRecipe.requiredMaterials)
			{
				var materialItem = DataBank.Instance.GetData<ItemData>(materialDef.itemRef);

				var newItem = Instantiate(P_MAterialItem);

				newItem.transform.SetParent(T_MaterialParent);

				newItem.transform.localScale = Vector3.one;

				var icon = newItem.transform.GetChild(0).GetChild(0).GetComponent<Image>();
				var amount = newItem.GetComponentInChildren<Text>();

				icon.sprite = SpriteBank.GetSprite(materialItem.iconRef);
				var matCount = m_InventoryComponent.GetItemCount(materialDef.itemRef);
				amount.text = matCount + "/" + materialDef.amount;
				if (matCount >= materialDef.amount)
				{
					amount.color = Color.white;
				}
				else
				{
					amount.color = Color.red;
				}

                AddMaterialIconEvents(newItem, materialItem.Id);
			}

			foreach (Transform recipeT in T_RecipeParent)
			{
				recipeT.GetChild(0).gameObject.SetActive(m_CraftingComp.GetActiveRecipe().Id == recipeT.name);
			}

			INPUT_CraftAmount.text = m_CraftingComp.GetCraftAmount().ToString();
		}

		// Adding events directly in the for loops leads to weirdness
		private void AddRecipeItemEvents(GameObject recipeItem, CraftingRecipeData data)
		{
			EventTrigger eventTrigger = recipeItem.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				m_CraftingComp.SetActiveRecipe(data);

                UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);

				var flash = recipeItem.transform.GetChild(3).gameObject.AddComponent<FlashSprite>();
				flash.Init(Color.white);

				ReRenderChangingUI();
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				recipeItem.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

                UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
            });
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				recipeItem.GetComponent<Image>().color = new Color(0.13333f, 0.13333f, 0.13333f, 1);
			});
			eventTrigger.triggers.Add(entry);
		}

        private void AddMaterialIconEvents(GameObject itemObject, string id)
        {
            var go = itemObject;

            EventTrigger eventTrigger = go.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) =>
            {
                UIManager.HoverBoxGen.CreateItemHoverBox(id);
            });
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) =>
            {
                HB_Factory.DestroyHoverbox();
            });
            eventTrigger.triggers.Add(entry);
        }

        private void AddPreviewIconEvents()
		{
			var go = IMG_ItemPreviewIcon.gameObject;

			EventTrigger eventTrigger = go.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				UIManager.HoverBoxGen.CreateItemHoverBox(m_CraftingComp.GetActiveRecipe().productRef);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				HB_Factory.DestroyHoverbox();
			});
			eventTrigger.triggers.Add(entry);
		}

		private void AddSetMaximumButtonEvents()
		{
			EventTrigger eventTrigger = G_SetMaximum.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				m_CraftingComp.AmountSetMaximum();
				INPUT_CraftAmount.text = m_CraftingComp.GetCraftAmount().ToString();

				var flash = G_SetMaximum.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
				flash.Init(Color.white);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				G_SetMaximum.GetComponent<Image>().color = new Color(0.9f, 0.7f, 0.5f, 1);

				UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				G_SetMaximum.GetComponent<Image>().color = new Color(0.75f, 0.46f, 0.2f, 1);
			});
			eventTrigger.triggers.Add(entry);
		}

		private void AddCraftButtonEvents()
		{
			EventTrigger eventTrigger = G_Craft.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				m_CraftingComp.NE_Send_Craft();

				var flash = G_Craft.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
				flash.Init(Color.white);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				G_Craft.GetComponent<Image>().color = new Color(0.55f, 0.8f, 0.55f, 1);

				UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				G_Craft.GetComponent<Image>().color = new Color(0.25f, 0.48f, 0.26f, 1);
			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnCraftAmountChange(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
				m_CraftingComp.AmountSetDirect(1);
				INPUT_CraftAmount.caretPosition = 1;
			}

			int integer = -1;
			if(int.TryParse(val, out integer)){
				if(integer < 1)
                {
					m_CraftingComp.AmountSetDirect(1);
					INPUT_CraftAmount.caretPosition = 1;
				}
                else
                {
					m_CraftingComp.AmountSetDirect(integer);
					INPUT_CraftAmount.caretPosition = m_CraftingComp.GetCraftAmount().ToString().Length;
				}
            }

			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);

			INPUT_CraftAmount.text = m_CraftingComp.GetCraftAmount().ToString();
		}



		public void OnCraftAmountSubmit(string val)
		{

		}

		private void OnDestroy()
		{
			m_CraftingComp.d_OnCraftUpdate -= ReRenderChangingUI;
			m_InventoryComponent.d_OnInventoryUpdate -= ReRenderChangingUI;
		}
	}
}