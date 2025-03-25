using UnityEditor;

public class BuildAssetBundles
{
    [MenuItem("Assets/Build Asset Bundles Tom")]
    public static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundlesTom", 
            BuildAssetBundleOptions.None, 
            BuildTarget.StandaloneWindows); // Change to your target platform
    }
}