using FNZ.Client.Model.Entity.Components.Door;
using FNZ.Client.Model.Entity.Components.Inventory;
using FNZ.Client.Model.World;
using FNZ.Client.Systems;
using FNZ.Client.View.Audio;
using FNZ.Client.View.Camera;
using FNZ.Client.View.EntityView;
using FNZ.Client.View.Prefab;
using FNZ.Client.View.WorldPrompts;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Effect;
using FNZ.Shared.Model.Effect.RealEffect;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.EntityViewData;
using FNZ.Shared.Model.Items.Components;
using FNZ.Shared.Model.VFX;
using System.Collections.Generic;
using FNZ.Client.Model.Entity.Components.EdgeObject;
using FNZ.Shared.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using FNZ.Client.View.UI.Manager;

namespace FNZ.Client.View.Manager
{
	public class ViewAPI
	{
		private RenderMeshSpawnerSystem m_RenderMeshSpawnerSystem;

        // These are prefabs not loaded from XML
        private GameObject P_WorldPrompt;
        private GameObject P_BaseFail;

		public ViewAPI()
		{
			m_RenderMeshSpawnerSystem = GameClient.ECS_ClientWorld.GetOrCreateSystem<RenderMeshSpawnerSystem>();

            P_WorldPrompt = Resources.Load<GameObject>("Prefab/WorldPrompt/WorldPrompt");
        }

		public void QueueViewForSpawn(FNEEntity entityModel, string viewDataRef)
		{
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>(viewDataRef);
			if (viewData == null)
			{
				Debug.LogError($"viewRef: {viewDataRef} returned null");
				return;
			}

			if (viewData.viewIsGameObject)
			{
				QueueGameObjectViewForSpawn(entityModel, viewData);
				
				if (entityModel.EntityType == EntityType.EDGE_OBJECT)
				{
					entityModel.GetComponent<EdgeObjectComponentClient>().UpdateMountedView();
				}
			}
			else
			{
				float x;
				float y = -viewData.heightPos;
				float z;

				if (entityModel.EntityType == EntityType.TILE_OBJECT)
				{
					x = entityModel.Position.x + 0.5f;
					z = entityModel.Position.y + 0.5f;
				}
				else
				{
					x = entityModel.Position.x;
					z = entityModel.Position.y;
				}

				float3 position = new float3(x, y, z);

				Quaternion quat;

				if (entityModel.EntityType == EntityType.TILE_OBJECT)
				{
					quat = Quaternion.Euler(0, -entityModel.RotationDegrees, 0);
				}
				else
				{
					quat = Quaternion.AngleAxis(entityModel.RotationDegrees, Vector3.down);

					if (entityModel.EntityType == EntityType.EDGE_OBJECT)
					{
						entityModel.GetComponent<EdgeObjectComponentClient>().UpdateMountedView();
					}
				}

				QueueEntityViewForSpawn(viewData, position, quat, entityModel.Scale, entityModel.NetId);
			}
		}

        private void QueueGameObjectViewForSpawn(FNEEntity entity, FNEEntityViewData viewData)
		{
			var position = new Vector3(entity.Position.x, -viewData.heightPos, entity.Position.y);
			string type = entity.EntityType;
			string entityId = entity.EntityId;
			int netId = entity.NetId;
			Quaternion rotation = Quaternion.AngleAxis(entity.RotationDegrees, Vector3.down);
			Vector3 scale = entity.Scale * viewData.scaleMod;

			if (GameObjectPoolManager.HasObjectInstance(entity.EntityId))
			{
				GameObject instance = GameObjectPoolManager.GetObjectInstance(
					viewData.Id,
					PrefabType.FNEENTIY,
					new float2(position.x, position.y)
				);
;
				if (type == EntityType.TILE_OBJECT)
				{
					instance.transform.position = position + new Vector3(0.5f, 0.0f, 0.5f);
					instance.transform.rotation = rotation;
				}
				else
				{
					instance.transform.position = position;
					instance.transform.rotation = rotation;
				}

				// Unity normally convets blender scale (centimeters) to unity scale. (Multiply by 0.01)
				instance.transform.localScale = scale;
				GameClient.Spawner.QueueForActivation(new GameObjectActivationData
				{
					NetId = netId,
					View = instance
				});

                ResetGameObjectViewSpecifics(instance, netId);
            }
			else
			{
				var spawnData = new GameObjectSpawnData
				{
					type = type,
					id = viewData.Id,
					netID = netId,
					position = position,
					rotation = rotation,
					scale = scale
				};

				GameClient.Spawner.QueueForSpawning(spawnData);
			}
		}

		public void QueueEntityViewForSpawn(FNEEntityViewData viewData, float3 position, quaternion rotation, float3 scale, int netId = -1)
		{
			var prefab = PrefabBank.GetPrefab(null, viewData.Id);
			var entitySpawnData = ConvertGameObjectToEntitySpawnData(prefab, position, rotation, scale * viewData.scaleMod, netId);

			entitySpawnData.IsSubView = false;
			m_RenderMeshSpawnerSystem.AddEntitySpawnData(ref entitySpawnData);
		}

		public RenderMeshEntitySpawnData ConvertGameObjectToEntitySpawnData(GameObject prefab, float3 position, quaternion rotation, float3 scale, int netId = -1)
		{
			var entitySpawnData = new RenderMeshEntitySpawnData();

			var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
			var meshRenderer = (Renderer)prefab.GetComponentInChildren<MeshRenderer>();

			var skinnedRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

			Mesh meshToUse = null;
			Material materialToUse = null;
			Renderer rendererToUse = null;
			if (meshFilter != null)
			{
				meshToUse = meshFilter.sharedMesh;
				materialToUse = meshRenderer.sharedMaterial;
				rendererToUse = (Renderer)prefab.GetComponentInChildren<MeshRenderer>();
			}
			else if (skinnedRenderer != null)
			{
				meshToUse = skinnedRenderer.sharedMesh;
				materialToUse = skinnedRenderer.sharedMaterial;
				rendererToUse = (Renderer)prefab.GetComponentInChildren<SkinnedMeshRenderer>();
			}
			else
			{
				Debug.Log("Mesh did not exist, time to panic!");
			}

			var renderMesh = new RenderMesh
			{
				mesh = meshToUse,
				material = materialToUse,
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				layer = prefab.layer
			};

			var localToWorld = new LocalToWorld
			{
				Value = float4x4.TRS(
					position,
					rotation,
					scale
				)
			};

			renderMesh.needMotionVectorPass = (rendererToUse.motionVectorGenerationMode == MotionVectorGenerationMode.Object) ||
												  (rendererToUse.motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion);

			entitySpawnData.Mesh = renderMesh;
			entitySpawnData.Transform = localToWorld;
			entitySpawnData.MotionVectors = CreateMotionVectorsParams(ref renderMesh, ref rendererToUse);
			entitySpawnData.RenderBounds = new RenderBounds { Value = renderMesh.mesh.bounds.ToAABB() };

			entitySpawnData.RenderingLayer = new BuiltinMaterialPropertyUnity_RenderingLayer
			{
				Value = new uint4(rendererToUse.renderingLayerMask, 0, 0, 0)
			};

			entitySpawnData.WorldTransform = new BuiltinMaterialPropertyUnity_WorldTransformParams
			{
				Value = new float4(0, 0, 0, 1)
			};

			entitySpawnData.NetId = netId;

			return entitySpawnData;
		}

		private BuiltinMaterialPropertyUnity_MotionVectorsParams CreateMotionVectorsParams(ref RenderMesh mesh, ref Renderer meshRenderer)
		{
			float s_bias = -0.001f;
			float hasLastPositionStream = mesh.needMotionVectorPass ? 1.0f : 0.0f;
			var motionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
			float forceNoMotion = (motionVectorGenerationMode == MotionVectorGenerationMode.ForceNoMotion) ? 0.0f : 1.0f;
			float cameraVelocity = (motionVectorGenerationMode == MotionVectorGenerationMode.Camera) ? 0.0f : 1.0f;
			return new BuiltinMaterialPropertyUnity_MotionVectorsParams { Value = new float4(hasLastPositionStream, forceNoMotion, s_bias, cameraVelocity) };
		}

		public void ActivateGameObject(GameObjectActivationData activationData)
		{
			var instance = activationData.View;
			activationData.View.SetActive(true);
			GameClient.ViewConnector.AddGameObject(activationData.View, activationData.NetId);
			GameClient.WorldView.GetChunkView(new float2(instance.transform.position.x, instance.transform.position.z)).AddGameObject(instance);
			var idHolder = instance.GetComponent<GameObjectIdHolder>();

			var viewScript = instance.GetComponent<EntityViewScript>();
			viewScript.Init(GameClient.NetConnector.GetEntity(activationData.NetId));

			if (idHolder == null)
			{
				idHolder = instance.AddComponent<GameObjectIdHolder>();
			}

			foreach (Transform child in instance.transform)
			{
				if (child.tag.Equals("Editor"))
				{
                    UnityEngine.Object.Destroy(child.gameObject);
				}
			}

			idHolder.id = GameClient.NetConnector.GetEntity(activationData.NetId).EntityId;

            ResetGameObjectViewSpecifics(activationData.View, activationData.NetId);
        }

		public void SpawnGameObject(GameObjectSpawnData spawnData)
		{
			Profiler.BeginSample("GetObjectInstance");
			GameObject instance = GameObjectPoolManager.GetObjectInstance(spawnData.id, PrefabType.FNEENTIY, new float2(spawnData.position.x, spawnData.position.y));
			Profiler.EndSample();
			var entity = GameClient.NetConnector.GetEntity(spawnData.netID);

			var viewScript = instance.GetComponent<EntityViewScript>();
			if(viewScript == null)
				viewScript = instance.AddComponent<EntityViewScript>();
			viewScript.Init(entity);

			if (spawnData.type == EntityType.TILE_OBJECT)
			{
				instance.transform.position = spawnData.position + new Vector3(0.5f, 0.0f, 0.5f);
				instance.transform.rotation = spawnData.rotation;
				instance.transform.localScale = spawnData.scale;
			}
			else
			{
				if (spawnData.type == EntityType.EDGE_OBJECT)
				{
					var edgeObjComp = entity.GetComponent<EdgeObjectComponentClient>();

					if (edgeObjComp.PreviousMountedObject != edgeObjComp.MountedObjectData)
					{
						if (edgeObjComp.MountedObjectData == null)
						{
							// Object.Destroy(edgeObjComp.MountedGameObjectView);
						}
						else
						{
							//if (edgeObjComp.MountedGameObjectView != null)
								//Object.Destroy(edgeObjComp.MountedGameObjectView);
						
							var viewData = DataBank.Instance.GetData<FNEEntityViewData>(edgeObjComp.MountedObjectData.viewVariations[0]);

							edgeObjComp.MountedGameObjectView = instance;
						
							edgeObjComp.MountedGameObjectView.transform.position = new Vector3(entity.Position.x, viewData.heightPos, entity.Position.y);
							edgeObjComp.MountedGameObjectView.transform.localScale = Vector3.one * viewData.scaleMod;

							// Vertical wall
							if (entity.Position.x % 1 == 0)
							{
								if (!edgeObjComp.OppositeMountedDirection)
									edgeObjComp.MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 90, 0);
								else
									edgeObjComp.MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 270, 0);
							}
							// Horizontal wall
							else
							{
								if (!edgeObjComp.OppositeMountedDirection)
									edgeObjComp.MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 0, 0);
								else
									edgeObjComp.MountedGameObjectView.transform.rotation = Quaternion.Euler(0, 180, 0);
							}
						
						}
					}
					
					instance.transform.position = spawnData.position;
					instance.transform.rotation = spawnData.rotation;
					instance.transform.localScale = spawnData.scale;
				}
			}

			GameClient.ViewConnector.AddGameObject(instance, spawnData.netID);
			GameClient.WorldView.GetChunkView(new Vector2(spawnData.position.x, spawnData.position.z)).AddGameObject(instance);

			var idHolder = instance.GetComponent<GameObjectIdHolder>();

			if (idHolder == null)
			{
				idHolder = instance.AddComponent<GameObjectIdHolder>();
			}

			Profiler.BeginSample("Destroy Children");
			foreach (Transform child in instance.transform)
			{
				if (child.tag.Equals("Editor"))
				{
                    Object.Destroy(child.gameObject);
				}
			}
			Profiler.EndSample();

			idHolder.id = spawnData.id;
			
			Profiler.BeginSample("ResetGameObjectViewSpecifics");
            ResetGameObjectViewSpecifics(instance, spawnData.netID);
            Profiler.EndSample();
		}

		public GameObject SpawnItemOnGroundView(float2 position)
        {
			var viewData = DataBank.Instance.GetData<FNEEntityViewData>("ph_item_on_ground_view");
			var itemOnGroundView = PrefabBank.GetInstanceFromViewRef(viewData.Id);
			itemOnGroundView.transform.position = new Vector3(position.x, 0, position.y);
			itemOnGroundView.transform.localScale = Vector3.one * viewData.scaleMod;

			return itemOnGroundView;
		}

		public GameObject SpawnPlayerView()
		{
			return (GameObject)GameObject.Instantiate(Resources.Load("Prefab/Entity/Player/Player"));
		}

		public GameObject SpawnEnemyView(string prefabPath, int netId)
		{
			var obj = (GameObject)GameObject.Instantiate(Resources.Load(prefabPath));

			if (!obj.GetComponent<GameObjectIdHolder>())
				obj.AddComponent<GameObjectIdHolder>().id = GameClient.NetConnector.GetEntity(netId).EntityId;

			return obj;
		}

		public void QueueGameObjectsForDeactivation(List<GameObject> gameObjectViews)
		{
			foreach (var go in gameObjectViews)
			{
				QueueGameObjectForDeactivation(go);
			}
		}

		public void QueueGameObjectForDeactivation(GameObject view)
		{
			if (view == null)
			{
				UIManager.ErrorHandler.NewErrorMessage("Du kan va en string", "Du, view e null, Not Okay");
			}
			var data = new GameObjectDeactivationData
			{
				id = view.GetComponent<GameObjectIdHolder>().id,
				go = view
			};

			GameClient.Spawner.QueueForDeactivation(data);
		}

		public void DestroyViewEntities(List<Entity> entityViews)
		{
			var entitiesToDestroy = new NativeArray<Entity>(entityViews.Count, Allocator.Temp, NativeArrayOptions.ClearMemory);
			entitiesToDestroy.CopyFrom(entityViews.ToArray());
			GameClient.ECS_ClientWorld.EntityManager.DestroyEntity(entitiesToDestroy);
			entitiesToDestroy.Dispose();
		}
		
		public void DestroySubViewEntities(List<Entity> entityViews)
		{
			var entitiesToDestroy = new NativeArray<Entity>(entityViews.Count, Allocator.Temp, NativeArrayOptions.ClearMemory);
			entitiesToDestroy.CopyFrom(entityViews.ToArray());
			GameClient.ECS_ClientWorld.EntityManager.DestroyEntity(entitiesToDestroy);
			entitiesToDestroy.Dispose();
		}

        /*
         * Spawn effect is responsible for spawning all effects in the game on the client.
         * 
         * isLocal determins whether an effect was triggered by the client or by the server
         * 
         * spawnRealEffects is false when the real effect of an effect is expected to be triggered by the 
         * server. An example is death effects, where only the VFX of a dying projectile is spawned 
         * client-side, and additional effects are triggered by the server. For example, an explosion.
         * 
         * ownerNetId is only relevant for projectiles fired by other entities, which should not collide with themselves
         */

        public void SpawnEffect(
			string effectId, 
			Vector2 position, 
			float rotationDegrees, 
			bool isLocal, 
			bool spawnRealEffects,
			string[] modItemIds,
			ItemWeaponModComponentData[] mods,
			float effectHeight = 0, 
			int ownerNetId = 0
		)
		{
			var effectData = DataBank.Instance.GetData<EffectData>(effectId);

			string vfxId = effectData.vfxRef != string.Empty ? effectData.vfxRef : string.Empty;
			string sfxId = effectData.sfxRef != string.Empty ? effectData.sfxRef : string.Empty;

			var locationChunk = GameClient.World.GetWorldChunk<ClientWorldChunk>(position);
			var playerChunk = GameClient.World.GetWorldChunk<ClientWorldChunk>(GameClient.LocalPlayerEntity.Position);

			if (playerChunk == null || locationChunk == null)
			{
				Debug.LogWarning("Playerchunk or Locationchunk is null");
				return;
			}
			
			var adjacentChunks = GameClient.World.GetNeighbouringChunks(playerChunk);

			if (locationChunk == playerChunk || adjacentChunks.Contains(locationChunk))
			{
				if (vfxId != null)
					SpawnVFX(vfxId, position, rotationDegrees, effectHeight);

				if (spawnRealEffects)
				{
					SpawnRealEffects(
						effectData,
						isLocal,
						position,
						rotationDegrees,
						modItemIds,
						mods,
						effectHeight,
                        ownerNetId
                    );
				}

                if (!string.IsNullOrEmpty(sfxId))
                    AudioManager.Instance.PlaySfx3dClip(sfxId, position);

                if (effectData.screenShake >= 0)
				{
					var distanceMultiplier = Vector2.Distance(position, GameClient.LocalPlayerEntity.Position);
					distanceMultiplier = distanceMultiplier < 5 ? 1 : 5 / distanceMultiplier;

					if (distanceMultiplier <= 1 &&  distanceMultiplier >= 0.01f)
						CameraScript.shakeCamera(effectData.screenShake * distanceMultiplier, effectData.screenShake * distanceMultiplier);
				}
            }
		}

		public void SpawnVFX(string vfxId, Vector2 position, float rotationDegrees, float heightPos)
		{
			var vfx = GameObjectPoolManager.GetObjectInstance(vfxId, PrefabType.VFX, new float2(0, 0));
			var vfxData = DataBank.Instance.GetData<VFXData>(vfxId);
			var effectHeightPos = vfxData.heightPos;
			var effectScale = vfxData.scale;
			vfx.transform.position = new Vector3(position.x, heightPos == -100 ? -effectHeightPos : heightPos, position.y);
			vfx.transform.rotation = Quaternion.Euler(0, -rotationDegrees, 0);
			vfx.transform.localScale = new Vector3(effectScale, effectScale, effectScale);
			vfx.SetActive(true);
		}

		public void SpawnRealEffects(
			EffectData effectData, 
			bool isLocal, 
			float2 position, 
			float rotationDegrees,
			string[] modItemIds,
			ItemWeaponModComponentData[] mods,
			float effectHeight = -1, 
			int ownerNetId = 0
		)
		{
			if (effectData.HasRealEffect())
			{
				if (effectData.GetRealEffectDataType() == typeof(ProjectileEffectData))
				{
					for (int i = 0; i < effectData.repetitions; i++)
						SpawnProjectile(
							isLocal,
							effectData,
							position,
							rotationDegrees,
							modItemIds,
							mods,
							effectHeight,
                            ownerNetId
                        );
				}
			}
		}

		private void SpawnProjectile(
			bool isLocal, 
			EffectData effectData, 
			float2 position, 
			float rotationDegrees,
			string[] modItemIds,
			ItemWeaponModComponentData[] mods,
			float effectHeight = -1, 
			int ownerNetId = 0
		)
		{
			if (isLocal)
				GameClient.RealEffectManager.SpawnProjectileLocalAuthority(
					effectData,
					(ProjectileEffectData)effectData.RealEffectData,
					position,
					rotationDegrees,
					modItemIds,
					mods,
					effectHeight
				);
			else
				GameClient.RealEffectManager.SpawnProjectileServerAuthority(
					effectData,
					(ProjectileEffectData)effectData.RealEffectData,
					position,
					rotationDegrees,
                    ownerNetId
                );
		}

        private void ResetGameObjectViewSpecifics(GameObject go, int netId)
        {
            var entity = GameClient.NetConnector.GetEntity(netId);
            var doorComp = entity.GetComponent<DoorComponentClient>();
			var inventoryComp = entity.GetComponent<InventoryComponentClient>();
			if (doorComp != null)
            {
                doorComp.OnViewInit();
            }
			if (inventoryComp != null)
			{
				inventoryComp.OnViewInit();
			}
		}

        public void SpawnWorldPrompt(string text, float2 position)
        {
            var wp = GameClient.Instantiate(P_WorldPrompt);
            var wpScript = wp.GetComponent<WorldPrompt>();
            wpScript.Init(text, position);
        }

        public void HighlightBaseFails(List<FNEEntity> entityList)
        {
            if (P_BaseFail == null)
                P_BaseFail = Resources.Load<GameObject>("Prefab/Effects/VFX/BaseEmergency/BaseEmergencyBeacon");

            foreach (var entity in entityList)
            {
                GameClient.Instantiate(
                    P_BaseFail, 
                    new Vector2(entity.Position.x + 0.5f, entity.Position.y + 0.5f), 
                    Quaternion.Euler(-90, 0, 0)
                );
            }
            
        }

        public RenderMeshSpawnerSystem GetRenderMeshSpawnerSystem()
        {
	        return m_RenderMeshSpawnerSystem;
        }
	}
}