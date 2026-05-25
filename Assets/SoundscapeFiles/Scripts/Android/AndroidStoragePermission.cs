using UnityEngine;
using UnityEngine.Android;

public class AndroidStoragePermission : MonoBehaviour
{
    private bool waiting_for_user_permission = false;
    public GameObject permissionsMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        permissionsMenu.SetActive(false);
        #if UNITY_ANDROID && !UNITY_EDITOR
            if (!checkManageStoragePermission())
            {
                Debug.LogWarning("User does not have Storage Permissions. Please show the user the instructions on how to give the Storage Permission");
                waiting_for_user_permission = true;
                showStoragePermissionQuestTutorial();
                //skipStoragePermissions(); //remove this when the tutorial menu is done
            }else{
                SoundscapeSceneLoad.LoadMainScene();
            }
        #elif UNITY_EDITOR
            showStoragePermissionQuestTutorial();
        #else
            Debug.LogWarning("This system is not running Android. Please remove the Android Permission Scene from this build");
            SoundscapeSceneLoad.LoadMainScene();
        #endif

    }

    void showStoragePermissionQuestTutorial()
    {
        //TODO: Make tutorial instructions for the storage permission in the Meta Quest
        permissionsMenu.SetActive(true);
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
                Debug.LogWarning("Storage Permission was not granted...");
                //If the user does not give the permission, allow to be skipped in the UI and call directly the main scene loader
            }

        }
    }


    public static bool checkManageStoragePermission()
    {
        /*
        string ManageStorage = "android.permission.MANAGE_EXTERNAL_STORAGE";
        return Permission.HasUserAuthorizedPermission(ManageStorage); //New manage storage API (doesn't work with Meta's storage permissions)*/

        //The meta quest actually uses the old ExternalStorageReadWrite permissions
        //but we don't have access in unity to prompt the user to give that permission automatically
        return Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) && Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
    }

    void permissionSuccessful()
    {
        //TODO: Show a cute UI animation that tells the user the permission was given successfully before loading the main scene
        SoundscapeSceneLoad.LoadMainScene();
    }

    public static void skipStoragePermissions()
    {
        Debug.LogWarning("Storage Permission was not granted...");
        SoundscapeSceneLoad.LoadMainScene();
    }

}
