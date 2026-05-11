using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class WorldAudioSource: MonoBehaviour
{
    public AudioSource audioSource;
    public bool debug = true;

    public SoundscapeWrapper soundscapeWrapper;
    public SoundscapeLibrary library;
    public VrStateData vrStateData;
    public AudioListeningType playbackType;
    public ReapyControllerCommunication reaperController;

    public Guid GetUUID()
    {
        return audioSource.GetUUID();
    }

    public void SetUUID(Guid uuid)
    {
        audioSource.SetUUID(uuid);
    }

    void OnEnable(){
        if (playbackType.Equals(AudioListeningType.BINAURAL))
        {
          _ = PlayAudio();
        }else if (playbackType.Equals(AudioListeningType.ENCOMPASSING))
        {
            //TODO: reaper DAW logic here.
            
            //Tell reaper the audio source was created so it makes the new track
            reaperController.NewTrack(audioSource.GetAudioId(),GetUUID());
        }

        
        
    }

    //called when playing or replaying audio source.
    public async Task PlayAudio()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = debug;
        UnityEngine.AudioSource src = gameObject.GetComponent<UnityEngine.AudioSource>();
        IAudioGenerator gen = await library.GetClipForAudio(audioSource.GetAudioId());
        if(gen == null)
        {
            Destroy(gameObject);            
            return;
        }
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
}