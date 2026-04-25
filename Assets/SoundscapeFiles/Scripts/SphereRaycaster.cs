using UnityEngine;
using UnityEngine.InputSystem; // Using New Input System

public class SphereRaycaster : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SoundscapePlayerEditor editor;
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private LayerMask sphereLayer; // Put your sphere on a specific layer
    [SerializeField] private LayerMask soundSourcesLayer; // Put your sphere on a specific layer
    [SerializeField] private LayerMask UiLayer; // Put your sphere on a specific layer

    void Update()
    {
        // Example: Trigger raycast on Mouse Click or Controller Trigger
        if (Mouse.current.leftButton.wasPressedThisFrame && !vrStateData.IsInVr)
        {
            GameObject obj = RaycastAudioSources();
            if(obj != null) editor.SelectAudioSource(obj);
            editor.FirstPlaceAudio(RaycastSphere());
        }
    }

    public GameObject RaycastAudioSources()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, UiLayer))
        {
            GameObject sourceFound = hit.collider.gameObject;
            if(!sourceFound.CompareTag("AudioSource")) return null;
            return sourceFound;
        }
        return null;
    }

    public Vector3 RaycastSphere()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, UiLayer))
        {
            Vector3 hitPoint = hit.point;
            GameObject sourceFound = hit.collider.gameObject;
            if(!sourceFound.CompareTag("AimingSphere")) return SoundscapePlayerEditor.INVALID_VEC_POS;
            return hitPoint;
        }

        return SoundscapePlayerEditor.INVALID_VEC_POS;
    }
}