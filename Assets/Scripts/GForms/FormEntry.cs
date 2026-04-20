using UnityEngine;

public class FormEntry : MonoBehaviour
{
    public string entryID;
    public string questionType;
    public string questionName;
    private string value;

    public void formEntryValue(string newValue){
        value = newValue;
    }

    public string getValue(){
        return value;
    }
}
