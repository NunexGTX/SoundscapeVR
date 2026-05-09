using UnityEngine;
using UnityEngine.Android;

public class AndroidStoragePermission : MonoBehaviour
{
    private const string ManageStorage = "android.permission.MANAGE_EXTERNAL_STORAGE";

    private bool waiting_for_user_permission = false;

    // Static instance accessible from any scene
    //public static AndroidStoragePermission Instance { get; private set; }

    /*
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persists across scenes
    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (!checkManageStoragePermission())
            {
                Debug.LogWarning("User does not have Manage Storage Permissions. Opening settings and asking for user's permission");
                waiting_for_user_permission = true;
                openStorageSettings();
            }else{
                SoundscapeSceneLoad.LoadMainScene();
            }   
        #else
            Debug.LogWarning("This system is not running Android. Please remove the Android Permission Scene from this build");
            SoundscapeSceneLoad.LoadMainScene();
        #endif

    }

    private void openStorageSettings()
    {
        AndroidJavaObject intent = new AndroidJavaObject(
        "android.content.Intent",
        "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION"
        );

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("startActivity", intent);
    }

    //Should be called when the user comes back from Android settings page
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && waiting_for_user_permission)
        {
            waiting_for_user_permission = false;

            if (checkManageStoragePermission())
            {
                Debug.Log("Permission granted. Loading main scene...");
            }
            else
            {
                Debug.LogWarning("Manage Storage Permission was not granted. Use another storage path instead.");
            }
            SoundscapeSceneLoad.LoadMainScene();
        }
    }


    public static bool checkManageStoragePermission()
    {
        return Permission.HasUserAuthorizedPermission(ManageStorage);
    }

}
