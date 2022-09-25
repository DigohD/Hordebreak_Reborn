using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace FNZ.Client.Systems
{
	public struct RenderMeshEntitySpawnData
	{
		public int NetId;
		public bool IsSubView;

		public RenderMesh Mesh;
		public RenderBounds RenderBounds;
		public LocalToWorld Transform;
		public BuiltinMaterialPropertyUnity_MotionVectorsParams MotionVectors;
		public BuiltinMaterialPropertyUnity_RenderingLayer RenderingLayer;
		public BuiltinMaterialPropertyUnity_WorldTransformParams WorldTransform;
	}

	[UpdateInGroup(typeof(SimulationSystemGroup))]
	[UpdateAfter(typeof(ViewManagerSystem))]
	public class RenderMeshSpawnerSystem : SystemBase
	{
		private Queue<RenderMeshEntitySpawnData> m_SpawnQueue;

		private EntityArchetype m_RenderMeshArchetype;

		public void AddEntitySpawnData(ref RenderMeshEntitySpawnData spawnData)
		{
			m_SpawnQueue.Enqueue(spawnData);
		}

		protected override void OnCreate()
		{
			m_RenderMeshArchetype = EntityManager.CreateArchetype(new ComponentType[]
			{
				// Absolute minimum set of components required by Hybrid Renderer
				// to be considered for rendering. Entities without these components will
				// not match queries and will never be rendered.
				ComponentType.ReadWrite<WorldRenderBounds>(),
				ComponentType.ReadWrite<LocalToWorld>(),
				ComponentType.ReadWrite<RenderMesh>(),
				ComponentType.ChunkComponent<ChunkWorldRenderBounds>(),
				ComponentType.ChunkComponent<HybridChunkInfo>(),
				// Extra transform related components required to render correctly
				// using many default SRP shaders. Custom shaders could potentially
				// work without it.
				ComponentType.ReadWrite<WorldToLocal_Tag>(),
				// Components required by Hybrid Renderer visibility culling.
				ComponentType.ReadWrite<RenderBounds>(),
				ComponentType.ReadWrite<PerInstanceCullingTag>(),
				// Components for setting common built-in material properties required
				// by most SRP shaders that don't fall into the other categories.
	#if USE_HYBRID_SHARED_COMPONENT_OVERRIDES
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_RenderingLayer_Shared>(),
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_WorldTransformParams_Shared>(),
	#else
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_RenderingLayer>(),
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_WorldTransformParams>(),
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MotionVectorsParams>(),
	#endif
				// Components required by objects that use per-object light lists. Currently only
				// used by URP, and there is no automatic support in Hybrid Renderer.
				// Can be empty if disabled.
	#if USE_HYBRID_BUILTIN_LIGHTDATA
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_LightData>(),
	#endif
				// Components required by objects that need to be rendered in per-object motion passes.
	#if USE_HYBRID_MOTION_PASS
				ComponentType.ReadWrite<BuiltinMaterialPropertyUnity_MatrixPreviousM>(),
	#endif
			});

			m_SpawnQueue = new Queue<RenderMeshEntitySpawnData>();
		}

		protected override void OnUpdate()
		{
			UpdateViews();
		}

		private void UpdateViews()
		{
			if (m_SpawnQueue.Count <= 0) return;
			
			var entities = new NativeArray<Entity>(m_SpawnQueue.Count, Allocator.TempJob);
			var spawnTransformDataArray = new NativeArray<LocalToWorld>(m_SpawnQueue.Count, Allocator.TempJob);
			var spawnRenderBoundsDataArray = new NativeArray<RenderBounds>(m_SpawnQueue.Count, Allocator.TempJob);
			var spawnMotionVectorsDataArray = new NativeArray<BuiltinMaterialPropertyUnity_MotionVectorsParams>(m_SpawnQueue.Count, Allocator.TempJob);
			var spawnRenderLayerDataArray = new NativeArray<BuiltinMaterialPropertyUnity_RenderingLayer>(m_SpawnQueue.Count, Allocator.TempJob);
			var spawnWorldTransformDataArray = new NativeArray<BuiltinMaterialPropertyUnity_WorldTransformParams>(m_SpawnQueue.Count, Allocator.TempJob);

			EntityManager.CreateEntity(m_RenderMeshArchetype, entities);

			var ecb = new EntityCommandBuffer(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				var spawnData = m_SpawnQueue.Dequeue();

				spawnTransformDataArray[i] = spawnData.Transform;
				spawnRenderBoundsDataArray[i] = spawnData.RenderBounds;
				spawnMotionVectorsDataArray[i] = spawnData.MotionVectors;
				spawnRenderLayerDataArray[i] = spawnData.RenderingLayer;
				spawnWorldTransformDataArray[i] = spawnData.WorldTransform;

				var entity = entities[i];
				var worldView = GameClient.WorldView;
				var chunkView = worldView.GetChunkView(new float2(spawnData.Transform.Position.x, spawnData.Transform.Position.z));

				if (spawnData.IsSubView)
				{
					chunkView.AddSubViewEntity(entity);
				}
				else
				{
					chunkView.AddEntity(entity);
				}

				if (spawnData.NetId != -1)
				{
					if (spawnData.IsSubView)
					{
						GameClient.ViewConnector.AddSubViewEntity(entity, spawnData.NetId);
					}
					else
					{
						GameClient.ViewConnector.AddEntity(entity, spawnData.NetId);
					}
				}
				
				ecb.SetSharedComponent(entity, spawnData.Mesh);
			}

			var updateTransformJob = new UpdateTransformsJob
			{
				SpawnedEntities = entities,

				SpawnDataTransforms = spawnTransformDataArray,
				AllTransforms = GetComponentDataFromEntity<LocalToWorld>(),

				SpawnDataRenderBounds = spawnRenderBoundsDataArray,
				AllRenderBounds = GetComponentDataFromEntity<RenderBounds>(),

				SpawnDataMotionVectors = spawnMotionVectorsDataArray,
				AllMotionVectors = GetComponentDataFromEntity<BuiltinMaterialPropertyUnity_MotionVectorsParams>(),

				SpawnDataRenderingLayer = spawnRenderLayerDataArray,
				AllRenderingLayers = GetComponentDataFromEntity<BuiltinMaterialPropertyUnity_RenderingLayer>(),

				SpawnDataWorldTransforms = spawnWorldTransformDataArray,
				AllWorldTransforms = GetComponentDataFromEntity<BuiltinMaterialPropertyUnity_WorldTransformParams>(),
			};

			Dependency = updateTransformJob.Schedule(entities.Length, 128, Dependency);

			ecb.Playback(EntityManager);
			ecb.Dispose();
		}
		
		[BurstCompile]
		private struct UpdateTransformsJob : IJobParallelFor
		{
			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<Entity> SpawnedEntities;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<LocalToWorld> SpawnDataTransforms;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<LocalToWorld> AllTransforms;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<RenderBounds> SpawnDataRenderBounds;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<RenderBounds> AllRenderBounds;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<BuiltinMaterialPropertyUnity_MotionVectorsParams> SpawnDataMotionVectors;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<BuiltinMaterialPropertyUnity_MotionVectorsParams> AllMotionVectors;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<BuiltinMaterialPropertyUnity_RenderingLayer> SpawnDataRenderingLayer;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<BuiltinMaterialPropertyUnity_RenderingLayer> AllRenderingLayers;

			[ReadOnly, DeallocateOnJobCompletion]
			public NativeArray<BuiltinMaterialPropertyUnity_WorldTransformParams> SpawnDataWorldTransforms;

			[WriteOnly, NativeDisableParallelForRestriction]
			public ComponentDataFromEntity<BuiltinMaterialPropertyUnity_WorldTransformParams> AllWorldTransforms;

			public void Execute(int index)
			{
				AllTransforms[SpawnedEntities[index]] = SpawnDataTransforms[index];
				AllRenderBounds[SpawnedEntities[index]] = SpawnDataRenderBounds[index];
				AllMotionVectors[SpawnedEntities[index]] = SpawnDataMotionVectors[index];
				AllRenderingLayers[SpawnedEntities[index]] = SpawnDataRenderingLayer[index];
				AllWorldTransforms[SpawnedEntities[index]] = SpawnDataWorldTransforms[index];
			}
		}
	}
}