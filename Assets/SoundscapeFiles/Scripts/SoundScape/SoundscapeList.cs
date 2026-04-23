using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SoundscapeList
{
    public List<VideoSoundscape> videoSoundscapes;
    public List<ImageSoundscape> imageSoundscapes;
    public IEnumerable<Soundscape> GetSoundscapes()
    {
        // Combines both into a single stream of Soundscape objects
        return videoSoundscapes.Cast<Soundscape>().Concat(imageSoundscapes.Cast<Soundscape>());
    }
    
    public SoundscapeList(){videoSoundscapes = new();}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static SoundscapeList ReadFromJson()
    {
        string path = SoundscapeLibrary.GetConfigPath("soundscapes.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SoundscapeList>(json);
        }
        return new();
    }

    public string Print()
    {
        return JsonUtility.ToJson(this);
    }

}