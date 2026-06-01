using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AudioClipCache
{
    private Dictionary<string, AudioClip> cache = new();
    private string lastId = "";
    private List<AssetBundle> bundles = new();

    /// <summary>
    /// Clears the cache if the given soundscape id/title isn't currently loaded
    /// </summary>
    /// <param name="id"></param>
    public void Clear(string id)
    {
        if(lastId.Equals(id)) return;
        Clear();
        lastId = id;
    }
    public void LoadedBundle(AssetBundle bundle){bundles.Add(bundle);}
    public void Clear()
    {
        Debug.Log($"[SoundscapeVR] Clearing audio clips cache with last known id being {lastId}");
        cache.Clear();
        foreach(AssetBundle bundle in bundles)
            bundle.Unload(true);
        bundles = new();
    }

    public bool IsAudioInCache(string audioId){return cache.ContainsKey(audioId);}

    //adds the audio to the cache and loads it to make sure its ready to play.
    public async Task AddAudio(string audioId, AudioClip clip)
    {
        if(IsAudioInCache(audioId)) return;
        Debug.Log($"[SoundscapeVR] Loading audio data for clip {audioId}");
        if(!clip.LoadAudioData())
            Debug.Log($"[SoundscapeVR] Audio Clip doesn't exist in memory for audio {audioId}");
        while(clip.loadState == AudioDataLoadState.Loading)
            await Task.Yield();
        if(clip.loadState == AudioDataLoadState.Failed)
            Debug.LogError($"[SoundscapeVR] Failed to load audio clip for audio {audioId}");
        else if(clip.loadState != AudioDataLoadState.Loaded)
            Debug.LogWarning($"[SoundscapeVR] Unexpected loading state for audio {audioId}");
        cache.Add(audioId, clip);
    }

    public AudioClip GetAudioClip(string audioId){return cache[audioId];}
}