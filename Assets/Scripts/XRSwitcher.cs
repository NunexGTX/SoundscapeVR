using System;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class XRSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject vrCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject crosshairCanvas;
    [SerializeField] private GameObject AudioVideoMenu;
    public VrStateData vrStateData;

    public void Start()
    {
        if(vrStateData.IsInVr) EnterVR();
        else ExitVR();
    }
    public void EnterVR()
    {
        playerCamera.SetActive(false);
        vrCamera.SetActive(true);
        crosshairCanvas.SetActive(false);
        StartCoroutine(VRSwitcher.EnableXRCoroutine());
        vrStateData.IsInVr = true;
        EditorUtility.SetDirty(vrStateData);
    }

    public void ExitVR()
    {
        playerCamera.SetActive(true);
        vrCamera.SetActive(false);
        crosshairCanvas.SetActive(true);
        VRSwitcher.DisableXR();
        //FixOptions();
        vrStateData.IsInVr = false;
        EditorUtility.SetDirty(vrStateData);
    }

    void FixOptions()
    {
        VisualElement root = AudioVideoMenu.GetComponent<UIDocument>().rootVisualElement;
        RadioButtonGroup videoOptions = root.Q<RadioButtonGroup>("videoOptions");
        if (videoOptions.value == 1)
        {
            videoOptions.value = 2;
        }
    }
}
