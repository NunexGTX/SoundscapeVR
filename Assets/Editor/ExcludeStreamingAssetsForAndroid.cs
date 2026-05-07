using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class ExcludeStreamingAssetsForAndroid : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    private static readonly string StreamingAssetsPath = Application.streamingAssetsPath;
    private static readonly string TempBackupPath = "Temp/StreamingAssetsBackup";

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        if (!Directory.Exists(StreamingAssetsPath)) return;

        // Move StreamingAssets out before build
        if (Directory.Exists(TempBackupPath))
            Directory.Delete(TempBackupPath, true);

        Directory.Move(StreamingAssetsPath, TempBackupPath);
        AssetDatabase.Refresh();
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        // Move it back after build (even if build failed)
        if (!Directory.Exists(TempBackupPath)) return;

        if (Directory.Exists(StreamingAssetsPath))
            Directory.Delete(StreamingAssetsPath, true);

        Directory.Move(TempBackupPath, StreamingAssetsPath);
        AssetDatabase.Refresh();
    }
}