using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class HighLowPassEffectData : AudioEffectData
{
    public int lowPassFreq = maxLowPassFreq;
    public int highPassFreq = minHighPassFreq;
    public const int minLowPassFreq = 1000;
    public const int maxLowPassFreq = 22000;

    //if changing these values, also change the UI limits.
    public const int minHighPassFreq = 0;
    public const int maxHighPassFreq = 1000;

    public HighLowPassEffectData(int lowPassFreq = maxLowPassFreq, int highPassFreq = minHighPassFreq)
    {
        this.lowPassFreq = Math.Clamp(lowPassFreq, minLowPassFreq, maxLowPassFreq);
        this.highPassFreq = Math.Clamp(highPassFreq, minHighPassFreq, maxHighPassFreq);
    }

    public override AudioEffectData CopyValues(AudioEffectData other)
    {
        if(other is not HighLowPassEffectData otherPass) return null;
        lowPassFreq = otherPass.lowPassFreq;
        highPassFreq = otherPass.highPassFreq;
        return this;
    }
    
    public override void ApplyEffectToSource(GameObject audioClipObject)
    {
        AudioLowPassFilter lowPass = audioClipObject.AddComponent<AudioLowPassFilter>();
        AudioHighPassFilter highPass = audioClipObject.AddComponent<AudioHighPassFilter>();
        lowPass.cutoffFrequency = lowPassFreq;
        highPass.cutoffFrequency = highPassFreq;
    }

    public override bool IsEffectAppliedInUI(VisualElement root) => root.Q<Toggle>("passToggle").value;

    public override AudioEffectData CollectFromUI(VisualElement root)
    {
        int lowFreq = root.Q<SliderInt>("lowPassFreq").value;
        int highFreq = root.Q<SliderInt>("highPassFreq").value;
        return new HighLowPassEffectData(lowFreq, highFreq).WithId(effectId);
    }

    public override void ApplyDefaultUI(VisualElement root)
    {
        root.Q<Foldout>("highAndLowPassFoldout").value = false;
        root.Q<Toggle>("passToggle").value = false;
    }

    public override void ApplyValuesToUI(VisualElement root)
    {
        root.Q<Foldout>("highAndLowPassFoldout").value = true;
        root.Q<Toggle>("passToggle").value = true;
        root.Q<SliderInt>("lowPassFreq").value = lowPassFreq;
        root.Q<SliderInt>("highPassFreq").value = highPassFreq;
    }
    public override string[] GetTrackParams()
    {
        return new string[] { "true", highPassFreq.ToString(), lowPassFreq.ToString() };
    }
}