using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SoundscapeLibrary: MonoBehaviour
{
    public AudioList audioList = new();
    public SoundscapeList soundscapeList = new();
    public Form form = new();
    public LoadingManager loadingManager;

    private static readonly string SOUNDSCAPE_PATH = "soundscapes.json";
    
    private async void Start()
    {
        await LoadFiles(GetSoundscapeFilePath(), GetAudioFilePath(), GetFormFilePath());

        StartCoroutine(LoadAudios(audioList.GetAudios(), loadingManager));
    }
    public void ReplaceOrAddSoundscape(Soundscape updatedSoundscape)
    {
        soundscapeList.ReplaceOrAddSoundscape(updatedSoundscape);
        if(!SaveSoundscapeList())
            Debug.LogWarning($"Soundscape '{updatedSoundscape.title}' was not saved successfully.");
    }

    public void UpdateSoundscapeData(string soundscapeId, Soundscape updatedSoundscape)
    {
        soundscapeList.UpdateSoundscapeData(soundscapeId, updatedSoundscape);
        if(!SaveSoundscapeList())
            Debug.LogWarning($"Soundscape '{soundscapeId}' was not saved successfully.");
    }

    private bool SaveSoundscapeList()
    {
        string json = JsonUtility.ToJson(soundscapeList, true);
        string fullPath = GetSoundscapeFilePath();
        
        try
        {
            File.WriteAllText(fullPath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save soundscape JSON: {e.Message}");
        }
        return false;
    }

    public AudioSource CreateSourceAt(Audio audio, Vector3 spherePosition)
    {
        return new(audio, spherePosition);
    }
    public AudioSource CreateSourceAt(string audioId, Vector3 spherePosition)
    {
        Audio audio = audioList.TryGetAudioById(audioId);
        if(audio == null) return null;
        return CreateSourceAt(audio, spherePosition);
    }

    public bool DoesSoundscapeExist(string soundscapeTitle)
    {
        foreach(Soundscape ss in soundscapeList.GetSoundscapes())
        {
            if(ss.title.Equals(soundscapeTitle)) return true;
        }
        return false;
    }

    public void DeleteSoundscape(string soundscapeTitle)
    {
        soundscapeList.RemoveSoundscape(soundscapeTitle);
        if(!SaveSoundscapeList())
            Debug.LogWarning($"{soundscapeTitle} failed to be deleted");
    }
    
    public static string GetStreamingPath(string fileName)
    {
        string filePath = fileName;

        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Standard PC StreamingAssets path
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #elif UNITY_ANDROID
            string rootPath = "/storage/emulated/0/SoundscapeVr";
            //return androidStorage.GetPersistentPath();
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #else
            return Path.Combine(Application.streamingAssetsPath, filePath);
        #endif
    }
    
    public static string GetSoundscapeFilePath()
    {
        return GetPersistentConfigPath(SOUNDSCAPE_PATH);
    }
    public static string GetAudioFilePath()
    {
        return GetStreamingPath("configs/audios.json");
    }

    private void ReadFormFromJson(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            form = JsonUtility.FromJson<Form>(json);
            GoogleForm.formID = form.formViewId;
        }
        else
        {
            Debug.LogWarning($"Could not find questionnaire_config.json in path {path}. Please ensure it exists to use the questionnaire.");
            form = new();
        }
    }

    public static string GetFormFilePath()
    {
        return GetStreamingPath("configs/questionnaire_config.json");
    }

    private static string GetPersistentConfigPath(string fileName)
    {
        string folderName = "configs";
        string rootPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        return Path.Combine(rootPath, fileName);
    }

    private static string GetPersistentPath(string path)
    {
        string rootPath = Path.Combine(Application.persistentDataPath, path);
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        return rootPath;
    }
    
    private async Task LoadFiles(string soundscapePath, string audioPath, string formPath)
    {
        soundscapeList = await Task.Run(() => SoundscapeList.ReadFromJson(soundscapePath));
        audioList      = await Task.Run(() => AudioList.ReadFromJson(audioPath));
        await Task.Run(() => ReadFormFromJson(formPath));
    }

    private IEnumerator LoadAudios(List<Audio> audios, LoadingManager loadingManager)
    {
        while (!loadingManager.CanLoad(true))
        {
            yield return new WaitForSeconds(1);
        }
        loadingManager.SetToStart(audios.Count);
        foreach (Audio audio in audios)
        {
            if (!string.IsNullOrWhiteSpace(audio.imagePath))
            {
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(audio.GetRealAudioPath()))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Image load failed: " + uwr.error);
                    }
                    else
                    {
                        Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
                        loadedTexture.wrapMode = TextureWrapMode.Clamp;
                        audio.SetTexture(loadedTexture);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(audio.fileName))
            {
                AudioType type = audio.fileType switch
                {
                    "wav" => AudioType.WAV,
                    "ogg" => AudioType.OGGVORBIS,
                    _ => AudioType.MPEG
                };

                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(GetStreamingPath(audio.fileName), type))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                        audio.SetClip(downloadedClip);
                        Debug.Log($"Successfully loaded: {audio.name}");
                    }
                    else
                    {
                        Debug.LogError($"Error loading {audio.fileName}: {www.error}");
                    }
                }
            }
            loadingManager.Increment();
            yield return null; // let the UI update
        }
        loadingManager.EndLoading();
    }

    public static Vector3 WorldToSphere(Vector3 worldCoords)
    {
        float x = worldCoords.x;
        float y = worldCoords.y;
        float z = worldCoords.z;

        float r = math.sqrt(x * x + y * y + z * z);
        if (r < 0.001f) return Vector3.zero;

        // zenith (elevation): angle from horizon (-pi/2 to pi/2)
        float zenith = math.asin(y / r);
        // azimuth: angle on XZ plane. Atan2(z, x) makes +X zero.
        float azimuth = math.atan2(z, x);

        return new Vector3(r, zenith*Mathf.Rad2Deg, azimuth*Mathf.Rad2Deg);
    }

    public static Vector3 SphereToWorld(Vector3 sphereCoords)
    {
        //azimuth 0 forward
        //azimuth -90 is right, 90 is left
        //latitude 90 is up, -90 is down
        float r = sphereCoords.x;
        float zenith = sphereCoords.y*Mathf.Deg2Rad; // elevation
        float azimuth = sphereCoords.z*Mathf.Deg2Rad;

        // Standard conversion for Y-up:
        // x = r * cos(elevation) * cos(azimuth)
        // y = r * sin(elevation)
        // z = r * cos(elevation) * sin(azimuth)
        
        float cosZenith = math.cos(zenith);
        
        float x = r * cosZenith * math.cos(azimuth);
        float y = r * math.sin(zenith);
        float z = r * cosZenith * math.sin(azimuth);

        return new Vector3(x, y, z);
    }
    public static float DistanceInSphere(Vector3 sphere1, Vector3 sphere2)
    {
        float r1 = sphere1.x;
        float zen1 = sphere1.y; // Elevation
        float azi1 = sphere1.z; // Azimuth

        float r2 = sphere2.x;
        float zen2 = sphere2.y;
        float azi2 = sphere2.z;

        // 1. Calculate the cosine of the angle between the two vectors (dot product of unit vectors)
        // cos(angle) = sin(phi1)sin(phi2) + cos(phi1)cos(phi2)cos(theta1 - theta2)
        float cosAngle = (math.sin(zen1) * math.sin(zen2)) + 
                        (math.cos(zen1) * math.cos(zen2) * math.cos(azi1 - azi2));

        // 2. Use the Law of Cosines to find the Euclidean distance
        // d^2 = r1^2 + r2^2 - 2 * r1 * r2 * cos(angle)
        float distSq = (r1 * r1) + (r2 * r2) - (2 * r1 * r2 * cosAngle);

        // 3. Return the square root, ensuring we don't sqrt a tiny negative due to float precision
        return math.sqrt(math.max(0, distSq));
    }

    public IAudioGenerator GetClipForAudio(string audioId)
    {
        foreach(Audio audio in audioList.GetAudios())
        {
            if (audio.id.Equals(audioId))
            {
                return audio.GetClip();
            }
        }
        return null;
    }

    public string GetAudioType(string audioId)
    {
        foreach(Audio audio in audioList.GetAudios())
        {
            if(audio.id.Equals(audioId)) return audio.type;
        }
        return "";
    }
}