using FNZ.Client.Model.Entity.Components.BuildingAddon;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Client.View.Player.Building;
using FNZ.Client.View.UI.HoverBox;
using FNZ.Client.View.UI.Manager;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.BuildingAddon;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FNZ.Client.View.UI.Addon 
{

	public class AddonUI : MonoBehaviour
	{
		private FNEEntity m_Target;

		[SerializeField]
		private GameObject P_AddonOption;

		[SerializeField]
		private Transform[] m_OptionAnchors;

		private PlayerBuildView m_PlayerBuildView;

	    void Update()
	    {
		    if (m_Target.Data.entityType == EntityType.TILE_OBJECT)
		    {
			    transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
				    new Vector3(
					    m_Target.Position.x + 0.5f,
					    1,
					    m_Target.Position.y + 0.5f
				    ),
				    UnityEngine.Camera.MonoOrStereoscopicEye.Mono
			    );
		    }
		    else
		    {
			    transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
				    new Vector3(
					    m_Target.Position.x,
					    1,
					    m_Target.Position.y
				    ),
				    UnityEngine.Camera.MonoOrStereoscopicEye.Mono
			    );
		    }
		    
		    var distance = math.distance(m_Target.Position, GameClient.LocalPlayerEntity.Position);

		    if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		    {
			    Destroy(gameObject);
		    }
		    else if(distance > 4)
			{
				Destroy(gameObject);
				HB_Factory.DestroyHoverbox();
			}
	    }

		public void Init(FNEEntity target, PlayerBuildView playerBuildView)
		{
			m_PlayerBuildView = playerBuildView;
			m_Target = target;

			var addonOptions = target.GetComponent<BuildingAddonComponentClient>().m_Data.addonRefs;
			Render(addonOptions);
		}

		private void Render(List<string> addonOptions)
		{
			var edgeComp = m_Target.GetComponent<EdgeObjectComponentClient>();

			int anchorCounter = 0;
			foreach (var optionId in addonOptions)
			{
				var addonData = DataBank.Instance.GetData<BuildingAddonData>(optionId);
				if (edgeComp != null && edgeComp.MountedObjectData != null)
				{
					var productData = DataBank.Instance.GetData<FNEEntityData>(addonData.productRef);
					if (!productData.isMountable)
						continue;
				}
				

				var option = Instantiate(P_AddonOption);
				option.transform.SetParent(m_OptionAnchors[anchorCounter]);
				option.transform.localPosition = Vector3.zero;

				var bg = option.GetComponent<Image>();
				bg.color = FNEUtil.ConvertHexStringToColor(addonData.addonColor);

				var icon = option.transform.GetChild(0).GetComponent<Image>();
				icon.sprite = SpriteBank.GetSprite(addonData.iconRef);

				AddEventToOption(option, addonData);
				anchorCounter++;
			}

			if (edgeComp != null && edgeComp.MountedObjectData != null)
			{
				var removeMountOption = Instantiate(P_AddonOption);
				removeMountOption.transform.SetParent(m_OptionAnchors[anchorCounter]);
				removeMountOption.transform.localPosition = Vector3.zero;

				var bg = removeMountOption.GetComponent<Image>();
				bg.color = Color.red;

				var icon = removeMountOption.transform.GetChild(0).GetComponent<Image>();
				icon.sprite = SpriteBank.GetSprite("remove_mount_icon");

				AddEventToRemoveMountOption(removeMountOption);
			}

			if (m_Target.Data.entityType == EntityType.TILE_OBJECT)
			{
				transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
					new Vector3(
						m_Target.Position.x + 0.5f,
						1,
						m_Target.Position.y + 0.5f
					),
					UnityEngine.Camera.MonoOrStereoscopicEye.Mono
				);
			}
			else
			{
				transform.position = UnityEngine.Camera.main.WorldToScreenPoint(
					new Vector3(
						m_Target.Position.x,
						1,
						m_Target.Position.y
					),
					UnityEngine.Camera.MonoOrStereoscopicEye.Mono
				);
			}
			
			
		}

		private void AddEventToOption(GameObject option, BuildingAddonData addonData)
		{
			EventTrigger eventTrigger = option.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnOptionClick(option, addonData);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnOptionHover(option, addonData);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				OnOptionExit(option, addonData);
			});
			eventTrigger.triggers.Add(entry);
		}

		private void OnOptionClick(GameObject option, BuildingAddonData addonData)
		{
			Destroy(gameObject);

			m_PlayerBuildView.ExecuteAddonOption(m_Target, addonData);
			HB_Factory.DestroyHoverbox();
		}

		private void OnOptionHover(GameObject option, BuildingAddonData addonData)
		{
			var overlayImage = option.transform.GetChild(1).GetComponent<Image>();
			var addonColor = FNEUtil.ConvertHexStringToColor(addonData.addonColor);
			overlayImage.color = new Color(addonColor.r, addonColor.g, addonColor.b, 1);

			UIManager.HoverBoxGen.CreateBuildingAddonHoverBox(addonData);
			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
		}

		private void OnOptionExit(GameObject option, BuildingAddonData addonData)
		{
			var overlayImage = option.transform.GetChild(1).GetComponent<Image>();
			overlayImage.color = Color.clear;

			HB_Factory.DestroyHoverbox();
		}

		private void AddEventToRemoveMountOption(GameObject option)
		{
			EventTrigger eventTrigger = option.AddComponent<EventTrigger>();

			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerDown;
			entry.callback.AddListener((eventData) =>
			{
				OnRemoveMountOptionClick(option);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) =>
			{
				OnRemoveMountOptionHover(option);
			});
			eventTrigger.triggers.Add(entry);

			entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerExit;
			entry.callback.AddListener((eventData) =>
			{
				OnRemoveMountOptionExit(option);
			});
			eventTrigger.triggers.Add(entry);
		}

		private void OnRemoveMountOptionClick(GameObject option)
		{
			Destroy(gameObject);

			m_PlayerBuildView.ExecuteRemoveMountOption(m_Target);
			HB_Factory.DestroyHoverbox();
		}

		private void OnRemoveMountOptionHover(GameObject option)
		{
			var overlayImage = option.transform.GetChild(1).GetComponent<Image>();
			overlayImage.color = new Color(1, 0, 0, 1);

			UIManager.HoverBoxGen.CreateRemoveMountHoverBox();
			UIManager.Instance.PlaySound(EffectIdConstants.S_BUTTON_HOVER);
		}

		private void OnRemoveMountOptionExit(GameObject option)
		{
			var overlayImage = option.transform.GetChild(1).GetComponent<Image>();
			overlayImage.color = Color.clear;

			HB_Factory.DestroyHoverbox();
		}

		public void OnCloseUI()
		{
			Destroy(gameObject);
		}
	}
}