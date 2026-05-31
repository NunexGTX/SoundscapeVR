using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ImageEncompassingDisplay : MonoBehaviour
{    
    public string imagePath;
    private Material defaultSkybox;
    private Material skyboxMaterial;
    private Texture2D loadedTexture;
    public Action onImagePrepared = null;
    public Action onImageFailed = null;
    public string projectionType = "";

    void Start()
    {
        StartCoroutine(LoadAndApply());

    }
    
    private IEnumerator LoadAndApply()
    {
        imagePath = "file://" + imagePath;
        DownloadedTextureParams downloadParams = DownloadedTextureParams.Default;
        downloadParams.mipmapChain = false;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath, downloadParams))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SoundscapeVR] Soundscape Image {imagePath} load failed: " + uwr.error);
                if(onImageFailed != null) onImageFailed.Invoke();
                yield break;
            }

            // 1. Get the texture and fix seams
            loadedTexture = DownloadHandlerTexture.GetContent(uwr);
            loadedTexture.wrapMode = TextureWrapMode.Repeat;
            loadedTexture.filterMode = FilterMode.Bilinear;

            if (loadedTexture != null)
            {
                Debug.Log($"Texture loaded: {loadedTexture.width}x{loadedTexture.height}");
            }

            // 2. Create the material
            var shader = Shader.Find("Skybox/Panoramic");
            if (shader == null)
            {
                Debug.LogError("[SoundscapeVR] Skybox/Panoramic shader not found");
                yield break;
            }
            skyboxMaterial = new Material(shader);
            Soundscape.SetProjectionType(projectionType, skyboxMaterial);
            
            Debug.Log(skyboxMaterial);
            skyboxMaterial.SetTexture("_MainTex", loadedTexture);

            // 3. Apply and update
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            if(onImagePrepared != null) onImagePrepared.Invoke();
        }
    }

    void OnDestroy()
    {
        if (loadedTexture != null) Destroy(loadedTexture);
        if (skyboxMaterial != null) Destroy(skyboxMaterial);
    }

}
