using System;
using UnityEngine;

[Serializable]
public class AudioEffectInstance
{
    [SerializeReference]
    public AudioEffectData effectData;
    public AudioEffectInstance(AudioEffectData audioEffectData)
    {
        this.effectData = audioEffectData;
    }

    public AudioEffectInstance(AudioEffectInstance other)
    {
        this.effectData = other.effectData;
    }
    public void ApplyEffectToSource(GameObject audioClipObject)
    {
        effectData?.ApplyEffectToSource(audioClipObject);
    }


}