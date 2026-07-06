using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowsHEVC : MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;
    private static readonly IntPtr HKLM = new(unchecked((int)0x80000002));
    private static readonly IntPtr HKCU = new(unchecked((int)0x80000001));

    private const string PackagesPath =
        @"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages";

    private const uint KEY_READ = 0x20019;
    private const uint KEY_WOW64_64KEY = 0x0100; // always read the 64-bit view, even from a 32-bit build
    private const int ERROR_SUCCESS = 0;
    private const int ERROR_MORE_DATA = 234;
    private const int ERROR_NO_MORE_ITEMS = 259;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    static extern int RegOpenKeyExW(IntPtr hKey, string subKey, uint ulOptions, uint samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    static extern int RegEnumKeyExW(IntPtr hKey, uint dwIndex, StringBuilder lpName, ref uint lpcchName,
        IntPtr reserved, IntPtr lpClass, IntPtr lpcchClass, out long lpftLastWriteTime);

    [DllImport("advapi32.dll")]
    static extern int RegCloseKey(IntPtr hKey);

    public bool IsAvailable { get; private set; }
    public GameObject[] HEVCWarnings;
    public GameObject HEVCWarningUI;

    void Start()
    {
        #if UNITY_STANDALONE_WIN
            IsAvailable = Check();
            Debug.Log($"[WindowsHEVCChecker] HEVC Video Extensions available: {IsAvailable}");
            if (IsAvailable)
                SoundscapeSceneLoad.LoadMainScene();
            else
                ShowNoPluginMessage();
        #elif UNITY_EDITOR
            //ShowNoPluginMessage();

            //TEMPORARY
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
        if (CheckHive(HKLM, "HKLM")) return true;
        if (CheckHive(HKCU, "HKCU")) return true;
        Debug.LogWarning("[WindowsHEVCChecker] HEVC Video Extensions not found in any registry hive.");
        return false;
    }

    static bool CheckHive(IntPtr hive, string hiveName)
    {
        int open = RegOpenKeyExW(hive, PackagesPath, 0, KEY_READ | KEY_WOW64_64KEY, out IntPtr hKey);
        if (open != ERROR_SUCCESS)
        {
            Debug.LogWarning($"[WindowsHEVCChecker] Could not open Packages key under {hiveName} (error {open}).");
            return false;
        }

        try
        {
            var sb = new StringBuilder(512);
            int count = 0;
            for (uint i = 0; ; i++)
            {
                uint nameLen = (uint)sb.Capacity;
                int ret = RegEnumKeyExW(hKey, i, sb, ref nameLen, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out _);
                if (ret == ERROR_NO_MORE_ITEMS) break;
                if (ret == ERROR_MORE_DATA) continue; // name longer than buffer; not an HEVC package name
                if (ret != ERROR_SUCCESS)
                {
                    Debug.LogWarning($"[WindowsHEVCChecker] RegEnumKeyExW failed at index {i} under {hiveName} (error {ret}).");
                    break;
                }

                count++;
                if (sb.ToString().StartsWith("Microsoft.HEVCVideoExtension", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[WindowsHEVCChecker] Found HEVC package under {hiveName}: {sb}");
                    return true;
                }
            }
            Debug.Log($"[WindowsHEVCChecker] Enumerated {count} packages under {hiveName}, no HEVC extension found.");
        }
        finally
        {
            RegCloseKey(hKey);
        }
        return false;
    }

    void ShowNoPluginMessage()
    {
        if (vrStateData.IsInVr)
        {
            foreach (GameObject HEVCWarning in HEVCWarnings)
            {
                HEVCWarning.SetActive(true);
                registerButtonsEvents(HEVCWarning);
            }
        }
        else
        {
            HEVCWarningUI.SetActive(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            registerButtonsEvents(HEVCWarningUI);
        }
    }

    void registerButtonsEvents(GameObject uiSource)
    {
        VisualElement root = uiSource.GetComponent<UIDocument>().rootVisualElement;
        
        root.Q<Button>("skip-button").RegisterCallback<ClickEvent>(e => SoundscapeSceneLoad.LoadMainScene());
        root.Q<Button>("get-hevc").RegisterCallback<ClickEvent>(e => getExtensionAndQuit());
        root.Q<Button>("quit").RegisterCallback<ClickEvent>(e => Quit());
    }

    void getExtensionAndQuit()
    {
        Application.OpenURL("http://dl.delivery.mp.microsoft.com/filestreamingservice/files/c59271a2-7509-4a90-b8fb-ab8512b78135");
        Quit();
    }

    static void Quit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
