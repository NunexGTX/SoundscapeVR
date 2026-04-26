using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundscapeScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
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
        var videos = library.soundscapeList.GetSoundscapes();

        foreach(Soundscape soundscape in videos)
        {
            //create button
            System.Action action = () => PlayVideo(soundscape);
            VisualElement vElement = buttonTemplate.Instantiate();
            Button button = vElement.Q<Button>("button");
            button.text = soundscape.GetTitle();
            
            //wrap button in handler to later allow to free memory from binding this action
            ButtonHandler handler = new(button);
            handler.SetAction(action);

            //add button to the UI
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
    
    private void PlayVideo(Soundscape soundscape)
    {
        ssWrapper.SetAndStartSoundscape(soundscape);
    }

}
