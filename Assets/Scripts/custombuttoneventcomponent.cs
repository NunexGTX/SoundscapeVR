using UnityEngine;
using UnityEngine.UIElements;

public class custombuttoneventcomponent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button1 = root.Q<Button>("button_name_here"); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
