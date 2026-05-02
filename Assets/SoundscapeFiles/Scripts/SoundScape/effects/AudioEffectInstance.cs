using System;
using UnityEngine;

[Serializable]
public class AudioEffectInstance
{
    public const string Echo = "echo";
    public string effectId;
    public EchoEffectData echoEffectData = null;
    public AudioEffectInstance(string effectId, EchoEffectData echoEffectData)
    {
        this.effectId = effectId;
        this.echoEffectData = echoEffectData;
    }
    public void ApplyEffectToSource(GameObject audioClipObject)
    {
        switch (effectId)
        {
            case "echo":
                echoEffectData.ApplyEffectToSource(audioClipObject);
                return;
        }
        Debug.LogWarning($"Could not find effect for id: {effectId}");
    }
}