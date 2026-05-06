using UnityEngine;
using System.Reflection;

public class AudioClipFlags : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        showAllAudioClipFlags();
    }

    public static void showAllAudioClipFlags()
    {
        foreach (var f in typeof(AudioClip).GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            Debug.Log(f.Name);
    }
}
