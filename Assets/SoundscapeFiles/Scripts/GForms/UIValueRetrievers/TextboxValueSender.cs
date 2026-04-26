/**
This is a class that allows to retrieve the value and send it to the form entry data holder because we can't send the inserted text with the native unity actions directly 
OnValueChange and OnEndEdit do not provide something like this*/
/*
public class TextboxValueSender : MonoBehaviour
{
    private TMP_InputField formTextField;
    public FormEntry formEntry;

    void Start()
    {
        formTextField = gameObject.GetComponent<TMP_InputField>();
    }

    private string getInputFieldText(){
        return formTextField.text;
    }

    //Call this function to update the value on the form entry data holder (that's why its the only public, so you can call it)
    public void sendTextboxValueGFormDataHolder()
    {
        formEntry.formEntryValue(getInputFieldText());
    }
}*/
