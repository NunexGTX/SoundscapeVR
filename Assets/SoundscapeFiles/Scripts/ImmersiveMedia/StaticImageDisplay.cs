using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class StaticImageDisplay : MonoBehaviour
{
    public string imagePath;

    void Start()
    {
        StartCoroutine(LoadImage());
    }

    private IEnumerator LoadImage()
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imagePath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                // Create a Sprite from the loaded Texture
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                
                GetComponent<Image>().sprite = sprite;
            }
        }
    }
}