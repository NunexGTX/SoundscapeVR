using UnityEngine;

public class SceneSkybox : MonoBehaviour
{
    public static SceneSkybox Instance;

    // The Material reference
    public Material skyboxMaterial;
    public Material skyboxMaterial2;
    private GameObject room;
    public int RoomLayerIndex = 6; //Room layer

    private void Awake()
    {
        // Standard Singleton setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        room = GameObject.Find("Room");

        ApplyDefaultSkybox();
    }

    public void reactivateRoom()
    {
        Camera.main.cullingMask |= (1 << RoomLayerIndex);
        ApplyDefaultSkybox();
    }

    public void ApplyDefaultSkybox()
    {
        // This is where the 'Material' meets the engine settings
        RenderSettings.skybox = skyboxMaterial;
        
        // Required to update the scene lighting/reflections
        DynamicGI.UpdateEnvironment();
    }
}
