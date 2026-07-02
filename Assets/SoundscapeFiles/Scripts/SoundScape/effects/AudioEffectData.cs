using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class AudioEffectData
{
    public string effectId;
    protected bool IsAudioAmbisonic()
    {
        return SoundscapeLibrary.GetAudioType(effectId) == Audio.TYPE_AMBISONIC;
    }
    
 /*   public void ApplyEffectToSource(GameObject audioClipObject, AudioEffectData data)
    {
        if(data.GetType() != this.GetType()) return;
        CopyValues(data);
        ApplyEffectToSource(audioClipObject);
    }*/

    public abstract AudioEffectData CopyValues(AudioEffectData other);
    //called to apply this effect to the source GameObject
    public abstract void ApplyEffectToSource(GameObject audioClipObject);

    public AudioEffectData WithId(string effectId)
    {
        this.effectId = effectId;
        return this;
    }
    public abstract string[] GetTrackParams();
    /// <summary>
    /// called to check if, according to the AudioEffectsUI, this effect was applied (should be the checkbox corresponding to this effect).
    /// will call CollectFromUI right after
    /// </summary>
    /// <param name="root"> root of the UI document</param>
    /// <returns> true if the UI says to add this effect, false otherwise</returns>
    public abstract bool IsEffectAppliedInUI(VisualElement root);
    /// <summary>
    /// called to collect an instance of this effect based on the root of the document. if it is called, it means that IsEffectAppliedInUI returned true
    /// </summary>
    /// <param name="root"> root of the UI document</param>
    /// <returns> a new instance of this effect class</returns>
    public abstract AudioEffectData CollectFromUI(VisualElement root);
    /// <summary>
    /// Set the values in the UI to the default (aka, what should it be by default if the effect is not applied.)
    /// Should generally just be setting the checkbox and foldout values to false.
    /// </summary>
    /// <param name="root"> root of the UI document</param>
    public abstract void ApplyDefaultUI(VisualElement root);
    /// <summary>
    /// Apply the values of this very effect data instance to the UI.
    /// </summary>
    /// <param name="root"> root of the UI document</param>
    public abstract void ApplyValuesToUI(VisualElement root);
}