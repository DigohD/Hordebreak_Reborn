using System.Collections.Generic;
using UnityEngine;

namespace FNZ.Shared.Utils
{
	public static class AssetBundleLoader
	{
		private static readonly Dictionary<string, AssetBundle> s_AssetBundlesCache 
			= new Dictionary<string, AssetBundle>();

		public static bool IsAssetBundleLoaded(string assetBundlePath)
		{
			return s_AssetBundlesCache.ContainsKey(assetBundlePath);
		}

		public static T LoadAssetFromAssetBundle<T>(string assetBundlePath, string assetName) where T : Object
		{
			if (s_AssetBundlesCache.ContainsKey(assetBundlePath))
				return s_AssetBundlesCache[assetBundlePath].LoadAsset(assetName) as T;
			
			var bundle = AssetBundle.LoadFromFile($"{Application.streamingAssetsPath}/{assetBundlePath}");
			s_AssetBundlesCache.Add(assetBundlePath, bundle);
			return s_AssetBundlesCache[assetBundlePath].LoadAsset(assetName) as T;
		}
	}
}