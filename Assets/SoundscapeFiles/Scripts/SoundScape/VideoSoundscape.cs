using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class VideoSoundscape : Soundscape
{
    public VideoSoundscape(string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop) : this(new(), title, time, staticPath, encompassingPath, projectionType, loop){}
    protected VideoSoundscape(List<AudioSource> srcs, string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop) : base(srcs, title, time, staticPath, encompassingPath, projectionType, loop){}
    
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
        return new VideoSoundscape(
        audioSources.ConvertAll(src => new AudioSource(src)), 
        title, timeSeconds, staticPath, encompassingPath, projectionType, loop);
    }

    public override GameObject TryGetVisuals(VisualType visualType, out bool videoPlaying)
    {
        if(visualType == VisualType.NONE)
        {
            MakeSkyDark();
            onVisualsLoaded.Invoke();
            videoPlaying = false;
            return new GameObject(NO_VIDEO_NAME);        
        }
        videoPlaying = true;
        try
        {
            if(!string.IsNullOrWhiteSpace(encompassingPath)) return GetVideo(visualType);
            if(visualType == VisualType.STATIC && !string.IsNullOrWhiteSpace(staticPath)) return GetVideo(visualType);
        }
        catch
        {
            videoPlaying = false;
        }
        videoPlaying = false;
        return null;
    }

    private VideoPlayer videoPlayer = null;

    private GameObject GetVideo(VisualType visualType)
    {
        GameObject go = new GameObject(VIDEO_GO_NAME);
        videoPlayer = go.AddComponent<VideoPlayer>();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        if(visualType == VisualType.ENCOMPASSING)
        {
            //videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            VideoPlayer360 v360 = go.AddComponent<VideoPlayer360>();
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            Material mat = new Material(Shader.Find("Skybox/Panoramic"));
            SetProjectionType(projectionType, mat);                                      
            v360.Video360Material = mat;
        } else if(!string.IsNullOrWhiteSpace(staticPath))
        {
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(staticPath);
            CreateScreenAdapter(createCanvas: true);
        } else if(!string.IsNullOrWhiteSpace(encompassingPath))
        {
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            CreateScreenAdapter(createCanvas: false);
        }
        videoPlayer.prepareCompleted += OnVideoPrepared;
        VideoPlayer.ErrorEventHandler onError = (vp, mess) => {onVisualsFailed.Invoke();};
        videoPlayer.errorReceived += onError;
        videoPlayer.Prepare(); 
        return go;
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        Debug.Log("Video player prepared");
        AdaptEncompassingToScreen(player);
        onVisualsLoaded.Invoke();
    }

    private void AdaptEncompassingToScreen(VideoPlayer player)
    {
        if(!IsScreenCreated()) return;
        int videoWidth = (int)player.width;
        int videoHeight = (int)player.height;
        if(videoWidth < 1 || videoHeight < 1)
            Debug.LogError($"[SoundscapeVR] Error: Video player has invalid sizes: {videoWidth} width by {videoHeight} height");
        player.targetTexture = null; 
        screenTexture = new RenderTexture((int)player.width, (int)player.height, 0);
        player.targetTexture = screenTexture;
        if(screenMat == null)
            Debug.LogError($"[SoundscapeVR] Error: Can't adapt encompassing video to cinema projection because the relevant material is null");
        screenMat.SetTexture("_MainTex",screenTexture);
        if(rawImage != null) rawImage.texture = screenTexture;
    }

    public override void ClearVisualsBinding()
    {
        if(videoPlayer != null) videoPlayer.prepareCompleted -= OnVideoPrepared;
        base.ClearVisualsBinding();
    }

    public override void RestartVisuals(GameObject ssVisual, VisualType visualType, float delay)
    {
        Debug.Log($"[SoundscapeVR] Restarting visuals instantly via rewinding");
        if(ssVisual.name.Equals(NO_VIDEO_NAME)) return;
        VideoPlayer videoPlayer = ssVisual.GetComponent<VideoPlayer>();
        videoPlayer.time = delay; 
        if (!videoPlayer.isPlaying)
            videoPlayer.Play();
    }
}