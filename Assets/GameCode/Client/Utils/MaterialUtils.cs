using FNZ.Client.View.World;
using FNZ.Shared.Model.Entity.EntityViewData;
using System.Collections.Generic;
using FNZ.Shared.Utils;
using UnityEngine;

namespace FNZ.Client.Utils
{
    public static class MaterialUtils
    {
        public static Material CreateGPUSkinningMaterial(FNEEntityTextureData fneEntityTextureData)
        {
            var sourceMaterial = Resources.Load<Material>("Material/Entity/GPUSkinningMaterial");

            if (sourceMaterial == null)
            {
                return null;
            }

            var mat = new Material(sourceMaterial);

            if (!string.IsNullOrEmpty(fneEntityTextureData.AlbedoName))
            {
                var albedoTexture = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                    fneEntityTextureData.AssetBundlePath,
                    fneEntityTextureData.AlbedoName
                );
                mat.SetTexture("_BaseColorMap", albedoTexture);
                mat.EnableKeyword("_BaseColorMap");
            }

            if (!string.IsNullOrEmpty(fneEntityTextureData.NormalMapName))
            {
                var normalTexture = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                    fneEntityTextureData.AssetBundlePath,
                    fneEntityTextureData.NormalMapName
                );

                mat.SetTexture("_NormalMap", normalTexture);
                mat.SetFloat("_NormalMapSpace", 0.0f);
                mat.SetFloat("_NormalScale", 0.0f);
                mat.EnableKeyword("_NormalMap");
                mat.EnableKeyword("_NormalMapSpace");
                mat.EnableKeyword("_NormalScale");
                mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
            }

            if (!string.IsNullOrEmpty(fneEntityTextureData.MaskMapName))
            {
                var maskMapTexture = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                    fneEntityTextureData.AssetBundlePath,
                    fneEntityTextureData.MaskMapName
                );

                mat.SetTexture("_MaskMap", maskMapTexture);
                mat.SetFloat("_MetallicRemapMin", 0);
                mat.SetFloat("_MetallicRemapMax", 1);
            }
            else
            {
                var maskMapTexture = new Texture2D(
                    0,
                    0,
                    TextureFormat.RGBA32,
                    false,
                    true
                );
                maskMapTexture = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");
                mat.SetTexture("_MaskMap", maskMapTexture);
            }

            mat.EnableKeyword("_MaskMap");
            mat.EnableKeyword("_MASKMAP");
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_AORemapMin", 0);
            mat.SetFloat("_AORemapMax", 1);

            if (!string.IsNullOrEmpty(fneEntityTextureData.EmissiveMapName))
            {
                var emissiveTexture = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                    fneEntityTextureData.AssetBundlePath,
                    fneEntityTextureData.EmissiveMapName
                );

                mat.SetTexture("_EmissiveColorMap", emissiveTexture);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.white);
                mat.SetFloat("_EmissiveExposureWeight", 0.5f);
                mat.SetFloat("_EmissiveFactor", fneEntityTextureData.EmissiveFactor);
            }
            else
            {
                var emissiveNull = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");

                mat.SetTexture("_EmissiveColorMap", emissiveNull);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.black);
                mat.SetFloat("_EmissiveExposureWeight", 0f);
            }

            mat.EnableKeyword("_DISABLE_DECALS");
            mat.EnableKeyword("_DISABLE_SSR_TRANSPARENT");

            mat.Lerp(mat, mat, 1);

            return mat;
        }

        static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        /// <summary>
        /// Create a new material using the Mesh Data of the object.
        /// </summary>
        /// <param name="fneEntityTextureData">Texture Data object containing information for all texture paths</param>
        /// <param name="isTransparent">Enable transparency (for transparent materials, or materials with alpha cutouts)</param>
        public static Material CreateMaterialFromTextureData(FNEEntityTextureData fneEntityTextureData,
            bool isTransparent, bool isVegetation = false)
        {
            string materialKey = null;

            if (!string.IsNullOrEmpty(fneEntityTextureData.AssetBundlePath))
            {
                materialKey = fneEntityTextureData.AlbedoName + '|' + fneEntityTextureData.NormalMapName + '|' +
                              fneEntityTextureData.EmissiveMapName + '|' + fneEntityTextureData.MaskMapName;
            }
            else
            {
                materialKey = fneEntityTextureData.AlbedoPath + '|' + fneEntityTextureData.NormalPath + '|' +
                              fneEntityTextureData.EmissivePath + '|' + fneEntityTextureData.MaskMapPath;
            }

            if (materialCache.ContainsKey(materialKey))
            {
                return materialCache[materialKey];
            }

            Material mat;
            
            if (isTransparent)
            {
                if (isVegetation)
                {
                    mat = new Material(Resources.Load<Material>("Material/Vegetation/VegetationMaterialTransparent"));
                    mat.SetFloat("_BendStrength", 7.0f);
                    mat.SetFloat("_Speed", 0.1f);
                    mat.SetFloat("_NoiseScale", 5.0f);
                    mat.SetVector("_Direction", new Vector2(FNERandom.GetRandomFloatInRange(-1, 1), FNERandom.GetRandomFloatInRange(-1, 1)));
                }
                else
                {
                    mat = new Material(Resources.Load<Material>("Material/Entity/All/CutoutBase"));
                }
            }
            else
            {
                if (isVegetation)
                {
                    mat = new Material(Resources.Load<Material>("Material/Vegetation/VegetationMaterial"));
                    mat.SetFloat("_BendStrength", 7.0f);
                    mat.SetFloat("_Speed", 0.1f);
                    mat.SetFloat("_NoiseScale", 5.0f);
                    mat.SetVector("_Direction", new Vector2(FNERandom.GetRandomFloatInRange(-1, 1), FNERandom.GetRandomFloatInRange(-1, 1)));
                }
                else
                {
                    mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
                }
            }

            Texture2D albedo = null;
            Texture2D normalMap = null;
            Texture2D maskMap = null;
            Texture2D emissiveMap = null;

            if (!string.IsNullOrEmpty(fneEntityTextureData.AssetBundlePath))
            {
                if (!string.IsNullOrEmpty(fneEntityTextureData.AlbedoName))
                {
                    albedo = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                        fneEntityTextureData.AssetBundlePath,
                        fneEntityTextureData.AlbedoName
                    );
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.NormalMapName))
                {
                    normalMap = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                        fneEntityTextureData.AssetBundlePath,
                        fneEntityTextureData.NormalMapName
                    );
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.MaskMapName))
                {
                    maskMap = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                        fneEntityTextureData.AssetBundlePath,
                        fneEntityTextureData.MaskMapName
                    );
                }
                else
                {
                    maskMap = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.EmissiveMapName))
                {
                    emissiveMap = AssetBundleLoader.LoadAssetFromAssetBundle<Texture2D>(
                        fneEntityTextureData.AssetBundlePath,
                        fneEntityTextureData.EmissiveMapName
                    );
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(fneEntityTextureData.AlbedoPath))
                {
                    var albedoTexture = new Texture2D(0, 0);
                    albedo = FNEFileLoader.TryLoadImage(fneEntityTextureData.AlbedoPath, albedoTexture);
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.NormalPath))
                {
                    var normalTexture = new Texture2D(
                        0,
                        0,
                        TextureFormat.RGBA32,
                        false,
                        true
                    );

                    normalMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.NormalPath, normalTexture);
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.MaskMapPath))
                {
                    var maskMapTexture = new Texture2D(
                        0,
                        0,
                        TextureFormat.RGBA32,
                        false,
                        true
                    );

                    maskMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.MaskMapPath, maskMapTexture);
                }
                else
                {
                    maskMap = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");
                }

                if (!string.IsNullOrEmpty(fneEntityTextureData.EmissivePath))
                {
                    var emissiveTexture = new Texture2D(0, 0);
                    emissiveMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.EmissivePath, emissiveTexture);
                }
            }

            if (albedo != null)
            {
                mat.SetTexture("_BaseColorMap", albedo);
                mat.EnableKeyword("_BaseColorMap");
            }

            if (normalMap != null)
            {
                mat.SetTexture("_NormalMap", normalMap);
                mat.SetFloat("_NormalMapSpace", 0.0f);
                mat.SetFloat("_NormalScale", 1.0f);
                mat.EnableKeyword("_NormalMap");
                mat.EnableKeyword("_NormalMapSpace");
                mat.EnableKeyword("_NormalScale");
                mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
            }

            if (maskMap != null)
            {
                mat.SetTexture("_MaskMap", maskMap);
                mat.SetFloat("_MetallicRemapMin", 0);
                mat.SetFloat("_MetallicRemapMax", 1);
            }

            mat.EnableKeyword("_MaskMap");
            mat.EnableKeyword("_MASKMAP");
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_AORemapMin", 0);
            mat.SetFloat("_AORemapMax", 1);

            if (emissiveMap != null)
            {
                mat.SetTexture("_EmissiveColorMap", emissiveMap);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.white * 500);
                mat.SetFloat("_EmissiveExposureWeight", 0.5f);
                mat.SetFloat("_EmissiveFactor", fneEntityTextureData.EmissiveFactor);
            }
            else
            {
                var emissiveNull = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");

                mat.SetTexture("_EmissiveColorMap", emissiveNull);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.black);
                mat.SetFloat("_EmissiveExposureWeight", 0f);
                mat.SetFloat("_EmissiveFactor", 0.0f);
            }

            mat.Lerp(mat, mat, 1);

            materialCache.Add(materialKey, mat);

            return mat;
        }

        public static Material CreateMaterialFromMeshData(FNEEntityMeshData fneEntityTextureData, bool isTransparent)
        {
            string materialKey = fneEntityTextureData.AlbedoPath + '|' + fneEntityTextureData.NormalPath + '|' +
                                 fneEntityTextureData.EmissivePath + '|' + fneEntityTextureData.MaskMapPath;


            if (materialCache.ContainsKey(materialKey))
            {
                return materialCache[materialKey];
            }

            Material mat;


            if (isTransparent)
            {
                mat = new Material(Resources.Load<Material>("Material/Entity/All/CutoutBase"));
            }
            else
            {
                mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
            }


            Texture2D albedo = null;
            Texture2D normalMap = null;
            Texture2D maskMap = null;
            Texture2D emissiveMap = null;

            if (!string.IsNullOrEmpty(fneEntityTextureData.AlbedoPath))
            {
                var albedoTexture = new Texture2D(0, 0);
                albedo = FNEFileLoader.TryLoadImage(fneEntityTextureData.AlbedoPath, albedoTexture);
            }

            if (!string.IsNullOrEmpty(fneEntityTextureData.NormalPath))
            {
                var normalTexture = new Texture2D(
                    0,
                    0,
                    TextureFormat.RGBA32,
                    false,
                    true
                );
                normalMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.NormalPath, normalTexture);
            }

            if (!string.IsNullOrEmpty(fneEntityTextureData.MaskMapPath))
            {
                var maskMapTexture = new Texture2D(
                    0,
                    0,
                    TextureFormat.RGBA32,
                    false,
                    true
                );
                maskMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.MaskMapPath, maskMapTexture);
            }
            else
            {
                maskMap = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");
            }

            if (!string.IsNullOrEmpty(fneEntityTextureData.EmissivePath))
            {
                var emissiveTexture = new Texture2D(0, 0);
                emissiveMap = FNEFileLoader.TryLoadImage(fneEntityTextureData.EmissivePath, emissiveTexture);
            }


            if (albedo != null)
            {
                mat.SetTexture("_BaseColorMap", albedo);
                mat.EnableKeyword("_BaseColorMap");
            }

            if (normalMap != null)
            {
                mat.SetTexture("_NormalMap", normalMap);
                mat.SetFloat("_NormalMapSpace", 0.0f);
                mat.SetFloat("_NormalScale", 1.0f);
                mat.EnableKeyword("_NormalMap");
                mat.EnableKeyword("_NormalMapSpace");
                mat.EnableKeyword("_NormalScale");
                mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
            }

            if (maskMap != null)
            {
                mat.SetTexture("_MaskMap", maskMap);
                mat.SetFloat("_MetallicRemapMin", 0);
                mat.SetFloat("_MetallicRemapMax", 1);
            }

            mat.EnableKeyword("_MaskMap");
            mat.EnableKeyword("_MASKMAP");
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_AORemapMin", 0);
            mat.SetFloat("_AORemapMax", 1);

            if (emissiveMap != null)
            {
                mat.SetTexture("_EmissiveColorMap", emissiveMap);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.white * 500);
                mat.SetFloat("_EmissiveExposureWeight", 0.5f);
            }
            else
            {
                var emissiveNull = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");

                mat.SetTexture("_EmissiveColorMap", emissiveNull);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.black);
                mat.SetFloat("_EmissiveExposureWeight", 0f);
            }

            mat.Lerp(mat, mat, 1);

            materialCache.Add(materialKey, mat);

            return mat;
        }

        /// <summary>
        /// Create a material for the Tile Sheet using the paths for different tiles within the Tile Sheet
        /// </summary>
        public static Material CreateTileSheetMaterial(string albedoPath, string normalPath, string maskPath,
            string emissivePath, bool isTransparent)
        {
            Material mat;

            if (isTransparent)
            {
                mat = new Material(Resources.Load<Material>("Material/Entity/All/CutoutBase"));
            }
            else
            {
                mat = new Material(Resources.Load<Material>("Material/Entity/All/Base"));
            }

            if (!string.IsNullOrEmpty(albedoPath))
            {
                Texture2D albedoTexture = ClientTileSheetPacker.GetAtlas(albedoPath);
                mat.SetTexture("_BaseColorMap", albedoTexture);
                mat.EnableKeyword("_BaseColorMap");
            }

            if (!string.IsNullOrEmpty(normalPath))
            {
                Texture2D normalTexture = ClientTileSheetPacker.GetAtlas(normalPath);
                mat.SetTexture("_NormalMap", normalTexture);
                mat.SetFloat("_NormalMapSpace", 0.0f);
                mat.SetFloat("_NormalScale", 1.0f);
                mat.EnableKeyword("_NormalMap");
                mat.EnableKeyword("_NormalMapSpace");
                mat.EnableKeyword("_NormalScale");
                mat.EnableKeyword("_NORMALMAP");
                mat.EnableKeyword("_NORMALMAP_TANGENT_SPACE");
            }

            if (!string.IsNullOrEmpty(maskPath))
            {
                Texture2D maskMapTexture = ClientTileSheetPacker.GetAtlas(maskPath);
                mat.SetTexture("_MaskMap", maskMapTexture != null ? maskMapTexture : Texture2D.blackTexture);
                mat.EnableKeyword("_MaskMap");
                mat.EnableKeyword("_MASKMAP");
                mat.SetFloat("_MetallicRemapMin", 0);
                mat.SetFloat("_MetallicRemapMax", 1);
                mat.SetFloat("_Metallic", 1f);
            }
            else
            {
                var maskMapTexture = new Texture2D(
                    0,
                    0,
                    TextureFormat.RGBA32,
                    false,
                    true
                );
                maskMapTexture = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");
                mat.SetTexture("_MaskMap", maskMapTexture);
            }

            mat.EnableKeyword("_MaskMap");
            mat.EnableKeyword("_MASKMAP");
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_AORemapMin", 0);
            mat.SetFloat("_AORemapMax", 0);

            if (!string.IsNullOrEmpty(emissivePath))
            {
                Texture2D emissiveTexture = ClientTileSheetPacker.GetAtlas(emissivePath);
                mat.SetTexture("_EmissiveColorMap", emissiveTexture);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.black);
                mat.SetFloat("_EmissiveExposureWeight", 1f);
                mat.EnableKeyword("_EmissiveIntensity");
            }
            else
            {
                var emissiveNull = Resources.Load<Texture2D>("Texture/Material/MaskMapNull");

                mat.SetTexture("_EmissiveColorMap", emissiveNull);
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", Color.black);
                mat.SetFloat("_EmissiveExposureWeight", 0f);
            }

            mat.Lerp(mat, mat, 1);

            return mat;
        }
    }
}