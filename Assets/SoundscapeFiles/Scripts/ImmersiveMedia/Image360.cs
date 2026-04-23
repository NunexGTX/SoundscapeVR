using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System;
using System.Collections;
using UnityEngine.Networking;

public class Image360 : MonoBehaviour
{    
    public string imagePath;
    private Material defaultSkybox;
    private Material skyboxMaterial;
    private Texture2D loadedTexture;

    void Start()
    {
        defaultSkybox = RenderSettings.skybox;
        StartCoroutine(LoadAndApply());
    }
    
    private IEnumerator LoadAndApply()
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image load failed: " + uwr.error);
                yield break;
            }

            // 1. Get the texture and fix seams
            loadedTexture = DownloadHandlerTexture.GetContent(uwr);
            loadedTexture.wrapMode = TextureWrapMode.Clamp;

            if (loadedTexture != null)
            {
                Debug.Log($"Texture loaded: {loadedTexture.width}x{loadedTexture.height}");
            }

            // 2. Create the material (Skybox/Panoramic is the 360 standard)
            skyboxMaterial = new Material(Shader.Find("Skybox/Panoramic"));
            
            Debug.Log(skyboxMaterial);
            skyboxMaterial.SetTexture("_MainTex", loadedTexture);

            // 3. Apply and update
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }

    void OnDestroy()
    {
        if (loadedTexture != null) Destroy(loadedTexture);
        if (skyboxMaterial != null) Destroy(skyboxMaterial);
        RenderSettings.skybox = defaultSkybox;
        DynamicGI.UpdateEnvironment();
    }

}
