using UnityEngine;

public class GoogleForm : MonoBehaviour
{
    public string formID;
    private string responseURL;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        responseURL = "https://docs.google.com/forms/d/e/" + formID + "/formResponse";
    }

    public void submitForm()
    {

    }

    private void gatherFormEntries()
    {

    }
    
    private string GFormPOSTRequest()
    {
        return "";
    }
}
