using System;
using System.Collections.Generic;
using UnityEngine;


// Source - https://stackoverflow.com/a/64749403
// Posted by Kinin Roza, modified by community. See post 'Timeline' for change history
// Retrieved 2026-05-24, License - CC BY-SA 4.0
// required to make record classes work
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}

[Serializable]
public class AudioSource : ISerializationCallbackReceiver
{
    //id of the audio file it uses
    [SerializeField] private string audioId = "";
    //id of the source itself
    [SerializeField] private string sourceId;
    [SerializeField] private SerializableVector3 spherePosition;
    
    private AudioData _audioData = new();

    public AudioData audioData
    {
        get => _audioData with { audioId = this.audioId };
        private set => _audioData = value;
    }

    public AudioSource(AudioSource other)
    {
        this.audioId = other.audioId;
        this.sourceId = Guid.NewGuid().ToString();
        this.spherePosition = new(other.spherePosition.ToVector3());

        // Deep copy the properties from the other source's record
        this.audioData = new AudioData(
            CloneEffectsFrom(other.audioData.effectInstances),
            other.audioData.volume,
            other.audioData.delaySeconds,
            other.audioData.loop,
            other.audioData.mute,
            other.audioData.visible
        );
    }
    public AudioSource(string audioId, Vector3 spherePosition, Guid uuid)
    {
        this.audioId = audioId;
        this.spherePosition = new(spherePosition);
        this.sourceId = uuid.ToString();
    }

    public AudioSource(string audioId, Vector3 spherePosition): this(audioId, spherePosition, Guid.NewGuid()){}
    public AudioSource(Audio audio, Vector3 spherePosition): this(audio.id, spherePosition, Guid.NewGuid()){}
    public string GetAudioId(){return audioId;}
    public Guid GetUUID(){return Guid.Parse(sourceId);}
    public void SetUUID(Guid id){sourceId = id.ToString();}

    public Vector3 GetSpherePosition()
    {
        //distance: x
        //elevation: y
        //azimuth: z
        return spherePosition.ToVector3(); 
    }

    public AudioSource WithNewPosition(Vector3 newCoords)
    {
        this.spherePosition = new(newCoords);
        return this;
    }

    public void ApplyEffects(GameObject go)
    {
        var effects = audioData.effectInstances ?? new List<AudioEffectInstance>();
        
        foreach(AudioEffectInstance effect in effects) effect.ApplyEffectToSource(go, SoundscapeLibrary.GetAudioType(audioId).Equals(Audio.TYPE_AMBISONIC));
        go.GetComponent<UnityEngine.AudioSource>().volume = audioData.volume;
        go.GetComponent<UnityEngine.AudioSource>().loop = audioData.loop;
    }


    //shadow fields for JSON serialization without changing the entire .json format we have until now.
    //these are not to be used, only shaded. the real, defacto values are in audioData
    [SerializeField] private bool loop = true;
    [SerializeField] private bool mute = false;
    [SerializeField] private bool visible = false;
    [SerializeField] private int delaySeconds = 0;
    [SerializeField] private float volume = 1f;
    [SerializeField] private List<AudioEffectInstance> audioEffects = new();
    // public "read only" properties (Expression-bodied members) for external use
    public bool Loop => audioData.loop;
    public bool Mute => audioData.mute;
    public bool Visible => audioData.visible;
    public int DelaySeconds => audioData.delaySeconds;
    public float Volume => audioData.volume;
    public List<AudioEffectInstance> AudioEffects => audioData.effectInstances;

    public AudioEffectInstance GetEffect(AudioEffectData audioEffect)
    {
        return AudioEffects.Find(eff => eff.effectData.effectId.Equals(audioEffect.effectId));
    }

    //called before unity serializes the class into json.
    //we basically want to fill in the shadow fields
    public void OnBeforeSerialize()
    {
        if(_audioData == null) _audioData = new();
        loop = _audioData.loop;
        mute = _audioData.mute;
        visible = _audioData.visible;
        delaySeconds = _audioData.delaySeconds;
        volume = _audioData.volume;
        audioEffects = _audioData.effectInstances ?? new List<AudioEffectInstance>();
    }

    //called after unity deserialized the class from json
    //we want to update the real values based on the shadow ones
    public void OnAfterDeserialize()
    {
        if(string.IsNullOrEmpty(sourceId))
            sourceId = Guid.NewGuid().ToString();
            
        _audioData = new AudioData(
            audioEffects,
            volume,
            delaySeconds,
            loop,
            mute,
            visible
        );
    }

    public void PasteDataFrom(AudioSource other)
    {
        PasteDataFrom(other.audioData);
    }

    public void PasteDataFrom(AudioData otherData)
    {
        this.audioData = otherData with 
        { 
            effectInstances = CloneEffectsFrom(otherData.effectInstances)
        };
    }

    public void SetMute(bool newMute)
    {
        this.audioData = audioData with
        {
            mute = newMute
        };
    }
    private List<AudioEffectInstance> CloneEffectsFrom(List<AudioEffectInstance> effectInstances)
    {
        List<AudioEffectInstance> newList = new();
        if (effectInstances == null) return newList;

        foreach(AudioEffectInstance effect in effectInstances)
        {
            newList.Add(new(effect));
        }
        return newList;
    }

    public AudioSource CloneWithEffect(AudioEffectInstance effectToApply)
    {
        AudioSource clone = CloneWithoutEffect(effectToApply.effectData);
        clone.AudioEffects.Add(effectToApply);
        return clone;
    }

    public AudioSource CloneWithoutEffect(AudioEffectData effectToRemove)
    {
        AudioSource clone = new(this);
        clone.AudioEffects.RemoveAll(eff => eff.effectData.effectId.Equals(effectToRemove.effectId));
        return clone;
    }

    public record AudioData(
        List<AudioEffectInstance> effectInstances = default, 
        float volume = 1f, 
        int delaySeconds = 0, 
        bool loop = true, 
        bool mute = false, 
        bool visible = false,
        string audioId = ""
    ){
        public List<AudioEffectInstance> effectInstances { get; init; } = effectInstances ?? new();
    }
}


