using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioSource
{
    [SerializeField] private string audioId;
    [SerializeField] private SerializableVector3 spherePosition;
    public float volume = 1f;
    public List<AudioEffectInstance> audioEffects = new();

    public AudioSource(string audioId, Vector3 spherePosition)
    {
        this.audioId = audioId;
        this.spherePosition = new(spherePosition);
    }
    public AudioSource(Audio audio, Vector3 spherePosition)
    {
        this.audioId = audio.id;
        this.spherePosition = new(spherePosition);
    }
    public string GetAudioId(){return audioId;}

    public Vector3 GetSpherePosition()
    {
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
    }
}