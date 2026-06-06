using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class EchoEffectData : AudioEffectData
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

    public override AudioEffectData CopyValues(AudioEffectData other)
    {
        if(other is not EchoEffectData otherEcho) return null;
        delay_ms = otherEcho.delay_ms;
        decay_ratio = otherEcho.decay_ratio;
        dry_mix = otherEcho.dry_mix;
        wet_mix = otherEcho.wet_mix;
        return this;
    }
    
    public override void ApplyEffectToSource(GameObject audioClipObject)
    {
        AudioEchoFilter filter = audioClipObject.AddComponent<AudioEchoFilter>();
        filter.delay = delay_ms;
        filter.decayRatio = decay_ratio;
        filter.dryMix = dry_mix;
        filter.wetMix = wet_mix;
    }

    public override bool IsEffectAppliedInUI(VisualElement root) => root.Q<Toggle>("echoToggle").value;

    public override AudioEffectData CollectFromUI(VisualElement root)
    {
        int echoDelay = root.Q<SliderInt>("echoDelay").value;
        float echoDecay = root.Q<Slider>("echoDecay").value;
        float echoDry = root.Q<Slider>("echoDry").value;
        float echoWet = root.Q<Slider>("echoWet").value;
        return new EchoEffectData(echoDelay, echoDecay, wet: echoWet, dry: echoDry);
    }

    public override void ApplyDefaultUI(VisualElement root)
    {
        root.Q<Foldout>("echoFoldout").value = false;
        root.Q<Toggle>("echoToggle").value = false;
    }

    public override void ApplyValuesToUI(VisualElement root)
    {
        root.Q<Foldout>("echoFoldout").value = true;
        root.Q<Toggle>("echoToggle").value = true;
        root.Q<SliderInt>("echoDelay").value = delay_ms;
        root.Q<Slider>("echoDecay").value = decay_ratio;
        root.Q<Slider>("echoDry").value = dry_mix;
        root.Q<Slider>("echoWet").value = wet_mix;
    }

    public override string[] GetTrackParams()
    {
        return new string[] { "true", delay_ms.ToString(), decay_ratio.ToString(), dry_mix.ToString(), wet_mix.ToString() };
    }
}