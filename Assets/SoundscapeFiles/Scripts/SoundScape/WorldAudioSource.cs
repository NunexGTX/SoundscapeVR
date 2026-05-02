using System;
using System.Collections.Generic;
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
    public AudioListeningType playbackType;

    void OnEnable()
    {
        if (playbackType.Equals(AudioListeningType.BINAURAL))
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
            if (playing)
            {
                src.Play();
                Debug.Log("Playing");
            }
            return;
        } 
        //TODO: reaper DAW logic here.
    }
}