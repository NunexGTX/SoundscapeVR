using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundscapeSceneLoad : MonoBehaviour
{
    public const string SoundscapeVRSceneName = "VrScene";

    public static void LoadMainScene()
    {
        SceneManager.LoadScene(SoundscapeVRSceneName);
    }
}
