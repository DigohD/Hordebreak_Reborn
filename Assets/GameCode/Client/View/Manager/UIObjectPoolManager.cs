using FNZ.Client.View.Prefab;
using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Client.View.UI
{

	public enum UIObjectType
	{
		INVENTORY_ITEM_CELL = 0,
		INVENTORY_CELL_FRAME = 1,
		INVENTORY_ITEM = 2,
		WEAPON_MOD_SLOT = 3,
	}

	public static class UIObjectPoolManager
	{
		private static Dictionary<UIObjectType, Stack<GameObject>> s_Pool = new Dictionary<UIObjectType, Stack<GameObject>>();
		private static Transform m_PoolParent = GameObject.Find("UIObjectPool").transform;

		public static void RecycleObject(UIObjectType objectType, GameObject go)
		{
			if (!s_Pool.ContainsKey(objectType))
				s_Pool.Add(objectType, new Stack<GameObject>());

			go.transform.SetParent(m_PoolParent, false);
			go.SetActive(false);
			s_Pool[objectType].Push(go);
		}

		public static bool HasObjectInstance(UIObjectType objectType)
		{
			return (s_Pool.ContainsKey(objectType) && s_Pool[objectType].Count > 0);
		}

		public static GameObject GetObjectInstance(UIObjectType objectType)
		{
			if (s_Pool.ContainsKey(objectType) && s_Pool[objectType].Count > 0)
			{
				var go = s_Pool[objectType].Pop();
				go.SetActive(true);

				return go;
			}

			return GameObject.Instantiate(GetUIObjectPrefab(objectType));
		}

		private static GameObject GetUIObjectPrefab(UIObjectType objectType)
		{
			switch (objectType)
			{
				case UIObjectType.INVENTORY_ITEM_CELL:
					return PrefabBank.GetPrefab("Prefab/ComponentUI/InventoryUI/ItemCell");

				case UIObjectType.INVENTORY_CELL_FRAME:
					return PrefabBank.GetPrefab("Prefab/ComponentUI/InventoryUI/CellFrame");

				case UIObjectType.INVENTORY_ITEM:
					return PrefabBank.GetPrefab("Prefab/ComponentUI/InventoryUI/Item");

				case UIObjectType.WEAPON_MOD_SLOT:
					return PrefabBank.GetPrefab("Prefab/ComponentUI/InventoryUI/WeaponSlot");
			}

			return null;
		}
	}
}