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
            //Tell reaper the audio source was created so it makes the new track
            Debug.Log("Sending Audio Source to Reaper");
            reaperController.NewTrack(audioSource.GetAudioId(),GetUUID(),audioSource.loop);
            updateReaperTrackPosition();
        }
    }

    void OnDestroy()
    {
        if (playbackType.Equals(AudioListeningType.ENCOMPASSING))
        {
            deleteReaperTrack();
        }
    }

    void updateReaperTrackPosition()
    {
        float distance = audioSource.GetSpherePosition().x;
        float elevation = audioSource.GetSpherePosition().y;
        float azimuth = audioSource.GetSpherePosition().z;

        reaperController.UpdateSourcePosition(audioSource.GetAudioId(), azimuth,elevation,distance);
    }

    void deleteReaperTrack()
    {
        reaperController.DeleteTrack(audioSource.GetAudioId());
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