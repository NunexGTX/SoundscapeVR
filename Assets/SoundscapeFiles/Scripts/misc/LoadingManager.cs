using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public PlayerControl player;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject image;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI infoText;
    private float progress = 0f;
    private float end = 1f;
    private bool softConcurrencyCheck = false;
    private bool queued = false;

    public void SetInfo(string text)
    {
        infoText.text = text;
    }

    public bool CanLoad(bool queueUp){
        if (softConcurrencyCheck && queueUp)
        {
            queued = true;
        }    
        return !softConcurrencyCheck;
    }

    public void SetToStart(float end)
    {
        if(softConcurrencyCheck) return;
        if(end == 0) return;
        timeInQueue = 0f;
        softConcurrencyCheck = true;
        progress = 0f;
        this.end = end;
        image.gameObject.SetActive(true);
        loadingBar.value = 0;
        player.SetPlayerRotation(false);
        SetInfo("Starting up...");
    }

    public void Increment(float amount = 1f)
    {
        if(progress < end)
        {
            progress += amount;
        }
        UpdateBar();
    }

    public float GetCurrentProgress()
    {
        return progress/end;
    }

    private void UpdateBar()
    {
        loadingBar.value = GetCurrentProgress();
        if(GetCurrentProgress() >= 1f)EndLoading();
    }
    public void EndLoading()
    {
        loadingBar.value = 0;
        softConcurrencyCheck = false;
        //if theres another loader queued up, dont disable the visuals.
        if(queued) return;
        image.gameObject.SetActive(false);
        player.SetPlayerRotation(true);
    }

    private float time = 0f;
    private float timeInQueue = 0f;

    void Update()
    {
        time += Time.deltaTime;
        if(time > 0.5f)
        {
            time = 0f;
            int dotsNow = loadingText.text[7..].Length;
            string res = "Loading";
            for(int i = 0; i < (dotsNow + 1)%4; i++)
            {
                res += ".";
            }
            loadingText.text = res;
        }
        //if someone queued up but theres nothing loading
        if(queued && !softConcurrencyCheck)
        {
            timeInQueue += Time.deltaTime;
            if(timeInQueue > 2)
            {
                timeInQueue = 0f;
                queued = false;
                EndLoading();
            }
        }
    }
}
