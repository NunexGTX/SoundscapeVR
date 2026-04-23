using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System;

public class VideoPlayer360 : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public string videoFileName;
    private RenderTexture videoTex;
    public Material Video360Material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        string videoPath = videoFileName;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        //videoPlayer.Play();

        //Wait for promise for video player to get prepared to play
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        // Now that the video is loaded, we can access its dimensions to create the image texture dynamically
        videoTex = new RenderTexture((int)source.width, (int)source.height, 0);

        // Player now draws on the texture
        source.targetTexture = videoTex;

        // Apply the video texture to the skybox
        Video360Material.SetTexture("_MainTex",videoTex);
        RenderSettings.skybox = Video360Material;
        
        // Required to update the scene lighting/reflections
        DynamicGI.UpdateEnvironment();
        
        //  Start playing
        source.Play();
    }
    
    private void OnDestroy()
    {
        // Clean up the texture when the scene changes or app closes
        if (videoTex != null) videoTex.Release();
    }

}
