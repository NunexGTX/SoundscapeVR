using UnityEngine;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class AndroidStoragePermission : MonoBehaviour
{
    public static string MainScene = "VrScene";

    // Static instance accessible from any scene
    public static AndroidStoragePermission Instance { get; private set; }

    public bool PermissionsAcquired { get; private set; }

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            bool hasAll = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
                        Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);

            if (hasAll)
            {
                LoadMainScene(); // Already granted, skip straight through
            }
            else
            {
                RequestPermissions();
            }
        #else
            Debug.LogWarning("This system is not running Android. Please remove the Android Permission Scene from this build");
            LoadMainScene();
        #endif

    }

    private void RequestPermissions()
    {
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += OnGranted;
        callbacks.PermissionDenied += OnDenied;

        Permission.RequestUserPermissions(new string[]
        {
            Permission.ExternalStorageRead,
            Permission.ExternalStorageWrite
        }, callbacks);
    }

    private void OnGranted(string permission)
    {
        // Check if ALL permissions are now granted before proceeding
        bool hasAll = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
                      Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
        if (!hasAll)
        {
            Debug.LogError("Storage Permission was Granted, but the permissions were not acquired.");
        }
        //We procceed anyways
        LoadMainScene();
        
    }

    private void OnDenied(string permission)
    {
        Debug.LogWarning($"Storage Permission denied: {permission}");
        // Optionally show a UI message, or still load the scene with limited functionality
        LoadMainScene();
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene(MainScene);
    }

}
