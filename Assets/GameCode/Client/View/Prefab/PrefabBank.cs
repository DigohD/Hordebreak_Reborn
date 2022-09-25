using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Entity.MountedObject;
using FNZ.Shared.Model.VFX;
using FNZ.Shared.Utils;
using Siccity.GLTFUtility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering.HighDefinition;

namespace FNZ.Client.View.Prefab
{
	public static class PrefabBank
	{
		private static Dictionary<string, GameObject> PrefabStorage = new Dictionary<string, GameObject>();
		
		public static GameObject GetPrefab(string path, string viewId = null)
		{
			FNEEntityViewData viewData = null;
			FNEEntityMeshData meshData = null;

			if (viewId != null)
			{
				viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewId);
				meshData = DataBank.Instance.GetData<FNEEntityMeshData>(viewData.entityMeshData);
				path = $"{Application.streamingAssetsPath}/{meshData.MeshPath}";
			}

			if (viewId == null && PrefabStorage.ContainsKey(path))
			{
				return PrefabStorage[path];
			}
			else if (viewId != null && PrefabStorage.ContainsKey(viewId))
			{
				return PrefabStorage[viewId];
			}

			GameObject go;

			if (viewData != null)
			{
				Profiler.BeginSample("LoadFromFile");
				go = Importer.LoadFromFile(path);
				Profiler.EndSample();

				
				Material mat;
				if (string.IsNullOrEmpty(viewData.entityTextureData))
				{
					Debug.Log($"ViewId not converted to (ass)etbundle {viewData.Id}");
					Profiler.BeginSample("CreateMaterialFromMeshData");
					mat = MaterialUtils.CreateMaterialFromMeshData(meshData, viewData.isTransparent);
					Profiler.EndSample();
				}else
				{
					Profiler.BeginSample("CreateMaterialFromTextureData");
					var textureData = DataBank.Instance.GetData<FNEEntityTextureData>(viewData.entityTextureData);
					mat = MaterialUtils.CreateMaterialFromTextureData(textureData, viewData.isTransparent, viewData.isVegetation);
					Profiler.EndSample();
				}

				MeshRenderer mr = go.GetComponentInChildren<MeshRenderer>();
				if (mr == null)
				{
					var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
					smr.sharedMaterial = mat;
				}
				else
				{
					mr.sharedMaterial = mat;
				}

				//Add light source if the prefab should have one.
				if (viewData.entityLightSourceData != null && viewData.viewIsGameObject)
				{
					if (!go.GetComponentInChildren<HDAdditionalLightData>())
					{
						var lightObject = new GameObject();
						lightObject.AddComponent<HDAdditionalLightData>();
						lightObject.transform.SetParent(go.transform);
						lightObject.name = "LightSource";
					}

					SetLightValues(go, viewData, viewData.scaleMod);
				}

				//Add VFX if the prefab should have one
				if (viewData.entityVfxData != null)
				{
					var vfxData = DataBank.Instance.GetData<VFXData>(viewData.entityVfxData.vfxRef);
					GameObject vfxAsset;

					#region IsVfxFromAssetBundle?
					if (vfxData.assetBundleName != null)
					{
						if (vfxData.assetBundlePath != null)
						{
							if (vfxData.effectName != null)
							{
								vfxAsset = AssetBundleLoader.LoadAssetFromAssetBundle<GameObject>(vfxData.assetBundlePath, vfxData.effectName);
								AddVfxToGO(vfxAsset, go, viewData);
							}
						}
					}
					#endregion
					else
					{
						vfxAsset = Resources.Load<GameObject>(vfxData.prefabPath);
						AddVfxToGO(vfxAsset, go, viewData);
					}
				}

				go.SetActive(false);
			}
			else
			{
				go = Resources.Load<GameObject>(path);
			}

			if (go == null)
			{
				return null;
			}

			if (viewId == null)
			{
				PrefabStorage.Add(path, go);
			}
			else
			{
				PrefabStorage.Add(viewId, go);
			}

			return go;
		}

		public static bool DoesPrefabExist(string path)
		{
			GameObject go = Resources.Load<GameObject>(path);

			if (go == null)
			{
				return false;
			}

			return true;
		}

		public static GameObject GetInstanceFromViewRef(string viewRef)
		{
			GameObject prefab = GetPrefab(null, viewRef);
			GameObject instance = Object.Instantiate(prefab);
			instance.SetActive(true);

			return instance;
		}

		public static GameObject GetInstanceOfMountedObjectPrefab(string id)
		{
			var objectData = DataBank.Instance.GetData<MountedObjectData>(id);
			var instance = GetInstanceFromViewRef(objectData.viewVariations[0]);

			instance.AddComponent<GameObjectIdHolder>().id = id;

			return instance;
		}

		public static GameObject GetInstanceOfFNEEntityPrefab(string id)
		{
			var entityData = DataBank.Instance.GetData<FNEEntityData>(id);
			var instance = GetInstanceFromViewRef(entityData.entityViewVariations[0]);

			instance.AddComponent<GameObjectIdHolder>().id = id;

			return instance;
		}

		public static GameObject GetInstanceOfVFXPrefab(string vfxId)
		{
			GameObject prefab;
			GameObject instance;
			var vfxData = DataBank.Instance.GetData<VFXData>(vfxId);

			var path = vfxData.assetBundlePath;
			if (path == null)
			{
				path = vfxData.prefabPath;
				prefab = GetPrefab(path);
			}
			else
			{
				prefab = AssetBundleLoader.LoadAssetFromAssetBundle<GameObject>(vfxData.assetBundlePath, vfxData.effectName);
			}

			instance = Object.Instantiate(prefab);
			instance.AddComponent<GameObjectIdHolder>().id = vfxId;

			return instance;
		}

		private static void SetLightValues(GameObject go, FNEEntityViewData data, float scaleMod)
		{
			if (go == null || data == null)
				return;

			var light = go.GetComponentInChildren<Light>();
			var lightComp = go.GetComponentInChildren<HDAdditionalLightData>();

			if (data.entityLightSourceData.lightType == EntityLightSourceData.LightType.Spot)
			{
				lightComp.type = HDLightType.Spot;
				lightComp.transform.localRotation = Quaternion.Euler(data.entityLightSourceData.rotationX, data.entityLightSourceData.rotationY, data.entityLightSourceData.rotationZ);
				lightComp.SetSpotAngle(data.entityLightSourceData.spotOuterAngle, data.entityLightSourceData.spotInnerAnglePercent);
				lightComp.shapeRadius = 0f;
			}
			light.bounceIntensity = 0;
			lightComp.transform.localPosition = new Vector3(data.entityLightSourceData.offsetX, data.entityLightSourceData.offsetY, data.entityLightSourceData.offsetZ) / scaleMod;
			lightComp.range = data.entityLightSourceData.range;
			lightComp.SetIntensity(data.entityLightSourceData.intensity);

			lightComp.EnableShadows(true);
			lightComp.SetShadowResolution(256);
			
			var layerMask = 1 << LayerMask.NameToLayer("Player");
			lightComp.SetCullingMask(layerMask);
			
			Color color = lightComp.color;
			ColorUtility.TryParseHtmlString(data.entityLightSourceData.color, out color);
			lightComp.SetColor(color);
		}

		private static void AddVfxToGO(GameObject vfxAsset, GameObject go, FNEEntityViewData viewData)
		{
			//viewData.entityVfxData.alwaysOn

			vfxAsset = Object.Instantiate(vfxAsset);
			vfxAsset.name = "VFXSource";
			vfxAsset.transform.SetParent(go.transform);

			vfxAsset.transform.localScale = Vector3.one * (viewData.entityVfxData.scaleMod / viewData.scaleMod);

			vfxAsset.transform.localPosition = new Vector3(
				viewData.entityVfxData.offsetX,
				viewData.entityVfxData.offsetY,
				viewData.entityVfxData.offsetZ) / viewData.scaleMod;
		}

	}
}
