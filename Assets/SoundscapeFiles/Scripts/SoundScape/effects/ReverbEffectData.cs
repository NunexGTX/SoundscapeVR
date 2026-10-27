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
        Parliment,
        Cinema
    }

    public float wet = 0.2f;

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
            case ReverbPresets.Cinema:
                return AudioReverbPreset.Concerthall;
        }
        return AudioReverbPreset.Off;
    }

    public ReverbEffectData(ReverbPresets reverbEffectData = ReverbPresets.Parliment, float wet = 0.2f)
    {
        this.wet = wet;
        this.currentPreset = reverbEffectData;
    }

    public override AudioEffectData CopyValues(AudioEffectData other)
    {
        if(other is not ReverbEffectData otherReverb) return null;
        currentPreset = otherReverb.currentPreset;
        return this;
    }

    public override void ApplyEffectToSource(GameObject audioClipObject, bool isAmbisonic)
    {
        if(isAmbisonic) return;
        AudioReverbFilter reverbFilter = audioClipObject.AddComponent<AudioReverbFilter>();
        reverbFilter.reverbPreset = unityEffectPreset(currentPreset);
        reverbFilter.reverbPreset = AudioReverbPreset.User;
        float mdB = 10000*(wet - 1);
        reverbFilter.room  = mdB;
    }

    public override bool IsEffectAppliedInUI(VisualElement root) => root.Q<Toggle>("applyReverb").value;

    public override AudioEffectData CollectFromUI(VisualElement root, bool isAmbisonic)
    {
        if(isAmbisonic) return null;
        ReverbPresets reverbPreset = Enum.Parse<ReverbPresets>(root.Q<DropdownField>("reverbPresetDropdown").value);
        float wetVal = root.Q<Slider>("wetLevel").value;
        return new ReverbEffectData(reverbPreset, wetVal).WithId(effectId);
    }

    public override void ApplyDefaultUI(VisualElement root, bool isAmbisonic)
    {
        if (isAmbisonic)
        {
            root.Q<Foldout>("reverbFoldout").visible = false;
            return;   
        }
        root.Q<Foldout>("reverbFoldout").visible = true;
        root.Q<Foldout>("reverbFoldout").value = false;
        root.Q<Toggle>("applyReverb").value = false;
        root.Q<DropdownField>("reverbPresetDropdown").choices = getPresetNames();
        root.Q<DropdownField>("reverbPresetDropdown").value = currentPreset.ToString();
    }

    public override void ApplyValuesToUI(VisualElement root)
    {
        root.Q<DropdownField>("reverbPresetDropdown").value = currentPreset.ToString();
        root.Q<Foldout>("reverbFoldout").visible = true;
        root.Q<Foldout>("reverbFoldout").value = true;
        root.Q<Toggle>("applyReverb").value = true;
        root.Q<Slider>("wetLevel").value = wet;
    }

    public override string[] GetTrackParams(bool isAmbisonic)
    {
        //Can only be used for reaper in this case
        if(isAmbisonic) return null;
        return new string[] {"true", ReverbRiRs.getPresetCode(currentPreset), wet.ToString().Replace(',', '.')};
    }
    
}
