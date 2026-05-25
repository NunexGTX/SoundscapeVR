using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class ImageSoundscape : Soundscape
{
    public ImageSoundscape(string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop) : this(new(), title, time, staticPath, encompassingPath, loop, projectionType){}
    protected ImageSoundscape(List<AudioSource> srcs, string title, int time, string staticPath, string encompassingPath, bool loop, string projectionType) : base(srcs, title, time, staticPath, encompassingPath, projectionType, loop){}
    [Obsolete("")]
    public override VisualType GetSupportedVisual()
    {
        if(null == staticPath && null == encompassingPath)
        {
            return VisualType.NONE;
        }
        if(null == staticPath && null != encompassingPath)
        {
            return VisualType.ENCOMPASSING;
        }
        if(null != staticPath && null == encompassingPath)
        {
            return VisualType.STATIC;
        }
        return VisualType.BOTH;
    }
    public override Soundscape Copy()
    {
        return new ImageSoundscape(
            audioSources.ConvertAll(src => new AudioSource(src)), 
            title, timeSeconds, staticPath, encompassingPath, loop, projectionType
        );
    }

    public override GameObject TryGetVisuals(VisualType visualType)
    {   
        if(visualType == VisualType.NONE)
        {
            onVisualsLoaded.Invoke();
            return new GameObject("NoVideo");        
        }
        if(!string.IsNullOrWhiteSpace(encompassingPath)) return GetImage(visualType);
        if(visualType == VisualType.STATIC && !string.IsNullOrWhiteSpace(staticPath)) return GetImage(visualType);
        return null;
    }

    private GameObject GetImage(VisualType visualType)
    {
        GameObject go = new GameObject(VIDEO_GO_NAME);
        if(visualType == VisualType.ENCOMPASSING)
        {
            //encompassing
            ImageEncompassingDisplay i360 = go.AddComponent<ImageEncompassingDisplay>();
            i360.imagePath = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            i360.onImagePrepared = onVisualsLoaded;
            i360.onImageFailed = onVisualsFailed;
            return go;
        }  else if(!string.IsNullOrWhiteSpace(staticPath))
        {
            //if were asking for flat, check flat 
            // 1. Setup Canvas for Camera Overlay
            CreateScreenAdapter(createCanvas: true);
            _ =LoadCanvasImage(SoundscapeLibrary.GetStreamingPath(staticPath), applyToScreen: false, onVisualsFailed, onVisualsLoaded);
            /*Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1; // Distance from camera (Near Plane)

            //TODO verify this needs to be here and what it does.
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();

            // 2. Add UI Image component
            Image uiImage = go.AddComponent<Image>();
            uiImage.preserveAspect = true; // Prevents stretching

            // 3. Stretch Image to fill screen
            RectTransform rect = uiImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            // 4. Attach the loader script
            StaticDisplay loader = go.AddComponent<StaticDisplay>();
            loader.imagePath = SoundscapeLibrary.GetStreamingPath(staticPath);
            loader.onImagePrepared = onVisualsLoaded;
            loader.onLoadFailed = onVisualsFailed;*/
            return go;
        } else if (!string.IsNullOrWhiteSpace(encompassingPath))
        {
            //finalmente, se se quer flat mas apenas temos encompassing, usar custom mat
            CreateScreenAdapter(createCanvas: false);
            _ =LoadCanvasImage(SoundscapeLibrary.GetStreamingPath(encompassingPath), applyToScreen: true, onVisualsFailed, onVisualsLoaded);
            return go;
        }
        return null;
    }

    private async Task LoadCanvasImage(string imagePath, bool applyToScreen = true, Action onImageFailed = null, Action onImagePrepared = null)
    {
        DownloadedTextureParams downloadParams = DownloadedTextureParams.Default;
        downloadParams.mipmapChain = false;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath, downloadParams))
        {
            var operation = uwr.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image load failed: " + uwr.error);
                if(onImageFailed != null) onImageFailed.Invoke();
                return;
            }

            Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
            /*loadedTexture.wrapMode = TextureWrapMode.Repeat;
            loadedTexture.filterMode = FilterMode.Bilinear;*/
            if (loadedTexture != null)
                Debug.Log($"Texture loaded: {loadedTexture.width}x{loadedTexture.height}");

            if (applyToScreen)
                screenMat.SetTexture("_MainTex", loadedTexture);
            else
                rawImage.texture = loadedTexture;

            onImagePrepared?.Invoke();
        }
    }

    public override void RestartVisuals(GameObject ssVisual, VisualType visualType){}
}