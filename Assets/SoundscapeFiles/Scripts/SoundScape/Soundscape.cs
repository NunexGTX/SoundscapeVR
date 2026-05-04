using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Soundscape
{
    [SerializeField] public string title;
    [SerializeField] public int timeSeconds = 90;
    [SerializeField] public string staticPath = null;
    [SerializeField] public string encompassingPath = null;
    [SerializeField] public string projectionType = "None";
    [SerializeField] public bool loop = true;
    [SerializeField] protected List<AudioSource> audioSources = new();
    protected Action onVisualsLoaded = null;
    public static readonly string VIDEO_GO_NAME = "soundscapeVideo";

    public static Soundscape CreateSoundscape(string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop, bool video)
    {
        if (video)
        {
            return new VideoSoundscape(title, time, staticPath, encompassingPath, projectionType, loop);
        } else
        {
            return new ImageSoundscape(title, time, staticPath, encompassingPath, projectionType, loop);
        }
    }
    protected Soundscape(List<AudioSource> srcs, string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop)
    {
        audioSources = srcs;
        this.title = title;
        this.timeSeconds = time;
        this.staticPath = staticPath;
        this.encompassingPath = encompassingPath;
        this.projectionType = projectionType;
        this.loop = loop;
    }

    public void OnVisualsLoaded(Action action){onVisualsLoaded = action;}
    public virtual void ClearVisualsBinding(){onVisualsLoaded=null;}

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
    
    public void CopySourcesFrom(Soundscape other)
    {
        audioSources = new(other.audioSources);
    }
    public void RemoveSource(AudioSource audio){RemoveSource(audio.GetAudioId(), audio.GetSpherePosition());}
    public abstract Soundscape Copy();
    public abstract GameObject TryGetVisuals(VisualType visualType);
    public abstract VisualType GetSupportedVisual();
    public abstract void RestartVisuals(GameObject ssVisual, VisualType visualType);

}