using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Linq;

public class GoogleForm : MonoBehaviour
{
    public static string formID = "";
    private Func<string> responseURL = () => {
        return "https://docs.google.com/forms/d/e/" + formID + "/formResponse";
    };

    public void submitForm(FormAnswersHolder answers)
    {
        StartCoroutine(GFormPOSTRequest(answers.GetAnswerList(), answers.pageCount));
    }

    IEnumerator GFormPOSTRequest(List<KeyValuePair<string, string>> entries, int pageCount)
    {
        WWWForm GFormSend = new WWWForm(); //This will build the POST Request's json
        GFormSend.AddField("pageHistory", string.Join(",", Enumerable.Range(0, pageCount)));

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
