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
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer source)
    {
        videoTex = new RenderTexture((int)source.width, (int)source.height, 0);
        source.targetTexture = videoTex;

        Video360Material.SetTexture("_MainTex",videoTex);
        RenderSettings.skybox = Video360Material;
        DynamicGI.UpdateEnvironment();

        source.Play();
    }
    
    private void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        if (videoTex != null) videoTex.Release();
    }

}
