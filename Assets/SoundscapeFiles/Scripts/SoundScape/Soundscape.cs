
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Soundscape
{
    [SerializeField] protected string title;
    [SerializeField] protected List<AudioSource> audioSources = new();
    public static readonly string VIDEO_GO_NAME = "soundscapeVideo";

    public Soundscape(List<AudioSource> srcs, string title)
    {
        this.audioSources = srcs;
        this.title = title;
    }

    public string GetTitle(){return title;}

    public void AddSource(AudioSource src){}

    public void RemoveSource(string audiodId, Vector3 position){}
    public void RemoveSource(string audiodId){}
    public abstract Soundscape Copy();
    public abstract GameObject TryGetVisuals(VisualType visualType);
    public abstract VisualType GetSupportedVisual();
}