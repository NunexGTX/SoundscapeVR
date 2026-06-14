using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class UiTestClick : MonoBehaviour
{
    [SerializeField] private GameObject configMenu;
    [SerializeField] private GameObject audioVideoMenu;
    
    [SerializeField] private GameObject soundscapeMenu;
    [SerializeField] private GameObject editMenu;
    [SerializeField] private GameObject formMenu;
    [SerializeField] private GameObject homeMenu;
    private enum MenuType
    {
        MAIN,
        CONFIG,
        SOUNDSCAPE,
        EDIT,
        FORM,
        HOME
    }
    private Button formBtn;
    private Button ssBtn;
    private Button editBtn;
    private Button configBtn;
    private Button exitBtn;
    private Button returnBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        formBtn = root.Q<Button>("formBtn"); 
        ssBtn = root.Q<Button>("ssBtn"); 
        configBtn = root.Q<Button>("configBtn"); 
        Button questBtn = root.Q<Button>("questBtn"); 
        exitBtn = root.Q<Button>("exitBtn"); 
        editBtn = root.Q<Button>("editSsBtn"); 
        returnBtn = root.Q<Button>("returnBtn"); 

        ssBtn.clicked += GoToSS;
        formBtn.clicked += GoToForm;
        configBtn.clicked += GoToConfig;
        editBtn.clicked += GoToEdit;
        exitBtn.clicked += OnQuitClicked;
        returnBtn.clicked += OnReturn;
    }

    void OnDisable()
    {
        configBtn.clicked -= GoToSS;
        ssBtn.clicked -= GoToConfig;
        editBtn.clicked -= GoToEdit;
        exitBtn.clicked -= OnQuitClicked;
        returnBtn.clicked -= OnReturn;
    }

    private void OnReturn() => GoTo(MenuType.HOME);

    private void GoToSS()
    {
        GoTo(MenuType.SOUNDSCAPE);
    }
    
    private void GoToForm()
    {
        GoTo(MenuType.FORM);
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
        formMenu.SetActive(false);
        homeMenu.SetActive(false);
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
            case MenuType.FORM:
                formMenu.SetActive(true);
                break;
            case MenuType.HOME:
                homeMenu.SetActive(true);
                audioVideoMenu?.SetActive(false);
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
