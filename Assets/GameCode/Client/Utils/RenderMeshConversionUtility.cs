using FNZ.Client.View.Prefab;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity.EntityViewData;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace FNZ.Client.Utils
{
	public static class RenderMeshConversionUtility
	{
		private static readonly Dictionary<string, Entity> s_RenderMeshEntityPrototypes = new Dictionary<string, Entity>();
		
		public static Entity GetRenderMeshEntityPrototype(string viewId, bool castShadows)
		{
			if (s_RenderMeshEntityPrototypes.ContainsKey(viewId))
				return s_RenderMeshEntityPrototypes[viewId];

			var entityManager = GameClient.ECS_ClientWorld.EntityManager;
			var entityViewData = DataBank.Instance.GetData<FNEEntityViewData>(viewId);
			var prefab = PrefabBank.GetPrefab(null, entityViewData.Id);
			var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
			var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
			var material = meshRenderer.sharedMaterial;

			var desc = new RenderMeshDescription(
				meshFilter.sharedMesh,
				material,
				shadowCastingMode: castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
				receiveShadows: false
			);

			var prototype = entityManager.CreateEntity();

			RenderMeshUtility.AddComponents(
			   prototype,
			   entityManager,
			   desc
			);

			entityManager.AddComponentData(prototype, new LocalToWorld());

			s_RenderMeshEntityPrototypes.Add(viewId, prototype);

			return prototype;
		}
	}
}