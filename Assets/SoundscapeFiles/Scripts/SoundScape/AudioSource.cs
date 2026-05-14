using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioSource : ISerializationCallbackReceiver
{
    [SerializeField] private string audioId;
    [SerializeField] private string sourceId;
    [SerializeField] private SerializableVector3 spherePosition;
    public bool loop = true;
    public bool mute = true;
    public bool visible = false;
    public int delaySeconds = 0;
    public float volume = 1f;
    public List<AudioEffectInstance> audioEffects = new();

    public AudioSource(AudioSource other)
    {
        this.audioId = other.audioId;
        this.sourceId = Guid.NewGuid().ToString(); // new UUID for the clone
        this.spherePosition = new(other.spherePosition.ToVector3());
        this.loop = other.loop;
        this.mute = other.mute;
        this.visible = other.visible;
        this.delaySeconds = other.delaySeconds;
        this.volume = other.volume;
        this.audioEffects = new(other.audioEffects); // shallow copy of effects list
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

    public void WithNewPosition(Vector3 newCoords)
    {
        this.spherePosition = new(newCoords);
    }

    public void ApplyEffects(GameObject go)
    {
        foreach(AudioEffectInstance effect in audioEffects) effect.ApplyEffectToSource(go);
        go.GetComponent<UnityEngine.AudioSource>().volume = volume;
        go.GetComponent<UnityEngine.AudioSource>().loop = loop;
    }

    public void OnBeforeSerialize(){}

    public void OnAfterDeserialize()
    {
        if(string.IsNullOrEmpty(sourceId))
            sourceId = Guid.NewGuid().ToString();
    }
}