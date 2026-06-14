using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Management;

public class HomeScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    
    [SerializeField] private GameObject formMenu;
    [SerializeField] private GameObject AudioVideoMenu;
    private Button mainBtn;
    private Button formBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        formBtn = root.Q<Button>("formBtn"); 
        mainBtn = root.Q<Button>("mainBtn"); 

        formBtn.clicked += GoToForm;
        mainBtn.clicked += GoToMain;
    }

    void OnDisable()
    {
        formBtn.clicked -= GoToForm;
        mainBtn.clicked -= GoToMain;
    }

    private void GoToForm()
    {
        formMenu.SetActive(true);
        mainMenu.SetActive(false);
        gameObject.SetActive(false);
    }

    private void GoToMain()
    {
        mainMenu.SetActive(true);
        AudioVideoMenu?.SetActive(true);
        formMenu.SetActive(false);
        gameObject.SetActive(false);
    }
}
