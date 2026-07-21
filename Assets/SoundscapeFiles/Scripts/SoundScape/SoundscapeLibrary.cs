using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using System.Linq;

public class SoundscapeLibrary: MonoBehaviour
{
    private static SoundscapeLibrary INSTANCE = null;
    public AudioList audioList = new();
    public SoundscapeList soundscapeList = new();
    private Form form = null;
    public LoadingManager loadingManager;
    private AudioClipCache audioClipCache = new();
    public Texture2D defaultAudioIcon;

    private static readonly string SOUNDSCAPE_PATH = "soundscapes.json";
    private static readonly string REAPYIP_PATH = "controller.conf";
    private static readonly string QUESTIONNAIREJSON_PATH = "questionnaire.json";
    private async void Start()
    {
        if(INSTANCE == null) INSTANCE = this;
        Debug.Log("[SoundscapeVR] Loading files...");
        //await CheckPermissions();
        await LoadFiles(GetSoundscapeFilePath(), GetAudioFilePath());
        
        Debug.Log("[SoundscapeVR] Files loaded. Caching assets...");
        await LoadAudioIcons(audioList.GetAudios(), loadingManager);
        Debug.Log("[SoundscapeVR] All audio icons and form images cached successfully.");
    }
    public void ReplaceOrAddSoundscape(Soundscape updatedSoundscape)
    {
        soundscapeList.ReplaceOrAddSoundscape(updatedSoundscape);
        if(!SaveSoundscapeList())
            Debug.LogWarning($"Soundscape '{updatedSoundscape.title}' was not saved successfully.");
    }

    public void UpdateSoundscapeData(string soundscapeId, Soundscape updatedSoundscape)
    {
        if(!soundscapeList.UpdateSoundscapeData(soundscapeId, updatedSoundscape))
        Debug.LogWarning($"Soundscape '{soundscapeId}' failed to be updated");
        if(!SaveSoundscapeList())
            Debug.LogWarning($"Soundscape '{soundscapeId}' was not saved successfully.");
    }

    private bool SaveSoundscapeList()
    {
        string path = GetSoundscapeFilePath();
        bool success = soundscapeList.SaveToFile(path);
        if(success) 
            RefreshMediaScanner(path);
        return success;
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
            if(ss.title.ToLower().Equals(soundscapeTitle.ToLower())) return true;
        }
        return false;
    }

    public bool DeleteSoundscape(string soundscapeTitle)
    {
        soundscapeList.RemoveSoundscape(soundscapeTitle);
        if (!SaveSoundscapeList())
        {
            Debug.LogWarning($"{soundscapeTitle} failed to be deleted");
            return false;
        }
        return true;
    }
    
    public static string GetStreamingPath(string fileName = "")
    {
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            // Standard PC StreamingAssets path
            return string.IsNullOrWhiteSpace(fileName) ? Application.streamingAssetsPath : Path.Combine(Application.streamingAssetsPath, fileName);
        #elif UNITY_ANDROID
            if(HasPermissions())
                return string.IsNullOrWhiteSpace(fileName) ? GetAndroidPath() : Path.Combine(GetAndroidPath(), fileName);
            else
                return Path.Combine(Application.streamingAssetsPath, fileName); 
        #else
            return Path.Combine(Application.streamingAssetsPath, fileName);
        #endif
    }
    private static string GetStreamingConfigPath(string fileName)
    {
        string root = GetStreamingPath();
        string rootPath = Path.Combine(root, "configs");
        if (!Directory.Exists(rootPath))
        {
            Directory.CreateDirectory(rootPath); //FOR ANDROID: If we try to create a folder in streaming assets on android we'll be delighted by a read only error
            RefreshMediaScanner(rootPath);
        }
        return Path.Combine(rootPath, fileName);
    }

    private static string GetPersistentConfigPath(string fileName)
    {
        string folderName = "configs";
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            string rootPath = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
                RefreshMediaScanner(rootPath);
            }
            return Path.Combine(rootPath, fileName);
        #elif UNITY_ANDROID
        string rootPath = "";
        if(HasPermissions()){
            rootPath = Path.Combine(GetAndroidPath(), folderName);
        } else {
            rootPath = Path.Combine(Application.persistentDataPath, folderName);
        }
        if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
        return Path.Combine(rootPath, fileName);
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
        return GetStreamingConfigPath("audios.json");
    }

    public static string GetReaperControllerIPFilePath()
    {
        return GetStreamingConfigPath(REAPYIP_PATH);
    }

    public static string GetFormFilePath()
    {
        return GetStreamingConfigPath(QUESTIONNAIREJSON_PATH);
    }
    
    private static string GetAndroidPath()
    {
        //its private, so we assume the app has permissions
    #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
        {
            using (AndroidJavaObject storageDir = environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
            {
                string absRootPath = storageDir.Call<string>("getAbsolutePath");
                string customFolderPath = Path.Combine(absRootPath, "SoundscapeVR");

                if (!Directory.Exists(customFolderPath))
                {
                    Directory.CreateDirectory(customFolderPath);
                    RefreshMediaScanner(customFolderPath);
                }
                return customFolderPath;
            }
        }
    #else
    return Application.streamingAssetsPath;
    #endif
    }
    private static void RefreshMediaScanner(string filePath)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                {
                    using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
                    {
                        mediaScanner.CallStatic("scanFile", context, new string[] { filePath }, null, null);
                    }
                }
            }
        }
    #else
        Debug.Log($"Media Scanner skip: Not on Android. Path: {filePath}");
    #endif
    }
    private async Task ReadFormFromJson(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            try
            {
                form = await GoogleForm.FetchForm(json);            
            } catch
            {
                Debug.LogWarning($"[SoundscapeVR] Failed to load questionnaire information.");
                form = new();
            }
        }
        else
        {
            Debug.LogWarning($"Could not find {QUESTIONNAIREJSON_PATH} in path {path}. Please ensure it exists to use the questionnaire.");
            form = new();
        }
    }
    private async Task LoadFormImages()
    {
        if (form == null || form.questions == null) return;

        form.cachedImages ??= new Dictionary<string, Texture2D>();

        foreach (FormElement element in form.questions)
        {
            if (element.type != FormElement.IMAGE) continue;
            if (form.cachedImages.ContainsKey(element.id)) continue;

            // find url, prioritize the first slot in extra, falling back to description
            string url = "";
            if (element.extra != null && element.extra.Count > 0 && !string.IsNullOrWhiteSpace(element.extra[0]))
            {
                url = element.extra[0];
            }
            else if (!string.IsNullOrWhiteSpace(element.description))
            {
                url = element.description;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                Debug.LogWarning($"[SoundscapeVR] Form element {element.id} is marked as IMAGE but has no URL in 'extra' or 'description'.");
                continue;
            }


            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = uwr.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield(); // Keeps execution bound safely to Unity's main thread loop
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[SoundscapeVR] Form Image download failed for element {element.id} from URL '{url}': {uwr.error}");
                }
                else
                {
                    Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
                    loadedTexture.wrapMode = TextureWrapMode.Clamp;
                    form.cachedImages[element.id] = loadedTexture;
                }
            }
        }
    }

    public async Task<Form> LoadForm()
    {
        if (loadingManager.CanLoad(true))
        {
            await Task.Delay(100);
        }
        loadingManager.SetToStart(2);
        loadingManager.SetInfo("Loading Form Data");
        string formPath = GetFormFilePath();
        await ReadFormFromJson(formPath);
        loadingManager.Increment();        
        loadingManager.SetInfo("Loading Form Images");
        await LoadFormImages();
        loadingManager.Increment();        
        return form;
    }
    private async Task LoadFiles(string soundscapePath, string audioPath)
    {
        soundscapeList = await Task.Run(() => SoundscapeList.ReadFromJson(soundscapePath));
        audioList      = await Task.Run(() => AudioList.ReadFromJson(audioPath));
    }

    static bool HasPermissions()
    {
        return AndroidStoragePermission.checkManageStoragePermission();
    }
    /*static async Task CheckPermissions()
    {
        Debug.Log("[SoundscapeVR] Checking permissions");
        bool hasRead = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        bool hasWrite = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);

        Permission.RequestUserPermissions(new string[] { 
            Permission.ExternalStorageRead, 
            Permission.ExternalStorageWrite
        });

        while (!hasRead || !hasWrite)
        {
            hasRead = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
            hasWrite = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
            await Task.Delay(100);
            #if UNITY_EDITOR
            break;
            #endif
        }
        Debug.Log("[SoundscapeVR] Permissions acquired");
        GetAndroidPath();
        Debug.Log("[SoundscapeVR] Gotten an android path");
    }*/
    private async Task LoadAudioIcons(List<Audio> audios, LoadingManager loadingManager)
    {
        while (!loadingManager.CanLoad(true))
        {
            await Task.Delay(100); 
        }
        loadingManager.SetToStart(audios.Count);
        foreach (Audio audio in audios)
        {
            if (!string.IsNullOrWhiteSpace(audio.imagePath))
            {
                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://"+audio.GetRealAudioPath()))
                {
                    var operation = uwr.SendWebRequest();

                    while (!operation.isDone)
                    {
                        await Task.Yield(); // Returns control to Unity until the next frame
                    }

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[SoundscapeVR] Audio Icon {audio.fileName} load failed: " + uwr.error);
                    }
                    else
                    {
                        Texture2D loadedTexture = DownloadHandlerTexture.GetContent(uwr);
                        loadedTexture.wrapMode = TextureWrapMode.Clamp;
                        audio.SetTexture(loadedTexture);
                    }
                }
            }
            loadingManager.Increment();
            await Task.Yield(); // Equivalent to yield return null
        }
        loadingManager.EndLoading();
    }

    public async Task<IAudioGenerator> GetClipForAudio(string audioId)
    {
        if(audioClipCache.IsAudioInCache(audioId)) return audioClipCache.GetAudioClip(audioId);
        await LoadAudioClipsAsync(new List<string>{audioId});
        return audioClipCache.GetAudioClip(audioId);
    }
    public async Task LoadAudioClipsAsync(List<string> audioIds)
    {
        await LoadAudioClipsAsync(audioList.GetAudios().FindAll(audio => audioIds.Contains(audio.id)));
    }
    public async Task LoadAudioClipsAsync(List<Audio> audios)
    {
        if(audios == null || audios.Count == 0) return;
        while (!loadingManager.CanLoad(true))
        {
            await Task.Delay(100); 
        }

        loadingManager.SetToStart(audios.Count);
        int i = 0;
        foreach (Audio audio in audios)
        {
            loadingManager.SetInfo($"Loading audio clip {audio.id}: {i++}/{audios.Count}");
            if (!string.IsNullOrWhiteSpace(audio.fileName) && !audioClipCache.IsAudioInCache(audio.id))
            {
                if (audio.IsAmbisonic())
                {
                    Debug.Log($"[SoundscapeVR] Loading ambisonic audio {audio.fileName}");
                    if (string.IsNullOrWhiteSpace(audio.ambisonicAssetName))
                    {
                        Debug.LogWarning($"[SoundscapeVR] Failed to load ambisonic audio {audio.fileName} because no ambisonic asset name was given");
                        continue;
                    }
                    AudioClip downloadedClip = await LoadAudioFromBundle(audioClipCache, audio.fileName, audio.ambisonicAssetName);
                    await audioClipCache.AddAudio(audio.id, downloadedClip);
                    //TODO deal with ambisonic audio delays
                    if(downloadedClip != null) Debug.Log($"[SoundscapeVR] Successfully loaded ambisonic audio: {audio.name}");
                    else Debug.LogWarning($"[SoundscapeVR] Failed to load ambisonic audio {audio.fileName}");
                } else
                {
                    AudioType type = GetAudioType(audio.fileType, audio.fileName);

                    string url = "file://" + GetStreamingPath(audio.fileName);
                    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
                    {
                        // We await the request completion
                        var operation = www.SendWebRequest();

                        while (!operation.isDone)
                        {
                            await Task.Yield(); // Returns control to Unity until the next frame
                        }

                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            AudioClip downloadedClip = DownloadHandlerAudioClip.GetContent(www);
                            await audioClipCache.AddAudio(audio.id, downloadedClip);
                            //audio.SetClip(downloadedClip);
                            Debug.Log($"Successfully loaded: {audio.name}");
                        }
                        else
                        {
                            HandleLoadError(www, audio);
                        }
                    }
                }
            }

            loadingManager.Increment();
            await Task.Yield();
        }

        loadingManager.EndLoading();
    }

    public void ClearAudioCache(string soundscapeId)
    {
        audioClipCache.Clear(soundscapeId);
    }

    private AudioType GetAudioType(string extension, string fileName)
    {
        return extension.ToLower() switch
        {
            "wav"            => AudioType.WAV,
            "ogg"            => AudioType.OGGVORBIS,
            "mp3" or "mp2"   => AudioType.MPEG,
            "aiff" or "aif"  => AudioType.AIFF,
            "aac" or "m4a"   => AudioType.ACC,
            "it"             => AudioType.IT,
            "mod"            => AudioType.MOD,
            "s3m"            => AudioType.S3M,
            "xm"             => AudioType.XM,
            "xma"            => AudioType.XMA,
            "vag"            => AudioType.VAG,
            "flac" or "alac" => throw new NotSupportedException($"Sorry Audiophiles, '{extension}' is not supported. Convert '{fileName}' to a mediocre peasant format. I suppose you know how that works at that point"),
            _                => AudioType.UNKNOWN
        };
    }

    private void HandleLoadError(UnityWebRequest www, Audio audio)
    {
        if (audio.fileType == "m4a")
        {
            Debug.LogError($"Error loading {audio.fileName}: {www.error}. Likely Apple Lossless Codec (ALAC).");
        }
        else
        {
            Debug.LogError($"Error loading {audio.fileName}: {www.error}");
        }
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

    //Loads ambisonic audio, assumes its from a bundle
    private async static Task<AudioClip> LoadAudioFromBundle(AudioClipCache cache, string bundlePath, string assetName)
    {
        string fullPath = Path.Combine(GetStreamingPath(), bundlePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[SoundscapeVR] Loading Audio {assetName} from {bundlePath} - File doesn't exist: {fullPath}");
            return null;
        } 

        // 1. load bundle
        AssetBundle localBundle;
        AssetBundle existingBundle = Enumerable.FirstOrDefault(AssetBundle.GetAllLoadedAssetBundles(), (bundle) => {return bundle.Contains(assetName);});
        if(existingBundle != null)
        {
            localBundle = existingBundle;
        } else
        {
            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(fullPath);
            while (!bundleRequest.isDone)
                await Task.Yield();
            localBundle = bundleRequest.assetBundle;
            if (localBundle == null)
            {
                Debug.LogWarning($"[SoundscapeVR] Loading Audio {assetName} -Local bundle doesn't exist: {fullPath}");
                foreach(AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
                {
                    foreach(string name in bundle.GetAllAssetNames())
                    {                    
                        Debug.Log($"[SoundscapeVR: SoundscapeLibrary] {name}");
                    }
                }
                return null;
            } 
        }

        // 2. load audio
        AssetBundleRequest assetRequest = localBundle.LoadAssetAsync<AudioClip>(assetName);
        while (!assetRequest.isDone)
            await Task.Yield();
        AudioClip customClip = assetRequest.asset as AudioClip;
        if(existingBundle == null) cache.LoadedBundle(localBundle);
        if(customClip == null) return null;
        return customClip;
    }

    public static bool IsAudioAmbisonic(string audioId)
    {
        return GetAudioType(audioId).Equals(Audio.TYPE_AMBISONIC);
    }
    public static string GetAudioType(string audioId)
    {
        foreach(Audio audio in INSTANCE.audioList.GetAudios())
        {
            if(audio.id.Equals(audioId)) return audio.type;
        }
        return "";
    }

    public bool IsAudioLoaded(string audioId)
    {
        return audioClipCache.IsAudioInCache(audioId);
    }

    public Texture2D GetIconForAudio(string audioId)
    {
        foreach(Audio audio in audioList.GetAudios())
        {
            if (audio.id.Equals(audioId))
            {
                Texture2D tex = audio.texture2D;
                return tex == null ? defaultAudioIcon : tex;
            }
        }
        return defaultAudioIcon;
    }
}