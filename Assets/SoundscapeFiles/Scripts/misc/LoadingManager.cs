using System;
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
    private float totalLoadingIncrementCount = 1f;
    private bool softConcurrencyCheck = false;
    private bool queued = false;
    private Action onLoadingFinished = null;
    private Action onTimeout = null;

    private float animationTime = 0f;
    private float timeInQueue = 0f;
    private int timeoutSeconds = 5;
    private float timeSinceLastUpdate = 0;
    private bool timedout = false;

    public void SetInfo(string text)
    {
        infoText.text = text;
    }

    public void OnLoaded(Action action)
    {
        onLoadingFinished = action;
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
        totalLoadingIncrementCount = end;
        image.SetActive(true);
        loadingBar.value = 0;
        player.SetPlayerRotation(false);
        SetInfo("Starting up...");
        timeSinceLastUpdate = 0;
        timedout = false;
    }

    public void Increment(float amount = 1f)
    {
        if(timedout) return;
        if(progress < totalLoadingIncrementCount)
        {
            progress += amount;
        }
        UpdateBar();
        timeSinceLastUpdate = 0;
    }

    public float GetCurrentProgress()
    {
        return progress/totalLoadingIncrementCount;
    }

    private void UpdateBar()
    {
        loadingBar.value = GetCurrentProgress();
        if(GetCurrentProgress() >= 1f)EndLoading();
    }
    public void EndLoading()
    {
        loadingBar.value = 0;
        totalLoadingIncrementCount = 0;
        softConcurrencyCheck = false;
        if(!timedout) onLoadingFinished?.Invoke();
        onLoadingFinished = null;
        onTimeout = null;
        timeSinceLastUpdate = 0;
        timedout = false;
        //if theres another loader queued up, dont disable the visuals.
        if(queued) return;
        image.SetActive(false);
        player.SetPlayerRotation(true);
    }

    public LoadingManager OnTimeout(Action onTimeout, int timeoutSeconds = 5)
    {
        this.timeoutSeconds = timeoutSeconds;
        this.onTimeout = onTimeout;
        return this;
    }

    void Update()
    {
        if(totalLoadingIncrementCount <= 0) return;
        animationTime += Time.deltaTime;
        timeSinceLastUpdate += Time.deltaTime;
        if(animationTime > 0.5f)
        {
            animationTime = 0f;
            int dotsNow = loadingText.text[7..].Length;
            string res = "Loading";
            for(int i = 0; i < (dotsNow + 1)%4; i++)
            {
                res += ".";
            }
            loadingText.text = res;
        }
        if(timeSinceLastUpdate > timeoutSeconds)
        {
            timedout = true;
            SetInfo("Loading timed out.");
            if(timeSinceLastUpdate > timeoutSeconds + 2)
            {
                onTimeout?.Invoke();
                EndLoading();
            }
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
