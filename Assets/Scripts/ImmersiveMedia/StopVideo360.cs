using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; //Call needs it

public class StopVideo360 : MonoBehaviour
{
    private System.IDisposable _listener;
    public PlayerControl player;

    void endVideo()
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
        _listener = InputSystem.onAnyButtonPress
            .Call(ctrl =>
            {
                if(ctrl.name.ToLower().Contains("leftbutton") 
                    || ctrl.name.ToLower().Contains("trigger"))
                    endVideo();
                Debug.Log($"Button {ctrl.name} was pressed");
            } 
            );
    }

    void OnDisable()
    {
        // You MUST dispose the listener to prevent memory leaks
        _listener?.Dispose();
    }
}
