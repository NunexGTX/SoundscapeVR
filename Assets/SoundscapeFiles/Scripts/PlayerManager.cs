using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

//Class to manage both the VR and the flat players.
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject vrCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject crosshairCanvas;
    [SerializeField] private GameObject AudioVideoMenu;
    [SerializeField] private PlayerControl player;
    [SerializeField] private GameObject vrPlayer;
    [SerializeField] private  DynamicMoveProvider moveProvider;
    [SerializeField] private  TeleportationProvider teleportProvider;
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private CharacterController vrCollider;
    [SerializeField] private CharacterController flatCollider;
    [SerializeField] private GravityProvider vrGravity;
    [SerializeField] private List<int> cameraHideMask = new();

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
    
    public void SetPlayerMovement(bool canMove)
    {
        if(player != null) player.ActivatePlayer(canMove);
        if (moveProvider != null) moveProvider.enabled = canMove;
        if (teleportProvider != null) teleportProvider.enabled = canMove;
        if (vrGravity != null) vrGravity.enabled = canMove;

        if (canMove)
        {
            
        } else
        {
            Vector3 position = new(0, 0, 0);
            player.transform.position = position;
            vrPlayer.transform.position = position;

            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            player.transform.rotation = rotation;
            vrPlayer.transform.rotation = rotation;
        }
    }

    private int GetCameraMask()
    {
        int resMask = 0;
        foreach(int mask in cameraHideMask)
        {
            int layerMask = 1 << mask;
            resMask |= layerMask;
        }
        return resMask;
    }

    public void ShowCameraLayers()
    {
        //show stuff
        Camera.main.cullingMask |= GetCameraMask();
    }
    public void HideCameraLayers()
    {
        //hide stuff
        Camera.main.cullingMask &= ~GetCameraMask();

    }
}
