using System;
using SerializableCallback;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class SphereInteraction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ClickOnSphere(ActivateEventArgs args)
    {
        String hand = args.interactorObject.handedness.ToString();
        Vector3 hitPos = GetSelectionPoint(args);
    }

    private Vector3 GetSelectionPoint(ActivateEventArgs args)
    {
        if (args.interactorObject is NearFarInteractor nearFar)
        {
            // 2. Get the Far Interactor (which handles the ray)
            String region = nearFar.selectionRegion.ToString();
            
            if (nearFar.farInteractionCaster is ICurveInteractionCaster farCaster)
            {
                return farCaster.lastSamplePoint;
            }
        }
        throw new("Invalid arguments provided: " + args);
    }
}
