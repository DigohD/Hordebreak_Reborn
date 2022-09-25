using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Model.Entity.Components.Refinement;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.Components.Refinement;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Component.Refinement
{
	public class RefinementUI : MonoBehaviour
	{

		private GameObject P_RecipeItem;
		private GameObject P_InputOutput;

		[SerializeField]
		private TextMeshProUGUI TXT_PopupHeader;

		[SerializeField]
		private Image IMG_ItemPreviewIcon;

		[SerializeField]
		private TextMeshProUGUI TXT_ItemName;
		[SerializeField]
		private TextMeshProUGUI TXT_ItemDescription;

		[SerializeField]
		private Transform T_RecipeParent;
		[SerializeField]
		private Transform T_InputParent;
		[SerializeField]
		private Transform T_OutputParent;

		[SerializeField]
		private GameObject G_BurnableSlot;

		[SerializeField]
		private RectTransform RT_ProgressBar;

        [SerializeField]
        private GameObject G_ActiveOverlay;

        private float m_InitBarWidth;

		private RefinementComponentClient m_RefinementComponent;

		public void Init(RefinementComponentClient refinementComp)
		{
			m_RefinementComponent = refinementComp;
			m_RefinementComponent.d_OnRefinementUpdate += ReRenderChangingUI;
			m_RefinementComponent.d_OnRefinementProgressUpdate += ReRenderProgress;

			m_InitBarWidth = ((RectTransform)RT_ProgressBar.parent).rect.width;
		}

		// Start is called before the first frame update
		void Start()
		{
			P_RecipeItem = PrefabBank.GetPrefab("Prefab/ComponentUI/RefinementUI/RefinementListItem");
			P_InputOutput = PrefabBank.GetPrefab("Prefab/ComponentUI/RefinementUI/InputOutputSlot");

			BuildUI();
		}

		private void BuildUI()
		{
			foreach (Transform t in T_RecipeParent)
				Destroy(t.gameObject);

			TXT_PopupHeader.text = m_RefinementComponent.ParentEntity.Data.editorName;

			foreach (var recipeRef in m_RefinementComponent.Data.recipes)
			{
				var recipe = DataBank.Instance.GetData<RefinementRecipeData>(recipeRef);

				var newItem = Instantiate(P_RecipeItem);

				newItem.transform.SetParent(T_RecipeParent);

				newItem.transform.localScale = Vector3.one;

				var icon = newItem.transform.GetChild(1).GetChild(0).GetComponent<Image>();
				var text = newItem.GetComponentInChildren<Text>();

				icon.sprite = SpriteBank.GetSprite(recipe.processIconRef);
				text.text = Localization.GetString(recipe.processNameRef);

                newItem.transform.GetChild(0).gameObject.SetActive(m_RefinementComponent.GetActiveRecipe().Id == recipe.Id);
                newItem.name = recipe.Id;

                AddRecipeItemEvents(newItem, recipe, (ushort)m_RefinementComponent.Data.recipes.IndexOf(recipeRef));
			}

			AddBurnableSlotEvents();

			ReRenderChangingUI();
			ReRenderProgress();
		}

		public RefinementComponentClient GetRefinementComp()
		{
			return m_RefinementComponent;
		}

		private void ReRenderChangingUI()
		{
			foreach (Transform t in T_InputParent)
				Destroy(t.gameObject);

			foreach (Transform t in T_OutputParent)
				Destroy(t.gameObject);

			var activeRecipe = m_RefinementComponent.GetActiveRecipe();

			IMG_ItemPreviewIcon.sprite = SpriteBank.GetSprite(activeRecipe.processIconRef);

			TXT_ItemName.text = Localization.GetString(activeRecipe.processNameRef);
			TXT_ItemDescription.text = Localization.GetString(activeRecipe.processDescriptionRef);

			byte slotIndex = 0;
			foreach (var materialDef in activeRecipe.requiredMaterials)
			{
				var materialItem = DataBank.Instance.GetData<ItemData>(materialDef.itemRef);

				var newItem = Instantiate(P_InputOutput);

				newItem.transform.SetParent(T_InputParent);

				newItem.transform.localScale = Vector3.one;

				var ghostIcon = newItem.transform.GetChild(0).GetComponent<Image>();
				var icon = ghostIcon.transform.GetChild(0).GetComponent<Image>();
				//var amount = newItem.GetComponentInChildren<Text>();

				ghostIcon.sprite = SpriteBank.GetSprite(materialItem.iconRef);
				var slotItem = m_RefinementComponent.GetItemInSlot(RefinementSlotType.INPUT, slotIndex);
				icon.sprite = slotItem != null ? SpriteBank.GetSprite(slotItem.Data.iconRef) : null;
				icon.color = slotItem != null ? Color.white : Color.clear;

				var itemAmount = slotItem != null ? slotItem.amount : 0;
				newItem.GetComponentInChildren<Text>().text = itemAmount + "/" + materialDef.amount;

				//var matCount = m_InventoryComponent.GetItemCount(materialDef.nameRef);
				//amount.text = matCount + "/" + materialDef.amount;
				/*if (matCount > materialDef.amount)
                {
                    amount.color = Color.white;
                }
                else
                {
                    amount.color = Color.red;
                }*/

				AddSlotEvents(newItem, RefinementSlotType.INPUT, slotIndex);

				slotIndex++;
			}

			slotIndex = 0;
			foreach (var materialDef in activeRecipe.producedMaterials)
			{
				var materialItem = DataBank.Instance.GetData<ItemData>(materialDef.itemRef);

				var newItem = Instantiate(P_InputOutput);

				newItem.transform.SetParent(T_OutputParent);

				newItem.transform.localScale = Vector3.one;

				var ghostIcon = newItem.transform.GetChild(0).GetComponent<Image>();
				var icon = ghostIcon.transform.GetChild(0).GetComponent<Image>();
				//var amount = newItem.GetComponentInChildren<Text>();

				ghostIcon.sprite = SpriteBank.GetSprite(materialItem.iconRef);
				var slotItem = m_RefinementComponent.GetItemInSlot(RefinementSlotType.OUTPUT, slotIndex);
				icon.sprite = slotItem != null ? SpriteBank.GetSprite(slotItem.Data.iconRef) : null;
				icon.color = slotItem != null ? Color.white : Color.clear;

				var itemAmount = slotItem != null ? slotItem.amount : 0;
				newItem.GetComponentInChildren<Text>().text = itemAmount.ToString();

				//var matCount = m_InventoryComponent.GetItemCount(materialDef.nameRef);
				//amount.text = matCount + "/" + materialDef.amount;
				/*if (matCount > materialDef.amount)
                {
                    amount.color = Color.white;
                }
                else
                {
                    amount.color = Color.red;
                }*/

				AddSlotEvents(newItem, RefinementSlotType.OUTPUT, slotIndex);

				slotIndex++;
			}

            foreach (Transform recipeT in T_RecipeParent)
            {
                recipeT.GetChild(0).gameObject.SetActive(m_RefinementComponent.GetActiveRecipe().Id == recipeT.name);
            }

            if (string.IsNullOrEmpty(m_RefinementComponent.Data.burnGradeRef))
            {
				G_BurnableSlot.SetActive(false);
            }
            else
            {
				var burnableGhostIcon = G_BurnableSlot.transform.GetChild(0).GetComponent<Image>();
				var burnableIcon = burnableGhostIcon.transform.GetChild(0).GetComponent<Image>();

				var burnable = m_RefinementComponent.GetItemInSlot(RefinementSlotType.BURNABLE, 0);

				var burnGradeData = DataBank.Instance.GetData<BurnableData>(m_RefinementComponent.Data.burnGradeRef);
				burnableGhostIcon.sprite = SpriteBank.GetSprite(burnGradeData.iconRef);

				burnableIcon.sprite = burnable != null ? SpriteBank.GetSprite(burnable.Data.iconRef) : null;
				burnableIcon.color = burnable != null ? Color.white : Color.clear;

				var burnableAmount = burnable != null ? burnable.amount : 0;
				G_BurnableSlot.GetComponentInChildren<Text>().text = burnableAmount.ToString();
			}
            
		}

		private void ReRenderProgress()
		{
			var percentDone = m_RefinementComponent.GetProgress();
			RT_ProgressBar.sizeDelta = new Vector2(percentDone * m_InitBarWidth, 0);
		}

		// Adding events directly in the for loops leads to weirdness
		private void AddRecipeItemEvents(GameObject recipeItem, RefinementRecipeData data, ushort recipeIndex)
		{
			EventTrigger eventTrigger = recipeItem.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) =>
            {
                m_RefinementComponent.NE_Send_SetActiveRecipe(recipeIndex);

                recipeItem.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

                var flash = recipeItem.transform.GetChild(3).gameObject.AddComponent<FlashSprite>();
                flash.Init(Color.white);

                UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);
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

        private void AddSlotEvents(GameObject slotItem, RefinementSlotType slotType, byte slotIndex)
		{
			EventTrigger eventTrigger = slotItem.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
                var item = m_RefinementComponent.GetItemInSlot(slotType, slotIndex);
                var cursorItem = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().GetItemOnCursor();

                if (cursorItem != null)
                {
                    UIManager.Instance.PlaySound(cursorItem.Data.laydownSoundRef);
                }
                if (item != null)
                {
                    UIManager.Instance.PlaySound(item.Data.pickupSoundRef);
                }
				
				if (UnityEngine.Input.GetKey(KeyCode.LeftShift) && UnityEngine.Input.GetMouseButton(1))
				{
					m_RefinementComponent.NE_Send_SlotShiftRightClick(slotType, slotIndex);
				}
                else if (UnityEngine.Input.GetMouseButton(1))
				{
					m_RefinementComponent.NE_Send_SlotRightClick(slotType, slotIndex);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
				{
					m_RefinementComponent.NE_Send_SlotShiftClick(slotType, slotIndex);
				}
				else
				{
					m_RefinementComponent.NE_Send_SlotClick(slotType, slotIndex);
				}
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				slotItem.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

                UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);

                var item = m_RefinementComponent.GetItemInSlot(slotType, slotIndex);
                if(item != null)
                    UIManager.HoverBoxGen.CreateItemHoverBox(item);
            });
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				slotItem.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1);

                HB_Factory.DestroyHoverbox();
			});
			eventTrigger.triggers.Add(entry);
		}

		private void AddBurnableSlotEvents()
		{
			EventTrigger eventTrigger = G_BurnableSlot.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
                var item = m_RefinementComponent.GetItemInSlot(RefinementSlotType.BURNABLE, 0);
                var cursorItem = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().GetItemOnCursor();

                if (cursorItem != null)
                {
                    UIManager.Instance.PlaySound(cursorItem.Data.laydownSoundRef);
                }
                if (item != null)
                {
                    UIManager.Instance.PlaySound(item.Data.pickupSoundRef);
                }

                if (UnityEngine.Input.GetMouseButton(1))
				{
					m_RefinementComponent.NE_Send_SlotRightClick(RefinementSlotType.BURNABLE, 0);
				}
				else if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
				{
					m_RefinementComponent.NE_Send_SlotShiftClick(RefinementSlotType.BURNABLE, 0);
				}
				else
				{
					m_RefinementComponent.NE_Send_SlotClick(RefinementSlotType.BURNABLE, 0);
				}
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				G_BurnableSlot.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

                UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);

                var item = m_RefinementComponent.GetItemInSlot(RefinementSlotType.BURNABLE, 0);
                if (item != null)
                    UIManager.HoverBoxGen.CreateItemHoverBox(item);
            });
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				G_BurnableSlot.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1);

                HB_Factory.DestroyHoverbox();
            });
			eventTrigger.triggers.Add(entry);
		}

		private void OnDestroy()
		{
			m_RefinementComponent.d_OnRefinementUpdate -= ReRenderChangingUI;
			m_RefinementComponent.d_OnRefinementProgressUpdate -= ReRenderProgress;
		}
	}
}