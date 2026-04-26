using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class SoundscapePlayerEditor: MonoBehaviour
{
    public SoundscapeWrapper soundscapeWrapper;

    public InputActionReference TriggerAction;
    public InputActionReference cancelAction;
    public InputActionReference scrollAction;
    public GameObject aimingSphere;
    public GameObject audioSourcePlacerObject;
    public Material placingMaterial;
    public Material defaultMaterial;
    public Material deletingMaterial;
    [SerializeField] private float scrollStep = 0.2f;
    private GameObject currentPlacingObject;

    private Audio currentSelectedAudio;
    private AudioSource currentAudioAsSource;
    private GameObject currentSelectedAudioSource;
    private enum State
    {
        Standby,
        TryPlace,
        AdjustPlace,
        TryRemove

    }
    private State currentState = State.Standby;

    void OnEnable()
    {
        cancelAction.action.performed +=  CancelState;
        TriggerAction.action.performed += Triggered;
        scrollAction.action.performed += ScrollPosition;
    }

    void OnDisable()
    {
        cancelAction.action.performed -= CancelState;
        TriggerAction.action.performed -= Triggered;
        scrollAction.action.performed -= ScrollPosition;
    }

    //Called by edit UI, when clicking on an audio source
    public void SelectAudio(Audio audio)
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        currentSelectedAudio = audio;
        currentState = State.TryPlace;
        aimingSphere.SetActive(true);
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
        if(currentSelectedAudio == null)
        {
            Debug.LogError("Tried to place a null audio");
            return;
        }
        Debug.Log("Placing audio: " + currentSelectedAudio.id + " at " + worldPos);
        //convert to sphere coords
        Vector3 sphereCoords = SoundscapeLibrary.WorldToSphere(worldPos);
        //create audiosource
        currentAudioAsSource = new(currentSelectedAudio.id, sphereCoords);
        //place placeholder gameobject
        CreatePlaceholderPlacer(worldPos);
        //change state
        currentState = State.AdjustPlace;
        aimingSphere.SetActive(false);
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

    private void Triggered(InputAction.CallbackContext context)
    {
        if(currentState == State.AdjustPlace)
            FinalizePlaceAudio();
        else if (currentState == State.TryRemove)
            DeleteAudioSource();
    }

    //called by this script to set distance to center
    private void FinalizePlaceAudio()
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        Debug.Log("Finalizing audio placement.");
        Destroy(currentPlacingObject);
        soundscapeWrapper.PlaceAudioSource(currentAudioAsSource, true, true, true);
        currentState = State.Standby;
    }

    //called by the world source objects
    public void SelectAudioSource(GameObject src)
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        if(currentState != State.Standby) return;
        src.GetComponent<MeshRenderer>().material = deletingMaterial;
        currentSelectedAudioSource = src;
        currentState = State.TryRemove;
    }

    //called by this script 
    private void DeleteAudioSource()
    {
        if(!soundscapeWrapper.HasSoundscape()) return;
        //contact wrapper to delete this src
        if(currentState != State.TryRemove) return;
        soundscapeWrapper.DeleteAudio(currentSelectedAudioSource);
        currentSelectedAudioSource = null;
        currentState = State.Standby;
    }

    
    //called by this action to cancel the current state and return to standby
    private void CancelState(InputAction.CallbackContext context)
    {
        CancelState();
    }
    private void CancelState()
    {
        if(currentState == State.Standby) return;
        currentState = State.Standby;
        currentAudioAsSource = null;
        currentSelectedAudio = null;
        if(currentSelectedAudioSource != null)
        {
            currentSelectedAudioSource.GetComponent<MeshRenderer>().material = defaultMaterial;        
            currentSelectedAudioSource = null;
        }
        if(currentPlacingObject != null)
        {
            Destroy(currentPlacingObject);       
            currentPlacingObject = null;
        }
        aimingSphere.SetActive(false);
    }

    //called to scroll the audio position
    private void ScrollPosition(InputAction.CallbackContext context)
    {
        if(currentState != State.AdjustPlace) return;
        Vector2 scrollValue = context.ReadValue<Vector2>();
        Debug.Log(scrollValue);
        float scrollY = scrollValue.y;
        Vector3 sphereCoords = currentAudioAsSource.GetSpherePosition();
        Vector3 newCoords = new()
        {
            y = sphereCoords.y,
            z = sphereCoords.z,
            x = sphereCoords.x + (scrollY > 0 ? 1 : -1) * scrollStep
        };
        Vector3 worldPos = SoundscapeLibrary.SphereToWorld(newCoords);

        Destroy(currentPlacingObject);
        CreatePlaceholderPlacer(worldPos);
        currentAudioAsSource.WithNewPosition(newCoords);
    }

    public void LeaveEdit()
    {
        CancelState();
    }
}