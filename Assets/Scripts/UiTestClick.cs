using UnityEngine;
using UnityEngine.UIElements;

public class UiTestClick : MonoBehaviour
{
    public GameObject rickrollVideo360;
    public GameObject falingVideo360;
    public int RoomLayerIndex = 6; //Room layer
    public PlayerControl player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button rickrollBtn = root.Q<Button>("rickrollBtn"); 
        Button fallingBtn = root.Q<Button>("fallingBtn"); 
        
        rickrollBtn.clicked += () => start360Video(rickrollVideo360);
        fallingBtn.clicked += () => start360Video(falingVideo360);
    }


    public void start360Video(GameObject video)
    {
        if(player != null && player.IsDisabled()) return;
        //Deselect room from culling (not render)
        Camera.main.cullingMask &= ~(1 << RoomLayerIndex);

        //lock non vr player position (not implemented)
        lockScreenPlayer();

        //Start video playback
        video.GetComponent<StopVideo360>().player = player;
        Instantiate(video);
    }
    
    //Locks the non vr player in place
    void lockScreenPlayer()
    {
        if(player != null)
            player.ActivatePlayer(false);
    }
}
