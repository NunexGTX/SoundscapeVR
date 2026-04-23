using System.IO;
using UnityEngine;

[System.Serializable]
public class Audio
{
    public string id;
    public string name;
    public string fileName;
    public string imagePath;
    public string type;

    public static string TYPE_STANDARD = "standard";
    public static string TYPE_3D = "3d";

}
