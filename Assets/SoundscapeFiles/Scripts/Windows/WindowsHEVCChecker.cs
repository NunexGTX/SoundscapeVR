using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowsHEVC : MonoBehaviour
{
    //TODO: Test this script
    private static readonly IntPtr HKLM = new(unchecked((int)0x80000002));
    private static readonly IntPtr HKCU = new(unchecked((int)0x80000001));

    private const string PackagesPath =
        @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";

    private const uint KEY_READ = 0x20019;
    private const int ERROR_SUCCESS = 0;
    private const int ERROR_NO_MORE_ITEMS = 259;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    static extern int RegOpenKeyExW(IntPtr hKey, string subKey, uint ulOptions, uint samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    static extern int RegEnumKeyExW(IntPtr hKey, uint dwIndex, StringBuilder lpName, ref uint lpcchName,
        IntPtr reserved, IntPtr lpClass, IntPtr lpcchClass, out long lpftLastWriteTime);

    [DllImport("advapi32.dll")]
    static extern int RegCloseKey(IntPtr hKey);

    public bool IsAvailable { get; private set; }
    public GameObject HEVCWarning;

    void Start()
    {
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IsAvailable = Check();
            Debug.Log($"[WindowsHEVCChecker] HEVC Video Extensions available: {IsAvailable}");
            if (IsAvailable)
                SoundscapeSceneLoad.LoadMainScene();
            else
                ShowNoPluginMessage();
        #else
            Debug.LogWarning("This system is not running Microsoft Windows (or maybe a really old one). Please remove the Windows HEVC Checker scene from this build");
            SoundscapeSceneLoad.LoadMainScene();
        #endif
    }

    public static bool Check()
    {
        if (CheckHive(HKLM)) return true;
        if (CheckHive(HKCU)) return true;
        Debug.LogWarning("[WindowsHEVCChecker] HEVC Video Extensions not found.");
        return false;
    }

    static bool CheckHive(IntPtr hive)
    {
        if (RegOpenKeyExW(hive, PackagesPath, 0, KEY_READ, out IntPtr hKey) != ERROR_SUCCESS)
            return false;

        try
        {
            var sb = new StringBuilder(256);
            uint nameLen = (uint)sb.Capacity;
            for (uint i = 0; ; i++, nameLen = (uint)sb.Capacity)
            {
                int ret = RegEnumKeyExW(hKey, i, sb, ref nameLen, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out _);
                if (ret == ERROR_NO_MORE_ITEMS) break;
                if (ret != ERROR_SUCCESS) break;

                if (sb.ToString().StartsWith("Microsoft.HEVCVideoExtension", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[WindowsHEVCChecker] Found HEVC package: {sb}");
                    return true;
                }
            }
        }
        finally
        {
            RegCloseKey(hKey);
        }
        return false;
    }

    void ShowNoPluginMessage()
    {
        HEVCWarning.SetActive(true);

        VisualElement root = HEVCWarning.GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("skip-button").clicked += SoundscapeSceneLoad.LoadMainScene;
        root.Q<Button>("get-hevc").clicked += () => 
            Application.OpenURL("ms-windows-store://pdp/?ProductId=9nmzlz57r3t7");
        root.Q<Button>("exit").clicked += () => Application.Quit();
            
            
    }
}
