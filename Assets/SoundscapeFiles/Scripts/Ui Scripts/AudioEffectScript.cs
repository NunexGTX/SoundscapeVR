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

        root.Q<Foldout>("echoFoldout").value = false;
        root.Q<Toggle>("echoToggle").value = false;

        root.Q<Foldout>("occlusionFoldout").value = false;
        root.Q<Toggle>("occlusionToggle").value = false;

        root.Q<Slider>("volume").value = audioData.volume;
        root.Q<SliderInt>("delay").value = audioData.delaySeconds;
        root.Q<Toggle>("loop").value = audioData.loop;
        root.Q<Toggle>("mute").value = audioData.mute;
        root.Q<Toggle>("visible").value = audioData.visible;
        root.Q<Label>("audioName").text = audioData.audioId;

        foreach(AudioEffectInstance effect in audioData.effectInstances)
        {
            switch (effect.effectId)
            {
                case AudioEffectInstance.Echo:
                    root.Q<Foldout>("echoFoldout").value = true;
                    root.Q<Toggle>("echoToggle").value = true;
                    root.Q<SliderInt>("echoDelay").value = effect.echoEffectData.delay_ms;
                    root.Q<Slider>("echoDecay").value = effect.echoEffectData.decay_ratio;
                    root.Q<Slider>("echoDry").value = effect.echoEffectData.dry_mix;
                    root.Q<Slider>("echoWet").value = effect.echoEffectData.wet_mix;
                    break;
                case AudioEffectInstance.Occlusion:
                    root.Q<Foldout>("occlusionFoldout").value = true;
                    root.Q<Toggle>("occlusionToggle").value = true;
                    root.Q<Toggle>("freqToggle").value = effect.occlusionEffectData.frequency_dependent;
                    root.Q<Slider>("occlusionValue").value = effect.occlusionEffectData.occlusion_value;
                    root.Q<Slider>("lowFreqOcc").value = effect.occlusionEffectData.occlusion_low_freq;
                    root.Q<Slider>("midFreqOcc").value = effect.occlusionEffectData.occlusion_mid_freq;
                    root.Q<Slider>("highFreqOcc").value = effect.occlusionEffectData.occlusion_high_freq;
                    break;
            }
        }
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
        if (root.Q<Toggle>("echoToggle").value)
        {
            int echoDelay = root.Q<SliderInt>("echoDelay").value;
            float echoDecay = root.Q<Slider>("echoDecay").value;
            float echoDry = root.Q<Slider>("echoDry").value;
            float echoWet = root.Q<Slider>("echoWet").value;
            effects.Add(new(AudioEffectInstance.Echo, new EchoEffectData(echoDelay, echoDecay, wet: echoWet, dry: echoDry)));
        }
        if (root.Q<Toggle>("occlusionToggle").value)
        {
            bool dependency = root.Q<Toggle>("freqToggle").value;
            float occlusionValue = root.Q<Slider>("occlusionValue").value;
            float lowOcc = root.Q<Slider>("lowFreqOcc").value;
            float midOcc = root.Q<Slider>("midFreqOcc").value;
            float highOcc = root.Q<Slider>("highFreqOcc").value;
            effects.Add(new(AudioEffectInstance.Occlusion, new OcclusionEffectData(dependency, occlusionValue, lowOcc, midOcc, highOcc)));
        }
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