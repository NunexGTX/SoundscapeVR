using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class RenameOBB : IPostprocessBuildWithReport
{
    public int callbackOrder => 1;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android) return;

        string buildPath = report.summary.outputPath; // e.g. "Builds/Android/build.apk"
        string buildDir = Path.GetDirectoryName(buildPath);
        string baseName = Path.GetFileNameWithoutExtension(buildPath); // "build"

        string wrongOBB = Path.Combine(buildDir, baseName + ".main.obb");
        if (!File.Exists(wrongOBB)) return;

        string packageName = PlayerSettings.applicationIdentifier;
        int versionCode = PlayerSettings.Android.bundleVersionCode;
        string correctOBB = Path.Combine(buildDir, $"main.{versionCode}.{packageName}.obb");

        if (File.Exists(correctOBB))
            File.Delete(correctOBB);

        File.Move(wrongOBB, correctOBB);

        UnityEngine.Debug.Log($"OBB renamed to: {Path.GetFileName(correctOBB)}");
    }
}