using System.Collections.Generic;
using Unity.Mathematics;
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
    private GameObject currentPlacingObject;
    private GameObject currentTextAsset = null;

    private Audio currentPickedAudio;
    private AudioSource pickedAudioAsSource;
    private GameObject currentSelectedSourceObject;
    private enum State
    {
        Standby,
        TryPlace,
        AdjustPlace,
        TryRemove,
        Select

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
        aimingSphere.SetActive(true);
        return true;
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
        if(!soundscapeWrapper.HasSoundscape()) return;
        if(currentState != State.TryPlace) return;
        if(worldPos.Equals(INVALID_VEC_POS)) return;
        if(currentPickedAudio == null)
        {
            UnselectLibraryAudio();
            CancelState();
            Debug.LogError("Tried to place a null audio");
            return;
        }
        Debug.Log("Placing audio: " + currentPickedAudio.id + " at " + worldPos);
        Vector3 sphereCoords = SoundscapeLibrary.WorldToSphere(worldPos);
        pickedAudioAsSource = new(currentPickedAudio.id, sphereCoords);
        CreatePlaceholderPlacer(worldPos);
        //change state
        currentState = State.AdjustPlace;
        aimingSphere.SetActive(false);
        DistanceFeedback(worldPos);
    }

    private void CreatePlaceholderPlacer(Vector3 worldPos)
    {
        currentPlacingObject = Instantiate(audioSourcePlacerObject);
        currentPlacingObject.transform.position = worldPos;
        currentPlacingObject.GetComponent<MeshRenderer>().material = placingMaterial;
    }
    
    private Vector3 GetSelectionPoint(ActivateEventArgs args)
    {
        if (args.interactorObject is NearFarInteractor nearFar)
        {
            if (nearFar.farInteractionCaster is ICurveInteractionCaster farCaster)
            {
                return farCaster.lastSamplePoint;
            }
        }
        throw new("Invalid arguments provided: " + args);
    }

    // called by this script for confirming/selecting
    private void ConfirmAction(InputAction.CallbackContext context)
    {
        if(currentState == State.AdjustPlace)
            FinalizePlaceAudio();
    }

    //called by this script to set distance to center
    private void FinalizePlaceAudio()
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        Debug.Log("Finalizing audio placement.");
        Destroy(currentPlacingObject);
        soundscapeWrapper.PlaceNewAudioSource(pickedAudioAsSource);
        currentState = State.Standby;
        DestroyTextAsset();
        UnselectLibraryAudio();
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
        effectFiltersUi.SetValues(src);
        effectFiltersUi.gameObject.SetActive(true);
        currentState = State.Select;
        Debug.Log("State changed to select");
    }

    //called by audio effect UI to collect the data from it and write it into currentSelectedSourceObject
    public void UpdateSourceData(List<AudioEffectInstance> effectInstances, float volume, int delaySeconds, bool loop, bool mute, bool visible){
        AudioSource src = currentSelectedSourceObject.GetComponent<WorldAudioSource>().audioSource;
        src.audioEffects = effectInstances;
        src.volume = volume;
        src.loop = loop;
        src.delaySeconds = delaySeconds;
        src.mute = mute;
        src.visible = visible;
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
            aimingSphere.SetActive(false);
            DestroyTextAsset();
        }
    }

    //called to scroll the audio position
    private void ScrollAction(InputAction.CallbackContext context)
    {
        if(currentState != State.AdjustPlace) return;
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
}