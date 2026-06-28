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

    void FixedUpdate()
    {
        if (!vrStateData.IsInVr)
        {
            editor.PreviewPlaceAudio(RaycastSphere());
        }
    }

    void Update()
    {
        if(!vrStateData.IsInVr){
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                GameObject obj = RaycastAudioSources();
                if(obj != null)
                {
                    editor.SelectAudioSource(obj);
                    obj.GetComponent<WorldAudioSource>().FlipMuteState();
                    return;
                }
                
                editor.FirstPlaceAudio(RaycastSphere());
            }
        }
    }

    public GameObject RaycastAudioSources()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        int combinedLayerMask = soundSourcesLayer | UiLayer;

        if (Physics.Raycast(ray, out hit, 100f, combinedLayerMask))
        {
            GameObject sourceFound = hit.collider.gameObject;
            return !sourceFound.CompareTag("AudioSource") ? null : sourceFound;
        }
        return null;
    }

    public Vector3 RaycastSphere()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        int combinedLayerMask = sphereLayer | UiLayer;
        if (Physics.Raycast(ray, out hit, 100f, combinedLayerMask))
        {
            Vector3 hitPoint = hit.point;
            GameObject sourceFound = hit.collider.gameObject;
            if(!sourceFound.CompareTag("AimingSphere")) return SoundscapePlayerEditor.INVALID_VEC_POS;
            return hitPoint;
        }

        return SoundscapePlayerEditor.INVALID_VEC_POS;
    }
}