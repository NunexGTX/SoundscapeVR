
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal.Internal;

[System.Serializable]
public class AudioList
{
    private List<Audio> sounds;
    public AudioList(){sounds = new();}

    public Audio TryGetAudioById(string audioId)
    {
        foreach(Audio audio in sounds)
        {
            if(audio.id.Equals(audioId)) return audio;
        }
        return null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AudioList ReadFromJson()
    {
        string path = SoundscapeLibrary.GetConfigPath("audios.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<AudioList>(json);
        }
        return new();
    }
    public string Print()
    {
        return JsonUtility.ToJson(this);
    }

}