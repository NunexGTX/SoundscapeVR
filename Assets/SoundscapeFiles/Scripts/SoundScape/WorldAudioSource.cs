using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class WorldAudioSource: MonoBehaviour
{
    public AudioSource audioSource;
    public bool debug = true;

    public SoundscapeWrapper soundscapeWrapper;
    public GameObject canvas;
    public SoundscapeLibrary library;
    public VrStateData vrStateData;
    public AudioListeningType playbackType;
    public ReapyControllerCommunication reaperController;
    public LoadingManager loadingManager;


    public Guid GetUUID()
    {
        return audioSource.GetUUID();
    }

    public void SetUUID(Guid uuid)
    {
        audioSource.SetUUID(uuid);
    }

    void OnEnable(){
        if (playbackType.Equals(AudioListeningType.ENCOMPASSING))
        {
            //Tell reaper the audio source was created so it makes the new track
            Debug.Log("Sending Audio Source to Reaper");
            reaperController.NewTrack(audioSource.GetAudioId(),GetUUID(),audioSource.Volume, audioSource.Loop, (uint)audioSource.DelaySeconds);
            updateReaperTrackPosition();

            //Apply mute/unmute
            if (audioSource.Mute)
                reaperController.MuteTrack(GetUUID());
            else
                reaperController.UnmuteTrack(GetUUID());

            //Send effects that are being used
            reaperSendAudioEffects();
        } else _ = PlayAudio();

        gameObject.GetComponent<MeshRenderer>().enabled = debug;

        //if audiosource is visible -> we want to render the icon, not the cube, in display mode.
        //otherside, just render the cube
        if (audioSource.Visible && !debug)
        {
            GetComponent<MeshRenderer>().enabled = false;
            Texture2D soundIcon = library.GetIconForAudio(audioSource.GetAudioId());
            canvas.SetActive(true);
            canvas.GetComponent<RawImage>().texture = soundIcon;
            SetIconEnabledState(audioSource.Mute);
        } else if (debug)
        {
            ResetMaterial(new Material(Shader.Find("Universal Render Pipeline/Lit")));
        }
    }

    public void DestroySource()
    {
        deleteReaperTrack();
        Destroy(gameObject);
    }

    void updateReaperTrackPosition()
    {
        float distance = audioSource.GetSpherePosition().x;
        float elevation = audioSource.GetSpherePosition().y;
        float azimuth = audioSource.GetSpherePosition().z;

        reaperController.UpdateSourcePosition(audioSource.GetUUID(), azimuth,elevation,distance);
    }

    void deleteReaperTrack()
    {
        reaperController.DeleteTrack(audioSource.GetUUID());
    }

    void reaperSendAudioEffects()
    {
        List<AudioEffectInstance> audioSourceEffects = audioSource.AudioEffects;
        
        foreach(AudioEffectInstance audioEffectInstance in audioSourceEffects)
        {
            reaperController.AddEffectToAudio(audioSource.GetUUID(),audioEffectInstance);
        }
        
    }

    bool isWorldSourceAmbisonic()
    {
        return library.GetAudioType(audioSource.GetAudioId()) == Audio.TYPE_AMBISONIC;
    }

    //called when playing or replaying audio source.
    public async Task PlayAudio()
    {
        if(playbackType == AudioListeningType.BINAURAL)
        {
            UnityEngine.AudioSource src = gameObject.GetComponent<UnityEngine.AudioSource>();
            IAudioGenerator gen = await library.GetClipForAudio(audioSource.GetAudioId());
            if(gen == null)
            {
                //TODO fix this, cant be null bc of the map in wrapper
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
            src.mute = audioSource.Mute;
            src.loop = audioSource.Loop;
            src.time = 0;
            src.PlayDelayed(audioSource.DelaySeconds);
        }
        loadingManager.Increment();
        return;
    }

    public void SetIconEnabledState(bool isMuted)
    {
        Color color = canvas.GetComponent<RawImage>().color;
        color.a = isMuted ? 0.4f : 1f;
        canvas.GetComponent<RawImage>().color = color;
    }
    
    public void FlipMuteState()
    {
        if(!audioSource.Visible) return;
        if(soundscapeWrapper.inEditMode) return;
        bool newMute = !audioSource.Mute;
        SetIconEnabledState(newMute);
        audioSource.SetMute(newMute);
        gameObject.GetComponent<UnityEngine.AudioSource>().mute = newMute;

        if(newMute)
            reaperController.MuteTrack(GetUUID());
        else 
            reaperController.UnmuteTrack(GetUUID());
    }

    public bool IsVisible(){return audioSource.Visible;}

    public void PasteDataFrom(AudioSource copiedAudioSource)
    {
        audioSource.PasteDataFrom(copiedAudioSource);
    }

    public void ResetMaterial(Material defaultMaterial)
    {
        Material mat = new(defaultMaterial);
        Texture2D icon = library.GetIconForAudio(audioSource.GetAudioId());
        mat.mainTexture = icon;
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }
}