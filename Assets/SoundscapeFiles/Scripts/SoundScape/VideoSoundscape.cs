using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class VideoSoundscape : Soundscape
{
    [SerializeField] private string staticPath = null;
    [SerializeField] private string encompassingPath = null;

    public VideoSoundscape(List<AudioSource> srcs, string title, string staticPath, string encompassingPath) : base(srcs, title)
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
        return new VideoSoundscape(audioSources, title, staticPath, encompassingPath);
    }

    public override GameObject TryGetVisuals(VisualType visualType)
    {
        if(visualType == VisualType.STATIC && null != staticPath) return GetVideo(visualType);
        if(visualType == VisualType.ENCOMPASSING && null != encompassingPath) return GetVideo(visualType);
        return null;
    }

    private GameObject GetVideo(VisualType visualType)
    {
        GameObject go = new GameObject(VIDEO_GO_NAME);
        VideoPlayer videoPlayer = go.AddComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        if(visualType == VisualType.ENCOMPASSING)
        {
            //videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            VideoPlayer360 v360 = go.AddComponent<VideoPlayer360>();
            v360.videoFileName = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            v360.Video360Material = new Material(Shader.Find("Skybox/Panoramic"));
        } else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(staticPath);
        }
        videoPlayer.Prepare();
        return go;

    }
}