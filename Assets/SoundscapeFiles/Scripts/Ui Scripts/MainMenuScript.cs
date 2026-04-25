using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class UiTestClick : MonoBehaviour
{
    [SerializeField] private GameObject configMenu;
    
    [SerializeField] private GameObject soundscapeMenu;
    [SerializeField] private GameObject editMenu;
    private enum MenuType
    {
        MAIN,
        CONFIG,
        SOUNDSCAPE,
        EDIT
    }
    private Button ssBtn;
    private Button editBtn;
    private Button configBtn;
    private Button exitBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        ssBtn = root.Q<Button>("ssBtn"); 
        configBtn = root.Q<Button>("configBtn"); 
        Button questBtn = root.Q<Button>("questBtn"); 
        exitBtn = root.Q<Button>("exitBtn"); 
        editBtn = root.Q<Button>("editSsBtn"); 

        ssBtn.clicked += GoToSS;
        configBtn.clicked += GoToConfig;
        editBtn.clicked += GoToEdit;
        exitBtn.clicked += OnQuitClicked;
    }

    void OnDisable()
    {
        configBtn.clicked -= GoToSS;
        ssBtn.clicked -= GoToConfig;
        editBtn.clicked -= GoToEdit;
        exitBtn.clicked -= OnQuitClicked;
    }

    private void GoToSS()
    {
        GoTo(MenuType.SOUNDSCAPE);
    }
    
    private void GoToConfig()
    {
        GoTo(MenuType.CONFIG);
    }
    
    private void GoToEdit()
    {
        GoTo(MenuType.EDIT);
    }

    private void GoTo(MenuType targetMenu)
    {
        configMenu.SetActive(false);
        soundscapeMenu.SetActive(false);
        editMenu.SetActive(false);
        switch (targetMenu)
        {
            case MenuType.CONFIG:
                configMenu.SetActive(true);
                break;
            case MenuType.SOUNDSCAPE:
                soundscapeMenu.SetActive(true);
                break;
            case MenuType.EDIT:
                editMenu.SetActive(true);
                break;
        }
        gameObject.SetActive(false);
    }
    void OnQuitClicked()
    {
        // 1. Stop XR Subsystems (Tells SteamVR the session is over)
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
        // Reference the quitter logic or call directly
        Debug.Log("Quitting...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
