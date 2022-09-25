using System;
using FNZ.Client.Systems.Hordes;
using FNZ.Client.Systems.Hordes.Components;
using FNZ.Client.Utils;
using FNZ.Shared.Model;
using FNZ.Shared.Model.Entity;
using FNZ.Shared.Model.Entity.Components.EnemyStats;
using FNZ.Shared.Model.Entity.EntityViewData;
using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FNZ.Shared.Model.Entity.Components.GPUAnimationData;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace FNZ.Client.GPUSkinning
{
    public static class GPUAnimationCharacterUtility
    {
        private static readonly Dictionary<string, Entity> s_GPUAnimationEntityPrefabs =
            new Dictionary<string, Entity>();

        private static readonly Dictionary<string, Entity> s_BlobShadowEntityPrefabs = 
            new Dictionary<string, Entity>();

        public static Entity? GetEntityPrefab(string id)
        {
            if (s_GPUAnimationEntityPrefabs.TryGetValue(id, out var entity)) return entity;
            Debug.LogError($"No GPUAnimation Entity prefab found for id: {id}");
            return null;
        }

        public static Entity? GetBlobShadowEntityPrefab(string id)
        {
            if (s_BlobShadowEntityPrefabs.TryGetValue(id, out var entity)) return entity;
            Debug.LogError($"No Blob shadow Entity prefab found for id: {id}");
            return null;
        }

        public static void CreateConvertToGPUPrefabs()
        {
            var dataDefs = DataBank.Instance.GetAllDataDefsOfType(
                typeof(FNEEntityData)).FindAll(d => ((FNEEntityData) d).entityType == EntityType.ECS_ENEMY);

            foreach (var dataDef in dataDefs)
            {
                var entityData = DataBank.Instance.GetData<FNEEntityData>(dataDef.Id);
                var entityViewData = DataBank.Instance.GetData<FNEEntityViewData>(entityData.ViewRef);

                var meshRef = entityViewData.entityMeshData;
                var meshData = DataBank.Instance.GetData<FNEEntityMeshData>(meshRef);
                
                if (meshData == null)
                {
                    Debug.LogError("Error: \n" +
                                   $"Could not find meshRef in EnemeyComponentData for Entity with id: {dataDef.Id}.");
                    continue;
                }

                var meshPath = $"{Application.streamingAssetsPath}/{meshData.MeshPath}";
                var characterRig = Importer.LoadFromFile(meshPath);
                if (characterRig == null)
                {
                    Debug.LogError(
                        "Error: \n" + $"Failed to import Mesh for id: {dataDef.Id} with meshPath: {meshPath}");
                    continue;
                }

                var renderer = characterRig.GetComponentInChildren<SkinnedMeshRenderer>();
                if (renderer == null)
                {
                    Debug.LogError(
                        $"No SkinnedMeshRenderer was found on GPU Animated Character GameObject for id: {dataDef.Id} with meshPath: {meshPath}");
                    Object.DestroyImmediate(characterRig);
                    continue;
                }
                
                var textureData = DataBank.Instance.GetData<FNEEntityTextureData>(entityViewData.entityTextureData);
                var material = MaterialUtils.CreateGPUSkinningMaterial(textureData);
                if (material == null)
                {
                    Debug.LogError(
                        $"No GPUSkinningMaterial was found on FNEEntityMeshData for id: {dataDef.Id} with meshPath: {meshPath}");
                    Object.DestroyImmediate(characterRig);
                    continue;
                }

                renderer.sharedMaterial = material;

                characterRig.transform.localScale *= entityViewData.scaleMod;

                var convertToEntity = characterRig.AddComponent<ConvertToEntity>();
                var convertToGPUCharacter = characterRig.AddComponent<ConvertToGPUCharacter>();

                convertToEntity.ConversionMode = ConvertToEntity.Mode.ConvertAndDestroy;
                convertToGPUCharacter.AnimationClips = GetAnimationClips(meshPath);
                convertToGPUCharacter.Id = dataDef.Id;
            }
        }

        private static AnimationClip[] GetAnimationClips(string path)
        {
            var importedAnimations = Importer.LoadAnimationsFromFile(path);

            if (importedAnimations == null)
            {
                Debug.LogError($"Error: {Path.GetFileName(path)} \n" + $"No animations found in object at {path}.");
                return Array.Empty<AnimationClip>();
            }

            var animClips = new List<AnimationClip>(importedAnimations.Length);
            animClips.AddRange(importedAnimations.Select(animation => animation.clip));

            var animTypes = Enum.GetValues(typeof(GPUAnimationType))
                .Cast<GPUAnimationType>()
                .ToList();

            var animClipsSorted = new List<AnimationClip>(animTypes.Count);
            for (var i = 0; i < animTypes.Count; i++)
                animClipsSorted.Add(null);

            foreach (var animClip in animClips)
            {
                foreach (var animType in animTypes)
                {
                    var input = animClip.name.ToLower();
                    var pattern = animType.ToString().ToLower();

                    if (input.Contains(pattern))
                    {
                        animClipsSorted[(int) animType] = animClip;
                    }
                }
            }

            if (animClipsSorted.All(x => x == null))
            {
                Debug.LogError($"No animation clips found with correct naming for path: {path}.");
            }

            return animClipsSorted.ToArray();
        }

        public static void AddCharacterComponents(
            EntityManager manager,
            Entity entity,
            GameObject characterRig,
            AnimationClip[] clips,
            string id,
            bool castShadows = false
        )
        {
            var renderer = characterRig.GetComponentInChildren<SkinnedMeshRenderer>();
            var sharedMesh = renderer.sharedMesh;

            var lod = new LodData
            {
                Lod1Mesh = sharedMesh,
                Lod2Mesh = sharedMesh,
                Lod3Mesh = sharedMesh,
                Lod1Distance = 0,
                Lod2Distance = 100,
                Lod3Distance = 10000,
            };

            const float frameRate = 60.0f;

            var bakedData = KeyframeTextureBaker.BakeClips(characterRig, clips, frameRate, lod);

            var materials = new List<Material>
            {
                renderer.sharedMaterial
            };

            manager.AddComponentData(entity, default(HordeEntity_Tag));
            manager.AddComponentData(entity, default(NetworkIdComponent));

            var animState = default(GPUAnimationState);
            animState.AnimationClipSet = CreateClipSet(bakedData);

            manager.AddComponentData(entity, animState);
            manager.AddComponentData(entity, default(AnimationTextureCoordinate));
            manager.AddComponentData(entity, default(GPUAnimationComponent));
            manager.AddComponentData(entity, default(HordeEntityServerPosition));
            manager.AddComponentData(entity, default(HordeEntityAttackTargetPosition));

            var dataDef = DataBank.Instance.GetData<FNEEntityData>(id);
            var stats = dataDef.GetComponentData<EnemyStatsComponentData>();

            var attackRange = 1.0f;

            if (stats != null)
            {
                attackRange = stats.attackRange;
            }

            var attackTargetData = new HordeEntityStatsData
            {
                AttackRange = attackRange
            };

            manager.AddComponentData(entity, attackTargetData);

            renderer.sharedMaterial.SetTexture("_AnimationTexture0", bakedData.AnimationTextures.Animation0);
            renderer.sharedMaterial.SetTexture("_AnimationTexture1", bakedData.AnimationTextures.Animation1);
            renderer.sharedMaterial.SetTexture("_AnimationTexture2", bakedData.AnimationTextures.Animation2);

            GPUAnimationMeshConversionUtility.Convert(entity, manager, renderer, bakedData.NewMesh, materials);

            var prefab = (GameObject) Resources.Load("Prefab/Effects/BlobShadow");

            var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
            var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
            var material = meshRenderer.sharedMaterial;

            var desc = new RenderMeshDescription(
                meshFilter.sharedMesh,
                material,
                shadowCastingMode: castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                receiveShadows: false
            );

            if (!castShadows)
            {
                var blobShadowEntityPrefab = manager.CreateEntity();

                RenderMeshUtility.AddComponents(
                    blobShadowEntityPrefab,
                    manager,
                    desc
                );

                var entityData = DataBank.Instance.GetData<FNEEntityData>(id);
                var animCompData = entityData.GetComponentData<GPUAnimationComponentData>();

                manager.AddComponentData(blobShadowEntityPrefab, default(BlobShadow_Tag));
                manager.AddComponentData(blobShadowEntityPrefab, default(LocalToWorld));
            
                manager.AddComponentData(blobShadowEntityPrefab, new Translation
                {
                    Value = new float3(0, animCompData.BlobShadowHeightOffset, 0)
                });
            
                manager.AddComponentData(blobShadowEntityPrefab, new Scale
                {
                    Value = animCompData.BlobShadowScale
                });
            
                manager.AddComponentData(blobShadowEntityPrefab, default(Rotation));
                manager.AddComponentData(blobShadowEntityPrefab, default(Parent));
                manager.AddComponentData(blobShadowEntityPrefab, default(LocalToParent));
                
                if (!s_BlobShadowEntityPrefabs.ContainsKey(id))
                    s_BlobShadowEntityPrefabs.Add(id, blobShadowEntityPrefab);
            }
            
            if (!s_GPUAnimationEntityPrefabs.ContainsKey(id))
                s_GPUAnimationEntityPrefabs.Add(id, entity);
        }

        private static BlobAssetReference<BakedAnimationClipSet> CreateClipSet(BakedData data)
        {
            using var builder = new BlobBuilder(Allocator.Temp);

            ref var root = ref builder.ConstructRoot<BakedAnimationClipSet>();
            var clips = builder.Allocate(ref root.Clips, data.Animations.Count);

            for (var i = 0; i < data.Animations.Count; i++)
            {
                clips[i] = new BakedAnimationClip(data.AnimationTextures, data.Animations[i]);
            }

            return builder.CreateBlobAssetReference<BakedAnimationClipSet>(Allocator.Persistent);
        }
    }
}