
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Soundscape
{
    [SerializeField] public string title;
    [SerializeField] protected List<AudioSource> audioSources = new();
    public static readonly string VIDEO_GO_NAME = "soundscapeVideo";

    public Soundscape(List<AudioSource> srcs, string title)
    {
        this.audioSources = srcs;
        this.title = title;
    }

    public string GetTitle(){return title;}

    public List<AudioSource> GetAudioSources(){return audioSources;}

    public void AddSource(AudioSource audio)
    {
        audioSources.Add(audio);
    }
    private void RemoveSource(string audiodId, Vector3 position)
    {
        int idxToRemove = -1;
        float bestDist = int.MaxValue;
        for(int i = 0; i < audioSources.Count; i++)
        {
            AudioSource src = audioSources[i];
            if (audiodId.Equals(src.GetAudioId()))
            {
                float dist = SoundscapeLibrary.DistanceInSphere(position, src.GetSpherePosition());
                if(dist < bestDist)
                    idxToRemove = i;
            }
        }
        if(idxToRemove != -1) audioSources.RemoveAt(idxToRemove);
    }
    public void RemoveSource(AudioSource audio){RemoveSource(audio.GetAudioId(), audio.GetSpherePosition());}
    public abstract Soundscape Copy();
    public abstract GameObject TryGetVisuals(VisualType visualType);
    public abstract VisualType GetSupportedVisual();
}