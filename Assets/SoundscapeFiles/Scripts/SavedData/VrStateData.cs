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
    AudioListeningType currentAudio = AudioListeningType.BINAURAL;
    [SerializeField, DontCreateProperty]
    float heightOffset = 1f;
    [SerializeField, DontCreateProperty]
    string ReaperControllerIP;

    [CreateProperty]
    public bool IsInVr
    {
        get => vrEnabled;
        set
        {
            vrEnabled = value;
            GameObject.Find("PlayerManager").GetComponent<PlayerManager>().SetUiPos();        
        } 
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
        get => (int)currentAudio;
        set => currentAudio = (AudioListeningType) value;
    }


    [CreateProperty]
    public float HeightOffset
    {
        get => heightOffset;
        set {
            heightOffset = value;
            GameObject.Find("PlayerManager").GetComponent<PlayerManager>().SetHeightOffset(heightOffset);
        }
    }

    [CreateProperty]
    public string CurrentReaperControllerServerIP
    {
        get => ReaperControllerIP;
        set {
            ReaperControllerIP = value;
            FindFirstObjectByType<ReapyControllerCommunication>().setServerIP(ReaperControllerIP);
        }
    }

    public VisualType GetCurrentVisuals()
    {
        return currentVisual;
    }

    public AudioListeningType GetCurrentAudio()
    {
        return currentAudio;
    }
}
