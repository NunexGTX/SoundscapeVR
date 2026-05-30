using System;
using UnityEngine;

[Serializable]
public class EchoEffectData
{
    public int delay_ms = 500;
    public float decay_ratio = 0.5f;
    public float wet_mix = 1.0f; 
    public float dry_mix = 1.0f;
    public const int minDelay = 10;
    public const int maxDelay = 5000;
    public const float minDecay = 0f;
    public const float maxDecay = 1f;
    public const float minWet = 0f;
    public const float maxWet = 1f;
    public const float minDry = 0f;
    public const float maxDry = 1f;

    public EchoEffectData(int delay = 500, float decay = 0.5f, float wet = 1f, float dry = 1f)
    {
        delay_ms = Math.Clamp(delay, minDelay, maxDelay);
        decay_ratio = Math.Clamp(decay, minDecay, maxDecay);
        dry_mix = Math.Clamp(dry, minDry, maxDry);
        wet_mix = Math.Clamp(wet, minWet, maxWet);
    }

    public EchoEffectData(EchoEffectData other)
    {
        delay_ms = other.delay_ms;
        decay_ratio = other.decay_ratio;
        dry_mix = other.dry_mix;
        wet_mix = other.wet_mix;
    }
    
    public void ApplyEffectToSource(GameObject audioClipObject)
    {
        AudioEchoFilter filter = audioClipObject.AddComponent<AudioEchoFilter>();
        filter.delay = delay_ms;
        filter.decayRatio = decay_ratio;
        filter.dryMix = dry_mix;
        filter.wetMix = wet_mix;
    }
}