using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Video;

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
        Debug.Log("Starting 360 video");
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        // Now that the video is loaded, we can access its dimensions to create the image texture dynamically
        videoTex = new RenderTexture((int)source.width, (int)source.height, 0);
        //videoTex.dimension = TextureDimension.Cube;

        // Player now draws on the texture
        source.targetTexture = videoTex;

        // Apply the video texture to the skybox
        Video360Material.SetTexture("_MainTex",videoTex);
        RenderSettings.skybox = Video360Material;
        
        // Required to update the scene lighting/reflections
        DynamicGI.UpdateEnvironment();
        
        //  Start playing
        source.Play();
        Debug.Log("Playing 360 video");
    }
    
    private void OnDestroy()
    {
        // Clean up the texture when the scene changes or app closes
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        if (videoTex != null) videoTex.Release();
        Debug.Log("Destroying 360 video");
    }

}
