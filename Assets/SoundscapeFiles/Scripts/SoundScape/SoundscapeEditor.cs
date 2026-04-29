using System;
using Unity.Tutorials.Core.Editor;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;

public class SoundscapeEditor
{
    private SoundscapeWrapper ssWrapper;
    private SoundscapeLibrary library;
    private TextField titleField;
    private IntegerField timeField;
    private Toggle videoToggle;
    private TextField staticPath;
    private TextField encompassingPath;
    private Label debugLabel;
    private Button createButton;
    private Button destroyButton;
    private int deleteCounter = 0;
    private const int deleteThreshold = 1;
    private Action<Soundscape> runOnCreate = null;
    private Action<string> runOnDestroy = null;
    public SoundscapeEditor(VisualElement foldout, SoundscapeWrapper ssWrapper, SoundscapeLibrary library)
    {   
        this.ssWrapper = ssWrapper;
        this.library = library;

        titleField = foldout.Q<TextField>("titleInput");
        timeField = foldout.Q<IntegerField>("timeInput");
        videoToggle = foldout.Q<Toggle>("videoToggle");
        staticPath = foldout.Q<TextField>("2dPath");
        encompassingPath = foldout.Q<TextField>("360Path");
        debugLabel = foldout.Q<Label>("debugLabel");
        createButton = foldout.Q<Button>("createBtn");
        destroyButton = foldout.Q<Button>("deleteBtn");
        
        createButton.clicked += OnCreate;
        destroyButton.clicked += OnDestroy;

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
        createButton.clicked -= OnCreate;
        destroyButton.clicked -= OnDestroy;
    }

    public void SetData(Soundscape soundscape)
    {
        titleField.value = soundscape.title;
        timeField.value = soundscape.timeSeconds;
        videoToggle.value = soundscape is VideoSoundscape;
        staticPath.value = soundscape.staticPath;
        encompassingPath.value = soundscape.encompassingPath;
        ResetDeleteCounter();
        debugLabel.text = "";
    }

    private void OnCreate()
    {
        if(titleField.value.IsNullOrEmpty())
        {
            debugLabel.text = "No title defined.";
            return;
        }
        if (library.DoesSoundscapeExist(titleField.value))
        {
            debugLabel.text = "Soundscape already exists. Can't create.";
            return;
        }
        debugLabel.text = "";
        Soundscape newSS = Soundscape.CreateSoundscape(titleField.value, timeField.value, staticPath.value, encompassingPath.value, videoToggle.value);
        ssWrapper.SetSoundscape(newSS);
        ssWrapper.MarkEdited();
        if(runOnCreate != null) runOnCreate.Invoke(newSS);
    }
    private void OnDestroy()
    {
        if (!library.DoesSoundscapeExist(titleField.value))
        {
            debugLabel.text = "Soundscape title doesn't exist in library";
            return;
        }
        if(deleteCounter++ >= deleteThreshold)
        {
            ResetDeleteCounter();
            ssWrapper.StopSoundscape(titleField.value);
            library.DeleteSoundscape(titleField.value);
            debugLabel.text = "";
            if(runOnDestroy != null) runOnDestroy.Invoke(titleField.value);
            return;
        }
        destroyButton.text = "Are you sure?";
    }

    private void ResetDeleteCounter()
    {
        deleteCounter = 0;
        destroyButton.text = "Delete";
    }
}