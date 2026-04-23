using UnityEngine;
using UnityEngine.InputSystem;

public class SoundscapeWrapper: MonoBehaviour
{
    public GameObject ssVisual;
    public Soundscape currentSoundscape;
    public PlayerManager playerManager;
    public VrStateData vrStateData;
    private bool videoPlaying = false;
    [SerializeField] private InputActionReference inputAction;


    void OnEnable()
    {
        // Hook into the global event
        inputAction.action.performed += StopSoundscape;
    }

    void OnDisable()
    {
        inputAction.action.performed -= StopSoundscape;
    }

    public void SetSoundscape(Soundscape ss)
    {currentSoundscape = ss.Copy();}
    

    public void StartSoundscape()
    {
        playerManager.SetPlayerMovement(false);
        playerManager.HideCameraLayers();
        
        ssVisual = currentSoundscape.TryGetVisuals(vrStateData.GetCurrentVisuals());
        if(ssVisual != null)
        {
            videoPlaying = true;
        }
    }

    private void StopSoundscape(InputAction.CallbackContext context){StopSoundscape();}

    public void StopSoundscape()
    {
        playerManager.SetPlayerMovement(true);
        playerManager.ShowCameraLayers();
        if (videoPlaying)
        {
            if(ssVisual != null) 
            {
                Destroy(ssVisual);
                ssVisual = null;
            }
            videoPlaying = false;
        }
    }
    public void SetAndStartSoundscape(Soundscape ss)
    {
        SetSoundscape(ss);
        StartSoundscape();
    }

    
}
