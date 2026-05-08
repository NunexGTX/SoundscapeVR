using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    void OnEnable(){
        _ = PlayAudio();
    }

    //called when playing or replaying audio source.
    public async Task PlayAudio()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = debug;
        if (playbackType.Equals(AudioListeningType.BINAURAL))
        {
            UnityEngine.AudioSource src = gameObject.GetComponent<UnityEngine.AudioSource>();
            IAudioGenerator gen = await library.GetClipForAudio(audioSource.GetAudioId());
            if(gen == null) return;
            src.generator = gen;
            src.time = 0;
            if (library.GetAudioType(audioSource.GetAudioId()).Equals(Audio.TYPE_AMBISONIC))
            {
                src.spatializePostEffects = false;
                src.spatialize = false;
                src.spatialBlend = 0;
            }
            src.loop = audioSource.loop;
            src.time = 0;
            src.PlayDelayed(audioSource.delaySeconds);
            return;
        } 
        //TODO: reaper DAW logic here.
    }
}