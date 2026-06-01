using System.Collections.Generic;
using UnityEngine;

public class AudioClipCache
{
    private Dictionary<string, AudioClip> cache = new();
    private string lastId = "";
    private List<AssetBundle> bundles = new();

    public void Clear(string id)
    {
        if(lastId.Equals(id)) return;
        lastId = id;
        Clear();
    }
    public void LoadedBundle(AssetBundle bundle){bundles.Add(bundle);}
    public void Clear()
    {
        cache.Clear();
        foreach(AssetBundle bundle in bundles)
            bundle.Unload(false);
        bundles.Clear();
    }

    public bool IsAudioLoaded(string audioId){return cache.ContainsKey(audioId);}

    public void AddAudio(string audioId, AudioClip clip)
    {
        if(IsAudioLoaded(audioId)) return;
        cache.Add(audioId, clip);
    }

    public AudioClip GetAudioClip(string audioId){return cache[audioId];}
}