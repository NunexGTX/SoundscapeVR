using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class SoundscapePlayerEditor: MonoBehaviour
{
    public SoundscapeWrapper soundscapeWrapper;

    public InputActionReference confirmAction;
    public InputActionReference cancelAction;
    public InputActionReference deleteAction;
    public InputActionReference scrollAction;
    public GameObject aimingSphere;
    public GameObject audioSourcePlacerObject;
    public Material placingMaterial;
    public Material previewMaterial;
    public Material defaultMaterial;
    public Material deletingMaterial;
    public Material selectingMaterial;
    [SerializeField] private float scrollStep = 0.2f;
    [SerializeField] private float previewMovSpeed = 40f;
    [SerializeField] private GameObject distanceTextAsset;
    [SerializeField] private GameObject distanceClue;
    [SerializeField] private GameObject distanceClueNoVR;
    [SerializeField] private GameObject deleteClue;
    [SerializeField] private GameObject deleteClueNoVR;
    [SerializeField] private EditSoundscapeScript editUi;
    [SerializeField] private AudioEffectScript effectFiltersUi;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private List<GameObject> screensToHideOnSphere;
    private GameObject currentPlacingObject;
    private GameObject currentPreviewObject;
    private GameObject currentTextAsset = null;
    private GameObject currentDistanceClueAsset = null;
    private GameObject currentDeleteClueAsset = null;
    private Audio currentPickedAudio;
    private AudioSource pickedAudioAsSource;
    private GameObject currentSelectedSourceObject;
    private AudioSource.AudioData copiedAudioData = null;
    private bool placingAudio = false;

    private enum State
    {
        //standby
        Standby,
        //has selected audio from library, is placing down (azimuth, zenith)
        TryPlace,
        //has placed down (azimuth, zenith), deciding distance
        AdjustPlace,
        //first tap to delete. does nothing, its for user experience. they must delete again, then it returns to standby
        TryRemove,
        //has selected an audio
        Select,
        //try place but for when moving audio
        TryMove,
        //adjust place but for when moving audio
        AdjustMove

    }
    private State currentState = State.Standby;

    void OnEnable()
    {
        cancelAction.action.performed +=  CancelAction;
        confirmAction.action.performed += ConfirmAction;
        scrollAction.action.performed += ScrollAction;
        deleteAction.action.performed += DeleteAction;
    }

    void OnDisable()
    {
        cancelAction.action.performed -= CancelAction;
        confirmAction.action.performed -= ConfirmAction;
        scrollAction.action.performed -= ScrollAction;
        deleteAction.action.performed -= DeleteAction;
    }

    //Called by edit UI, when clicking on an audio
    public bool SelectAudio(Audio audio)
    {
        if(!soundscapeWrapper.HasSoundscape() || (currentState != State.Standby && currentState != State.Select)) return false;
        CancelState();
        currentPickedAudio = audio;
        currentState = State.TryPlace;
        SetSphereActive(true);
        SetUIVisibility(false);
        return true;
    }

    public void SetSphereActive(bool isActive)
    {
        aimingSphere.SetActive(isActive);
    }

    private void SetUIVisibility(bool visible)
    {
        if (!visible)
        {
            playerManager.HideCameraLayers(PlayerManager.HIDE_PLACING);
            foreach(GameObject go in screensToHideOnSphere)
            {
                go.SetLayerRecursively(2);
            }            
        }
        else
        {            
            playerManager.ShowCameraLayers(PlayerManager.HIDE_PLACING);
            foreach(GameObject go in screensToHideOnSphere)
            {
                go.SetLayerRecursively(5);
            }            
        }
    }

    public void FirstPlaceAudio(ActivateEventArgs eventArgs)
    {
        Vector3 worldPos = GetSelectionPoint(eventArgs);
        FirstPlaceAudio(worldPos);
    }

    public static readonly Vector3 INVALID_VEC_POS = new(-1, -1, 5436127);
    //Called by the Sphere to set azimuth and latitude
    public void FirstPlaceAudio(Vector3 worldPos)
    {
        if(worldPos.Equals(INVALID_VEC_POS)) return;
        if(!soundscapeWrapper.HasSoundscape()) return;
        if(currentState != State.TryPlace && currentState != State.TryMove) return;

        placingAudio = true;
        Destroy(currentPreviewObject);
        currentPreviewObject = null;

        Vector3 sphereCoords = SoundscapeLibrary.WorldToSphere(worldPos);
        if(currentState == State.TryPlace)
        {    
            if(currentPickedAudio == null)
            {
                UnselectLibraryAudio();
                CancelState();
                Debug.LogError("Tried to place a null audio");

                placingAudio = false;
                return;
            }
            Debug.Log("Placing audio: " + currentPickedAudio.id + " at " + worldPos);
            pickedAudioAsSource = new(currentPickedAudio.id, sphereCoords);
            //change state
            currentState = State.AdjustPlace;
        } else if (currentState == State.TryMove)
        {
            if(currentSelectedSourceObject == null)
            {
                UnselectLibraryAudio();
                CancelState();
                Debug.LogError("Tried to move a null world audio source");

                placingAudio = false;
                return;
            }
            
            WorldAudioSource src = currentSelectedSourceObject.GetComponent<WorldAudioSource>();
            Debug.Log("Moving audio source: " + src.audioSource.GetAudioId() + " to " + worldPos);
            pickedAudioAsSource = new AudioSource(src.audioSource).WithNewPosition(sphereCoords);
            currentSelectedSourceObject.GetComponent<MeshRenderer>().enabled = false;
            //change state
            currentState = State.AdjustMove;
        }
        CreatePlaceholderPlacer(worldPos);
        DistanceFeedback(worldPos);
        DistanceAdjustClue(worldPos);
        SetSphereActive(false);
        placingAudio = false;
    }

    public void PreviewPlaceAudio(Vector3 worldPos)
    {
        if (placingAudio) return;
        if(worldPos.Equals(INVALID_VEC_POS)) return;
        if(!soundscapeWrapper.HasSoundscape()) return;
        if(currentState != State.TryPlace && currentState != State.TryMove) return;
        if(currentPreviewObject == null)
        {
            CreatePlaceholderPreview(worldPos);
        }

        Vector3 targetPos = new(
            Math.Min((aimingSphere.transform.localScale.x/100)-audioSourcePlacerObject.transform.localScale.x,worldPos.x),
            Math.Min((aimingSphere.transform.localScale.y/100)-audioSourcePlacerObject.transform.localScale.y,worldPos.y),
            Math.Min((aimingSphere.transform.localScale.z/100)-audioSourcePlacerObject.transform.localScale.z,worldPos.z));
        currentPreviewObject.transform.position = Vector3.MoveTowards(
            currentPreviewObject.transform.position,
            targetPos,
            Time.deltaTime * previewMovSpeed);
    }

    private void CreatePlaceholderPlacer(Vector3 worldPos)
    {
        currentPlacingObject = Instantiate(audioSourcePlacerObject);
        currentPlacingObject.transform.position = worldPos;
        currentPlacingObject.GetComponent<MeshRenderer>().material = placingMaterial;
    }

    private void CreatePlaceholderPreview(Vector3 worldPos)
    {
        currentPreviewObject = Instantiate(audioSourcePlacerObject);
        currentPreviewObject.transform.position = worldPos;
        currentPreviewObject.GetComponent<MeshRenderer>().material = previewMaterial;
        currentPreviewObject.layer = LayerMask.NameToLayer("SoundSources");
    }

    
    private Vector3 GetSelectionPoint(ActivateEventArgs args)
    {
        if (args.interactorObject is NearFarInteractor nearFar &&
            nearFar.farInteractionCaster is ICurveInteractionCaster farCaster)
        {
            Vector3 origin = farCaster.castOrigin.position;
            Vector3 direction = (farCaster.lastSamplePoint - origin).normalized;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity))
            {
                Debug.Log($"Hit: {hit.point} on {hit.collider.gameObject.name}");
                return hit.point;
            }
        }

        throw new("Invalid arguments provided: " + args);
    }

    // called by this script for confirming/selecting
    private void ConfirmAction(InputAction.CallbackContext context)
    {
        if(currentState == State.AdjustPlace || currentState == State.AdjustMove)
            FinalizePlaceAudio();
    }

    //called by this script to set distance to center
    private void FinalizePlaceAudio()
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        Debug.Log("Finalizing audio placement.");
        Destroy(currentPlacingObject);
        if(currentSelectedSourceObject != null)
        {
            AudioSource src = currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource;
            soundscapeWrapper.UpdateSource(src.GetUUID(), pickedAudioAsSource);
            currentSelectedSourceObject = null;
        } else 
            soundscapeWrapper.PlaceNewAudioSource(pickedAudioAsSource);
        currentState = State.Standby;
        DestroyTextAsset();
        DestroyDistanceAdjustClue();
        UnselectLibraryAudio();
        UnselectSource(goExists: false);
        SetUIVisibility(true);
    }

    //called by the world source objects
    public void SelectAudioSource(GameObject src)
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        if(!soundscapeWrapper.inEditMode) return;
        if(currentState != State.Standby && currentState != State.Select) return;
        UnselectSource();
        src.GetComponent<MeshRenderer>().material = selectingMaterial;
        currentSelectedSourceObject = src;
        effectFiltersUi.editor = this;
        effectFiltersUi.SetValues(src.GetComponent<WorldAudioSource>().audioSource.audioData);
        effectFiltersUi.gameObject.SetActive(true);
        currentState = State.Select;
        Vector3 worldPos = currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource.GetSpherePosition();
        ShowSourceDeleteClue(worldPos);
        Debug.Log("State changed to select");
    }

    private void ShowSourceDeleteClue(Vector3 worldPos)
    {
        if(currentDeleteClueAsset == null)
        {
            if(soundscapeWrapper.vrStateData.IsInVr == true)
            {
                currentDeleteClueAsset = Instantiate(deleteClue);
            }
            else
            {
                currentDeleteClueAsset = Instantiate(deleteClueNoVR);
            }
        }
        
        currentDeleteClueAsset.GetComponent<AudioSourceClue>().SetAndPlace(worldPos);
    }

    //called by audio effect UI to collect the data from it and write it into currentSelectedSourceObject
    public void UpdateSourceData(AudioSource.AudioData audioData){
        AudioSource src = currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource;
        src.PasteDataFrom(audioData);
        soundscapeWrapper.UpdateSource(currentSelectedSourceObject);
        UnselectSource();
        currentState = State.Standby;
    }

    //called by this script to attempt or finalize source deletion
    private void DeleteAudioSource()
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        //contact wrapper to delete this src
        if(currentState == State.TryRemove)
        {
            soundscapeWrapper.DeleteAudio(currentSelectedSourceObject);
            UnselectSource(false);
            currentState = State.Standby;
        }
        else if (currentState == State.Select)
        {
            currentSelectedSourceObject.GetComponent<MeshRenderer>().material = deletingMaterial;
            currentState = State.TryRemove;
        }
    }
    private void UnselectSource(bool goExists = true)
    {
        if(goExists && currentSelectedSourceObject != null)
            currentSelectedSourceObject.GetComponent<WorldAudioSource>().ResetMaterial(defaultMaterial);
        currentSelectedSourceObject = null;
        effectFiltersUi.gameObject.SetActive(false);

        DestroySourceDeleteClue();
    }

    private void DestroySourceDeleteClue()
    {
        if(currentDeleteClueAsset != null)
        {
            Destroy(currentDeleteClueAsset);
            currentDeleteClueAsset = null;
        }
    }

    //called by audio effect UI button to move the source.
    //means that we're moving the currently selected source.
    public void MoveAudioSource()
    {
        if(!soundscapeWrapper.HasSoundscape() || currentState != State.Select) return;
        currentState = State.TryMove;
        SetSphereActive(true); 
        SetUIVisibility(false);
        return;
    }
    
    //called by this script to cancel the current state and return to standby
    private void CancelAction(InputAction.CallbackContext context){CancelState();}
    public void CancelState(bool leaving = false)
    {
        if(currentState == State.Standby && !leaving) return;
        
        if(currentState == State.TryRemove && !leaving)
        {
            if(currentSelectedSourceObject != null)
                currentSelectedSourceObject.GetComponent<MeshRenderer>().material = selectingMaterial; 
            currentState = State.Select;
        }
        else
        {
            currentState = State.Standby;
            if(currentSelectedSourceObject != null)
                currentSelectedSourceObject.GetComponent<MeshRenderer>().enabled = true;
            UnselectSource();
            UnselectLibraryAudio();
            //currentSelectedAudio = null;
            if(currentPlacingObject != null)
            {
                Destroy(currentPlacingObject);       
                currentPlacingObject = null;
            }
            if (currentPreviewObject != null)
            {
                Destroy(currentPreviewObject);
                currentPreviewObject = null;
            }
            SetSphereActive(false);
            DestroyTextAsset();
            DestroyDistanceAdjustClue();
        }
        SetUIVisibility(true);
    }

    //called to scroll the audio position
    private void ScrollAction(InputAction.CallbackContext context)
    {
        if(currentState != State.AdjustPlace && currentState != State.AdjustMove) return;
        Vector2 scrollValue = context.ReadValue<Vector2>();
        float scrollY = scrollValue.y;
        if(math.abs(scrollY) < 0.5) return;
        Vector3 sphereCoords = pickedAudioAsSource.GetSpherePosition();
        Vector3 newCoords = new()
        {
            y = sphereCoords.y,
            z = sphereCoords.z,
            x = sphereCoords.x + (scrollY > 0 ? 1 : -1) * scrollStep
        };
        Vector3 worldPos = SoundscapeLibrary.SphereToWorld(newCoords);
        Destroy(currentPlacingObject);
        CreatePlaceholderPlacer(worldPos);
        pickedAudioAsSource.WithNewPosition(newCoords);
        DistanceFeedback(worldPos);
        DistanceAdjustClue(worldPos);
    }

    private void DeleteAction(InputAction.CallbackContext context)
    {
        DeleteAudioSource();
    }

    private void UnselectLibraryAudio()
    {
        editUi?.UnselectAudio();
        currentPickedAudio = null;
        pickedAudioAsSource = null;
    }

    public void LeaveEdit()
    {
        CancelState(leaving: true);
        editUi.Return();
    }

    private void DestroyTextAsset()
    {
        if(currentTextAsset != null)
        {
            Destroy(currentTextAsset);
            currentTextAsset = null;
        }
    }

    private void DestroyDistanceAdjustClue()
    {
        if(currentDistanceClueAsset != null)
        {
            Destroy(currentDistanceClueAsset);
            currentDistanceClueAsset = null;
        }
    }

    private void DistanceFeedback(Vector3 worldPos)
    {
        if(currentTextAsset == null)
            currentTextAsset = Instantiate(distanceTextAsset);
        currentTextAsset.GetComponent<DistText>().SetAndPlace(worldPos);
    }

    private void DistanceAdjustClue(Vector3 worldPos)
    {
        if(currentDistanceClueAsset == null)
        {
            if(soundscapeWrapper.vrStateData.IsInVr == true)
            {
                currentDistanceClueAsset = Instantiate(distanceClue);
            }
            else
            {
                currentDistanceClueAsset = Instantiate(distanceClueNoVR);
            }
        }
        
        currentDistanceClueAsset.GetComponent<AudioSourceClue>().SetAndPlace(worldPos);
    }
    
    /*
    Calculates the right side position for the AudioSourcePlacerObject so we can put the distance move UI cue there.
    In this case, we only calculate the right side based on the face facing the player, ignoring the cube's depth
    */
    private Vector3 AudioSourceCubeRightSide(Vector3 sphereSourceWorldPos)
    {
        Vector3 audioCubeScaleBounds = audioSourcePlacerObject.GetComponent<Renderer>().bounds.size;
        float halfSide = audioCubeScaleBounds.z/2; //Since its a cube any side is fine

        return sphereSourceWorldPos + new Vector3(0,0,halfSide);
    }

    public void CopyData(AudioSource.AudioData audioData)
    {
        copiedAudioData = audioData with {audioId = currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource.GetAudioId()};
    }

    //paste data from copiedAudioData, and returns the data
    public AudioSource.AudioData PasteData()
    {
        if(copiedAudioData == null) return null;
        currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource.PasteDataFrom(copiedAudioData);
        return copiedAudioData;
    }

    public string GetCopiedName()
    {
        if(copiedAudioData == null) return "";
        else return copiedAudioData.audioId;
    }

    internal bool ShouldPreview()
    {
        return currentState == State.TryPlace || currentState == State.TryMove;
    }
}