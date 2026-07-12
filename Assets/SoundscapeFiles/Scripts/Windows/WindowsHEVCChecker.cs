using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class WindowsHEVC : MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;

    public bool IsAvailable { get; private set; }
    public GameObject[] HEVCWarnings;
    public GameObject HEVCWarningUI;

    // ---- Media Foundation interop (mfplat.dll) ----
    // Ask Media Foundation directly whether an HEVC decoder MFT is available to this
    // process/user. That is the exact mechanism the media pipeline uses to decode, so its
    // answer matches whether the video will actually play. Unlike scanning the AppX package
    // registry, this is authoritative and correct per-user (a package registered only for
    // another Windows user will not enumerate here).
    private const uint MF_VERSION        = 0x00020070; // MF_SDK_VERSION<<16 | MF_API_VERSION
    private const uint MFSTARTUP_LITE    = 0x1;
    private const uint MFT_ENUM_FLAG_ALL = 0x3F;       // sync|async|hardware|fieldofuse|local|transcode

    private static readonly Guid MFT_CATEGORY_VIDEO_DECODER = new("d6c02d4b-6833-45b4-971a-05a4b04bab91");
    private static readonly Guid MFMediaType_Video          = new("73646976-0000-0010-8000-00aa00389b71");
    private static readonly Guid MFVideoFormat_HEVC         = new("43564548-0000-0010-8000-00aa00389b71"); // 'HEVC'
    private static readonly Guid MFVideoFormat_HEVC_ES      = new("53564548-0000-0010-8000-00aa00389b71"); // 'HEVS'

    [StructLayout(LayoutKind.Sequential)]
    private struct MFT_REGISTER_TYPE_INFO
    {
        public Guid guidMajorType;
        public Guid guidSubtype;
    }

    [DllImport("mfplat.dll", ExactSpelling = true)]
    static extern int MFStartup(uint version, uint flags);

    [DllImport("mfplat.dll", ExactSpelling = true)]
    static extern int MFShutdown();

    [DllImport("mfplat.dll", ExactSpelling = true)]
    static extern int MFTEnumEx(Guid guidCategory, uint flags,
        ref MFT_REGISTER_TYPE_INFO pInputType, IntPtr pOutputType,
        out IntPtr pppMFTActivate, out uint pcMFTActivate);

    void Start()
    {
        #if UNITY_STANDALONE_WIN
            IsAvailable = Check();
            Debug.Log($"[WindowsHEVCChecker] HEVC decoder available: {IsAvailable}");
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

    // True if Windows can decode HEVC for the current user (the HEVC Video Extension is
    // installed and its decoder MFT is registered). Any failure is treated as unavailable
    // so the (skippable) warning screen shows rather than silently playing broken video.
    public static bool Check()
    {
        try
        {
            int hr = MFStartup(MF_VERSION, MFSTARTUP_LITE);
            if (hr != 0)
            {
                Debug.LogWarning($"[WindowsHEVCChecker] MFStartup failed (hr=0x{hr:X8}); assuming HEVC unavailable.");
                return false;
            }

            try
            {
                bool found = HasDecoderFor(MFVideoFormat_HEVC) || HasDecoderFor(MFVideoFormat_HEVC_ES);
                if (!found)
                    Debug.LogWarning("[WindowsHEVCChecker] No HEVC decoder MFT registered for this user.");
                return found;
            }
            finally
            {
                MFShutdown();
            }
        }
        catch (DllNotFoundException)
        {
            Debug.LogWarning("[WindowsHEVCChecker] mfplat.dll not found (Windows N/KN without Media Feature Pack?); assuming HEVC unavailable.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[WindowsHEVCChecker] HEVC probe threw: {e.Message}; assuming HEVC unavailable.");
            return false;
        }
    }

    static bool HasDecoderFor(Guid subtype)
    {
        var inType = new MFT_REGISTER_TYPE_INFO
        {
            guidMajorType = MFMediaType_Video,
            guidSubtype = subtype
        };

        int hr = MFTEnumEx(MFT_CATEGORY_VIDEO_DECODER, MFT_ENUM_FLAG_ALL,
            ref inType, IntPtr.Zero, out IntPtr ppActivate, out uint count);
        if (hr != 0)
        {
            Debug.LogWarning($"[WindowsHEVCChecker] MFTEnumEx failed (hr=0x{hr:X8}).");
            return false;
        }

        try
        {
            Debug.Log($"[WindowsHEVCChecker] HEVC decoder MFTs for {subtype}: {count}");
            return count > 0;
        }
        finally
        {
            if (ppActivate != IntPtr.Zero)
            {
                for (uint i = 0; i < count; i++) // release each IMFActivate* the API AddRef'd
                {
                    IntPtr p = Marshal.ReadIntPtr(ppActivate, (int)(i * (uint)IntPtr.Size));
                    if (p != IntPtr.Zero) Marshal.Release(p);
                }
                Marshal.FreeCoTaskMem(ppActivate); // free the CoTaskMemAlloc'd array
            }
            Debug.Log($"[WindowsHEVCChecker] Enumerated {count} packages under {hiveName}, no HEVC extension found.");
        }
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
