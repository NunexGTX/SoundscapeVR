
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AudioList
{
    public List<Audio> sounds;
    public AudioList(){sounds = new();}
    public IEnumerable<Audio> GetAudios()
    {
        // Combines both into a single stream of Soundscape objects
        return sounds.Cast<Audio>();
    }
    

    public Audio TryGetAudioById(string audioId)
    {
        foreach(Audio audio in sounds)
        {
            if(audio.id.Equals(audioId)) return audio;
        }
        return null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static AudioList ReadFromJson(string path)
    {
        if (File.Exists(path))
        {
            Debug.Log("Found Audio configuration file.");
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<AudioList>(json);
        }
        Debug.Log("Could not find Audio configuration file.");
        return new();
    }

    
    public string Print()
    {
        return JsonUtility.ToJson(this);
    }

}