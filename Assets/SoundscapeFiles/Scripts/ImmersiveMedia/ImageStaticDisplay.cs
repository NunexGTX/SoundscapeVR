using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System;

public class ImageStaticDisplay : MonoBehaviour
{
    public string imagePath;
    public Action onImagePrepared;
    public Action onLoadFailed;

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
                if(onImagePrepared != null) onImagePrepared.Invoke();
            } else if(uwr.result != UnityWebRequest.Result.InProgress && onLoadFailed != null) onLoadFailed.Invoke();
        }
    }
}