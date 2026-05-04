using System.Collections.Generic;
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
    [SerializeField] private LayerMask cameraHideMask = new();
    //what to mask if debugging
    [SerializeField] private LayerMask debugCameraHideMask = new();

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
        #if UNITY_EDITOR
        EditorUtility.SetDirty(vrStateData);
        #endif
    }

    public void ExitVR()
    {
        playerCamera.SetActive(true);
        vrCamera.SetActive(false);
        crosshairCanvas.SetActive(true);
        VRSwitcher.DisableXR();
        //FixOptions();
        vrStateData.IsInVr = false;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(vrStateData);
        #endif
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

    public void JustStopPlayer(bool canMove)
    {
        if(player != null) player.ActivatePlayer(canMove);
        if (moveProvider != null) moveProvider.enabled = canMove;
        if (teleportProvider != null) teleportProvider.enabled = canMove;
        if (vrGravity != null) vrGravity.enabled = canMove;
    }
    
    public void SetPlayerMovement(bool canMove)
    {
        JustStopPlayer(canMove);
        if (canMove)
        {
            
        } else
        {
            Debug.Log("Setting orientation and position...");
            Vector3 position = new(0, 0, 0);
            player.transform.position = position;
            vrPlayer.transform.position = position;
            player.transform.rotation = Quaternion.Euler(0, 90, 0);
            vrPlayer.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
    }

    public Camera GetCurrentCamera()
    {
        if(vrStateData.IsInVr) return vrCamera.GetComponent<Camera>();
        return playerCamera.GetComponent<Camera>();
    }

    private int GetCameraMask(bool debug)
    {
        if (debug)
        {
            return debugCameraHideMask;
        } 
        return cameraHideMask;
    }

    public void ShowCameraLayers()
    {
        //show stuff
        Camera.main.cullingMask |= GetCameraMask(false);
    }
    public void HideCameraLayers(bool debug)
    {
        //hide stuff
        Camera.main.cullingMask &= ~GetCameraMask(debug);

    }
}
