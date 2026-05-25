using System;
using SteamAudio;
using UnityEngine;

[Serializable]
public class OcclusionEffectData
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
    public OcclusionEffectData(OcclusionEffectData other)
    {
        frequency_dependent = other.frequency_dependent;
        occlusion_value = other.occlusion_value;
        occlusion_low_freq = other.occlusion_low_freq;
        occlusion_mid_freq = other.occlusion_mid_freq;
        occlusion_high_freq = other.occlusion_high_freq;
    }
    
    public void ApplyEffectToSource(GameObject audioClipObject)
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
}