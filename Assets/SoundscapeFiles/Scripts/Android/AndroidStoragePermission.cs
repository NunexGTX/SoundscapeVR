using UnityEngine;
using UnityEngine.Android;

public class AndroidStoragePermission : MonoBehaviour
{
    private bool waiting_for_user_permission = false;
    public int maxPermissionTries = 3; //This is the max amount of times it is allowed to show the permission requets before it starts being ignored
    private int permissionTryCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (!checkManageStoragePermission())
            {
                Debug.LogWarning("User does not have Storage Permissions. Please show the user the prompt to give the Storage Permission");
                waiting_for_user_permission = true;
                RequestManageStoragePermission();
            }else{
                SoundscapeSceneLoad.LoadMainScene();
            }
        #else
            Debug.LogWarning("This system is not running on Meta Oculus Quest. Please remove the Android Permission Scene from this build");
            SoundscapeSceneLoad.LoadMainScene();
        #endif

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
                permissionSuccessful();
            }
            else
            {
                if (permissionTryCounter < maxPermissionTries)
                {
                    waiting_for_user_permission = true;
                    RequestManageStoragePermission();
                }
                else
                {
                    Application.Quit();
                }
                
            }

        }
    }


    public static bool checkManageStoragePermission()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            // MANAGE_EXTERNAL_STORAGE is an AppOps permission — Permission.HasUserAuthorizedPermission
            // always returns false for it on API 30+. Environment.isExternalStorageManager() is the correct check.
            if (buildVersion.GetStatic<int>("SDK_INT") >= 30)
            {
                using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                    return environment.CallStatic<bool>("isExternalStorageManager");
            }
        }
        return Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
        #else
        return true;
        #endif
    }

    public void RequestManageStoragePermission()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        permissionTryCounter++;
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
        using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
        {
            string packageName = activity.Call<string>("getPackageName");
            using (AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + packageName))
            {
                intent.Call<AndroidJavaObject>("setAction", "android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION");
                intent.Call<AndroidJavaObject>("setData", uri);
                activity.Call("startActivity", intent);
            }
        }
        #endif
    }

    void permissionSuccessful()
    {
        SoundscapeSceneLoad.LoadMainScene();
    }

    public static void skipStoragePermissions()
    {
        Debug.LogWarning("Storage Permission was not granted...");
        SoundscapeSceneLoad.LoadMainScene();
    }

}
