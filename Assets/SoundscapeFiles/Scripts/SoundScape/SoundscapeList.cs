using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SoundscapeList
{
    public List<VideoSoundscape> videoSoundscapes = new();
    public List<ImageSoundscape> imageSoundscapes = new();
    public IEnumerable<Soundscape> GetSoundscapes()
    {
        // Combines both into a single stream of Soundscape objects
        return videoSoundscapes.Cast<Soundscape>().Concat(imageSoundscapes.Cast<Soundscape>());
    }
    
    public SoundscapeList(){videoSoundscapes = new();}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static SoundscapeList ReadFromJson(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SoundscapeList>(json);
        }
        Debug.Log("Could not find soundscapes.json");
        return new();
    }

    public string Print()
    {
        return JsonUtility.ToJson(this);
    }

    public void UpdateList(Soundscape soundscape)
    {
        if(soundscape is VideoSoundscape vSoundscape)
        {
            int index = videoSoundscapes.FindIndex(s => s.title == vSoundscape.title);

            if (index != -1)
                videoSoundscapes[index] = vSoundscape;
            else
                videoSoundscapes.Add(vSoundscape);
        } else if(soundscape is ImageSoundscape iSoundscape)
        {
            int index = imageSoundscapes.FindIndex(s => s.title == iSoundscape.title);

            if (index != -1)
                imageSoundscapes[index] = iSoundscape;
            else
                imageSoundscapes.Add(iSoundscape);
        }
    }
}