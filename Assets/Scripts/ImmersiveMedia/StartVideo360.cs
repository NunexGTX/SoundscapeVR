using UnityEngine;
using UnityEngine.Video;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject video360;
    public int RoomLayerIndex = 6; //Room layer
    public PlayerControl player;

    public void start360Video()
    {
        if(player != null && player.IsDisabled()) return;
        //Deselect room from culling (not render)
        Camera.main.cullingMask &= ~(1 << RoomLayerIndex);

        //lock non vr player position (not implemented)
        lockScreenPlayer();

        //Start video playback
        video360.GetComponent<StopVideo360>().player = player;
        Instantiate(video360);
    }
    
    //Locks the non vr player in place
    void lockScreenPlayer()
    {
        if(player != null)
            player.ActivatePlayer(false);
    }
}
