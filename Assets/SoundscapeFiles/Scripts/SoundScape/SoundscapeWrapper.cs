using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundscapeWrapper: MonoBehaviour
{
    private GameObject ssVisual;
    private Soundscape currentSoundscape;
    public PlayerManager playerManager;
    public VrStateData vrStateData;
    public GameObject audioSourceObject;
    public SoundscapeLibrary soundscapeLibrary;
    private bool videoPlaying = false;
    private bool edited = false;
    private Dictionary<Guid, GameObject> placedAudios = new();
    private bool editing = false;
    [SerializeField] private InputActionReference leaveSoundscapeAction;


    void OnEnable()
    {
        // Hook into the global event
        leaveSoundscapeAction.action.performed += StopSoundscape;
    }

    void OnDisable()
    {
        leaveSoundscapeAction.action.performed -= StopSoundscape;
    }

    private void SaveCurrentSoundscape()
    {
        if (edited && currentSoundscape != null)
        {
            soundscapeLibrary.SaveSoundscape(currentSoundscape);
            edited = false;            
        }
    }

    private void EditSoundscape(AudioSource src, bool add)
    {
        if(currentSoundscape == null) return;
        //1. save into local copy
        if(add)
            currentSoundscape.AddSource(src);
        else
            currentSoundscape.RemoveSource(src);
        edited = true;
        //2. go into library and save that.
        //3. remake UI
        //local copy and library should be synced
    }

    public void MarkEdited(){edited = true;}

    public void SetSoundscape(Soundscape ss)
    {
        StopSoundscape();
        currentSoundscape = ss.Copy();
    }
    
    public void DeleteAudio(GameObject srcObject)
    {
        if(srcObject == null) return;
        DeleteAudio(srcObject.GetComponent<WorldAudioSource>());
    }
    private void DeleteAudio(WorldAudioSource srcObject)
    {
        if(srcObject == null) return;
        DeleteAudio(srcObject.uuid);
    }
    public void UpdateSource(GameObject srcObject)
    {
        if(srcObject == null) return;
        WorldAudioSource worldAudioSource = srcObject.GetComponent<WorldAudioSource>();
        //1. remove it
        DeleteAudio(worldAudioSource);
        //2. add it.
        PlaceAudioSource(worldAudioSource.audioSource, play: true, debug: true, newSource: true);
    }

    public void DeleteAudio(Guid audioId)
    {
        if(currentSoundscape == null) return;
        if (placedAudios.ContainsKey(audioId))
        {
            AudioSource src = placedAudios[audioId].GetComponent<WorldAudioSource>().audioSource;
            EditSoundscape(src, false);
            Destroy(placedAudios[audioId]);
            return;
        }
        Debug.LogError($"Tried to remove an audio source that doesnt exist: {audioId}");
    }

    public void PlaceAudioSource(AudioSource src, bool play = true, bool debug = true, bool newSource = false)
    {
        if(currentSoundscape == null) return;
        Guid srcId = Guid.NewGuid();
        GameObject go = Instantiate(audioSourceObject);
        go.transform.position = SoundscapeLibrary.SphereToWorld(src.GetSpherePosition());
        WorldAudioSource worldSrc = go.GetComponent<WorldAudioSource>();
        worldSrc.uuid = srcId;
        worldSrc.audioSource = src;
        worldSrc.playing = play;
        worldSrc.debug = debug;
        worldSrc.soundscapeWrapper = this;
        worldSrc.library = soundscapeLibrary;
        worldSrc.playbackType = vrStateData.GetCurrentAudio();
        src.ApplyEffects(go);
        placedAudios.Add(srcId, go);
        go.SetActive(true);
        if(newSource) EditSoundscape(src, true);
    }

    public void StartSoundscape(bool debug)
    {
        playerManager.HideCameraLayers(debug);
        editing = debug;
        if (!debug)
        {
            playerManager.SetPlayerMovement(false);
            
            currentSoundscape.OnVisualsLoaded(() => StartSoundSources(debug));
            ssVisual = currentSoundscape.TryGetVisuals(vrStateData.GetCurrentVisuals());
            if(ssVisual != null)
                videoPlaying = true;
            return;
        }
        StartSoundSources(debug);

    }

    private void StartSoundSources(bool debug)
    {
        foreach(AudioSource src in currentSoundscape.GetAudioSources())
        {
            PlaceAudioSource(src, true, debug);
        }
    }

    private void StopSoundscape(InputAction.CallbackContext context)
    {
        if(editing) return;
        StopSoundscape();
    }

    public void StopSoundscape(string title)
    {if(title.Equals(currentSoundscape.title)) StopSoundscape();}

    public void StopSoundscape()
    {
        editing = false;
        SaveCurrentSoundscape();
        playerManager.SetPlayerMovement(true);
        playerManager.ShowCameraLayers();
        if (videoPlaying)
        {
            currentSoundscape.ClearVisualsBinding();
            if(ssVisual != null) 
            {
                Destroy(ssVisual);
                ssVisual = null;
            }
            videoPlaying = false;
        }
        foreach(GameObject go in placedAudios.Values)
        {
            Destroy(go);
        }
        placedAudios = new();
        currentSoundscape = null;
    }

    public void SetAndStartSoundscape(Soundscape ss, bool debug = false)
    {
        SetSoundscape(ss);
        StartSoundscape(debug);
    }

    public bool HasSoundscape(){return currentSoundscape != null;}

}
