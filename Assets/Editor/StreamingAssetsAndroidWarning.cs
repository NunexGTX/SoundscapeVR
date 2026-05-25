using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System.IO;
using Unity.Mathematics;

public class StreamingAssetsAndroidWarning : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        long streamingAssetsMegaBytes = streamingAssetsSize();
        Debug.Log($"StreamingAssets size: {streamingAssetsMegaBytes} Bytes");
        if (report.summary.platform == BuildTarget.Android)
        {
            if (AssetDatabase.IsValidFolder("Assets/StreamingAssets") && streamingAssetsMegaBytes>1)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "⚠️ Streaming Assets Detected for Android Build",
                    "The Streaming Assets folder is not necessary for SoundscapeVR's Android Build.\n\n" +
                    "This is because, the files won't actually be accessed, making you lose time in copying them and wasting space on the Quest/Android XR Device.\n\n" +
                    $"There isn't any automation for this due to data loss concerns. StreamingAssets size: {streamingAssetsMegaBytes}MB\n\n" +
                    "Move the Assets/StreamingAssets folder first out of the Unity's Project before procceding with this build. The files should be copied after to Android's Storage.\n\n" +
                    "Do you wish to procceed anyways?",
                    "OK",
                    "Cancel"
                );

                if (!proceed)
                {
                    throw new BuildFailedException("Build cancelled by user due to StreamingAssets exclusion in Android Builds");
                }

            }
        }
    }

    long streamingAssetsSize()
{
        long size = 0;
        foreach (string file in Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories))
        {
            size += new FileInfo(file).Length;
        }
        return size/(long)math.pow(2,20);
    }
}
