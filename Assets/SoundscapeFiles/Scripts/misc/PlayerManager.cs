using static SteamAudio.SteamAudioManager;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

//Class to manage both the VR and the flat players.
public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject vrCamera;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Camera vrUICamera;
    [SerializeField] private Camera playerUICamera;
    [SerializeField] private Canvas canvas;
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
    //what to mask when placing the audio
    [SerializeField] private LayerMask placingAudioMask = new();

    public void Start()
    {
        #if !UNITY_ANDROID || UNITY_EDITOR
        if(vrStateData.IsInVr) EnterVR();
        EnsureVrState(vrStateData.IsInVr);
        #else
        EnterVR();
        #endif
        //else ExitVR();
    }
    private void EnsureVrState(bool isInVr)
    {
        if (isInVr)
        {
            playerCamera.SetActive(false);
            vrCamera.SetActive(true);
            crosshairCanvas.SetActive(false);
            canvas.worldCamera = vrUICamera;
        } else
        {
            playerCamera.SetActive(true);
            vrCamera.SetActive(false);
            crosshairCanvas.SetActive(true);
            canvas.worldCamera = playerUICamera;
        }
    }
    public void EnterVR()
    {
        EnsureVrState(true);
        #if !UNITY_ANDROID || UNITY_EDITOR
        StartCoroutine(VRSwitcher.EnableXRCoroutine());
        #endif
        vrStateData.IsInVr = true;
        NotifyAudioListenerChanged();
        #if UNITY_EDITOR
        EditorUtility.SetDirty(vrStateData);
        #endif
    }

    public void ExitVR()
    {
        EnsureVrState(false);
        VRSwitcher.DisableXR();
        //FixOptions();
        vrStateData.IsInVr = false;
        NotifyAudioListenerChanged();
        #if UNITY_EDITOR
        EditorUtility.SetDirty(vrStateData);
        #endif
    }

    public void JustStopPlayer(bool canMove)
    {
        if(player != null) player.SetPlayerMovement(canMove);
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

    public const int HIDE_ALL = 0; 
    public const int HIDE_EDITING = 1; 
    public const int HIDE_PLACING = 2; 

    private int GetCameraMask(int type)
    {
        return type switch
        {
            HIDE_ALL => cameraHideMask,
            HIDE_EDITING => debugCameraHideMask,
            HIDE_PLACING => placingAudioMask,
            _ => cameraHideMask
        };
    }

    public void ShowCameraLayers(int mask = HIDE_ALL)
    {
        //show stuff
        Camera.main.cullingMask |= GetCameraMask(mask);
    }
    public void HideCameraLayers(int type)
    {
        //hide stuff
        Camera.main.cullingMask &= ~GetCameraMask(type);

    }
}
