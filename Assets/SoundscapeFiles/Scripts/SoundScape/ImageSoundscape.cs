using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class ImageSoundscape : Soundscape
{
    [SerializeField] private string staticPath = null;
    [SerializeField] private string encompassingPath = null;

    public ImageSoundscape(List<AudioSource> srcs, string title, string staticPath, string encompassingPath) : base(srcs, title)
    {
        this.staticPath = staticPath;
        this.encompassingPath = encompassingPath;
    }
    

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
        return new ImageSoundscape(audioSources, title, staticPath, encompassingPath);
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
            Image360 i360 = go.AddComponent<Image360>();
            i360.imagePath = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            return go;
        } else
        {
            // 1. Setup Canvas for Camera Overlay
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1; // Distance from camera (Near Plane)

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
            StaticImageDisplay loader = go.AddComponent<StaticImageDisplay>();
            loader.imagePath = SoundscapeLibrary.GetStreamingPath(staticPath);

            return go;
        }

    }
}