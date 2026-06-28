using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class InteractorRaycast : MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private LayerMask sphereLayer;
    [SerializeField] private SoundscapePlayerEditor editor;
    [SerializeField] private NearFarInteractor UiInteractor;

    void FixedUpdate()
    {
        if (vrStateData.IsInVr)
        {
             editor.PreviewPlaceAudio(RaycastSphere());
        }
    }

    private Vector3 RaycastSphere()
    {
        if (UiInteractor.farInteractionCaster is not ICurveInteractionCaster farCaster)
            throw new System.Exception("Error: farInteractionCaster is not an ICurveInteractionCaster");

        Vector3 origin = farCaster.castOrigin.position;
        Vector3 direction = (farCaster.lastSamplePoint - origin).normalized;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, Mathf.Infinity, sphereLayer))
            throw new System.Exception("Error: Raycast did not hit the AimingSphere layer");

        return hit.point;
    }
}
