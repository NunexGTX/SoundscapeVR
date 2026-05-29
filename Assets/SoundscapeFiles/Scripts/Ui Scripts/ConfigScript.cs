using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigScript : MonoBehaviour
{
    public PlayerManager playerManager;
    [SerializeField] private GameObject mainMenu;
    private TextField reaperControllerAddressField;
    private Button reaperControllerConnect;
    private Label reaperConnectionStatusMessage;
    private float connectionMessageTimeout = 4;
    public bool connectionTimeout = false;
    private IEnumerator messageClearCoroutine;
    private ReapyControllerCommunication reapyControllerCommunication;
    private Toggle VRToggle;
    private Button returnBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        reaperControllerAddressField = root.Q<TextField>("reaperControllerAddress");
        reaperControllerConnect = root.Q<Button>("reaperControllerConnect");
        reaperConnectionStatusMessage = root.Q<Label>("reaperConnectionStatusMessage");
        reapySetup();

        VRToggle = root.Q<Toggle>("vrToggle");
        returnBtn = root.Q<Button>("returnBtn");
        //TouchScreenKeyboard.Open();

        VRToggle.RegisterValueChangedCallback(DoToggleVr);
        returnBtn.clicked += Return;
    }

    void OnDisable()
    {
        if (VRToggle != null)
            VRToggle.UnregisterValueChangedCallback(DoToggleVr);
        returnBtn.clicked -= Return;

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
                return; //Dont do anything else
            case "connecting":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.orange);
                reaperConnectionStatusMessage.text = "Connecting...";
                break;
            case "connected":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.forestGreen);
                reaperConnectionStatusMessage.text = "Connected to Reaper Controller";
                //reaperConnectionStatusMessage.text = $"Connectied to Reapy Controller at: {reapyControllerCommunication.getServerIP()}:{reapyControllerCommunication.getPort()}";
                break;
            case "failed":
                reaperConnectionStatusMessage.style.color = new StyleColor(Color.red);
                reaperConnectionStatusMessage.text = "Connection Failed";
                break;
        }

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
}
