using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ImageSoundscape : Soundscape
{
    public ImageSoundscape(string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop) : this(new(), title, time, staticPath, encompassingPath, loop, projectionType){}
    protected ImageSoundscape(List<AudioSource> srcs, string title, int time, string staticPath, string encompassingPath, bool loop, string projectionType) : base(srcs, title, time, staticPath, encompassingPath, projectionType, loop){}

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
        return new ImageSoundscape(audioSources, title, timeSeconds, staticPath, encompassingPath, loop, projectionType);
    }

    public override GameObject TryGetVisuals(VisualType visualType)
    {
        if(visualType == VisualType.STATIC && null != staticPath) return GetImage(visualType);
        if(visualType == VisualType.ENCOMPASSING && null != encompassingPath) return GetImage(visualType);
        return null;
    }

    private GameObject GetImage(VisualType visualType)
    {
        GameObject go = new GameObject(VIDEO_GO_NAME);
        if(visualType == VisualType.ENCOMPASSING)
        {
            ImageEncompassingDisplay i360 = go.AddComponent<ImageEncompassingDisplay>();
            i360.imagePath = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            i360.onImagePrepared = onVisualsLoaded;
            return go;
        } else
        {
            // 1. Setup Canvas for Camera Overlay
            Canvas canvas = go.AddComponent<Canvas>();
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
            ImageStaticDisplay loader = go.AddComponent<ImageStaticDisplay>();
            loader.imagePath = SoundscapeLibrary.GetStreamingPath(staticPath);
            loader.onImagePrepared = onVisualsLoaded;
            
            return go;
        }

    }

    public override void RestartVisuals(GameObject ssVisual, VisualType visualType){}
}