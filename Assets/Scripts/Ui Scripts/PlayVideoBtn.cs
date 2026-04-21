using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Mathematics;
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
    private Button returnBtn;
    public VisualTreeAsset buttonTemplate; 
    private ScrollView scrollView;
    private ArrayList buttons = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        scrollView = root.Q<ScrollView>("scrollList");
        returnBtn = root.Q<Button>("returnBtn");
        var videos = new List<TestSoundscape> 
        {
            new("Rickroll", rickrollVideo360), 
            new("Falling", fallingVideo360), 
            new("Chernobyl", chernobylVideo360), 
            new("Big Bunny Hole", bunnyVideo360)
        };

        foreach(TestSoundscape soundscape in videos)
        {
            System.Action action = () => PlayVideo(soundscape.video360);
            VisualElement vElement = buttonTemplate.Instantiate();
            Button button = vElement.Q<Button>("button");

            button.text = soundscape.title;
            ButtonHandler handler = new(button);
            handler.SetAction(action);
            buttons.Add(handler);
            scrollView.Add(vElement);
        }

        returnBtn.clicked += Return;
    }

    void OnDisable()
    {
        returnBtn.clicked -= Return;
        foreach(ButtonHandler handler in buttons)
        {
            handler.Clear();
        }
    }

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    private void PlayVideo(GameObject video360)
    {
        if(player != null && player.IsDisabled()) return;
        Camera.main.cullingMask &= ~(1 << RoomLayerIndex);

        if(player != null)
            player.ActivatePlayer(false);

        video360.GetComponent<StopVideo360>().player = player;
        Instantiate(video360);
    }
}
