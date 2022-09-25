using FNZ.Client.Model.Entity.Components.Builder;
using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Client.Utils.UI;
using FNZ.Client.View.Player;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Building;
using FNZ.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Building
{
	public class BuildingUI : MonoBehaviour
	{
		public GameObject P_BuildingCategory;
		public GameObject P_BuildingEntry;

        public GameObject G_AddonModeButton;

        public Transform CategoryParent;
		public Transform BuildingParent;

		public Sprite s_CategoryInactive;
		public Sprite s_CategoryActive;
		public Sprite s_CategoryHover;
		public Sprite s_CategoryToggled;

		private BuilderComponentClient m_Builder;
		private PlayerComponentClient m_Player;

		private Dictionary<string, int> m_UnlockedCategories;
		private Dictionary<string, GameObject> m_UnlockedCategoryGameObjects;

		private List<string> m_UnlockedBuildings;
		private List<KeyValuePair<string, int>> m_SortedList;

		private string m_ActiveCategory = "";

		int categoryIndex;

		public static readonly int CATEGORY_WIDTH = 85;

		public void Init()
		{
			if (m_Player == null)
			{
				m_Player = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
				m_Builder = m_Player.ParentEntity.GetComponent<BuilderComponentClient>();

				m_Player.d_OnUnlockBuildingsChange += ReRenderBuildings;

				m_UnlockedCategories = new Dictionary<string, int>();
				m_UnlockedCategoryGameObjects = new Dictionary<string, GameObject>();

				m_SortedList = new List<KeyValuePair<string, int>>();

				categoryIndex = 0;
			}

			m_UnlockedBuildings = m_Player.GetUnlockedBuildings();

			if (string.IsNullOrEmpty(m_ActiveCategory))
			{
				var unlockedArr = m_UnlockedCategories.Keys.ToArray();
				if (unlockedArr.Length > 0)
				{
					m_ActiveCategory = unlockedArr[0];
					m_UnlockedCategoryGameObjects[m_ActiveCategory].GetComponent<Image>().sprite = s_CategoryToggled;
				}
			}
		}

		void Start()
		{
			P_BuildingEntry = PrefabBank.GetPrefab("Prefab/MainUI/Building/BuildingEntry");
			P_BuildingCategory = PrefabBank.GetPrefab("Prefab/MainUI/Building/CategoryEntry");

            AddEventToAddonModeButton();

            BuildUI();
		}

		private void BuildUI()
		{
			Dictionary<string, List<BuildingData>> categories = m_Builder.BuildingCategoryLists;
			m_UnlockedCategoryGameObjects.Clear();

			foreach (var categoryId in categories.Keys)
			{
				foreach (var building in m_UnlockedBuildings)
				{
					var buildingData = DataBank.Instance.GetData<BuildingData>(building);
					if (categoryId != buildingData.categoryRef || m_UnlockedCategories.ContainsKey(categoryId))
						continue;

					var categoryData = DataBank.Instance.GetData<BuildingCategoryData>(categoryId);
					var newCategory = Instantiate(P_BuildingCategory);

					newCategory.transform.SetParent(CategoryParent, false);
					// newCategory.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
					newCategory.transform.GetChild(0).GetComponent<Image>().sprite = SpriteBank.GetSprite(categoryData.iconRef);

					AddEventToCategory(newCategory, categoryData);

					newCategory.name = "Category: " + categoryId;
					m_UnlockedCategories.Add(categoryId, categoryData.preferredIndex);
					m_UnlockedCategoryGameObjects.Add(categoryId, newCategory);
					SortDictionary();
					categoryIndex++;
				}
			}

			if(string.IsNullOrEmpty(m_ActiveCategory))
			{
				m_ActiveCategory = m_UnlockedCategories.Keys.ToArray()[0];
				m_UnlockedCategoryGameObjects[m_ActiveCategory].GetComponent<Image>().sprite = s_CategoryToggled;
			}


			foreach (var categoryId in categories.Keys)
			{
				if (categoryId == m_ActiveCategory)
				{
					foreach (var buildingData in categories[categoryId])
					{
						if (m_UnlockedBuildings.Contains(buildingData.Id))
						{
							GameObject newBuilding = Instantiate(P_BuildingEntry);
							newBuilding.transform.SetParent(BuildingParent, false);
							// newBuilding.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
							newBuilding.transform.GetChild(0).GetComponent<Image>().sprite = SpriteBank.GetSprite(buildingData.iconRef);

							AddEventToBuilding(newBuilding, buildingData);
						}
					}
				}
			}

			//Places transforms in their proper place.
			int i = 0;
			foreach (var kvp in m_SortedList)
			{
				foreach (Transform child in CategoryParent)
				{
					if (child.name.Contains(kvp.Key))
					{
						child.transform.localPosition = new Vector2(i * CATEGORY_WIDTH, 0);
						i++;
						break;
					}
				}
			}

		}

		private void SortDictionary()
		{
			m_SortedList.Clear();
			m_SortedList = m_UnlockedCategories.OrderBy(x => x.Value).ToList();
		}

		public void ReRenderBuildings()
		{
			foreach (Transform t in BuildingParent)
				Destroy(t.gameObject);

			BuildUI();
		}

		private void AddEventToCategory(GameObject category, BuildingCategoryData categoryData)
		{
			EventTrigger eventTrigger = category.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnCategoryDownClick(category);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerUp;
			entry.callback.AddListener((eventData) =>
			{
				OnCategoryUpClick(category, categoryData.Id);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnCategoryHover(category, categoryData);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				OnCategoryExit(category, categoryData.Id);
			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnCategoryDownClick(GameObject category)
		{
			category.GetComponent<Image>().sprite = s_CategoryActive;
		}
		public void OnCategoryUpClick(GameObject category, string id)
		{
			m_ActiveCategory = id;

			foreach (var key in m_UnlockedCategoryGameObjects.Keys)
			{
				if (key == id)
				{
					m_UnlockedCategoryGameObjects[key].GetComponent<Image>().sprite = s_CategoryToggled;
				} else
				{
					m_UnlockedCategoryGameObjects[key].GetComponent<Image>().sprite = s_CategoryInactive;
				}
			}


			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_CLICK);

			ReRenderBuildings();
		}

		public void OnCategoryHover(GameObject category, BuildingCategoryData categoryData)
		{
			if (m_ActiveCategory != categoryData.Id)
			{
				category.GetComponent<Image>().sprite = s_CategoryHover;
			}

			UIManager.HoverBoxGen.CreateBuildingCategoryHoverBox(categoryData);
			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
		}

		public void OnCategoryExit(GameObject category, string id)
		{
			if (m_ActiveCategory != id)
			{
				category.GetComponent<Image>().sprite = s_CategoryInactive;
			}
			HB_Factory.DestroyHoverbox();
		}

		private void AddEventToBuilding(GameObject building, BuildingData buildingData)
		{
			EventTrigger eventTrigger = building.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnBuildingClick(building, buildingData);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnBuildingHover(building, buildingData);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				OnBuildingExit(building);
			});
			eventTrigger.triggers.Add(entry);
		}

		public void OnBuildingClick(GameObject building, BuildingData buildingData)
		{
			var flashSprite = building.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
			flashSprite.Init(Color.white);

			var pc = GameClient.LocalPlayerView.GetComponentInChildren<PlayerController>();
			pc.GetPlayerBuildSystem().IsBuildMode = true;
			pc.GetPlayerBuildSystem().ActiveBuilding = buildingData;

			GameClient.LocalPlayerView.GetComponentInChildren<PlayerBuildView>().SelectBuilding(buildingData);

			UIManager.Instance.PlaySound(EffectIdConstants.S_CONFIRM);
		}

		public void OnBuildingHover(GameObject building, BuildingData buildingData)
		{
			building.GetComponent<Image>().color = new Color(1, 1, 1, 1);
			UIManager.HoverBoxGen.CreateBuildingHoverBox(buildingData);
			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
		}

		public void OnBuildingExit(GameObject building)
		{
			building.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
			HB_Factory.DestroyHoverbox();
		}

        private void AddEventToAddonModeButton()
        {
            EventTrigger eventTrigger = G_AddonModeButton.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) =>
            {
                OnAddonModeButtonClick();
            });
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) =>
            {
                OnAddonModeButtonHover();
            });
            eventTrigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((eventData) =>
            {
                OnAddonModeButtonExit();
            });
            eventTrigger.triggers.Add(entry);
        }

        public void OnAddonModeButtonClick()
		{
			GameClient.LocalPlayerView.GetComponentInChildren<PlayerBuildView>().StartAddonMode();
			UIManager.Instance.PlaySound(EffectIdConstants.S_CONFIRM);

            var flashSprite = G_AddonModeButton.transform.GetChild(1).gameObject.AddComponent<FlashSprite>();
            flashSprite.Init(Color.white);
        }

        public void OnAddonModeButtonHover()
        {
            G_AddonModeButton.GetComponent<Image>().color = new Color(1, 1, 1, 1);
            UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
        }

        public void OnAddonModeButtonExit()
        {
            G_AddonModeButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
        }
    }
}