using System.Collections.Generic;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
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
    public Material defaultMaterial;
    public Material deletingMaterial;
    public Material selectingMaterial;
    [SerializeField] private float scrollStep = 0.2f;
    [SerializeField] private GameObject distanceTextAsset;
    [SerializeField] private EditScript editUi;
    [SerializeField] private AudioEffectScript effectFiltersUi;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private List<GameObject> screensToHideOnSphere;
    private GameObject currentPlacingObject;
    private GameObject currentTextAsset = null;

    private Audio currentPickedAudio;
    private AudioSource pickedAudioAsSource;
    private GameObject currentSelectedSourceObject;
    private AudioSource.AudioData copiedAudioData = null;
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

    //Called by edit UI, when clicking on an audio source
    public bool SelectAudio(Audio audio)
    {
        if(!soundscapeWrapper.HasSoundscape() || currentState != State.Standby) return false;
        currentPickedAudio = audio;
        currentState = State.TryPlace;
        SetSphereActive(true);
        return true;
    }

    private void SetSphereActive(bool isActive)
    {
        aimingSphere.SetActive(isActive);
        if (isActive)
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
        Vector3 sphereCoords = SoundscapeLibrary.WorldToSphere(worldPos);
        if(currentState == State.TryPlace)
        {    
            if(currentPickedAudio == null)
            {
                UnselectLibraryAudio();
                CancelState();
                Debug.LogError("Tried to place a null audio");
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
                return;
            }
            WorldAudioSource src = currentSelectedSourceObject.GetComponent<WorldAudioSource>();
            Debug.Log("Moving audio source: " + src.audioSource.GetAudioId() + " to " + worldPos);
            pickedAudioAsSource = new AudioSource(src.audioSource).WithNewPosition(sphereCoords);
            //change state
            currentState = State.AdjustMove;
        }
        CreatePlaceholderPlacer(worldPos);
        DistanceFeedback(worldPos);
        SetSphereActive(false);
    }

    private void CreatePlaceholderPlacer(Vector3 worldPos)
    {
        currentPlacingObject = Instantiate(audioSourcePlacerObject);
        currentPlacingObject.transform.position = worldPos;
        currentPlacingObject.GetComponent<MeshRenderer>().material = placingMaterial;
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
        UnselectLibraryAudio();
        UnselectSource(goExists: false);
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
        Debug.Log("State changed to select");
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
            currentSelectedSourceObject.GetComponent<MeshRenderer>().material = defaultMaterial;
        currentSelectedSourceObject = null;
        effectFiltersUi.gameObject.SetActive(false);
    }

    //called by audio effect UI button to move the source.
    //means that we're moving the currently selected source.
    public void MoveAudioSource()
    {
        if(!soundscapeWrapper.HasSoundscape() || currentState != State.Select) return;
        currentState = State.TryMove;
        SetSphereActive(true); 
        return;
    }

    
    //called by this script to cancel the current state and return to standby
    private void CancelAction(InputAction.CallbackContext context){CancelState();}
    private void CancelState(bool leaving = false)
    {
        if(currentState == State.Standby) return;
        
        if(currentState == State.TryRemove && !leaving)
        {
            if(currentSelectedSourceObject != null)
                currentSelectedSourceObject.GetComponent<MeshRenderer>().material = selectingMaterial; 
            currentState = State.Select;
        }
        else
        {
            UnselectSource();
            currentState = State.Standby;
            UnselectLibraryAudio();
            //currentSelectedAudio = null;
            if(currentPlacingObject != null)
            {
                Destroy(currentPlacingObject);       
                currentPlacingObject = null;
            }
            SetSphereActive(false);
            DestroyTextAsset();
        }
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
    }

    private void DeleteAction(InputAction.CallbackContext context)
    {
        DeleteAudioSource();
    }

    private void UnselectLibraryAudio()
    {
        editUi.UnselectAudio();
        currentPickedAudio = null;
        pickedAudioAsSource = null;
    }

    public void LeaveEdit()
    {
        CancelState(leaving: true);
    }

    private void DestroyTextAsset()
    {
        if(currentTextAsset != null)
        {
            Destroy(currentTextAsset);
            currentTextAsset = null;
        }
    }

    private void DistanceFeedback(Vector3 worldPos)
    {
        if(currentTextAsset == null)
            currentTextAsset = Instantiate(distanceTextAsset);
        currentTextAsset.GetComponent<DistText>().SetAndPlace(worldPos);
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
}