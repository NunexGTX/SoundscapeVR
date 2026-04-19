using UnityEngine;

public class FormEntry : MonoBehaviour
{
    public string entryID;
    private string value;

    public void formEntryValue(string newValue){
        value = newValue;
    }

    public string getValue(){
        return value;
    }
}
