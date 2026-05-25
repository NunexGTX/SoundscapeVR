using System;
using UnityEngine;

[Serializable]
public class AudioEffectInstance
{
    public const string Echo = "echo";
    public const string Occlusion = "occlusion";
    public string effectId;
    public EchoEffectData echoEffectData = null;
    public OcclusionEffectData occlusionEffectData = null;
    public AudioEffectInstance(string effectId, EchoEffectData echoEffectData)
    {
        this.effectId = effectId;
        this.echoEffectData = echoEffectData;
    }
    public AudioEffectInstance(string effectId, OcclusionEffectData occlusionEffectData)
    {
        this.effectId = effectId;
        this.occlusionEffectData = occlusionEffectData;
    }

    public AudioEffectInstance(AudioEffectInstance other)
    {
        effectId = other.effectId;
        if(other.echoEffectData != null) echoEffectData = new(other.echoEffectData);
        if(other.occlusionEffectData != null) occlusionEffectData = new(other.occlusionEffectData);
    }
    public void ApplyEffectToSource(GameObject audioClipObject)
    {
        switch (effectId)
        {
            case Echo:
                echoEffectData.ApplyEffectToSource(audioClipObject);
                return;
            case Occlusion:
                occlusionEffectData.ApplyEffectToSource(audioClipObject);
                return;
        }
        Debug.LogWarning($"Could not find effect for id: {effectId}");
    }
}