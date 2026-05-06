using UnityEngine;

[System.Serializable]
public class Audio
{
    public string id;
    public string name;
    public string fileName;
    public string imagePath;
    public string fileType;
    private AudioClip audioClip = null;
    private Texture2D texture2D;
    public Texture2D GetImage()
    {
        return texture2D;
    }

    public void SetClip(AudioClip clip){this.audioClip = clip;}
    public AudioClip GetClip(){return audioClip;}

    public void SetTexture(Texture2D texture){this.texture2D = texture;}

    public bool HasImage()
    {
        return texture2D != null;
    }
    
    public string GetRealAudioPath()
    {        
            return SoundscapeLibrary.GetStreamingPath(imagePath);
    }

    public string type;

    public const string TYPE_STANDARD = "standard";
    public const string TYPE_AMBISONIC = "ambisonic";

}
