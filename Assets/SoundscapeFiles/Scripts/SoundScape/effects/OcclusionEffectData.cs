using System;
using SteamAudio;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class OcclusionEffectData : AudioEffectData
{
    public float occlusion_value = 1f;
    public float occlusion_low_freq = 1f;
    public float occlusion_mid_freq = 1f;
    public float occlusion_high_freq = 1f;
    public bool frequency_dependent = false;
    public const float minOcclusion = 0f;
    public const float maxOcclusion = 1f;

    public OcclusionEffectData(bool frequencyDependent = false, float occlusionValue = 1f, float lowFreqOcc = 1f, float midFreqOcc = 1f, float highFreqOcc = 1f)
    {
        frequency_dependent = frequencyDependent;
        occlusion_value = occlusionValue;
        occlusion_low_freq = lowFreqOcc;
        occlusion_mid_freq = midFreqOcc;
        occlusion_high_freq = highFreqOcc;
    }
    public override AudioEffectData CopyValues(AudioEffectData other)
    {
        if(other is not OcclusionEffectData otherOcc) return null;
        frequency_dependent = otherOcc.frequency_dependent;
        occlusion_value = otherOcc.occlusion_value;
        occlusion_low_freq = otherOcc.occlusion_low_freq;
        occlusion_mid_freq = otherOcc.occlusion_mid_freq;
        occlusion_high_freq = otherOcc.occlusion_high_freq;
        return this;
    }
    
    public override void ApplyEffectToSource(GameObject audioClipObject, bool isAmbisonic)
    {
        if(audioClipObject.GetComponent<SteamAudioSource>() == null) audioClipObject.AddComponent<SteamAudioSource>();
        SteamAudioSource steamSource = audioClipObject.GetComponent<SteamAudioSource>();
        steamSource.occlusion = true;
        steamSource.occlusionInput = OcclusionInput.UserDefined;
        steamSource.occlusionValue = occlusion_value;
        steamSource.transmission = false;
        //if its frequency independent, we use only the basic occlusion and do nothing else.
        if(!frequency_dependent) return;
        steamSource.transmission = true;
        steamSource.transmissionType =TransmissionType.FrequencyDependent;
        steamSource.transmissionInput = TransmissionInput.UserDefined;
        steamSource.transmissionLow = occlusion_low_freq;
        steamSource.transmissionMid = occlusion_mid_freq;
        steamSource.transmissionHigh= occlusion_high_freq;

    }

    public override bool IsEffectAppliedInUI(VisualElement root) => root.Q<Toggle>("occlusionToggle").value;

    public override AudioEffectData CollectFromUI(VisualElement root, bool isAmbisonic)
    {
        bool dependency = root.Q<Toggle>("freqToggle").value;
        float occlusionValue = root.Q<Slider>("occlusionValue").value;
        float lowOcc = root.Q<Slider>("lowFreqOcc").value;
        float midOcc = root.Q<Slider>("midFreqOcc").value;
        float highOcc = root.Q<Slider>("highFreqOcc").value;
        return new OcclusionEffectData(dependency, occlusionValue, lowOcc, midOcc, highOcc).WithId(effectId);
    }

    public override void ApplyDefaultUI(VisualElement root, bool isAmbisonic)
    {
        root.Q<Foldout>("occlusionFoldout").value = false;
        root.Q<Toggle>("occlusionToggle").value = false;
    }

    public override void ApplyValuesToUI(VisualElement root)
    {
        root.Q<Foldout>("occlusionFoldout").value = true;
        root.Q<Toggle>("occlusionToggle").value = true;
        root.Q<Toggle>("freqToggle").value = frequency_dependent;
        root.Q<Slider>("occlusionValue").value = occlusion_value;
        root.Q<Slider>("lowFreqOcc").value = occlusion_low_freq;
        root.Q<Slider>("midFreqOcc").value = occlusion_mid_freq;
        root.Q<Slider>("highFreqOcc").value = occlusion_high_freq;
    }
    public override string[] GetTrackParams(bool isAmbisonic)
    {
        return new string[] { "true", frequency_dependent.ToString(), occlusion_value.ToString(), occlusion_low_freq.ToString(), occlusion_mid_freq.ToString(), occlusion_high_freq.ToString() };
    }
}