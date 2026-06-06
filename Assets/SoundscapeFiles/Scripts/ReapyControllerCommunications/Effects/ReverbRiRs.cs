using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReverbRiRs
{
    public static Dictionary<string, string> ReaperReverbPresets= new Dictionary<string, string>{
        {"Cinema Room", "Cinema_Room"},
        {"Parliment", "Parliment"}
    };

    public static string getPresetCode(string displayName)
    {
        return ReaperReverbPresets[displayName];
    }

    public static string[] presetDisplayList()
    {
        return ReaperReverbPresets.Keys.ToArray();
    }
}
