using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; //Call needs it
/*
public class StopVideo360 : MonoBehaviour
{
    public PlayerManager player;
    [SerializeField] private InputActionReference inputAction;

    void endVideo(InputAction.CallbackContext context)
    {
        Debug.Log("Stopping Video");
        //Change scene skybox and reactivate the scene
        SceneSkybox.Instance.reactivateRoom();

        player.SetPlayerMovement(true);
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
*/