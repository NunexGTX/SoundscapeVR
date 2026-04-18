using UnityEngine;

public class GoogleForm : MonoBehaviour
{
    public string formID;
    private string responseURL;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        responseURL = "https://docs.google.com/forms/d/e/"+formID+"/formResponse";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
