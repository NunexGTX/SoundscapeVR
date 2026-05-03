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
        volume = worldAudioSource.GetComponent<WorldAudioSource>().audioSource.volume;
        audioName = worldAudioSource.GetComponent<WorldAudioSource>().audioSource.GetAudioId();
        if(root == null) return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        root.Q<Foldout>("echoFoldout").value = false;
        root.Q<Toggle>("echoToggle").value = false;
        root.Q<Slider>("volume").value = volume;
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
        List<AudioEffectInstance> effects = new();
        if (root.Q<Toggle>("echoToggle").value)
        {
            int echoDelay = root.Q<SliderInt>("echoDelay").value;
            float echoDecay = root.Q<Slider>("echoDecay").value;
            float echoDry = root.Q<Slider>("echoDry").value;
            float echoWet = root.Q<Slider>("echoWet").value;
            effects.Add(new(AudioEffectInstance.Echo, new EchoEffectData(echoDelay, echoDecay, wet: echoWet, dry: echoDry)));
        }
        editor.UpdateSourceData(effects, volume);
    }
}