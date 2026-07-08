using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public class QuitHandler : MonoBehaviour
{
    static bool _xrTornDown;

    void Awake()
    {
        Application.wantsToQuit += OnWantsToQuit;   // catches the window X too
        DontDestroyOnLoad(gameObject);
    }

    bool OnWantsToQuit()
    {
        if (_xrTornDown) return true;               // second pass: really quit
        StartCoroutine(TearDownXrThenQuit());
        return false;                               // block the first attempt
    }

    IEnumerator TearDownXrThenQuit()
    {
        var mgr = XRGeneralSettings.Instance?.Manager;
        if (mgr != null && mgr.isInitializationComplete)
        {
            mgr.StopSubsystems();
            yield return null;                      // let a frame pass
            mgr.DeinitializeLoader();
            yield return null;
        }
        _xrTornDown = true;
        Application.Quit();
    }
}