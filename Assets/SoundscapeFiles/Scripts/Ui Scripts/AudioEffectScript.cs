using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioEffectScript: MonoBehaviour
{
    [SerializeField] public SoundscapePlayerEditor editor = null;
    private Button saveButton;
    private VisualElement root = null;
    private List<AudioEffectInstance> effects = new();
    private float volume = 1f;
    private string audioName = "Audio Name";
    private int delay = 0;
    private bool loop = true, visible = true, mute = false;
    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        saveButton = root.Q<Button>("saveBtn");
        saveButton.clicked += SaveData;
        if(editor == null) editor = GameObject.Find("PlayerManager").GetComponent<SoundscapePlayerEditor>();
        PopulateValues();
    }

    public void SetValues(GameObject worldAudioSource)
    {
        effects = new(worldAudioSource.GetComponent<WorldAudioSource>().audioSource.audioEffects);
        AudioSource src = worldAudioSource.GetComponent<WorldAudioSource>().audioSource;
        volume = src.volume;
        audioName = src.GetAudioId();
        delay = src.delaySeconds;
        loop = src.loop;
        visible = src.visible;
        mute = src.mute;
        if(root == null) return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        root.Q<Foldout>("echoFoldout").value = false;
        root.Q<Toggle>("echoToggle").value = false;

        root.Q<Foldout>("occlusionFoldout").value = false;
        root.Q<Toggle>("occlusionToggle").value = false;

        root.Q<Slider>("volume").value = volume;
        root.Q<SliderInt>("delay").value = delay;
        root.Q<Toggle>("loop").value = loop;
        root.Q<Toggle>("mute").value = mute;
        root.Q<Toggle>("visible").value = visible;
        root.Q<Label>("audioName").text = audioName;

        foreach(AudioEffectInstance effect in effects)
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
    }

    private void SaveData()
    {
        volume = root.Q<Slider>("volume").value;
        loop = root.Q<Toggle>("loop").value;
        mute = root.Q<Toggle>("mute").value;
        visible = root.Q<Toggle>("visible").value;
        delay = root.Q<SliderInt>("delay").value;
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
        editor.UpdateSourceData(effects, volume, delay, loop, mute, visible);
        
    }
}