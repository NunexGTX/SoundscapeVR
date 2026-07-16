using UnityEngine;
using System.IO;

public class StartupManager : MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;

    void Awake()
    {
        //if(true) return;
        
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        string path = Path.Combine(Application.dataPath, "../startupMode.txt");

        if (File.Exists(path))
        {
            string contents = File.ReadAllText(path).Trim();
            if (contents == "StartVr")
            {
                vrStateData.IsInVr = true;
                return;
            }
        }
        
        vrStateData.IsInVr = false;
        #elif UNITY_ANDROID
        vrStateData.IsInVr = true;
        #endif
    }
}