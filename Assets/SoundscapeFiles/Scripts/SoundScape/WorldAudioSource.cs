using System;
using UnityEngine;
using UnityEngine.Audio;

public class WorldAudioSource: MonoBehaviour
{
    public AudioSource audioSource;
    public Guid uuid;
    public bool playing = true;
    public bool debug = true;

    public SoundscapeWrapper soundscapeWrapper;
    public SoundscapeLibrary library;
    public VrStateData vrStateData;

    void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = debug;
        UnityEngine.AudioSource src = gameObject.GetComponent<UnityEngine.AudioSource>();
        IAudioGenerator gen = library.GetClipForAudio(audioSource.GetAudioId());
        if(gen == null) return;
        src.generator = gen;
        src.time = 0;
        if (library.GetAudioType(audioSource.GetAudioId()).Equals(Audio.TYPE_AMBISONIC))
        {
            src.spatializePostEffects = false;
            src.spatialize = false;
            src.spatialBlend = 0;
        }
        if (playing && debug)
        {
            src.Play();
        }
    }
}