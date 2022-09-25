using System;
using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.VFX;
using System.Collections.Generic;
using FNZ.Client.View.UI.Manager;
using FNZ.Shared.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

namespace FNZ.Client.View.Manager
{
	public enum PrefabType
	{
		FNEENTIY = 0,
		VFX = 1
	}

	public static class GameObjectPoolManager
	{
		private static Dictionary<string, Stack<GameObject>> s_Pool = new Dictionary<string, Stack<GameObject>>();

		public static void DoRecycle(string id, GameObject go)
		{
			if (!s_Pool.ContainsKey(id))
				s_Pool.Add(id, new Stack<GameObject>());

			if (go == null)
			{
				Debug.LogError("PUSHED NULL TO POOL: " + id);
				//UIManager.ErrorHandler.NewErrorMessage("PUSHED NULL TO POOL: " + id, "Not Okay");
			}

			go.SetActive(false);

			s_Pool[id].Push(go);
		}

		public static void RecycleObject(string id, GameObject go)
		{
			var data = new GameObjectDeactivationData
			{
				id = id,
				go = go
			};

			if (go == null)
			{
				Debug.LogError("DEACTIVATED NULL IN POOL: " + id);
				//UIManager.ErrorHandler.NewErrorMessage("DEACTIVATED NULL IN POOL: " + id, "Not Okay");
			}
			
			GameClient.Spawner.QueueForDeactivation(data);
		}

		public static bool HasObjectInstance(string id)
		{
			return (s_Pool.ContainsKey(id) && s_Pool[id].Count > 0);
		}

		public static GameObject GetObjectInstance(string viewRef, PrefabType prefabType, float2 position)
		{
			if (string.IsNullOrEmpty(viewRef))
			{
				throw new Exception("GetObjectInstance: Viewref is null!");
			}
			
			if (s_Pool.ContainsKey(viewRef) && s_Pool[viewRef].Count > 0)
			{
				var go = s_Pool[viewRef].Pop();
				
				if (go == null)
				{
					//UIManager.ErrorHandler.NewErrorMessage("POPPED NULL FROM POOL: " + viewRef, "Not Okay");
					Debug.LogError("POPPED NULL FROM POOL: " + viewRef);
				}
				
				go.SetActive(true);

				switch (prefabType)
				{
					case PrefabType.VFX:
						go.transform.position = new Vector3(position.x, 0, position.y);
						VFXInit(go, viewRef);
						return go;

					default:
						return go;
				}
			}

			switch (prefabType)
			{
				case PrefabType.FNEENTIY:
					return PrefabBank.GetInstanceFromViewRef(viewRef);

				case PrefabType.VFX:
					var go = PrefabBank.GetInstanceOfVFXPrefab(viewRef);
					go.transform.position = new Vector3(position.x, 0, position.y);

					var data = DataBank.Instance.GetData<VFXData>(viewRef);
					if (data.lightData != null)
					{
						var lightObject = new GameObject();
						var lightComp = lightObject.AddComponent<HDAdditionalLightData>();
						lightObject.transform.SetParent(go.transform);
						lightObject.name = "LightSource";

						lightObject.transform.SetParent(go.transform);
						lightObject.transform.localPosition = Vector3.zero;

						lightComp.range = data.lightData.range;
						lightComp.SetIntensity(data.lightData.intensity);

						Color color = lightComp.color;
						ColorUtility.TryParseHtmlString(data.lightData.startColor, out color);
						lightComp.SetColor(color);
					}

					VFXInit(go, viewRef);
					return go;
			}

			return null;
		}

		private static void VFXInit(GameObject instance, string vfxId)
		{
			var data = DataBank.Instance.GetData<VFXData>(vfxId);

			instance.name = vfxId;

            var timerScript = instance.GetComponent<EffectTimer>();

            if (!timerScript)
                timerScript = instance.AddComponent<EffectTimer>();

            timerScript.Init(data.lifetime, data.lightData);

			foreach (var vfx in instance.GetComponentsInChildren<VisualEffect>())
				vfx.Play();

			foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
			{
				ps.Stop();
				ps.Play();
			}

			foreach (var trail in instance.GetComponentsInChildren<TrailRenderer>())
				trail.Clear();
		}

	}
}