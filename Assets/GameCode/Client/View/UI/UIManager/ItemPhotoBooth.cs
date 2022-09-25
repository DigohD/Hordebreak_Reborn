using FNZ.Client.Utils;
using FNZ.Client.View.UI.Sprites;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items;
using FNZ.Shared.Model.Items.Components;
using Siccity.GLTFUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FNZ.Client.View.UI
{
	public class ItemPhotoBooth : MonoBehaviour
	{

		public MeshRenderer TorsoRenderer2;
		public MeshFilter TorsoRendererMesh;
		public SkinnedMeshRenderer TorsoRenderer;
		public SkinnedMeshRenderer WeaponRenderer;

		public Image test;

		public UnityEngine.Camera ClothesRenderCam;
		public UnityEngine.Camera WeaponRenderCam;

		public Transform TorsoAnchor, LegsAnchor, FeetAnchor, HandsAnchor, HeadAnchor;

		public CustomRenderTexture ClothingRT;
		public CustomRenderTexture WeaponRT;

		public void RenderAllGear()
		{
			StartCoroutine("ShootItems");
		}

		public static Dictionary<string, Texture2D> GeneratedIcons = new Dictionary<string, Texture2D>();

		Material mat;

		string currentClothingSpriteDataDef = string.Empty;
		string currentWeaponSpriteDataDef = string.Empty;

		private bool m_ClothessDone;
		private bool m_WeaponsDone;
		private bool m_AllDone;

		private GameObject tempClothingItem;
		private GameObject tempWeaponItem;

		public bool IsDone()
		{
			return m_AllDone;
		}

		public IEnumerator ShootItems()
		{
			var AllItems = DataBank.Instance.GetAllDataIdsOfType<ItemData>();

			var allClothing = AllItems.FindAll(item => item.GetComponentData<ItemClothingComponentData>() != null);
			var allWeapons = AllItems.FindAll(item => item.GetComponentData<ItemWeaponComponentData>() != null);

			int counter = 0;
			while (!m_ClothessDone || !m_WeaponsDone)
			{
				if (counter < allClothing.Count && !GeneratedIcons.ContainsKey(allClothing[counter].Id))
				{
					PrepareClothingItem(allClothing[counter]);
				}

				if (counter < allWeapons.Count && !GeneratedIcons.ContainsKey(allWeapons[counter].Id))
				{
					PrepareWeaponItem(allWeapons[counter]);
				}

				yield return new WaitForEndOfFrame();

				if (counter < allClothing.Count && !GeneratedIcons.ContainsKey(allClothing[counter].Id))
				{
					PrepareClothingRenderTexture(allClothing[counter]);
				}

				if (counter < allWeapons.Count && !GeneratedIcons.ContainsKey(allWeapons[counter].Id))
				{
					PrepareWeaponRenderTexture(allWeapons[counter]);
				}

				StartCoroutine("ShootClothingAndWeaponItem");

				Destroy(tempClothingItem);

				yield return new WaitForEndOfFrame();

				counter++;

				if (counter == allClothing.Count)
					m_ClothessDone = true;

				if (counter == allWeapons.Count)
					m_WeaponsDone = true;
			}

			m_AllDone = true;
		}

		private void PrepareClothingItem(ItemData itemData)
		{
			var eqCompData = (ItemClothingComponentData)itemData.GetComponentData<ItemClothingComponentData>();
			var itemMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(eqCompData.ItemMeshRef);
			var itemTextureData = DataBank.Instance.GetData<FNEEntityTextureData>(eqCompData.ItemTextureRef);

			var meshPath = itemMeshData.MeshPath;
			mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
			if (itemTextureData != null) {
				mat = MaterialUtils.CreateMaterialFromTextureData(itemTextureData, false);
			} else
			{
				mat = MaterialUtils.CreateMaterialFromMeshData(itemMeshData, false);
			}

			if (XMLValidation.ValidateMeshPath(
				new List<System.Tuple<string, string>>(),
				meshPath,
				itemData.fileName,
				itemData.Id,
				"meshPath"
				)
				)
			{
				tempClothingItem = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{meshPath}");

				if (tempClothingItem != null)
				{
					TorsoRendererMesh.sharedMesh = tempClothingItem.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
					TorsoRenderer2.sharedMaterial = mat;

					mat.Lerp(mat, mat, 1);
				}
			}
		}

		private void PrepareWeaponItem(ItemData itemData)
		{
			var weaponCompData = (ItemWeaponComponentData)itemData.GetComponentData<ItemWeaponComponentData>();
			var itemMeshData = DataBank.Instance.GetData<FNEEntityMeshData>(weaponCompData.ItemMeshRef);
			var itemTextureData = DataBank.Instance.GetData<FNEEntityTextureData>(weaponCompData.ItemTextureRef);

			var meshPath = itemMeshData.MeshPath;
			mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
			if (itemTextureData != null)
			{
				mat = MaterialUtils.CreateMaterialFromTextureData(itemTextureData, false);
			}
			else
			{
				mat = MaterialUtils.CreateMaterialFromMeshData(itemMeshData, false);
			}

			if (XMLValidation.ValidateMeshPath(
				new List<System.Tuple<string, string>>(),
				meshPath,
				itemData.fileName,
				itemData.Id,
				"meshPath"
				)
				)
			{
				tempWeaponItem = Importer.LoadFromFile($"{Application.streamingAssetsPath}/{meshPath}");

				if (tempWeaponItem != null)
				{
					WeaponRenderer.sharedMesh = tempWeaponItem.GetComponentInChildren<MeshFilter>().sharedMesh;
					WeaponRenderer.sharedMaterial = mat;

					WeaponRenderer.transform.localScale = Vector3.one * weaponCompData.iconScaleMod;
					WeaponRenderer.transform.localPosition = new Vector3(
						weaponCompData.iconOffsetRight,
						0,
						-weaponCompData.iconOffsetUp
					);

					mat.Lerp(mat, mat, 1);
				}
			}
		}

		private void PrepareClothingRenderTexture(ItemData itemData)
		{
			var eqCompData = (ItemClothingComponentData)itemData.GetComponentData<ItemClothingComponentData>();

			switch (eqCompData.Type)
			{
				case EquipmentType.Torso:
					ClothesRenderCam.transform.position = TorsoAnchor.transform.position;
					ClothesRenderCam.transform.rotation = TorsoAnchor.transform.rotation;
					break;

				case EquipmentType.Legs:
					ClothesRenderCam.transform.position = LegsAnchor.transform.position;
					ClothesRenderCam.transform.rotation = LegsAnchor.transform.rotation;
					break;

				case EquipmentType.Feet:
					ClothesRenderCam.transform.position = FeetAnchor.transform.position;
					ClothesRenderCam.transform.rotation = FeetAnchor.transform.rotation;
					break;
				
				case EquipmentType.Hands:
					ClothesRenderCam.transform.position = HandsAnchor.transform.position;
					ClothesRenderCam.transform.rotation = HandsAnchor.transform.rotation;
					break;
				
				case EquipmentType.Head:
					ClothesRenderCam.transform.position = HeadAnchor.transform.position;
					ClothesRenderCam.transform.rotation = HeadAnchor.transform.rotation;
					break;
			}

			currentClothingSpriteDataDef = itemData.Id;
		}

		private void PrepareWeaponRenderTexture(ItemData itemData)
		{
			var weaponCompData = (ItemWeaponComponentData)itemData.GetComponentData<ItemWeaponComponentData>();

			currentWeaponSpriteDataDef = itemData.Id;
		}

		private IEnumerator ShootClothingAndWeaponItem()
		{
			if (!m_ClothessDone && !GeneratedIcons.ContainsKey(currentClothingSpriteDataDef))
			{
				ClothingRT.Create();

				ClothesRenderCam.targetTexture = ClothingRT;
				RenderTexture.active = ClothingRT;

				ClothesRenderCam.Render();

				Texture2D tex = new Texture2D(ClothingRT.width, ClothingRT.height);
				tex.ReadPixels(new Rect(0, 0, ClothingRT.width, ClothingRT.height), 0, 0);

				var pixels = tex.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					if (pixels[i].g >= 0.99f && pixels[i].b >= 0.99f && pixels[i].r <= 0.01f)
					{
						pixels[i] = new Color(1, 0, 0, 0);
					}
				}
				tex.SetPixels(pixels);

				tex.Apply();

				var sprite = Sprite.Create(tex, new Rect(0, 0, ClothingRT.width, ClothingRT.height), new Vector2(0.5f, 0.5f));

				GeneratedIcons.Add(currentClothingSpriteDataDef, tex);

				ClothesRenderCam.targetTexture = null;
				RenderTexture.active = null;

				ClothingRT.Release();

			}

			if (!m_WeaponsDone && !GeneratedIcons.ContainsKey(currentWeaponSpriteDataDef))
			{
				WeaponRT.Create();

				WeaponRenderCam.targetTexture = WeaponRT;
				RenderTexture.active = WeaponRT;

				WeaponRenderCam.Render();

				Texture2D tex = new Texture2D(WeaponRT.width, WeaponRT.height);
				tex.ReadPixels(new Rect(0, 0, WeaponRT.width, WeaponRT.height), 0, 0);

				var pixels = tex.GetPixels();
				for (int i = 0; i < pixels.Length; i++)
				{
					if (pixels[i].g >= 0.98f && pixels[i].b >= 0.98f && pixels[i].r <= 0.02f)
					{
						pixels[i] = new Color(1, 0, 0, 0);
					}
				}
				tex.SetPixels(pixels);

				tex.Apply();

				var sprite = Sprite.Create(tex, new Rect(0, 0, WeaponRT.width, WeaponRT.height), new Vector2(0.5f, 0.5f));

				GeneratedIcons.Add(currentWeaponSpriteDataDef, tex);

				WeaponRenderCam.targetTexture = null;
				RenderTexture.active = null;

				WeaponRT.Release();

			}


			yield return new WaitForEndOfFrame();
		}

	}
}