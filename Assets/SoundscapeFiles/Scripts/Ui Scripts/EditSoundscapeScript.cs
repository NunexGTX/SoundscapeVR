using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EditSoundscapeScript : MonoBehaviour
{
    [SerializeField] private GameObject previousMenu;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private SoundscapePlayerEditor editorPlayer;
    [SerializeField] private SoundscapeEditor ssEditor;
    [SerializeField] private VisualTreeAsset libraryItemTemplate;
    [SerializeField] private VisualTreeAsset ssButtonTemplate;
    
    private Button returnBtn;
    private Button saveBtn;
    private VisualElement libraryList;
    private ArrayList libraryButtons = new();
    private Label audioLabel;
    public Soundscape selectedSoundscape = null;

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        libraryList = root.Q<VisualElement>("libraryList");
        returnBtn = root.Q<Button>("returnBtn");
        saveBtn = root.Q<Button>("saveBtn");
        audioLabel = root.Q<Label>("selectedAudio");

        ssEditor = new SoundscapeEditor(root.Q<Foldout>("settingsFoldout"), ssWrapper, library)
            .RunOnDestroy(OnDeleteSoundscape);
        ssEditor.SetData(selectedSoundscape);

        returnBtn.clicked += Return;
        saveBtn.clicked += OnSave;

        List<Audio> sounds = library.audioList.GetAudios().ToList();
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
    

    public void Return()
    {
        previousMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        returnBtn.clicked -= Return;
        foreach(ButtonHandler handler in libraryButtons)
            handler.Clear();
        ssWrapper.StopSoundscape();
        editorPlayer.LeaveEdit();
    }

    private void OnDeleteSoundscape(string title)
    {
        previousMenu.SetActive(true);
        gameObject.SetActive(false);
    }
    
    private void OnSave()
    {
        Soundscape newSoundscape = ssEditor.CollectUiValues();
        if(newSoundscape == null) return;
        editorPlayer.CancelState();
        string oldTitle = selectedSoundscape.title;
        newSoundscape.CopySourcesFrom(ssWrapper.GetSoundscape());
        library.UpdateSoundscapeData(oldTitle, newSoundscape);
        ssWrapper.SetAndStartSoundscape(newSoundscape, debug: true);
    }

    private void SelectAudio(Audio audio)
    {
        if(!editorPlayer.SelectAudio(audio)) return;
        audioLabel.text = $"Selected Audio: {audio.name}";
        Debug.Log("Selected audio: " + audio.id);
    }

    public void UnselectAudio()
    {
        if(audioLabel != null) audioLabel.text = "";
    }
}
