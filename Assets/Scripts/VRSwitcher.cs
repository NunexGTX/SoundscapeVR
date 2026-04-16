using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

public static class VRSwitcher
{
    public static IEnumerator EnableXRCoroutine()
    {
        // Make sure the XR is disabled and properly disposed. It can happen that there is an activeLoader left
        // from the previous run.
        if(XRGeneralSettings.Instance.Manager.activeLoader || XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            DisableXR();
            yield return null;
        }

        // Enable XR
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if(!XRGeneralSettings.Instance.Manager.activeLoader || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            // Something went wrong, XR is not enabled
            Debug.LogError("Initializing XR Failed. Check Editor XR Settings.");
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        yield return null;
    }

    public static void DisableXR()
    {
        if(XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}