using System;
using UnityEngine;

[Serializable]
public class AudioEffectInstance
{
    public const string Echo = "echo";
    public const string Occlusion = "occlusion";
    public const string Pass = "pass";
    public string effectId;
    public EchoEffectData echoEffectData = null;
    public OcclusionEffectData occlusionEffectData = null;
    public HighLowPassEffectData passEffectData = null;
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
    public AudioEffectInstance(string effectId, HighLowPassEffectData passEffectData)
    {
        this.effectId = effectId;
        this.passEffectData = passEffectData;
    }

    public AudioEffectInstance(AudioEffectInstance other)
    {
        effectId = other.effectId;
        if(other.echoEffectData != null) echoEffectData = new(other.echoEffectData);
        if(other.occlusionEffectData != null) occlusionEffectData = new(other.occlusionEffectData);
        if(other.passEffectData != null) passEffectData = new(other.passEffectData);
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
            case Pass:
                passEffectData.ApplyEffectToSource(audioClipObject);
                return;
        }
        Debug.LogWarning($"[SoundscapeVR] Could not find effect for id: {effectId}");
    }
}