using FNZ.Client.Model.Entity.Components.Player;
using FNZ.Shared.Model.Entity.Components;
using FNZ.Shared.Model.Entity.Components.Inventory;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model;
using Lidgren.Network;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using FNZ.Shared.Model.Entity.EntityViewData;
using Siccity.GLTFUtility;
using FNZ.Client.View.UI.Manager;
using System.IO;

public delegate void OnInventoryUpdate(List<int2> cellschanged);

namespace FNZ.Client.Model.Entity.Components.Inventory
{
	public class InventoryComponentClient : InventoryComponentShared, IInteractableComponent
	{
		public OnInventoryUpdate d_OnInventoryUpdate;
		private Animation animLegacy;
		private GameObject parentGO;
		private AnimatorOverrideController aoc;
		private InventoryAnimationClipOverrides clipOverrides;
		private bool isOpen = false;

		public void NE_Send_CellClicked(int x, int y)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var inventoryCellClicked = playerComp.OpenedInteractable != null ? new InventoryCellClickedData { x = x, y = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId } : new InventoryCellClickedData { x = x, y = y };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.CELL_CLICKED, inventoryCellClicked);
		}

		public void NE_Send_CellShiftClicked(int x, int y)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var inventoryCellClicked = playerComp.OpenedInteractable != null ? new InventoryCellClickedData { x = x, y = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId } : new InventoryCellClickedData { x = x, y = y };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.CELL_SHIFT_CLICKED, inventoryCellClicked);
		}

		public void NE_Send_CellRightClicked(int x, int y)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var inventoryCellClicked = playerComp.OpenedInteractable != null ? new InventoryCellClickedData { x = x, y = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId } : new InventoryCellClickedData { x = x, y = y };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.CELL_RIGHT_CLICKED, inventoryCellClicked);
		}

		public void NE_Send_CellShiftRightClicked(int x, int y)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var inventoryCellClicked = playerComp.OpenedInteractable != null ? new InventoryCellClickedData { x = x, y = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId } : new InventoryCellClickedData { x = x, y = y };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.CELL_SHIFT_RIGHT_CLICKED, inventoryCellClicked);
			return;
		}

		public void NE_Send_ModClicked(int x, int y, byte modIndex)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var itemModClicked = playerComp.OpenedInteractable != null ? new ItemModClickedData { itemX = x, itemY = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId, modIndex = modIndex } : new ItemModClickedData { itemX = x, itemY = y, modIndex = modIndex };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.MOD_CLICKED, itemModClicked);
		}
		public void NE_Send_ModRightClicked(int x, int y, byte modIndex)
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();
			var itemModClicked = playerComp.OpenedInteractable != null ? new ItemModClickedData { itemX = x, itemY = y, InteractedEntityId = playerComp.OpenedInteractable.ParentEntity.NetId, modIndex = modIndex } : new ItemModClickedData { itemX = x, itemY = y, modIndex = modIndex };

			GameClient.NetAPI.CMD_Entity_ComponentNetEvent(this, (byte)InventoryNetEvent.MOD_RIGHT_CLICKED, itemModClicked);
		}

		public override void Deserialize(NetBuffer br)
		{
			var oldCells = cells;

			base.Deserialize(br);

			d_OnInventoryUpdate?.Invoke(GetCellDiffs(oldCells));
		}

		private List<int2> serverClientDiff = new List<int2>();
		private List<int2> GetCellDiffs(Item[] oldCells)
		{
			serverClientDiff.Clear();

			for (int i = 0; i < m_Data.width; i++)
			{
				for (int j = 0; j < m_Data.height; j++)
				{
					if (!Item.IsItemIdentical(
						cells[i + j * m_Data.width],
						oldCells[i + j * m_Data.width]
					))
					{
						serverClientDiff.Add(new int2(i, j));
					}else if(DidItemChangePlace(i, j, oldCells))
                    {
						serverClientDiff.Add(new int2(i, j));
					}
				}
			}

			return serverClientDiff;
		}

		private bool DidItemChangePlace(int x, int y, Item[] oldCells)
        {
			var oldItem = oldCells[x + y * m_Data.width];
			if (oldItem == null)
				return false;

			int2 oldTopLeftPos = new int2(x, y);
			for(int i = 0; x - i >= 0 && i < oldItem.Data.width; i++)
            {
				for (int j = 0; y - j >= 0 && j < oldItem.Data.height; j++)
				{
					if(oldCells[(x - i) + ((y - j) * m_Data.width)] == oldItem)
                    {
						oldTopLeftPos = new int2((x - i), (y - j));
					}
				}
			}

			var newItem = cells[x + y * m_Data.width];
			int2 newTopLeftPos = new int2(x, y);
			for (int i = 0; x - i >= 0 && i < newItem.Data.width; i++)
			{
				for (int j = 0; y - j >= 0 && j < newItem.Data.height; j++)
				{
					if (cells[(x - i) + ((y - j) * m_Data.width)] == newItem)
					{
						newTopLeftPos = new int2((x - i), (y - j));
					}
				}
			}

			if(newTopLeftPos.x != oldTopLeftPos.x || newTopLeftPos.y != oldTopLeftPos.y)
            {
				return true;
            }

			return false;
		}

		public void OnPlayerInRange()
		{ }

		public void OnPlayerExitRange()
		{
			var playerComp = GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>();

			if (playerComp.OpenedInteractable == this)
			{
				playerComp.SetOpenedInteractable(null);
				if (isOpen)
				{
					PlayCloseEffects();
					isOpen = false;
				}
			}
		}

		public void OnInteract()
		{
			GameClient.LocalPlayerEntity.GetComponent<PlayerComponentClient>().SetOpenedInteractable(this);
			if (!isOpen)
			{
				isOpen = true;
				PlayOpenEffects();
			} else
			{
				isOpen = false;
				PlayCloseEffects();
			}
		}

		public string InteractionPromptMessageRef()
		{
			return "inventory_component_interact";
		}

		public bool IsInteractable()
		{
			return ParentEntity.GetComponent<PlayerComponentClient>() == null;
		}

		public void OnViewInit()
		{
			parentGO = GameClient.ViewConnector.GetGameObject(ParentEntity.NetId);

			InitAnimations();
		}

		public void InitAnimations()
		{
			if (!parentGO.GetComponent<Animation>())
				animLegacy = parentGO.AddComponent<Animation>();
			else if (animLegacy == null)
				animLegacy = parentGO.GetComponent<Animation>();

			var rac = Resources.Load("Prefab/Entity/TileObject/InventoryObjects/InventoryAnimator") as RuntimeAnimatorController;
			aoc = new AnimatorOverrideController(rac);
			clipOverrides = new InventoryAnimationClipOverrides(aoc.overridesCount);
			aoc.GetOverrides(clipOverrides);

			var viewRef = DataBank.Instance.GetData<FNEEntityViewData>(ParentEntity.Data.entityViewVariations[0]);
			var animClips = new List<AnimationClip>();

			foreach (var animData in viewRef.animations)
			{
				GLTFAnimation.ImportResult[] importedAnimations;
				importedAnimations = Importer.LoadAnimationsFromFile($"{Application.streamingAssetsPath}/{animData.animPath}");

				if (importedAnimations == null)
				{
					Debug.LogError($"Error: {Path.GetFileName(animData.animPath)} \n" + $"No animations found in object at {animData.animPath}.");
					break;
				}

				foreach (var animation in importedAnimations)
				{
					animClips.Add(animation.clip);
				}
			}

			//Add imported animations to override.
			foreach (var animation in animClips)
			{
				//These "ClipOverrides[-]" are the names of dummy animator animations.
				//Don't confuse them with the XML entries.
				if (m_Data.closeAnimationName != null && animation.name.Equals(m_Data.closeAnimationName))
				{
					clipOverrides["Close"] = animation;
				}

				if (m_Data.openAnimationName != null && animation.name.Equals(m_Data.openAnimationName))
				{
					clipOverrides["Open"] = animation;
				}
			}

			var openClip = clipOverrides["Open"];
			if (openClip != null)
            {
				animLegacy.AddClip(openClip, openClip.name);
			}

			var closeClip = clipOverrides["Close"];
			if (closeClip != null)
			{
				animLegacy.AddClip(closeClip, closeClip.name);
			}

			//Error handling
			/*foreach (var ovr in clipOverrides)
			{
				if (ovr.Value == null)
				{
					var meshRef = DataBank.Instance.GetData<FNEEntityMeshData>(viewRef.entityMeshData);
					switch (ovr.Key.name)
					{
						case "Open":
							Debug.LogWarning(
								$"Error: {Path.GetFileName($"{Application.streamingAssetsPath}/{meshRef.MeshPath}")} \n" +
								$"openAnimationName in InventoryComponentData for {ParentEntity.Data.Id} did not return a valid animation. Make sure the name is correct and that the animation exists.");
							break;

						case "Close":
							Debug.LogWarning(
								$"Error: {Path.GetFileName($"{Application.streamingAssetsPath}/{meshRef.MeshPath}")} \n" +
								$"closeAnimationName in InventoryComponentData for {ParentEntity.Data.Id} did not return a valid animation.Make sure the name is correct and that the animation exists.");
							break;

						default:
							Debug.LogWarning("This should never happen!");
							break;
					}
				}
			}*/

			PlayCloseEffects();
		}

		private void PlayOpenEffects()
		{
			var openClip = clipOverrides["Open"];
			if (openClip != null)
				animLegacy.Play(openClip.name);
		}

		private void PlayCloseEffects()
		{
			var closeClip = clipOverrides["Close"];
			if (closeClip != null)
				animLegacy.Play(closeClip.name);
		}
	}

	public class InventoryAnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
	{
		public InventoryAnimationClipOverrides(int capacity) : base(capacity) { }

		public AnimationClip this[string name]
		{
			get { return this.Find(x => x.Key.name.Equals(name)).Value; }
			set
			{
				int index = this.FindIndex(x => x.Key.name.Equals(name));
				if (index != -1)
					this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
			}
		}
	}
}
