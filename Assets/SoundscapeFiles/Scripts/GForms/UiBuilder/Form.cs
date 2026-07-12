using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Form
{
    public string formTitle = "";
    public string formViewId = "";
    public string formEditId = "";
    public string description = "";
    public List<FormElement> questions = new();

    // Runtime cache for downloaded form images (Ignored by JSON serialization)
    [System.NonSerialized]
    public Dictionary<string, Texture2D> cachedImages = new();

    public Texture2D GetImageForQuestion(string elementId)
{
    if (cachedImages.TryGetValue(elementId, out Texture2D tex))
    {
        return tex;
    }
    return null;
}
}