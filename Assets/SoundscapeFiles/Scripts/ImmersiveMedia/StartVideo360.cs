using UnityEngine;
using UnityEngine.Video;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject video360;
    public int RoomLayerIndex = 6; //Room layer
    //deixa null em vr
    //em flat screen, não importa trancar o jogador
    public PlayerControl player;

    public void start360Video()
    {
        if(player != null && player.IsDisabled()) return;
        //Deselect room from culling (not render)
        Camera.main.cullingMask &= ~(1 << RoomLayerIndex);

        //lock non vr player position (not implemented)
        lockScreenPlayer();

        //Start video playback
        Instantiate(video360);
    }
    
    //Locks the non vr player in place
    void lockScreenPlayer()
    {
        if(player != null)
            player.ActivatePlayer(false);
    }
}
