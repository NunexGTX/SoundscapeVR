using UnityEngine;
using UnityEngine.InputSystem; // Using New Input System

public class PlayerRaycasting : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private SoundscapePlayerEditor editor;
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private LayerMask sphereLayer;
    [SerializeField] private LayerMask soundSourcesLayer; 
    [SerializeField] private LayerMask UiLayer;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && !vrStateData.IsInVr)
        {
            GameObject obj = RaycastAudioSources();
            if(obj != null && obj.CompareTag("AudioSource"))
            {
                editor.SelectAudioSource(obj);
                obj.GetComponent<WorldAudioSource>().FlipMuteState();
                return;
            }
            editor.FirstPlaceAudio(RaycastSphere());
        }
    }

    public GameObject RaycastAudioSources()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, soundSourcesLayer))
        {
            GameObject sourceFound = hit.collider.gameObject;
            return sourceFound;
        }
        return null;
    }

    public Vector3 RaycastSphere()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, sphereLayer))
        {
            Vector3 hitPoint = hit.point;
            GameObject sourceFound = hit.collider.gameObject;
            if(!sourceFound.CompareTag("AimingSphere")) return SoundscapePlayerEditor.INVALID_VEC_POS;
            return hitPoint;
        }

        return SoundscapePlayerEditor.INVALID_VEC_POS;
    }
}