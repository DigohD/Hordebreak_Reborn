using UnityEngine;
using UnityEditor;

public class AssetBundleCreator : Editor
{
	[MenuItem("FarNorth/Build AssetBundles")]
	static void BuildAssetBundles()
	{
		BuildPipeline.BuildAssetBundles(
			$"{Application.dataPath}/StreamingAssets/Data/XML/AssetBundles",
			BuildAssetBundleOptions.ChunkBasedCompression,
			BuildTarget.StandaloneWindows64);
	}
}
