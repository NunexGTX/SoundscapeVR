using System.Collections;
using UnityEngine;

public class SendSomethingTest : MonoBehaviour
{
    private GoogleForm googleForm;

    //Test FormEntries
    public FormEntry text;
    public FormEntry scale;
    public FormEntry paragraph;

    public string textText = "Ola";
    public string sclace = "3";
    public string paragraphy = "Olá, eu sou a Arminda. Gosto de dar longos passeios pela praia enquanto como um gelado epá.";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        googleForm = GameObject.FindGameObjectWithTag("GForm").GetComponent<GoogleForm>();
        StartCoroutine(LateStart());
    }
    
    IEnumerator LateStart()
    {
        //Let's make sure we can send the data first after everything else has started
        yield return null;

        text.formEntryValue(textText);
        scale.formEntryValue(sclace);
        paragraph.formEntryValue(paragraphy);

        googleForm.submitForm();
    }
}
