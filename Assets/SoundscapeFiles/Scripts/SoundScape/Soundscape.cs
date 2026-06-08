using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public abstract class Soundscape
{
    [SerializeField] public string title;
    [SerializeField] public int timeSeconds = 90;
    [SerializeField] public string staticPath = null;
    [SerializeField] public string encompassingPath = null;
    [SerializeField] public string projectionType = "None";
    [SerializeField] public bool loop = true;
    [SerializeField] protected List<AudioSource> audioSources = new();
    protected Action onVisualsLoaded = null;
    protected Action onVisualsFailed = null;
    public static readonly string VIDEO_GO_NAME = "soundscapeVideo";
    private GameObject screenObject = null;
    protected RenderTexture screenTexture = null;
    protected Material screenMat = null;
    protected RawImage rawImage = null;

    public static Soundscape CreateSoundscape(string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop, bool video)
    {
        if (video)
        {
            return new VideoSoundscape(title, time, staticPath, encompassingPath, projectionType, loop);
        } else
        {
            return new ImageSoundscape(title, time, staticPath, encompassingPath, projectionType, loop);
        }
    }
    protected Soundscape(List<AudioSource> srcs, string title, int time, string staticPath, string encompassingPath, string projectionType, bool loop)
    {
        audioSources = srcs;
        this.title = title;
        this.timeSeconds = time;
        this.staticPath = staticPath;
        this.encompassingPath = encompassingPath;
        this.projectionType = projectionType;
        this.loop = loop;
    }

    public void OnVisualsLoaded(Action action){onVisualsLoaded = action;}
    public void OnVisualsFail(Action action){onVisualsFailed = action;}
    public virtual void ClearVisualsBinding(){
        onVisualsLoaded=null;
        onVisualsFailed = null;

    }

    public string GetTitle(){return title;}

    public List<AudioSource> GetAudioSources(){return audioSources;}

    public void AddSource(AudioSource audio)
    {
        audioSources.Add(audio);
    }
    private void RemoveSource(Guid audioId)
    {
        int idxToRemove = -1;
        for(int i = 0; i < audioSources.Count; i++)
        {
            AudioSource src = audioSources[i];
            if(src.GetUUID().Equals(audioId)) idxToRemove = i;
        }
        if(idxToRemove != -1) audioSources.RemoveAt(idxToRemove);
    }
    
    public void CopySourcesFrom(Soundscape other)
    {
        audioSources = new(other.audioSources);
    }

    protected void MakeSkyDark()
    {
        Material skyboxMat = new Material(Shader.Find("Skybox/Panoramic"));
        skyboxMat.SetColor("_Tint", new Color(0, 0, 0));
        RenderSettings.skybox = skyboxMat;
        DynamicGI.UpdateEnvironment();
    }

    protected void CreateScreenAdapter(bool createCanvas)
    {
        Debug.Log("Creating cinema adapter");
        MakeSkyDark();

        if (createCanvas)
        {
            screenObject = new GameObject("Canvas"); 
            Canvas canvas = screenObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.GetComponent<RectTransform>().localScale = new(0.01f, 0.01f, 0.0125f);
            canvas.GetComponent<RectTransform>().localPosition = new(0, 0, 0);

            GameObject img = new GameObject("VideoDisplay");
            img.transform.SetParent(screenObject.transform);
            img.transform.localPosition = new(0, 0, 0);
            img.transform.localScale = new(1, 1, 1);
            img.transform.rotation = Quaternion.Euler(0, 90, 0);
            rawImage = img.AddComponent<RawImage>();
        } else
        {
            screenMat = new Material(Shader.Find("Custom/CinemaStereoProjection"));
            screenObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screenObject.GetComponent<MeshRenderer>().material = screenMat;
            screenObject.transform.position = new(5.5f, 2f, 0);
            screenObject.transform.rotation = Quaternion.Euler(0, 90, 0);
            screenObject.transform.localScale = new(8, 5, 5);
        }
    }
    public void SetEdit(bool debug)
    {
        if(!debug) return;
        if(screenObject == null) return;
        screenMat.SetFloat("Opacity", 0.5f);
        if(rawImage == null) return;
        Color c = rawImage.color;
        c.a = 0.5f;
        rawImage.color = c;
    }

    public void DeleteScreen()
    {
        if(!IsScreenCreated()) return;
        if(screenTexture != null) screenTexture.Release();
        
        //if(rawImage != null) rawImage = null;
        
        screenTexture = null;
        screenObject = null;
        rawImage = null;
    }
    protected bool IsScreenCreated() => screenObject != null;
    public static void SetProjectionType(string projectionType, Material mat)
    {
        //2f corresponds to over under, 1f to side by side, 0f to none (default)
        mat.SetFloat("_Layout", 2f);
        if (!projectionType.Equals("Over Under"))
        {
            if(projectionType.Equals("Side by Side"))
            {
                mat.SetFloat("_Layout", 1f);
                Debug.Log($"[SoundscapeVR] Read soundscape projection as Side by Side");                    
            }
            else
            {
                mat.SetFloat("_Layout", 0f);
                Debug.Log($"[SoundscapeVR] Read soundscape projection as None (As Is)");                                        
            }
        } else 
            Debug.Log($"[SoundscapeVR] Read soundscape projection as Over Under");  
    }

    public void SetScreenAtHeight(float yLevel)
    {
        if(!IsScreenCreated()) return;
        Vector3 videoPos = screenObject.transform.position;  
        videoPos = new(videoPos.x, yLevel, videoPos.z);    
        screenObject.transform.position = videoPos;
    }

    public GameObject GetScreenObject(){return screenObject;}
    public void RemoveSource(AudioSource audio){RemoveSource(audio.GetUUID());}
    public abstract Soundscape Copy();
    public abstract GameObject TryGetVisuals(VisualType visualType);
    [Obsolete("Este método não deve ter uso atualmente, verificar para remover.")]
    public abstract VisualType GetSupportedVisual();
    public abstract void RestartVisuals(GameObject ssVisual, VisualType visualType);

}