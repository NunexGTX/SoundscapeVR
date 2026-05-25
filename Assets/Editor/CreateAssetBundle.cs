using System;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundle
{
    [MenuItem("Assets/Create Asset Bundles")]
    private static void BuildAllAssetBundles()
    {
        string dirPath = Application.dataPath + "/../ExportedAssetBundles";
        try
        {
            BuildPipeline.BuildAssetBundles(dirPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
