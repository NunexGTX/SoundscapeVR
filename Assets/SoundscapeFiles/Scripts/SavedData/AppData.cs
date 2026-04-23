using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

public class AppData
{
    

    public void SaveData() {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        string json = JsonUtility.ToJson(this);
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        File.WriteAllText(path, json);
    }

    public static SaveData LoadData() {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(path)) {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return new SaveData(); // Return default if no save exists
    }
}
