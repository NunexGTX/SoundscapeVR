using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReverbRiRs
{
    public static Dictionary<ReverbEffectData.ReverbPresets, string> ReaperReverbPresets= new Dictionary<ReverbEffectData.ReverbPresets, string>{
        //PresetName : RiReverb Preset
        {ReverbEffectData.ReverbPresets.Auditorium, "Auditorium"},
        {ReverbEffectData.ReverbPresets.Parliment, "Parliment"},
        {ReverbEffectData.ReverbPresets.Cinema, "CinemaRoom"}
    };

    public static string getPresetCode(ReverbEffectData.ReverbPresets preset)
    {
        return ReaperReverbPresets[preset];
    }
}