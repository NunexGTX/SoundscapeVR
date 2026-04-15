using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; //Call needs it

public class StopVideo360 : MonoBehaviour
{
    public PlayerControl player;
    [SerializeField] private InputActionReference inputAction;

    void endVideo(InputAction.CallbackContext context)
    {
        Debug.Log("Stopping Video");
        //Change scene skybox and reactivate the scene
        SceneSkybox.Instance.reactivateRoom();

        if(player != null)
            player.ActivatePlayer(true);
        //Destroy the video player itself
        Destroy(gameObject);

    }
    

    void OnEnable()
    {
        // Hook into the global event
        inputAction.action.performed += endVideo;
    }

    void OnDisable()
    {
        inputAction.action.performed -= endVideo;
    }
}
