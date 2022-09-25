using FNZ.Client.Model.Entity.Components.EquipmentSystem;
using FNZ.Client.Model.Entity.Components.Excavator;
using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EquipmentSystem;
using FNZ.Shared.Model.Entity.Components.Excavator;
using FNZ.Shared.Model.Entity.Components.PlayerViewSetup;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FNZ.Client.View.Player.PlayerStitching
{
	public class PlayerMeshStitcher : MonoBehaviour
	{
		//private List<PlayerMeshData> m_PlayerMeshData = DataBank.Instance.GetAllDataDefsOfType<PlayerMeshData>();
		private Dictionary<Slot, Mesh> defaultBodyParts = new Dictionary<Slot, Mesh>();
		private Dictionary<Slot, Material> defaultBodyMats = new Dictionary<Slot, Material>();

		private FNEEntity m_PlayerEntity;

		[SerializeField] private SkinnedMeshRenderer wornHeadRenderer;
		[SerializeField] private SkinnedMeshRenderer wornTorsoRenderer;
		[SerializeField] private SkinnedMeshRenderer wornHandsRenderer;
		[SerializeField] private SkinnedMeshRenderer wornLegsRenderer;
		[SerializeField] private SkinnedMeshRenderer wornFeetRenderer;

		[SerializeField] private MeshRenderer equippedWeaponRenderer;

		[SerializeField] private MeshRenderer idleWeaponRenderer1;
		[SerializeField] private MeshRenderer idleWeaponRenderer2;

		[SerializeField] private Mesh m_ExcavatorMesh;
		[SerializeField] private Material m_ExcavatorMat;

		[SerializeField] private Transform m_RifleAnchor;
		[SerializeField] private Transform m_HeavyAnchor;
		[SerializeField] private Transform m_LightAnchor;

		[SerializeField] private Transform m_RifleMuzzle;
		[SerializeField] private Transform m_HeavyMuzzle;
		[SerializeField] private Transform m_LightMuzzle;
		
		[SerializeField] private Transform m_Flashlight;

		[HideInInspector]
		public Transform WeaponMuzzle;

		private Mesh ConsumableMesh1, ConsumableMesh2, ConsumableMesh3, ConsumableMesh4;
		private Material ConsumableMat1, ConsumableMat2, ConsumableMat3, ConsumableMat4;

		void Start()
		{
			// If player entity is null, we are probably in start menu screen
			if (m_PlayerEntity == null)
				return;

			WeaponMuzzle = m_HeavyMuzzle;

			var eqComp = m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>();
			eqComp.d_OnEquipmentVisualChange += UpdateItemSlot;
			eqComp.d_OnActiveItemChange += UpdateActiveItem;
			eqComp.PlayerMeshStitcherMuzzle = WeaponMuzzle;
			
			var excComp = m_PlayerEntity.GetComponent<ExcavatorComponentClient>();
			excComp.PlayerMeshStitcherMuzzle = m_HeavyMuzzle;
		}

		public void Init(FNEEntity playerEntity)
		{
			m_PlayerEntity = playerEntity;
		}
		
		public void InitStitch(EquipmentSystemComponentClient eqComp)
		{
			UpdateItemSlot(eqComp.GetItemInSlot(Slot.Torso), Slot.Torso);
			UpdateItemSlot(eqComp.GetItemInSlot(Slot.Legs), Slot.Legs);
			UpdateItemSlot(eqComp.GetItemInSlot(Slot.Feet), Slot.Feet);
			
			UpdateItemSlot(eqComp.GetItemInSlot(Slot.Weapon1), Slot.Weapon1);
			UpdateItemSlot(eqComp.GetItemInSlot(Slot.Weapon2), Slot.Weapon2);
			
			UpdateActiveItem(eqComp.GetActiveItem(), eqComp.ActiveActionBarSlot);
		}

		public void SetDefaultMeshes(List<PlayerViewData> playerSetup, int variation)
		{
			if (playerSetup != null)
			{
				var viewData = DataBank.Instance.GetData<FNEEntityViewData>(playerSetup[variation].headRef);
				var meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				var textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
				
				var headRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
				headRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(textureData, false);
				SetNewSkinnedMesh(Slot.Head, headRenderer);
				defaultBodyParts[Slot.Head] = headRenderer.sharedMesh;

				viewData = DataBank.Instance.GetData<FNEEntityViewData>(playerSetup[variation].torsoRef);
				meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
				
				var torsoRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
				torsoRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(textureData, false);
				SetNewSkinnedMesh(Slot.Torso, torsoRenderer);
				defaultBodyParts[Slot.Torso] = torsoRenderer.sharedMesh;
				defaultBodyMats[Slot.Torso] = torsoRenderer.sharedMaterial;

				viewData = DataBank.Instance.GetData<FNEEntityViewData>(playerSetup[variation].handsRef);
				meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
				
				var handsRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
				handsRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(textureData, false);
				SetNewSkinnedMesh(Slot.Hands, handsRenderer);
				defaultBodyParts[Slot.Hands] = handsRenderer.sharedMesh;
				defaultBodyMats[Slot.Hands] = handsRenderer.sharedMaterial;
				
				viewData = DataBank.Instance.GetData<FNEEntityViewData>(playerSetup[variation].legsRef);
				meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
				
				var legsRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
				legsRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(textureData, false);
				SetNewSkinnedMesh(Slot.Legs, legsRenderer);
				defaultBodyParts[Slot.Legs] = legsRenderer.sharedMesh;
				defaultBodyMats[Slot.Legs] = legsRenderer.sharedMaterial;

				viewData = DataBank.Instance.GetData<FNEEntityViewData>(playerSetup[variation].feetRef);
				meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
				
				var feetRenderer = GetSkinnedMeshRendererFromPath(meshData.MeshPath);
				feetRenderer.sharedMaterial = MaterialUtils.CreateMaterialFromTextureData(textureData, false);
				SetNewSkinnedMesh(Slot.Feet, feetRenderer);
				defaultBodyParts[Slot.Feet] = feetRenderer.sharedMesh;
				defaultBodyMats[Slot.Feet] = feetRenderer.sharedMaterial;


				//var hairRenderer = GetRendererFromPath(playerSetup[variation].hairRef.path);
				//hairRenderer.sharedMaterial = CreateMaterialFromMeshData(playerSetup[variation].hairRef);
				//SetNewMesh(Slot.Hair, hairRenderer);
				//defaultBodyParts[Slot.Hair] = hairRenderer.sharedMesh;
			}
			else
			{
				Debug.LogError("Could not load 'playerSetup'");
			}
		}

		public void UpdateItemSlot(Item item, Slot slot)
		{
			if (item == null)
			{
				switch (slot)
				{
					case Slot.Head:
						wornHeadRenderer.sharedMesh = defaultBodyParts[Slot.Head];
						wornHeadRenderer.sharedMaterial = defaultBodyMats[Slot.Head];
						break;

					case Slot.Torso:
						wornTorsoRenderer.sharedMesh = defaultBodyParts[Slot.Torso];
						wornTorsoRenderer.sharedMaterial = defaultBodyMats[Slot.Torso];
						break;

					case Slot.Legs:
						wornLegsRenderer.sharedMesh = defaultBodyParts[Slot.Legs];
						wornLegsRenderer.sharedMaterial = defaultBodyMats[Slot.Legs];
						break;

					case Slot.Feet:
						wornFeetRenderer.sharedMesh = defaultBodyParts[Slot.Feet];
						wornFeetRenderer.sharedMaterial = defaultBodyMats[Slot.Feet];
						break;

					case Slot.Waist:
						break;

					case Slot.Back:
						break;

					case Slot.Hands:
						wornHandsRenderer.sharedMesh = defaultBodyParts[Slot.Hands];
						wornHandsRenderer.sharedMaterial = defaultBodyMats[Slot.Hands];
						break;

					case Slot.Weapon1:
						idleWeaponRenderer1.GetComponent<MeshFilter>().sharedMesh = null;
						break;

					case Slot.Weapon2:
						idleWeaponRenderer2.GetComponent<MeshFilter>().sharedMesh = null;
						break;

					case Slot.Trinket1:
						break;

					case Slot.Trinket2:
						break;

					case Slot.Consumable1:
						break;

					case Slot.Consumable2:
						break;

					case Slot.Consumable3:
						break;

					case Slot.Consumable4:
						break;

					case Slot.Excavator:
						break;

					case Slot.None:
						break;

					default:
						break;
				}
			}
			else
			{
				var itemData = item.GetComponent<ItemEquipmentComponent>().Data;
				//var excavatorMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(itemData.ItemMeshData);
				//var meshPath = itemData.MeshPath;
				
				FNEEntityMeshData itemMeshData = null;
				//FNEEntityTextureData itemTextureData = null;
				Material mat = null;
				FNEEntityMeshData equipmentMeshData = null;
				FNEEntityTextureData equipmentTextureData = null;

				if (!string.IsNullOrEmpty(itemData.ItemMeshRef))
				{
					itemMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(itemData.ItemMeshRef);
					var itemTextureData = DataBank.Instance.GetData<FNEEntityTextureData>(itemData.ItemTextureRef);

					var compDatas = DataBank.Instance.GetData<FNEEntityData>("player").components;
					var excavatorCompData = (ExcavatorComponentData)compDatas.Find(c => c is ExcavatorComponentData);
					var excavatorMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(excavatorCompData.excavatorMeshData);
					var excavatorTextureData = DataBank.Instance.GetData<FNEEntityTextureData>(excavatorCompData.excavatorTextureData);

					equipmentMeshData = slot == Slot.Excavator ? excavatorMeshData : itemMeshData;
					equipmentTextureData = slot == Slot.Excavator ? excavatorTextureData : itemTextureData;
				}				
				
				if (equipmentTextureData != null)
				{
					mat = MaterialUtils.CreateMaterialFromTextureData(equipmentTextureData, false);
				} else if (equipmentMeshData != null)
				{
					mat = MaterialUtils.CreateMaterialFromMeshData(equipmentMeshData, false);
				}

				switch (slot)
				{
					case Slot.Head:
					case Slot.Torso:
					case Slot.Legs:
					case Slot.Feet:
					case Slot.Waist:
					case Slot.Back:
					case Slot.Hands:
						var renderer = GetSkinnedMeshRendererFromPath(equipmentMeshData.MeshPath);

						mat.Lerp(mat, mat, 1);
						renderer.sharedMaterial = mat;

						if (renderer == null)
							Debug.LogError("Error: MeshPath didn't return a valid object.");
						else
						{
							SetNewSkinnedMesh(slot, renderer);
						}
						break;

					case Slot.Weapon1:
					case Slot.Weapon2:
						var weaponData = (ItemWeaponComponentData)itemData;
						var rendererAndMesh = GetMeshRendererFromPath(equipmentMeshData.MeshPath);

						mat.Lerp(mat, mat, 1);
						rendererAndMesh.Item1.sharedMaterial = mat;

						if (rendererAndMesh.Item1 == null)
							Debug.LogError("Error: MeshPath didn't return a valid object.");
						else
						{
							SetNewStaticMesh(slot, rendererAndMesh.Item1, rendererAndMesh.Item2, weaponData.scaleMod);
						}
						break;

					case Slot.Excavator:
						break;

					case Slot.Trinket1:
					case Slot.Trinket2:
						break;

					case Slot.Consumable1:
					case Slot.Consumable2:
					case Slot.Consumable3:
					case Slot.Consumable4:
						Tuple<MeshRenderer, Mesh> consumableRendererAndMesh;
						Material consumableMat;
						if (itemMeshData == null)
						{
							consumableRendererAndMesh = new Tuple<MeshRenderer, Mesh>(null, null);
							consumableMat = null;
						}
						else
						{
							var consumableData = (ItemConsumableComponentData)item.Data.GetComponentData<ItemEquipmentComponentData>();
							var meshData = DataBank.Instance.GetData<FNEEntityMeshData>(consumableData.ItemMeshRef);
							consumableRendererAndMesh = GetMeshRendererFromPath(meshData.MeshPath);

							consumableMat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
							consumableMat = MaterialUtils.CreateMaterialFromMeshData(meshData, false);
							consumableMat.Lerp(consumableMat, consumableMat, 1);
						}

						switch (slot)
                        {
							case Slot.Consumable1:
								ConsumableMesh1 = consumableRendererAndMesh.Item2;
								ConsumableMat1 = consumableMat;
								break;

							case Slot.Consumable2:
								ConsumableMesh2 = consumableRendererAndMesh.Item2;
								ConsumableMat2 = consumableMat;
								break;

							case Slot.Consumable3:
								ConsumableMesh3 = consumableRendererAndMesh.Item2;
								ConsumableMat3 = consumableMat;
								break;

							case Slot.Consumable4:
								ConsumableMesh4 = consumableRendererAndMesh.Item2;
								ConsumableMat4 = consumableMat;
								break;
						}
						break;

					case Slot.None:
						break;

					default:
						break;
				}


			}
		}

		private SkinnedMeshRenderer GetSkinnedMeshRendererFromPath(string path)
		{
			//Don't really NEED to do this but it keeps things nice and clean in the Hierarchy
			GameObject temp = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{path}");
			if (temp == null)
				return null;

			var renderer = temp.GetComponentInChildren<SkinnedMeshRenderer>();

			Destroy(temp);
			return renderer;
		}

		private Tuple<MeshRenderer, Mesh> GetMeshRendererFromPath(string path)
		{
			//Don't really NEED to do this but it keeps things nice and clean in the Hierarchy
			GameObject temp = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{path}");
			if (temp == null)
				return null;

			var renderer = temp.GetComponentInChildren<MeshRenderer>();
			var mesh = temp.GetComponentInChildren<MeshFilter>().mesh;

			Destroy(temp);
			return new Tuple<MeshRenderer, Mesh>(renderer, mesh);
		}

		private void SetNewSkinnedMesh(Slot slot, SkinnedMeshRenderer newRenderer)
		{
			if (newRenderer == null)
				return;

			switch (slot)
			{
				case Slot.Head:
					ProcessRenderer(wornHeadRenderer, newRenderer);
					wornHeadRenderer.sharedMesh = newRenderer.sharedMesh;
					wornHeadRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;

				case Slot.Torso:
					ProcessRenderer(wornTorsoRenderer, newRenderer);
					wornTorsoRenderer.sharedMesh = newRenderer.sharedMesh;
					wornTorsoRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;

				case Slot.Legs:
					ProcessRenderer(wornLegsRenderer, newRenderer);
					wornLegsRenderer.sharedMesh = newRenderer.sharedMesh;
					wornLegsRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;

				case Slot.Feet:
					ProcessRenderer(wornFeetRenderer, newRenderer);
					wornFeetRenderer.sharedMesh = newRenderer.sharedMesh;
					wornFeetRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;

				case Slot.Waist:
					break;

				case Slot.Back:
					break;

				case Slot.Hands:
					ProcessRenderer(wornHandsRenderer, newRenderer);
					wornHandsRenderer.sharedMesh = newRenderer.sharedMesh;
					wornHandsRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;

				case Slot.Trinket1:
					break;

				case Slot.Trinket2:
					break;

				case Slot.Consumable1:
					break;

				case Slot.Consumable2:
					break;

				case Slot.Consumable3:
					break;

				case Slot.Consumable4:
					break;

				case Slot.Excavator:
					break;

				case Slot.None:
					break;

				default:
					break;
			}
		}

		private void UpdateActiveItem(Item item, Slot slot)
		{
			switch (slot)
			{
				case Slot.Weapon1:
					equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = idleWeaponRenderer1.GetComponent<MeshFilter>().sharedMesh;
					equippedWeaponRenderer.sharedMaterial = idleWeaponRenderer1.sharedMaterial;

					idleWeaponRenderer1.enabled = false;
					idleWeaponRenderer2.enabled = true;

					if (item != null)
					{
						var weaponData = item.GetComponent<ItemWeaponComponent>().Data;

						switch (weaponData.weaponPosture)
						{
							case WeaponPosture.RIFLE:
								WeaponMuzzle = m_RifleMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_RifleAnchor);
								break;

							case WeaponPosture.HEAVY:
								WeaponMuzzle = m_HeavyMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_HeavyAnchor);
								break;

							case WeaponPosture.LIGHT:
								WeaponMuzzle = m_LightMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_LightAnchor);
								break;
						}

						m_Flashlight.transform.SetParent(WeaponMuzzle.transform);
						m_Flashlight.transform.localPosition = Vector3.zero;
						m_Flashlight.transform.localRotation = Quaternion.identity;
						
						equippedWeaponRenderer.transform.localPosition = Vector3.zero;
						equippedWeaponRenderer.transform.localRotation = Quaternion.identity;
						equippedWeaponRenderer.transform.localScale = Vector3.one * weaponData.scaleMod;

						WeaponMuzzle.transform.localPosition = new Vector3(
							weaponData.muzzleOffsetX,
							-weaponData.muzzleOffsetY,
							-weaponData.muzzleOffsetZ
						);

						var eqComp = m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>();
						eqComp.PlayerMeshStitcherMuzzle = WeaponMuzzle;
					}
					break;

				case Slot.Weapon2:
					equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = idleWeaponRenderer2.GetComponent<MeshFilter>().sharedMesh;
					equippedWeaponRenderer.sharedMaterial = idleWeaponRenderer2.sharedMaterial;
					idleWeaponRenderer2.enabled = false;
					idleWeaponRenderer1.enabled = true;

					if (item != null)
					{
						var weaponData = item.GetComponent<ItemWeaponComponent>().Data;

						switch (item.GetComponent<ItemWeaponComponent>().Data.weaponPosture)
						{
							case WeaponPosture.RIFLE:
								WeaponMuzzle = m_RifleMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_RifleAnchor);
								break;

							case WeaponPosture.HEAVY:
								WeaponMuzzle = m_HeavyMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_HeavyAnchor);
								break;

							case WeaponPosture.LIGHT:
								WeaponMuzzle = m_LightMuzzle;
								equippedWeaponRenderer.transform.SetParent(m_LightAnchor);
								break;
						}

						m_Flashlight.transform.SetParent(WeaponMuzzle.transform);
						m_Flashlight.transform.localPosition = Vector3.zero;
						m_Flashlight.transform.localRotation = Quaternion.identity;
						
						equippedWeaponRenderer.transform.localPosition = Vector3.zero;
						equippedWeaponRenderer.transform.localRotation = Quaternion.identity;
						equippedWeaponRenderer.transform.localScale = Vector3.one * weaponData.scaleMod;

						WeaponMuzzle.transform.localPosition = new Vector3(
							weaponData.muzzleOffsetX,
							-weaponData.muzzleOffsetY,
							-weaponData.muzzleOffsetZ
						);

						var eqComp = m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>();
						eqComp.PlayerMeshStitcherMuzzle = WeaponMuzzle;
					}
					break;

				case Slot.Excavator:
					if (m_ExcavatorMesh == null)
					{
						var compDatas = DataBank.Instance.GetData<FNEEntityData>("player").components;
						var excavatorCompData = (ExcavatorComponentData)compDatas.Find(c => c is ExcavatorComponentData);
						var excavatorMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(excavatorCompData.excavatorMeshData);
						var excavatorTexData = DataBank.Instance.GetData<FNEEntityTextureData>(excavatorCompData.excavatorTextureData);

						Material mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));

						mat = MaterialUtils.CreateMaterialFromTextureData(excavatorTexData, false);

						var rendererAndMesh = GetMeshRendererFromPath(excavatorMeshData.MeshPath);

						mat.Lerp(mat, mat, 1);
						rendererAndMesh.Item1.sharedMaterial = mat;

						if (rendererAndMesh.Item1 == null)
							Debug.LogError("Error: MeshPath didn't return a valid object.");
						else
						{
							SetNewStaticMesh(slot, rendererAndMesh.Item1, rendererAndMesh.Item2);
						}

						m_ExcavatorMesh = rendererAndMesh.Item2;
						m_ExcavatorMat = mat;
					}

					equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = m_ExcavatorMesh;
					equippedWeaponRenderer.sharedMaterial = m_ExcavatorMat;
					idleWeaponRenderer2.enabled = true;
					idleWeaponRenderer1.enabled = true;

					equippedWeaponRenderer.transform.SetParent(m_HeavyAnchor);
					equippedWeaponRenderer.transform.localPosition = Vector3.zero;
					equippedWeaponRenderer.transform.localRotation = Quaternion.identity;

					m_HeavyMuzzle.transform.localPosition = new Vector3(
						0.279f,
						0.0313f,
						0.0003f
					);
					
					m_Flashlight.transform.SetParent(m_HeavyMuzzle);
					m_Flashlight.transform.localPosition = Vector3.zero;
					m_Flashlight.transform.localRotation = Quaternion.identity;
					break;

				case Slot.Consumable1:
					OnEquipConsumableVisuals(item);

					if (item != null)
					{
						equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = ConsumableMesh1;
						equippedWeaponRenderer.sharedMaterial = ConsumableMat1;
					}
					break;

				case Slot.Consumable2:
					OnEquipConsumableVisuals(item);

					if (item != null)
					{
						equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = ConsumableMesh2;
						equippedWeaponRenderer.sharedMaterial = ConsumableMat2;
					}
					break;

				case Slot.Consumable3:
					OnEquipConsumableVisuals(item);

					if (item != null)
					{
						equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = ConsumableMesh3;
						equippedWeaponRenderer.sharedMaterial = ConsumableMat3;
					}
					break;

				case Slot.Consumable4:
					OnEquipConsumableVisuals(item);

					if (item != null)
					{
						equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = ConsumableMesh4;
						equippedWeaponRenderer.sharedMaterial = ConsumableMat4;
					}
					break;
			}
		}

		private void OnEquipConsumableVisuals(Item item)
        {
			if (item != null)
			{
				equippedWeaponRenderer.transform.SetParent(m_LightAnchor);
				equippedWeaponRenderer.transform.localPosition = Vector3.zero;
				equippedWeaponRenderer.transform.localRotation = Quaternion.identity;

				WeaponMuzzle = m_LightMuzzle;
				WeaponMuzzle.transform.localPosition = new Vector3(
					0,
					0,
					0
				);

				var eqComp2 = m_PlayerEntity.GetComponent<EquipmentSystemComponentClient>();
				eqComp2.PlayerMeshStitcherMuzzle = WeaponMuzzle;
			}
			{
				equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = null;
			}

			idleWeaponRenderer2.enabled = true;
			idleWeaponRenderer1.enabled = true;
		}

		private void SetEquippedItemStaticMesh(Slot slot, MeshRenderer newRenderer, Mesh mesh)
		{
			switch (slot)
			{
				case Slot.Weapon1:
				case Slot.Weapon2:
					equippedWeaponRenderer.GetComponent<MeshFilter>().sharedMesh = mesh;
					equippedWeaponRenderer.sharedMaterial = newRenderer.sharedMaterial;
					break;
			}
		}

		private void SetNewStaticMesh(Slot slot, MeshRenderer newRenderer, Mesh mesh, float scaleMod = 1)
		{
			switch (slot)
			{
				case Slot.Weapon1:
					idleWeaponRenderer1.GetComponent<MeshFilter>().sharedMesh = mesh;
					idleWeaponRenderer1.sharedMaterial = newRenderer.sharedMaterial;

					idleWeaponRenderer1.transform.localScale = Vector3.one * scaleMod;
					break;

				case Slot.Weapon2:
					idleWeaponRenderer2.GetComponent<MeshFilter>().sharedMesh = mesh;
					idleWeaponRenderer2.sharedMaterial = newRenderer.sharedMaterial;

					idleWeaponRenderer2.transform.localScale = Vector3.one * scaleMod;
					break;
			}
		}

		/// <summary>
		/// Process NewRenderer to sort the order of the bones according to the ReferenceRenderer and adjust boneweight and bindposes.
		/// </summary>
		/// <param name="referenceRenderer">Renderer that contains the correct order of bones</param>
		/// <param name="newRenderer">Renderer that contains the new Mesh</param>
		private void ProcessRenderer(SkinnedMeshRenderer referenceRenderer, SkinnedMeshRenderer newRenderer)
		{
			List<Transform> tListRenderer = newRenderer.bones.ToList();

			Dictionary<int, int> refMapping = new Dictionary<int, int>();

			for (int i = 0; i < referenceRenderer.bones.Length; i++)
			{
				Transform bone = Array.Find(newRenderer.bones, transform => transform.name == referenceRenderer.bones[i].name);
				refMapping[i] = tListRenderer.IndexOf(bone);
			}

			Transform[] newBoneOrder = new Transform[newRenderer.bones.Length];
			for (int i = 0; i < newRenderer.bones.Length; i++)
			{
				newBoneOrder[i] = newRenderer.bones[refMapping[i]];
			}
			List<Transform> newBoneOrderList = newBoneOrder.ToList();

			BoneWeight[] boneWeights = newRenderer.sharedMesh.boneWeights;
			for (int i = 0; i < boneWeights.Length; i++)
			{
				boneWeights[i].boneIndex0 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex0]);
				boneWeights[i].boneIndex1 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex1]);
				boneWeights[i].boneIndex2 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex2]);
				boneWeights[i].boneIndex3 = newBoneOrderList.IndexOf(newRenderer.bones[boneWeights[i].boneIndex3]);
			}

			Matrix4x4[] bindPoses = newRenderer.sharedMesh.bindposes;
			for (int i = 0; i < bindPoses.Length; i++)
			{
				bindPoses[i] = newRenderer.sharedMesh.bindposes[refMapping[i]];
			}

			newRenderer.bones = newBoneOrder;
			newRenderer.sharedMesh.boneWeights = boneWeights;
			newRenderer.sharedMesh.bindposes = bindPoses;
		}
	}
}