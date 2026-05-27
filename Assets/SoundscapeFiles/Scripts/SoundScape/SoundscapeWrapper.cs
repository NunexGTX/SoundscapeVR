using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class SoundscapeWrapper: MonoBehaviour
{
    private GameObject ssVisual;
    private GameObject screen;
    private Soundscape currentSoundscape;
    public PlayerManager playerManager;
    public VrStateData vrStateData;
    public GameObject audioSourceObject;
    public SoundscapeLibrary soundscapeLibrary;
    public ReapyControllerCommunication reapyController;
    public LayerMask untargetableSoundSourceLayer;
    private bool videoPlaying = false;
    private bool edited = false;
    private Dictionary<Guid, GameObject> placedAudios = new();
    public bool inEditMode {get; private set;} = false;
    private Material defaultSkybox;
    [SerializeField] private InputActionReference leaveSoundscapeAction;
    [SerializeField] private LoadingManager loadingManager;


    void OnEnable()
    {
        // Hook into the global event
        leaveSoundscapeAction.action.performed += StopSoundscape;
        defaultSkybox = RenderSettings.skybox;
    }

    void OnDisable()
    {
        leaveSoundscapeAction.action.performed -= StopSoundscape;
    }

    private void SaveCurrentSoundscape()
    {
        if (edited && currentSoundscape != null)
        {
            Debug.Log("Saving Soundscape");
            soundscapeLibrary.ReplaceOrAddSoundscape(currentSoundscape);
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
        DeleteAudio(srcObject.GetUUID());
    }
    public void UpdateSource(GameObject srcObject)
    {
        if(srcObject == null) return;
        WorldAudioSource worldAudioSource = srcObject.GetComponent<WorldAudioSource>();
        UpdateSource((worldAudioSource == null) ? null : worldAudioSource.GetUUID(), worldAudioSource.audioSource);
    }
    public void UpdateSource(Guid? sourceId, AudioSource audioSource)
    {
        if(sourceId != null && !sourceId.HasValue)
            Debug.LogWarning("Warning!: give source UUID is not null but has no value! Consider changing UpdateSource() method!");
        if(sourceId == null || !sourceId.HasValue) return;
        //1. remove it
        DeleteAudio(sourceId.Value);
        //2. add it.
        PlaceNewAudioSource(audioSource);
    }

    public void DeleteAudio(Guid audioId)
    {
        if(currentSoundscape == null) return;
        if (placedAudios.ContainsKey(audioId))
        {
            AudioSource src = placedAudios[audioId].GetComponent<WorldAudioSource>().audioSource;
            EditSoundscape(src, false);
            Destroy(placedAudios[audioId]);
            placedAudios.Remove(audioId);
            return;
        }
        Debug.LogError($"Tried to remove an audio source that doesnt exist: {audioId}");
    }

    public void PlaceNewAudioSource(AudioSource src)
    {
        if(currentSoundscape == null) return;
        PlaceBaseSource(src, debug: true);
        EditSoundscape(src, add: true);
    }
    
    private void PlaceSoundscapeAudioSource(AudioSource src, bool debug)
    {
        if(currentSoundscape == null) return;
        PlaceBaseSource(src, debug);
    }

    private void PlaceBaseSource(AudioSource src, bool debug)
    {
        GameObject go = Instantiate(audioSourceObject);
        go.transform.position = SoundscapeLibrary.SphereToWorld(src.GetSpherePosition());

        WorldAudioSource worldSrc = go.GetComponent<WorldAudioSource>();
        worldSrc.audioSource = src;
        worldSrc.debug = debug;
        worldSrc.soundscapeWrapper = this;
        worldSrc.library = soundscapeLibrary;
        worldSrc.playbackType = vrStateData.GetCurrentAudio();
        worldSrc.reaperController = reapyController;

        if(!inEditMode && !worldSrc.IsVisible()) gameObject.SetLayerRecursively(untargetableSoundSourceLayer);

        src.ApplyEffects(go);
        placedAudios.Add(worldSrc.GetUUID(), go);
        go.SetActive(true);
    }

    //called when the audio sources are already created, we just want to replay them
    private void PlayExistingSourcesFromZero()
    {
        Debug.Log("Replaying audios");
        foreach(GameObject go in placedAudios.Values)
            PlayAudioSource(go);
    }

    private void PlayAudioSource(GameObject srcObj)
    {
        _ = srcObj.GetComponent<WorldAudioSource>().PlayAudio();
    }

    private void RestartSoundscape()
    {
        currentSoundscape.RestartVisuals(ssVisual, vrStateData.GetCurrentVisuals());
        PlayExistingSourcesFromZero();
        StartLoopCoroutine();
    }

    public async Task PrepareSoundscape(bool debug)
    {
        inEditMode = debug;
        playerManager.HideCameraLayers(inEditMode ? PlayerManager.HIDE_EDITING : PlayerManager.HIDE_ALL);
        /*if (debug)
        {
            StartSoundscape(debug);
            return;
        }*/
        playerManager.SetPlayerMovement(inEditMode);
        while (!loadingManager.CanLoad(true))
        {
            await Task.Delay(100);
        }
        loadingManager.SetToStart(1);
        loadingManager.SetInfo($"Loading visuals for: {currentSoundscape.title}");
        
        currentSoundscape.OnVisualsFail(() =>
        {
            _ = OnVisualFail(currentSoundscape.title);
        });
        currentSoundscape.OnVisualsLoaded(() =>
        {
            loadingManager.Increment();  
            loadingManager.EndLoading();  
            _ = PrepareAudiosAndPlaySoundscape();
        } );
        try
        {
            ssVisual = currentSoundscape.TryGetVisuals(vrStateData.GetCurrentVisuals());         
        }
        catch
        {
            Debug.LogWarning("Caught an error trying to load the visuals of a soundscape");
            _ = OnVisualFail($"Failed to load visuals {currentSoundscape.title}");
        }
        screen = currentSoundscape.GetScreenObject();
        currentSoundscape.SetEdit(inEditMode);
        videoPlaying = true;
        if(ssVisual == null)
        {
            videoPlaying = false;
            _ = OnVisualFail($"Visuals could not be loaded for {currentSoundscape.title}", stopSoundscape: false);
        }
        return;

    }

    private async Task OnVisualFail(string errorMessage, bool stopSoundscape = true)
    {
        loadingManager.SetInfo(errorMessage); 
        int increments = 10;
        for(int i = 0; i < increments; i++)
        {
            await Task.Delay(2000/increments);
            loadingManager.Increment(1f/increments); 
        }
        if(stopSoundscape)
            StopSoundscape();
        loadingManager.EndLoading(); 
    }

    private async Task PrepareAudiosAndPlaySoundscape()
    {
        List<string> audioIds = new();
        foreach(AudioSource src in currentSoundscape.GetAudioSources())
        {
            audioIds.Add(src.GetAudioId());
        }
        soundscapeLibrary.ClearAudioCache(currentSoundscape.title);
        await soundscapeLibrary.LoadAudioClipsAsync(audioIds);
        StartSoundscape();
    }

    private void StartSoundscape()
    {
        foreach(AudioSource src in currentSoundscape.GetAudioSources())
        {
            //if an audio isnt loaded here, that means it could not be loaded, so we dont instantiate it.
            if(!soundscapeLibrary.IsAudioLoaded(src.GetAudioId())) continue;
            PlaceSoundscapeAudioSource(src, inEditMode);
        }
        currentSoundscape.RestartVisuals(ssVisual, vrStateData.GetCurrentVisuals());
        StartLoopCoroutine();
    }

    //function to start counting time to loop the SS.
    //if currrentSS time is 0, this does nothing. otherwise starts the coroutine that will then call RestartSoundscape()
    private void StartLoopCoroutine()
    {
        int time = currentSoundscape.timeSeconds;
        if(time == 0) return;
        StartCoroutine(SoundscapeCountdown(time));
    }

    IEnumerator SoundscapeCountdown(int timeSeconds)
    {
        yield return new WaitForSeconds(timeSeconds);
        //if we're debugging, loop soundscape always.
        if(currentSoundscape.loop || inEditMode)
            RestartSoundscape();
        else
            StopSoundscape();
    }

    //function is called when player r-clicks / calls the cancel action keybind.
    //we early return in edit mode because that action is used for other purposes.
    private void StopSoundscape(InputAction.CallbackContext context)
    {
        if(inEditMode) return;
        StopSoundscape();
    }

    public void StopSoundscape(string title)
    {if(title.Equals(currentSoundscape.title)) StopSoundscape();}

    public void StopSoundscape()
    {
        inEditMode = false;
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
                RenderSettings.skybox = defaultSkybox;
                DynamicGI.UpdateEnvironment();
            }
            if(screen != null) 
            {
                currentSoundscape.DeleteScreen();
                Destroy(screen);
                screen = null;
            }
            videoPlaying = false;
        }
        RemoveAudioSources();
        currentSoundscape = null;
        StopAllCoroutines();
    }

    private void RemoveAudioSources()
    {
        foreach(GameObject go in placedAudios.Values)
        {
            Destroy(go);
        }
        placedAudios = new();
    }

    public void SetAndStartSoundscape(Soundscape ss, bool debug = false)
    {
        SetSoundscape(ss);
        _ = PrepareSoundscape(debug);
    }

    public bool HasSoundscape(){return currentSoundscape != null;}
}
