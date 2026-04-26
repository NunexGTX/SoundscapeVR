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

    public void SaveCurrentSoundscape()
    {
        if(edited)
            soundscapeLibrary.SaveSoundscape(currentSoundscape);
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

    public void SetSoundscape(Soundscape ss)
    {
        StopSoundscape();
        currentSoundscape = ss.Copy();
    }
    
    public void DeleteAudio(GameObject srcObject)
    {
        if(srcObject == null) return;
        WorldAudioSource src = srcObject.GetComponent<WorldAudioSource>();
        if(src == null) return;
        DeleteAudio(src.uuid);
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
        go.GetComponent<WorldAudioSource>().uuid = srcId;
        go.GetComponent<WorldAudioSource>().audioSource = src;
        go.GetComponent<WorldAudioSource>().playing = play;
        go.GetComponent<WorldAudioSource>().debug = debug;
        go.GetComponent<WorldAudioSource>().soundscapeWrapper = this;
        go.GetComponent<WorldAudioSource>().library = soundscapeLibrary;
        placedAudios.Add(srcId, go);
        go.SetActive(true);
        if(newSource) EditSoundscape(src, true);
    }

    public void StartSoundscape(bool debug)
    {
        if (!debug)
        {
            playerManager.SetPlayerMovement(false);
            playerManager.HideCameraLayers(debug);
            
            ssVisual = currentSoundscape.TryGetVisuals(vrStateData.GetCurrentVisuals());
            if(ssVisual != null)
            {
                videoPlaying = true;
            }
        }
        foreach(AudioSource src in currentSoundscape.GetAudioSources())
        {
            PlaceAudioSource(src, true, debug);
        }
    }

    private void StopSoundscape(InputAction.CallbackContext context)
    {
        StopSoundscape();
    }

    public void StopSoundscape()
    {
        playerManager.SetPlayerMovement(true);
        playerManager.ShowCameraLayers();
        if (videoPlaying)
        {
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
