using System;
using Unity.Properties;
using UnityEngine;

[CreateAssetMenu(fileName = "VrStateData", menuName = "Scriptable Objects/VrStateData")]
public class VrStateData : ScriptableObject
{
    [SerializeField, DontCreateProperty]
    bool vrEnabled = false;
    [SerializeField, DontCreateProperty]
    VisualType currentVisual = VisualType.ENCOMPASSING;
    [SerializeField, DontCreateProperty]
    AudioListeningType currentAudioSelection = AudioListeningType.BINAURAL;

    [CreateProperty]
    public bool IsInVr
    {
        get => vrEnabled;
        set => vrEnabled = value;
    }

    [CreateProperty]
    public int CurrentVisualChoice
    {
        get => (int)currentVisual;
        set => currentVisual = (VisualType) value;
    }


    [CreateProperty]
    public int CurrentAudioChoice
    {
        get => (int)currentAudioSelection;
        set => currentAudioSelection = (AudioListeningType) value;
    }

    public VisualType GetCurrentVisuals()
    {
        return currentVisual;
    }

    public AudioListeningType GetCurrentAudio()
    {
        return currentAudioSelection;
    }
}
