using System;
using UnityEngine;

[Serializable]
public class HighLowPassEffectData
{
    public int lowPassFreq = maxLowPassFreq;
    public int highPassFreq = minHighPassFreq;
    public const int minLowPassFreq = 1000;
    public const int maxLowPassFreq = 22000;

    //if changing these values, also change the UI limits.
    public const int minHighPassFreq = 0;
    public const int maxHighPassFreq = 1000;

    public HighLowPassEffectData(int lowPassFreq, int highPassFreq)
    {
        this.lowPassFreq = Math.Clamp(lowPassFreq, minLowPassFreq, maxLowPassFreq);
        this.highPassFreq = Math.Clamp(highPassFreq, minHighPassFreq, maxHighPassFreq);
    }

    public HighLowPassEffectData(HighLowPassEffectData other)
    {
        lowPassFreq = other.lowPassFreq;
        highPassFreq = other.highPassFreq;
    }
    
    public void ApplyEffectToSource(GameObject audioClipObject)
    {
        AudioLowPassFilter lowPass = audioClipObject.AddComponent<AudioLowPassFilter>();
        AudioHighPassFilter highPass = audioClipObject.AddComponent<AudioHighPassFilter>();
        lowPass.cutoffFrequency = lowPassFreq;
        highPass.cutoffFrequency = highPassFreq;
    }
}