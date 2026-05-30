using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EditScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject aimingSphere;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private SoundscapePlayerEditor editorPlayer;
    [SerializeField] private SoundscapeEditor ssEditor;
    [SerializeField] private VisualTreeAsset libraryItemTemplate;
    [SerializeField] private VisualTreeAsset ssButtonTemplate;
    
    private Button returnBtn;
    private VisualElement soundscapeList;
    private VisualElement libraryList;
    private ArrayList ssButtons = new();
    private ArrayList libraryButtons = new();
    private Label ssLabel;
    private Label audioLabel;

    void OnEnable()
    {
        //TODO: separate SS list logic so we dont have to remake this ui everytime a SS is changed/added.
        //bonus points if it only regenerates when one is added.
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        soundscapeList = root.Q<VisualElement>("soundscapeList");
        libraryList = root.Q<VisualElement>("libraryList");
        returnBtn = root.Q<Button>("returnBtn");
        ssLabel = root.Q<Label>("selectedSoundscape");
        audioLabel = root.Q<Label>("selectedAudio");

        ssEditor = new SoundscapeEditor(root.Q<Foldout>("settingsFoldout"), ssWrapper, library)
            .RunOnCreate(OnCreateSoundscape)
            .RunOnDestroy(OnDeleteSoundscape)
            .RunOnSave(OnSaveSoundscape);

        returnBtn.clicked += Return;

        var videos = library.soundscapeList.GetSoundscapes();
        List<Audio> sounds = library.audioList.GetAudios().ToList();

        
        foreach(Soundscape soundscape in videos)
            AddSoundscape(soundscape);

        foreach(Audio audio in sounds)
        {
            //create button
            Action action = () => SelectAudio(audio);
            VisualElement vElement = libraryItemTemplate.Instantiate();
            Button button = vElement.Q<Button>("button");
            Label txt = vElement.Q<Label>("btnLabel");
            Image icon = vElement.Q<Image>("btnImage");

            //wrap button in handler to later allow to free memory from binding this action
            ButtonHandler handler = new(button);
            handler.SetAction(action);
            libraryButtons.Add(handler);

            txt.text = audio.name;
            icon.image = library.GetIconForAudio(audio.id);
            libraryList.Add(vElement);
        }
    }

    private void AddSoundscape(Soundscape soundscape)
    {
        //create button
        Action action = () => SetSoundscape(soundscape);
        VisualElement vElement = ssButtonTemplate.Instantiate();
        vElement.name = soundscape.title;
        Button button = vElement.Q<Button>("button");
        button.text = soundscape.title;

        //wrap button in handler to later allow to free memory from binding this action
        ButtonHandler handler = new(button, soundscape.title);
        handler.SetAction(action);

        //add button to the UI
        ssButtons.Add(handler);
        soundscapeList.Add(vElement);
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

    private void OnCreateSoundscape(Soundscape soundscape)
    {
        AddSoundscape(soundscape);
        SetSoundscape(soundscape);
    }
    private void OnDeleteSoundscape(string title)
    {
        foreach(ButtonHandler handler in ssButtons)
        {
            if(handler.GetId().Equals(title)) handler.Clear();
        }
        soundscapeList.Q(title).RemoveFromHierarchy();
    }
    
    private void OnSaveSoundscape(string oldTitle, Soundscape soundscape)
    {
        foreach(ButtonHandler handler in ssButtons)
        {
            if (handler.GetId().Equals(oldTitle))
            {
                handler.SetId(soundscape.title);
                handler.SetAction(() => SetSoundscape(soundscape));
            } 
        }
    }
    

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
        ssWrapper.StopSoundscape();
        editorPlayer.LeaveEdit();
    }
    private void SetSoundscape(Soundscape soundscape)
    {
        ssWrapper.SetAndStartSoundscape(soundscape, true);
        ssLabel.text = $"Selected Soundscape: {soundscape.title}";
        audioLabel.text = "";
        Debug.Log("Selected soundscape: " + soundscape.title);
        ssEditor.SetData(soundscape);

    }

    private void SelectAudio(Audio audio)
    {
        if(!editorPlayer.SelectAudio(audio)) return;
        audioLabel.text = $"Selected Audio: {audio.name}";
        Debug.Log("Selected audio: " + audio.id);
    }

    public void UnselectAudio()
    {
        audioLabel.text = "";
    }
}
