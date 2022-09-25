using System;
using System.Collections;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Unity.Mathematics;
using UnityEngine;

namespace FNZ.Client.View.Player.Items
{
	
	
	public class PlayerItemTargetingView : MonoBehaviour
	{
		
		private enum TargetType
		{
			NADA = -1,
			TILE_OBJECT = 0,
		}
		
		[SerializeField]
		private GameObject P_TileObjectTargetPreview;

		private GameObject m_TargetPreview;

		private FNEEntity m_Player;

		private TargetType m_TargetType = TargetType.NADA;

		private float2 m_TargetPoint;
		
		public void Init(FNEEntity player)
		{
			m_Player = player;
			if (player == GameClient.LocalPlayerEntity)
			{
				player.GetComponent<EquipmentSystemComponentClient>().d_OnActiveItemChange += ActiveItemChange;
			}
		}

		public void ActiveItemChange(Item item, Slot slot)
		{
			if (m_TargetPreview != null)
			{
				Destroy(m_TargetPreview);
			}
			
			m_TargetType = TargetType.NADA;
			
			if (item == null)
			{
				return;
			}
			
			if (slot == Slot.Consumable1 
			    || slot == Slot.Consumable2 
			    || slot == Slot.Consumable3 
			    || slot == Slot.Consumable4)
			{
				var consumableComponent = item.GetComponent<ItemConsumableComponent>();
				if (consumableComponent != null)
				{
					var data = consumableComponent.Data;
					if (!string.IsNullOrEmpty(data.buildingRef))
					{
						m_TargetPreview = Instantiate(P_TileObjectTargetPreview);
						m_TargetType = TargetType.TILE_OBJECT;
					}
				}
			}
		}

		void Update()
		{
			RaycastHit hit;
			Ray ray = UnityEngine.Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
			int layerMask = 1 << LayerMask.NameToLayer("Ground");
			
			if (Physics.Raycast(ray, out hit, 10000f, layerMask))
			{
				switch (m_TargetType)
				{
					case TargetType.TILE_OBJECT:
						var snap = new Vector3(
							((int) hit.point.x) + 0.5f,
							0f,
							((int) hit.point.z) + 0.5f
						);
						m_TargetPoint = new float2(Mathf.Floor(hit.point.x), Mathf.Floor(hit.point.z));
						m_TargetPreview.transform.position = snap;
						m_Player.GetComponent<EquipmentSystemComponentClient>().CurrentTargetPosition = m_TargetPoint;
						break;
				}
			}
		}
	}
}