using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace FNZ.Client.GPUSkinning 
{
	public static class GPUAnimationMeshConversionUtility
	{
		public static void Convert(
			Entity entity,
			EntityManager dstEntityManager,
			Renderer meshRenderer,
			Mesh mesh,
			List<Material> materials)
		{
			var materialCount = materials.Count;

			if ((mesh != null) && (materialCount > 0))
			{
				var motionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
				var renderMesh = new RenderMesh
				{
					mesh = mesh,
					castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
					receiveShadows = false,
					layer = meshRenderer.gameObject.layer,
					needMotionVectorPass =
						(motionVectorGenerationMode == MotionVectorGenerationMode.Object) ||
						(motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion)
				};

				float4x4 localToWorld = meshRenderer.transform.localToWorldMatrix;
				var flipWinding = math.determinant(localToWorld) < 0.0;

				AddComponentsToEntity(
					renderMesh,
					entity,
					dstEntityManager,
					meshRenderer,
					mesh,
					materials,
					flipWinding,
					0
				);
			}

			static void AddComponentsToEntity(
				RenderMesh renderMesh,
				Entity entity,
				EntityManager dstEntityManager,
				Renderer meshRenderer,
				Mesh mesh,
				List<Material> materials,
				bool flipWinding,
				int id)
			{
				renderMesh.material = materials[id];
				renderMesh.subMesh = id;

				dstEntityManager.AddSharedComponentData(entity, renderMesh);

				dstEntityManager.AddComponentData(entity, new PerInstanceCullingTag());
				dstEntityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

				if (flipWinding)
					dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<RenderMeshFlippedWindingTag>());

				dstEntityManager.AddComponent(entity, ComponentType.ReadOnly<WorldToLocal_Tag>());

				if (renderMesh.needMotionVectorPass)
				{
					dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MatrixPreviousM>());
					dstEntityManager.AddComponent(entity, ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MatrixPreviousMI_Tag>());
				}

				dstEntityManager.AddComponentData(entity, CreateMotionVectorsParams(ref renderMesh, ref meshRenderer));

				dstEntityManager.AddComponentData(entity, new BuiltinMaterialPropertyUnity_RenderingLayer
				{
					Value = new uint4(meshRenderer.renderingLayerMask, 0, 0, 0)
				});

				dstEntityManager.AddComponentData(entity, new BuiltinMaterialPropertyUnity_WorldTransformParams
				{
					Value = flipWinding ? new float4(0, 0, 0, -1) : new float4(0, 0, 0, 1)
				});
			}

			static BuiltinMaterialPropertyUnity_MotionVectorsParams CreateMotionVectorsParams(ref RenderMesh mesh, ref Renderer meshRenderer)
			{
				var s_bias = -0.001f;
				var hasLastPositionStream = mesh.needMotionVectorPass ? 1.0f : 0.0f;
				var motionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
				var forceNoMotion = (motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion) ? 0.0f : 1.0f;
				var cameraVelocity = (motionVectorGenerationMode == MotionVectorGenerationMode.Camera) ? 0.0f : 1.0f;
				return new BuiltinMaterialPropertyUnity_MotionVectorsParams { Value = new float4(hasLastPositionStream, forceNoMotion, s_bias, cameraVelocity) };
			}
		}
	}
}