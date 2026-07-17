using System;
using UnityEngine.UIElements;
using UnityEngine;

public class SoundscapeEditor
{
    private SoundscapeWrapper ssWrapper;
    private SoundscapeLibrary library;
    private TextField titleField;
    private IntegerField timeField;
    private Toggle videoToggle;
    private EventCallback<ChangeEvent<bool>> videoToggleCallback;
    private Toggle loopToggle;
    private TextField staticPath;
    private TextField encompassingPath;
    private Label debugLabel;
    private Button createButton;
    private Button destroyButton;
    private DropdownField projectionDropdown;
    private int deleteCounter = 0;
    private const int deleteThreshold = 1;
    private Action<Soundscape> runOnCreate = null;
    private Action<string> runOnDestroy = null;
    private Soundscape currentSelectedSoundscape = null;
    public SoundscapeEditor(VisualElement foldout, SoundscapeWrapper ssWrapper, SoundscapeLibrary library)
    {   
        this.ssWrapper = ssWrapper;
        this.library = library;

        titleField = foldout.Q<TextField>("titleInput");
        timeField = foldout.Q<IntegerField>("timeInput");
        videoToggle = foldout.Q<Toggle>("videoToggle");
        loopToggle = foldout.Q<Toggle>("loopToggle");
        staticPath = foldout.Q<TextField>("2dPath");
        encompassingPath = foldout.Q<TextField>("360Path");
        debugLabel = foldout.Q<Label>("debugLabel");
        createButton = foldout.Q<Button>("createBtn");
        destroyButton = foldout.Q<Button>("deleteBtn");
        projectionDropdown = foldout.Q<DropdownField>("projectionDropdown");
        
        if(createButton != null) createButton.clicked += OnCreate;
        if(destroyButton != null) destroyButton.clicked += OnDestroy;

        prepareVideoUIFields();
    }

    public SoundscapeEditor RunOnCreate(Action<Soundscape> action)
    {
        runOnCreate = action;
        return this;
    }

    public SoundscapeEditor RunOnDestroy(Action<string> action)
    {
        runOnDestroy = action;
        return this;
    }

    public void UnbindButtons()
    {
        if(createButton != null) createButton.clicked -= OnCreate;
        if(destroyButton != null) destroyButton.clicked -= OnDestroy;
    }

    public void SetData(Soundscape soundscape)
    {
        titleField.value = soundscape.title;
        timeField.value = soundscape.timeSeconds;
        videoToggle.value = soundscape is VideoSoundscape;
        loopToggle.value = soundscape.loop;
        staticPath.value = soundscape.staticPath;
        encompassingPath.value = soundscape.encompassingPath;
        projectionDropdown.index = projectionDropdown.choices.IndexOf(soundscape.projectionType);
        ResetDeleteCounter();
        debugLabel.text = "";
        currentSelectedSoundscape = soundscape;
    }

    public void ClearFields(bool clearLabel = false)
    {
        titleField.value = "";
        timeField.value = 90;
        videoToggle.value = false;
        loopToggle.value = false;
        staticPath.value = "";
        encompassingPath.value = "";
        projectionDropdown.index = 0;
        ResetDeleteCounter();
        if(clearLabel) debugLabel.text = "";
        currentSelectedSoundscape = null;
    }

    private void OnCreate()
    {
        if(string.IsNullOrWhiteSpace(titleField.value))
        {
            debugLabel.text = "No title defined.";
            return;
        }
        if (library.DoesSoundscapeExist(titleField.value))
        {
            debugLabel.text = $"Soundscape {titleField.value} already exists. Can't create.";
            return;
        }

        Debug.Log($"[SoundscapeVR] Created soundscape {titleField.value}");
        debugLabel.text = $"Created soundscape {titleField.value}";
        Soundscape newSS = Soundscape.CreateSoundscape(titleField.value, timeField.value, staticPath.value, encompassingPath.value, projectionDropdown.choices[projectionDropdown.index], loopToggle.value, videoToggle.value);
        library.ReplaceOrAddSoundscape(newSS);
        //ssWrapper.SetAndStartSoundscape(newSS);
        //ssWrapper.MarkEdited();
        if(runOnCreate != null) runOnCreate.Invoke(newSS);
        
    }

    void prepareVideoUIFields()
    {
        // Store it in the field first, then register
        videoToggleCallback = evt =>
        {
            if (evt.newValue)
            {
                staticPath.label = "2D Video Location";
                encompassingPath.label = "360 Video Location";
            }
            else
            {
                staticPath.label = "2D Image Location";
                encompassingPath.label = "360 Image Location";
            }
        };

        videoToggle.RegisterValueChangedCallback(videoToggleCallback);
        staticPath.label = "2D Image Location";
        encompassingPath.label = "360 Image Location";
    }

    public void unresgisterVideoToggleEvents()
    {
        videoToggle?.UnregisterValueChangedCallback(videoToggleCallback);
    }

    /// <summary>
    /// Collect SS Settings UI values and return them in an empty Soundscape instance.
    /// </summary>
    /// <returns>Soundscape with no audio sources, but all the UI values</returns>
    public Soundscape CollectUiValues()
    {
        //inform user on the SS label that it was saved.
        //also save the SS as a whole.

        //1. collect UI values
        //2. tell wrapper to save.

        Soundscape currentSoundscape = ssWrapper.GetSoundscape();
        string newTitle = titleField.value;
        if(string.IsNullOrWhiteSpace(newTitle))
        {
            debugLabel.text = "No title defined.";
            return null;
        }
        if(!newTitle.Equals(currentSoundscape.title) && library.DoesSoundscapeExist(newTitle))
        {
            debugLabel.text = "New title already exists.";
            return null;
        }
        debugLabel.text = $"Soundscape {currentSoundscape.title} saved.";
        Soundscape newSS = Soundscape.CreateSoundscape(newTitle, timeField.value, staticPath.value, encompassingPath.value, projectionDropdown.choices[projectionDropdown.index], loopToggle.value, videoToggle.value);
        return newSS;
    }
    private void OnDestroy()
    {
        /*if (!library.DoesSoundscapeExist(titleField.value))
        {
            debugLabel.text = $"Soundscape {titleField.value} doesn't exist in library";
            return;
        }*/
        string ssTitle = currentSelectedSoundscape.title;
        if(deleteCounter++ >= deleteThreshold)
        {
            ResetDeleteCounter();
            ssWrapper.StopSoundscape(ssTitle);
            if (library.DeleteSoundscape(ssTitle))
                debugLabel.text = $"Soundscape {ssTitle} deleted successfully.";
            else 
                debugLabel.text = $"Soundscape {ssTitle} failed deletion.";
            currentSelectedSoundscape = null;
            if(runOnDestroy != null) runOnDestroy.Invoke(titleField.value);
            return;
        }
        destroyButton.text = $"Delete {ssTitle}?";
    }


    private void ResetDeleteCounter()
    {
        deleteCounter = 0;
        if(destroyButton != null) destroyButton.text = "Delete";
    }
}