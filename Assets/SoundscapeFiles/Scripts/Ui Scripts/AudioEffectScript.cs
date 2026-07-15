using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioEffectScript: MonoBehaviour
{
    [SerializeField] public SoundscapePlayerEditor editor = null;
    private Button saveButton, moveButton, pasteButton, copyButton;
    private VisualElement root = null;
    private AudioSource.AudioData audioData = null;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        if(root == null) return;
        saveButton = root.Q<Button>("saveBtn");
        saveButton.clicked += SaveData;
        copyButton = root.Q<Button>("copy");
        copyButton.clicked += CopyData;
        pasteButton = root.Q<Button>("paste");
        pasteButton.clicked += PasteData;
        moveButton = root.Q<Button>("move");
        moveButton.clicked += MoveAudio;
        if(editor == null) editor = GameObject.Find("PlayerManager").GetComponent<SoundscapePlayerEditor>();
        PopulateValues();
    }

    public void SetValues(AudioSource.AudioData data)
    {   
        audioData = data;
        PopulateValues();
    }

    private void PopulateValues()
    {
        if(root == null) return;
        string copiedId = editor.GetCopiedName();
        if(string.IsNullOrWhiteSpace(copiedId)) pasteButton.SetEnabled(false);
        else
        {
            pasteButton.SetEnabled(true);
            root.Q<Label>("copiedLabel").text = $"Data from: {copiedId}";
        }

        root.Q<Slider>("volume").value = audioData.volume;
        root.Q<SliderInt>("delay").value = audioData.delaySeconds;
        root.Q<Toggle>("loop").value = audioData.loop;
        root.Q<Toggle>("mute").value = audioData.mute;
        root.Q<Toggle>("visible").value = audioData.visible;
        root.Q<Label>("audioName").text = audioData.audioId;

        foreach(AudioEffectData data in AudioEffects.GetEffects())
            data.ApplyDefaultUI(root, SoundscapeLibrary.IsAudioAmbisonic(audioData.audioId));

        foreach(AudioEffectInstance effect in audioData.effectInstances)
            effect.effectData.ApplyValuesToUI(root);
    }

    void OnDisable()
    {
        saveButton.clicked -= SaveData;
        copyButton.clicked -= CopyData;
        pasteButton.clicked -= PasteData;
        moveButton.clicked -= MoveAudio;
    }

    private AudioSource.AudioData CollectUiValues()
    {
        float volume = root.Q<Slider>("volume").value;
        bool loop = root.Q<Toggle>("loop").value;
        bool mute = root.Q<Toggle>("mute").value;
        bool visible = root.Q<Toggle>("visible").value;
        int delay = root.Q<SliderInt>("delay").value;
        List<AudioEffectInstance> effects = new();

        foreach(AudioEffectData data in AudioEffects.GetEffects())
            if (data.IsEffectAppliedInUI(root))
                effects.Add(new(data.CollectFromUI(root, SoundscapeLibrary.GetAudioType(audioData.audioId).Equals(Audio.TYPE_AMBISONIC))));
        
        audioData = new(effects, volume, delay, loop, mute, visible);
        return audioData;
    }

    private void SaveData()
    {
        editor.UpdateSourceData(CollectUiValues());   
    }

    private void CopyData()
    {
        editor.CopyData(CollectUiValues());
        //set enabled for visual feedback
        pasteButton.SetEnabled(true);
        root.Q<Label>("copiedLabel").text = $"Copied data from: {editor.GetCopiedName()}";
    }

    private void PasteData()
    {
        SetValues(editor.PasteData());
        root.Q<Label>("copiedLabel").text = $"Pasted data from {editor.GetCopiedName()}";
    }

    private void MoveAudio()
    {
        editor.MoveAudioSource();
    }
}