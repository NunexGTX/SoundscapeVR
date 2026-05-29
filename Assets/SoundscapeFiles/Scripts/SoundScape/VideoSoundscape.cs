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

    public override GameObject TryGetVisuals(VisualType visualType)
    {
        if(visualType == VisualType.NONE)
        {
            MakeSkyDark();
            onVisualsLoaded.Invoke();
            return new GameObject("NoVideo");        
        }
        if(!string.IsNullOrWhiteSpace(encompassingPath)) return GetVideo(visualType);
        if(visualType == VisualType.STATIC && !string.IsNullOrWhiteSpace(staticPath)) return GetVideo(visualType);
        //onVisualsLoaded.Invoke();
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
            v360.videoFileName = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            Material mat = new Material(Shader.Find("Skybox/Panoramic"));
            //2f corresponds to over under, 1f to side by side, 0f to none (default)
            mat.SetFloat("_Layout", 2f);
            if (!projectionType.Equals("Over Under"))
            {
                if(projectionType.Equals("Side by Side"))
                    mat.SetFloat("_Layout", 1f);
                else
                    mat.SetFloat("_Layout", 0f);
            }
            v360.Video360Material = mat;
        } else if(!string.IsNullOrWhiteSpace(staticPath))
        {
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(staticPath);
            videoPlayer.prepareCompleted += AdaptEncompassingToScreen;
            CreateScreenAdapter(createCanvas: true);
        } else if(!string.IsNullOrWhiteSpace(encompassingPath))
        {
            videoPlayer.url = SoundscapeLibrary.GetStreamingPath(encompassingPath);
            CreateScreenAdapter(createCanvas: false);
            videoPlayer.prepareCompleted += AdaptEncompassingToScreen;
        }
        videoPlayer.prepareCompleted += OnVideoPrepared;
        VideoPlayer.ErrorEventHandler onError = (vp, mess) => {onVisualsFailed.Invoke();};
        videoPlayer.errorReceived += onError;
        videoPlayer.Prepare(); 
        return go;
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        onVisualsLoaded.Invoke();
    }

    private void AdaptEncompassingToScreen(VideoPlayer player)
    {
        screenTexture = new RenderTexture((int)player.width, (int)player.height, 0);
        player.targetTexture = screenTexture;
        screenMat.SetTexture("_MainTex",screenTexture);
        if(rawImage != null) rawImage.texture = screenTexture;
    }

    public override void ClearVisualsBinding()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted -= AdaptEncompassingToScreen;
        base.ClearVisualsBinding();
    }

    public override void RestartVisuals(GameObject ssVisual, VisualType visualType)
    {
        ssVisual.GetComponent<VideoPlayer>().Stop();
        ssVisual.GetComponent<VideoPlayer>().Play();
    }
}