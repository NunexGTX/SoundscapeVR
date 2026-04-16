using UnityEngine;
using UnityEngine.UIElements;

public class ConfigBtn : MonoBehaviour
{
    public XRSwitcher XRSwitcher;
    [SerializeField] private GameObject mainMenu;
    private Toggle VRToggle;
    private Button returnBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        VRToggle = root.Q<Toggle>("vrToggle"); 
        returnBtn = root.Q<Button>("returnBtn");

        VRToggle.RegisterValueChangedCallback(DoToggleVr);
        returnBtn.clicked += Return;
    }

    void OnDisable()
    {
        if (VRToggle != null)
            VRToggle.UnregisterValueChangedCallback(DoToggleVr);
        returnBtn.clicked -= Return;
    }

    private void DoToggleVr(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            XRSwitcher.EnterVR();
        } else
        {
            XRSwitcher.ExitVR();
        }
    }

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }
}
