using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System;

public class GoogleForm : MonoBehaviour
{
    public static string formID = "";
    private Func<string> responseURL = () => {
        return "https://docs.google.com/forms/d/e/" + formID + "/formResponse";
    };

    //When all necessary values have been assigned, we call this to gather all the FormDatas of the GForms children to prepare and send the post request
    public void submitForm(FormAnswersHolder answers)
    {
        StartCoroutine(GFormPOSTRequest(answers.GetAnswerList()));
    }

    IEnumerator GFormPOSTRequest(List<KeyValuePair<string, string>> entries)
    {
        WWWForm GFormSend = new WWWForm(); //This will build the POST Request's json

        foreach (KeyValuePair<string, string> entry in entries)
        {
            GFormSend.AddField(entry.Key, entry.Value);
        }

        // Create the POST request
        using (UnityWebRequest www = UnityWebRequest.Post(responseURL.Invoke(), GFormSend))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("GForm submission failed: " + www.error);
            }
            else
            {
                Debug.Log("Form successfully submitted to Google Forms!");
            }
        }
    }
}
