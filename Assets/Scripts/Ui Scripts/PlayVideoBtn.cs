using UnityEngine;
using UnityEngine.UIElements;

public class PlayVideoBtn : MonoBehaviour
{
    public GameObject rickrollVideo360;
    public GameObject fallingVideo360;
    public GameObject chernobylVideo360;
    public GameObject bunnyVideo360;
    public int RoomLayerIndex = 6; //Room layer
    public PlayerControl player;
    [SerializeField] private GameObject mainMenu;
    private Button rickrollBtn;
    private Button fallingBtn;
    private Button chernobylBtn;
    private Button bunnyBtn;
    private Button returnBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        rickrollBtn = root.Q<Button>("rickrollBtn"); 
        fallingBtn = root.Q<Button>("fallingBtn"); 
        chernobylBtn = root.Q<Button>("chernobylBtn"); 
        bunnyBtn = root.Q<Button>("bunnyBtn"); 
        returnBtn = root.Q<Button>("returnBtn");

        rickrollBtn.clicked += PlayRickRoll;
        fallingBtn.clicked += PlayFalling;
        chernobylBtn.clicked += PlayChernobyl;
        bunnyBtn.clicked += PlayBunny;
        returnBtn.clicked += Return;
    }

    void OnDisable()
    {
        rickrollBtn.clicked -= PlayRickRoll;
        fallingBtn.clicked -= PlayFalling;
        chernobylBtn.clicked -= PlayChernobyl;
        bunnyBtn.clicked -= PlayBunny;
        returnBtn.clicked -= Return;
    }

    private void PlayRickRoll()
    {
        start360Video(rickrollVideo360);
    }
    private void PlayFalling()
    {
        start360Video(fallingVideo360);
    }
    private void PlayChernobyl()
    {
        start360Video(chernobylVideo360);
    }
    private void PlayBunny()
    {
        start360Video(bunnyVideo360);
    }

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    private void start360Video(GameObject video)
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
