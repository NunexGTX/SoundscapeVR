using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

public class ConfigScript : MonoBehaviour
{
    public PlayerManager playerManager;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private VrStateData vrStateData;
    private TextField reaperControllerAddressField;
    private Button reaperControllerConnect;
    private Label reaperConnectionStatusMessage;
    private Image reaperConnectionIcon;
    private float connectionMessageTimeout = 4;
    public bool connectionTimeout = false;
    private IEnumerator messageClearCoroutine;
    private IEnumerator blinkCoroutine;
    private ReapyControllerCommunication reapyControllerCommunication;
    private Toggle VRToggle;
    private Button returnBtn;
    private Button heightBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        reaperControllerAddressField = root.Q<TextField>("reaperControllerAddress");
        reaperControllerConnect = root.Q<Button>("reaperControllerConnect");
        reaperConnectionStatusMessage = root.Q<Label>("reaperConnectionStatusMessage");
        reaperConnectionIcon = root.Q<Image>("connectionIcon");
        heightBtn = root.Q<Button>("heightBtn");

        
        returnBtn = root.Q<Button>("returnBtn");
        //TouchScreenKeyboard.Open();

        returnBtn.clicked += Return;
        heightBtn.clicked += CalibrateHeight;

        VRToggle = root.Q<Toggle>("vrToggle");
        #if UNITY_ANDROID && !UNITY_EDITOR
        VRToggle.visible = false;
        VRToggle.enabledSelf = false;
        VRToggle = null; 
        #else       
        VRToggle.RegisterValueChangedCallback(DoToggleVr);
        #endif

        reapySetup();
    }

    void OnDisable()
    {
        if (VRToggle != null)
            VRToggle.UnregisterValueChangedCallback(DoToggleVr);
        returnBtn.clicked -= Return;
        heightBtn.clicked -= CalibrateHeight;

        reaperDestroyUIEvents();
    }

    void reapySetup()
    {
        reapyControllerCommunication = FindFirstObjectByType<ReapyControllerCommunication>();

        reaperUIEvents();

        //Set the default UI values
        string serverIP = reapyControllerCommunication.getServerIP();
        if (serverIP != null)
            reaperControllerAddressField.value = serverIP;

        reaperControllerConnect.SetEnabled(!string.IsNullOrEmpty(reaperControllerAddressField.value));

        //Connection Message
        setConnectionMessage(reapyControllerCommunication.connection_status());
    }

    public void setConnectionMessage(string connectionStatus)
    {
        if (messageClearCoroutine != null)
        {
            StopCoroutine(messageClearCoroutine);
            messageClearCoroutine = null;
        }
        switch (connectionStatus)
        {
            default:
                reaperConnectionStatusMessage.text = "";
                reaperConnectionStatusMessage.visible = false;
                reaperConnectionIcon.style.display = DisplayStyle.None;
                StopBlink();
                return; //Dont do anything else
            case "connecting":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.orange);
                reaperConnectionIcon.tintColor = Color.orange;
                reaperConnectionStatusMessage.text = "Connecting...";
                break;
            case "connected":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.forestGreen);
                reaperConnectionIcon.tintColor = Color.forestGreen;
                reaperConnectionStatusMessage.text = "Connected to Reaper Controller";
                StartBlink();
                //reaperConnectionStatusMessage.text = $"Connectied to Reapy Controller at: {reapyControllerCommunication.getServerIP()}:{reapyControllerCommunication.getPort()}";
                break;
            case "failed":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.red);
                reaperConnectionIcon.tintColor = Color.red;
                reaperConnectionStatusMessage.text = "Not Connected";
                break;
        }
        reaperConnectionIcon.style.display = DisplayStyle.Flex;
        reaperConnectionStatusMessage.visible = true;
        if (connectionTimeout)
        {
            messageClearCoroutine = connectionMessageClear();
            StartCoroutine(messageClearCoroutine);
        }
    }

    IEnumerator connectionMessageClear()
    {
        yield return new WaitForSeconds(connectionMessageTimeout);
        setConnectionMessage("");
    }

    void reaperUIEvents()
    {
        reaperControllerConnect.RegisterCallback<ClickEvent>(reapyConnectAction);
        reaperControllerAddressField.RegisterValueChangedCallback(OnAddressFieldChanged);
    }

    void reaperDestroyUIEvents()
    {
        reaperControllerConnect.UnregisterCallback<ClickEvent>(reapyConnectAction);
        reaperControllerAddressField.UnregisterValueChangedCallback(OnAddressFieldChanged);
    }

    void OnAddressFieldChanged(ChangeEvent<string> evt)
    {
        reaperControllerConnect.SetEnabled(!string.IsNullOrEmpty(evt.newValue));
    }

    void reapyConnectAction(ClickEvent evt)
    {
        reapyControllerCommunication.setAndConnectIP(reaperControllerAddressField.value);
    }

    private void DoToggleVr(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            playerManager.EnterVR();
        } else
        {
            playerManager.ExitVR();
        }
    }

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void StartBlink()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = Blink();
        StartCoroutine(blinkCoroutine);
    }

    private void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        reaperConnectionIcon.style.opacity = 1;
    }

    private IEnumerator Blink()
    {
        var wait = new WaitForSeconds(0.8f);
        while (true)
        {
            reaperConnectionIcon.style.opacity = 0;
            yield return wait;
            reaperConnectionIcon.style.opacity = 1;
            yield return wait;
        }
    }

    private void CalibrateHeight()
    {
        float height = GetHmdHeight();
        vrStateData.HeightOffset = height;
    }
    
    private float GetHmdHeight()
    {
        Debug.Log($"[SoundscapeVR] Getting HMD Height");
        if (Gamepad.all.Count >= 0)
        {
            Vector3 hmdPosition = InputSystem.GetDevice<XRHMD>()?.centerEyePosition.ReadValue() ?? Vector3.zero;  
            Debug.Log($"[SoundscapeVR] Returning height {hmdPosition.y}");
            return hmdPosition.y;
        }
        Debug.Log($"[SoundscapeVR] Failed to get height, defaulting to 0");
        return 0f;
    }
}
