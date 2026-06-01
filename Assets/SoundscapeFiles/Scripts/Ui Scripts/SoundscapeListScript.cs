using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundscapeListScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private GameObject aimingSphere;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private SoundscapePlayerEditor editorPlayer;
    [SerializeField] private SoundscapeEditor ssEditor;

    [SerializeField] private VisualTreeAsset ssButtonTemplate;
    
    private Button returnBtn;
    private ListView soundscapeListView; // Changed from VisualElement to ListView
    private List<Soundscape> soundscapesSource; // Source data list

    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        returnBtn = root.Q<Button>("returnBtn");
        returnBtn.clicked += Return;

        ssEditor = new SoundscapeEditor(root.Q<Foldout>("settingsFoldout"), ssWrapper, library)
            .RunOnCreate(OnCreateSoundscape);

        soundscapesSource = new(library.soundscapeList.GetSoundscapes());
        soundscapeListView = root.Q<ListView>("soundscapeList");
        SetupListView();
    }

    private void SetupListView()
    {
        if (soundscapeListView == null) return;

        soundscapeListView.itemsSource = soundscapesSource;
        soundscapeListView.makeItem = () => ssButtonTemplate.Instantiate();
        soundscapeListView.bindItem = (VisualElement element, int index) =>
        {
            Soundscape soundscape = soundscapesSource[index];
            Button button = element.Q<Button>("button");
            
            if (button != null)
            {
                button.text = soundscape.title;
                
                // Clear any previous click listeners to prevent duplicate triggers when recycled
                button.clicked -= button.userData as Action; 
                
                // Create and assign the new action
                Action clickAction = () => SelectSoundscape(soundscape);
                button.clicked += clickAction;
                button.userData = clickAction; // Store action reference on the element to unbind safely later
            }
        };

        // Optional UI Toolkit optimization setup
        soundscapeListView.selectionType = SelectionType.None; // Using inner button click instead of list selection
        soundscapeListView.fixedItemHeight = 45; // Recommended performance boost if all items are uniform height
    }
    
    void OnDisable()
    {
        if (returnBtn != null)
            returnBtn.clicked -= Return;
    }

    private void OnCreateSoundscape(Soundscape soundscape)
    {
        soundscapesSource = new(library.soundscapeList.GetSoundscapes());
        soundscapeListView.Rebuild();
        ClearFields();
        //TODO show label here
    }

    private void SelectSoundscape(Soundscape soundscape)
    {
        editMenu.GetComponent<EditSoundscapeScript>().selectedSoundscape = soundscape;
        ssWrapper.SetAndStartSoundscape(soundscape, debug: true);
        gameObject.SetActive(false);
        editMenu.SetActive(true);
    }
    
    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
        ssWrapper.StopSoundscape();
        editorPlayer.LeaveEdit();
    }

    private void ClearFields()
    {
        ssEditor.ClearFields();
    }
}