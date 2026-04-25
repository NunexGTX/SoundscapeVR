using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class EditUi : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject aimingSphere;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private SoundscapePlayerEditor editorPlayer;
    [SerializeField] private VisualTreeAsset libraryItemTemplate;
    [SerializeField] private VisualTreeAsset ssButtonTemplate;
    
    private Button returnBtn;
    private ScrollView soundscapeList;
    private ScrollView libraryList;
    private ArrayList ssButtons = new();
    private ArrayList libraryButtons = new();

    void OnEnable()
    {
        //TODO: separate SS list logic so we dont have to remake this ui everytime a SS is changed/added.
        //bonus points if it only regenerates when one is added.
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        soundscapeList = root.Q<ScrollView>("soundscapeList");
        libraryList = root.Q<ScrollView>("libraryList");
        returnBtn = root.Q<Button>("returnBtn");
        returnBtn.clicked += Return;

        var videos = library.soundscapeList.GetSoundscapes();
        List<Audio> sounds = library.audioList.GetAudios().ToList();

        
        foreach(Soundscape soundscape in videos)
        {
            //create button
            System.Action action = () => SetSoundscape(soundscape);
            VisualElement vElement = ssButtonTemplate.Instantiate();
            Button button = vElement.Q<Button>("button");
            button.text = soundscape.GetTitle();
            
            //wrap button in handler to later allow to free memory from binding this action
            ButtonHandler handler = new(button);
            handler.SetAction(action);

            //add button to the UI
            ssButtons.Add(handler);
            soundscapeList.Add(vElement);
        }

        foreach(Audio audio in sounds)
        {
            //create button
            System.Action action = () => SelectAudio(audio);
            VisualElement vElement = libraryItemTemplate.Instantiate();
            Button button = vElement.Q<Button>("button");
            Label txt = vElement.Q<Label>("btnLabel");
            Image icon = vElement.Q<Image>("btnImage");

            //wrap button in handler to later allow to free memory from binding this action
            ButtonHandler handler = new(button);
            handler.SetAction(action);
            libraryButtons.Add(handler);

            txt.text = audio.name;
            if(audio.HasImage()) icon.image = audio.GetImage();
            libraryList.Add(vElement);
        }
    }
    
    void OnDisable()
    {
        returnBtn.clicked -= Return;
        foreach(ButtonHandler handler in ssButtons)
        {
            handler.Clear();
        }
        foreach(ButtonHandler handler in libraryButtons)
        {
            handler.Clear();
        }
        if(aimingSphere != null && aimingSphere.activeSelf) aimingSphere.SetActive(false);
    }

    

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
        ssWrapper.SaveCurrentSoundscape();
        ssWrapper.StopSoundscape();
        editorPlayer.LeaveEdit();
    }
    private void SetSoundscape(Soundscape soundscape)
    {
        ssWrapper.SetAndStartSoundscape(soundscape, true);
        Debug.Log("Selected soundscape: " + soundscape.title);
    }

    private void SelectAudio(Audio audio)
    {
        editorPlayer.SelectAudio(audio);
        Debug.Log("Selected audio: " + audio.id);
    }
}
