#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

public class CreateAssetBundle
{
    [MenuItem("Assets/Create Assets Bundles")]
    static void BuildAllAssetBundles()
    {
        Debug.Log("Creating Asset Bundle");
        // Define the output directory for the asset bundles
        string assetBundleDirectory = "Assets/CreatedAssetBundles";

        // Create the directory if it doesn't exist
        if (!System.IO.Directory.Exists(assetBundleDirectory))
            System.IO.Directory.CreateDirectory(assetBundleDirectory);

        try
        {
            BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows // Change to your desired platform
         );
            Debug.Log("Successfully Created Asset Bundle");
        }
        catch (Exception e)
        {

            Debug.LogWarning($"Failded To Create : {e}");
        }

    }
}
#endif
