using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ReverbEffectData: AudioEffectData
{   
    public enum ReverbPresets
    {
        Auditorium,
        Parliment
    }

    public ReverbPresets currentPreset = ReverbPresets.Parliment;

    public List<string> getPresetNames()
    {
        return Enum.GetNames(typeof(ReverbPresets)).ToList();
    }

    AudioReverbPreset unityEffectPreset(ReverbPresets reverbPreset)
    {
        switch (reverbPreset)
        {
            case ReverbPresets.Auditorium:
                return AudioReverbPreset.Auditorium;
            case ReverbPresets.Parliment:
                return AudioReverbPreset.Bathroom;
        }
        return AudioReverbPreset.Off;
    }

    public ReverbEffectData(ReverbPresets reverbEffectData = ReverbPresets.Parliment)
    {
        this.currentPreset = reverbEffectData;
    }

    public override AudioEffectData CopyValues(AudioEffectData other)
    {
        if(other is not ReverbEffectData otherReverb) return null;
        currentPreset = otherReverb.currentPreset;
        return this;
    }

    public override void ApplyEffectToSource(GameObject audioClipObject)
    {
        AudioReverbFilter reverbFilter = audioClipObject.AddComponent<AudioReverbFilter>();
        reverbFilter.reverbPreset = unityEffectPreset(currentPreset);
    }

    public override bool IsEffectAppliedInUI(VisualElement root) => root.Q<Toggle>("applyReverb").value;

    public override AudioEffectData CollectFromUI(VisualElement root)
    {
        ReverbPresets reverbPreset = Enum.Parse<ReverbPresets>(root.Q<DropdownField>("reverbPresetDropdown").value);
        return new ReverbEffectData(reverbPreset).WithId(effectId);
    }

    public override void ApplyDefaultUI(VisualElement root)
    {
       root.Q<DropdownField>("reverbPresetDropdown").choices = getPresetNames();
    }

    public override void ApplyValuesToUI(VisualElement root)
    {
        root.Q<DropdownField>("reverbPresetDropdown").value = currentPreset.ToString();
    }

    public override string[] GetTrackParams()
    {
        //Can only be used for reaper in this case
        return new string[] {"true", ReverbRiRs.getPresetCode(currentPreset)};
    }
    
}
