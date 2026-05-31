using UnityEngine;

[System.Serializable]
public class Audio
{
    public string id;
    public string name;
    public string fileName;
    public string imagePath;
    public string fileType;

    public string type;
    public string ambisonicAssetName = "";
    public AssetBundle bundle = null;

    public Texture2D texture2D{get; private set; }

    public void SetTexture(Texture2D texture){this.texture2D = texture;}

    public void UnloadBundle()
    {
        if(bundle != null) bundle.Unload(false);
        bundle = null;
    }

    public bool HasImage()
    {
        return texture2D != null;
    }
    
    public string GetRealAudioPath()
    {        
        return SoundscapeLibrary.GetStreamingPath(imagePath);
    }

    public bool IsAmbisonic(){return type.Equals(TYPE_AMBISONIC);}

    public const string TYPE_STANDARD = "standard";
    public const string TYPE_AMBISONIC = "ambisonic";

}
