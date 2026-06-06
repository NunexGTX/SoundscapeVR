using System;
using System.Collections.Generic;
using System.Linq;

public class AudioEffects
{
    private static Dictionary<string, AudioEffectData> REGISTRY = new();
    public static AudioEffectData ECHO_EFFECT = RegisterEffectData("echo", new EchoEffectData());
    public static AudioEffectData OCCLUSION_EFFECT = RegisterEffectData("occlusion", new OcclusionEffectData());
    public static AudioEffectData PASS_EFFECT = RegisterEffectData("pass", new HighLowPassEffectData());

    private static AudioEffectData RegisterEffectData(string effectId, AudioEffectData data)
    {
        REGISTRY.Add(effectId, data);
        data.effectId = effectId;
        return data;
    }

    public static AudioEffectData GetAudioEffectData(string effectId)
    {
        return REGISTRY.GetValueOrDefault(effectId, null);
    }

    public static List<AudioEffectData> GetEffects()
    {
        return REGISTRY.Values.ToList();
    }

}