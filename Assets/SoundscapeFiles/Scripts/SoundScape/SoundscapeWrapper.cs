using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private SoundscapePlayerEditor playerEditor;
    [SerializeField] private GameObject editListUi, editSsUi;

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
    public void UpdateSource(Guid? sourceId, AudioSource audioSource, bool propagate = true)
    {
        if(sourceId != null && !sourceId.HasValue)
            Debug.LogWarning("Warning!: give source UUID is not null but has no value! Consider changing UpdateSource() method!");
        if(sourceId == null || !sourceId.HasValue) return;
        //1. remove it
        DeleteAudio(sourceId.Value);
        //2. add it.
        PlaceNewAudioSource(audioSource);

        //3. Propagate reverb effect to every other effect.
        if(!propagate || SoundscapeLibrary.IsAudioAmbisonic(audioSource.GetAudioId())) return;
        AudioEffectInstance reverbEffect = audioSource.AudioEffects.Find(eff => eff.effectData.effectId.Equals(AudioEffects.REVERB_EFFECT.effectId));
        if(reverbEffect == null) return;
        foreach(KeyValuePair<Guid, GameObject> src in new Dictionary<Guid, GameObject>(placedAudios))
        {
            //skip this source
            if(src.Key == sourceId) continue;
            AudioSource iteratorSource = src.Value.GetComponent<WorldAudioSource>().audioSource;
            if(SoundscapeLibrary.GetAudioType(iteratorSource.GetAudioId()).Equals(Audio.TYPE_AMBISONIC)) continue;
            //create new AudioSource for iterator source.
            AudioSource srcWithUpdatedReverb = iteratorSource.CloneWithEffect(reverbEffect);
            UpdateSource(src.Key, srcWithUpdatedReverb, propagate: false);
        }
    }

    public void DeleteAudio(Guid audioId)
    {
        if(currentSoundscape == null) return;
        if (placedAudios.ContainsKey(audioId))
        {
            WorldAudioSource worldSrc = placedAudios[audioId].GetComponent<WorldAudioSource>();
            AudioSource src = worldSrc.audioSource;
            EditSoundscape(src, false);
            worldSrc.DestroySource();
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
    
    private void PlaceSoundscapeAudioSource(AudioSource src)
    {
        if(currentSoundscape == null) return;
        PlaceBaseSource(src, inEditMode);
    }

    private void PlaceBaseSource(AudioSource src, bool debug)
    {
        if(placedAudios.ContainsKey(src.GetUUID())) return;
        GameObject go = Instantiate(audioSourceObject);
        go.transform.position = SoundscapeLibrary.SphereToWorld(src.GetSpherePosition());

        WorldAudioSource worldSrc = go.GetComponent<WorldAudioSource>();
        worldSrc.audioSource = src;
        worldSrc.debug = debug;
        worldSrc.soundscapeWrapper = this;
        worldSrc.library = soundscapeLibrary;
        worldSrc.playbackType = vrStateData.GetCurrentAudio();
        worldSrc.reaperController = reapyController;
        worldSrc.loadingManager = loadingManager;

        if(!inEditMode && !worldSrc.IsVisible())
        {
            try
            {
                go.SetLayerRecursively(untargetableSoundSourceLayer.GetFirstLayerIndex());                        
            }
            catch
            {
                Debug.LogWarning($"Caught an error trying to set layer recursively: {untargetableSoundSourceLayer} for audioSource {src.GetAudioId()}");
            }
        } 

        src.ApplyEffects(go);
        placedAudios.Add(worldSrc.GetUUID(), go);
        go.SetActive(true);
        _ = worldSrc.PlayAudio(true);
    }

    //called when the audio sources are already created, we just want to replay them
    private void PlayExistingSourcesFromZero()
    {
        Debug.Log("Replaying audios");
        foreach(GameObject go in placedAudios.Values)
            _ = go.GetComponent<WorldAudioSource>().PlayAudio(false);
    }

    private void RestartSoundscape(float delaySeconds  = 0)
    {
        try
        {        
            if(videoPlaying) currentSoundscape.RestartVisuals(ssVisual, vrStateData.GetCurrentVisuals(), delaySeconds);
        }
        catch
        {
            return;
        }
        PlayExistingSourcesFromZero();
        StartLoopCoroutine(delaySeconds);
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
        loadingManager.SetInfo($"Loading visuals for: ");
        
        //when loading the visuals fails (given path isnt found, other READ-WRITE exceptions)
        currentSoundscape.OnVisualsFail(() =>
        {
            _ = OnVisualFail($"Failed to load visuals for: ");
        });
        //when loading the visuals was successful
        currentSoundscape.OnVisualsLoaded(() =>
        {
            loadingManager.Increment();
            currentSoundscape.ClearVisualsBinding();
            _ = PrepareAudiosAndPlaySoundscape();
        } );

        Debug.Log($"[SoundscapeVR] Preparing soundscape");
        try
        {
            ssVisual = currentSoundscape.TryGetVisuals(vrStateData.GetCurrentVisuals(), out videoPlaying);   
            currentSoundscape.SetScreenAtHeight(vrStateData.IsInVr ? 2 - vrStateData.HeightOffset : 2);
        }
        catch(Exception e)
        {
            _ = OnVisualFail($"Caught an error trying to load the visuals of soundscape {currentSoundscape.title}\n{e.Message}");
            Debug.LogError($"[SoundscapeVR] Error loading visuals: {e.StackTrace}");
            return;
        }
        Debug.Log($"[SoundscapeVR] Gotten visuals for soundscape");
        screen = currentSoundscape.GetScreenObject();
        currentSoundscape.SetEdit(inEditMode);
        if(ssVisual == null)
        {
            videoPlaying = false;
            _ = OnVisualFail($"Null Visuals for");
        }
        return;

    }

    private async Task OnVisualFail(string errorMessage)
    {
        Debug.LogWarning($"[SoundscapeVR] Failed to load visuals for soundscape. {errorMessage}");
        loadingManager.SetInfo(errorMessage); 
        int increments = 10;
        for(int i = 0; i < increments; i++)
        {
            await Task.Delay(2000/increments);
            loadingManager.Increment(1f/increments); 
        }
        loadingManager.EndLoading(); 
        if(inEditMode)
            _ = PrepareAudiosAndPlaySoundscape();
        else
            StopSoundscape();
    }

    private async Task PrepareAudiosAndPlaySoundscape()
    {
        Debug.Log($"[SoundscapeVR] Loading audios for soundscape.");
        List<string> audioIds = new();
        foreach(AudioSource src in currentSoundscape.GetAudioSources())
            audioIds.Add(src.GetAudioId());
            
        if(vrStateData.GetCurrentAudio() == AudioListeningType.BINAURAL)
        {
            soundscapeLibrary.ClearAudioCache(currentSoundscape.title);
            //ja gere o loadingManager dentro
            await soundscapeLibrary.LoadAudioClipsAsync(audioIds);   
        }
        StartSoundscape();
    }
    private async void StartSoundscape()
    {
        while (!loadingManager.CanLoad(true))
        {
            await Task.Delay(100);
        }
        if(currentSoundscape == null) return;

        if(vrStateData.GetCurrentAudio() == AudioListeningType.ENCOMPASSING){
            reapyController.loadingManager = loadingManager;
            ReaperSendSoundscapeInformation(currentSoundscape.timeSeconds,currentSoundscape.loop);
        }

        Debug.Log($"[SoundscapeVR] Playing audios");
        List<AudioSource> audioSources = currentSoundscape.GetAudioSources();

        loadingManager.SetToStart(audioSources.Count);
        loadingManager.SetInfo("Playing audios...");
        loadingManager.OnLoaded(() => {_ = OnSoundscapeLoaded();});

        bool timedout = false;
        loadingManager.OnTimeout(() =>
        {
            Debug.LogWarning($"[SoundscapeVR] Loading 'playing audios' timedout.");
            timedout = true;
            StopSoundscape();
        }, timeoutSeconds: 5);

        // Track preparation tasks
        List<Task> placementTasks = new List<Task>();

        foreach(AudioSource src in audioSources)
        {
            if (!soundscapeLibrary.IsAudioLoaded(src.GetAudioId()) && vrStateData.GetCurrentAudio() == AudioListeningType.BINAURAL)
            {
                loadingManager.Increment();
                continue;  
            } 
            if(!timedout) 
            {
                PlaceSoundscapeAudioSource(src);
            }
        }
        if(audioSources.Count == 0) loadingManager.EndLoading();
        timedout = false;
    }
    private async Task OnSoundscapeLoaded()
    {
        Debug.Log("[SoundscapeVR] All audios loaded. Soundscape starting.");
        //corre quando tudo carrega
        //Now reaper can start playing stuff
        int delayMs = 0;
        if(vrStateData.GetCurrentAudio() == AudioListeningType.ENCOMPASSING)
        {
            delayMs = (int) await StartSoundscapeReaper();
            Debug.Log($"Delaying Soundcsape Video by {delayMs}ms");
            
        }
        RestartSoundscape(delayMs/1000L);
    }
    private void ReaperSendSoundscapeInformation(int duration, bool loop)
    {
        reapyController.NewSoundscape(duration, loop);
    }

    private async Task<long> StartSoundscapeReaper()
    {
        long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await reapyController.StartSoundscapeLockAndLoad();
        long finish = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return finish - start;
    }

    //function to start counting time to loop the SS.
    //if currrentSS time is 0, this does nothing. otherwise starts the coroutine that will then call RestartSoundscape()
    private void StartLoopCoroutine(float delaySeconds)
    {
        int time = currentSoundscape.timeSeconds;
        if(time == 0) return;
        Debug.Log($"[SoundscapeVR] Setting Soundscape to loop in {time} seconds");
        StartCoroutine(SoundscapeCountdown(time - delaySeconds));
    }

    IEnumerator SoundscapeCountdown(float timeSeconds)
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
        /*if (inEditMode)
        {
            playerEditor.LeaveEdit();
            editListUi.SetActive(true);
            editSsUi.SetActive(false);
        }*/
        Debug.Log($"[SoundscapeVR] Stopping Soundscape {currentSoundscape?.title ?? "None"}");
        inEditMode = false;
        //SaveCurrentSoundscape();
        playerManager.SetPlayerMovement(true);
        playerManager.ShowCameraLayers();
        if (videoPlaying)
        {
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
        if(gameObject != null) StopAllCoroutines();
        loadingManager.EndLoading();

        if(vrStateData.GetCurrentAudio() == AudioListeningType.ENCOMPASSING) reapyController.StopSoundscape();
        //playerEditor?.LeaveEdit();
    }

    private void RemoveAudioSources()
    {
        foreach(GameObject go in placedAudios.Values)
        {
            if(go != null) go.GetComponent<WorldAudioSource>().DestroySource();
        }
        placedAudios = new();
    }

    public void SetAndStartSoundscape(Soundscape ss, bool debug = false)
    {
        SetSoundscape(ss);
        _ = PrepareSoundscape(debug);
    }

    public bool HasSoundscape(){return currentSoundscape != null;}

    public Soundscape GetSoundscape()
    {
        return currentSoundscape;
    }
}
