using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundscapeLibrary: MonoBehaviour
{
    public AudioList audioList;
    public SoundscapeList soundscapeList;
    
    void Start()
    {
        audioList = AudioList.ReadFromJson();
        soundscapeList = SoundscapeList.ReadFromJson();
        //Debug.Log(audioList.Print());
    }

    public AudioSource CreateSourceAt(Audio audio, Vector3 spherePosition)
    {
        return new(audio, spherePosition);
    }
    public AudioSource CreateSourceAt(string audioId, Vector3 spherePosition)
    {
        Audio audio = audioList.TryGetAudioById(audioId);
        if(audio == null) return null;
        return CreateSourceAt(audio, spherePosition);
    }

    
    public static string GetStreamingPath(string fileName)
    {
        string filePath = fileName;

        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Standard PC StreamingAssets path
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #elif UNITY_ANDROID
            // This points to the root of the "Internal Shared Storage" 
            // visible when you plug the Quest into a PC.
            string rootPath = "/storage/emulated/0/SoundscapeVr";
            
            // Ensure the directory exists so the app doesn't crash
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            
            return Path.Combine(rootPath, filePath);
        #else
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #endif
    }
    
    public static string GetConfigPath(string fileName)
    {
        string filePath = "configs/" + fileName;

        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Standard PC StreamingAssets path
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #elif UNITY_ANDROID
            // This points to the root of the "Internal Shared Storage" 
            // visible when you plug the Quest into a PC.
            string rootPath = "/storage/emulated/0/SoundscapeVr";
            
            // Ensure the directory exists so the app doesn't crash
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            
            return Path.Combine(rootPath, filePath);
        #else
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #endif
    }
}