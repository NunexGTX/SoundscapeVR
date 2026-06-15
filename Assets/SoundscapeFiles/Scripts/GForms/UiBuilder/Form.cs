using System.Collections.Generic;

[System.Serializable]
public class Form
{
    public string formTitle = "";
    public string formViewId = "";
    public string formEditId = "";
    public string description = "";
    public List<FormElement> questions = new();
}