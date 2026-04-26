/*
public class WorldSrcBinding : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        gameObject.GetComponent<XRSimpleInteractable>().activated.AddListener(SelectSelf);
    }

    void OnDisable()
    {
        gameObject.GetComponent<XRSimpleInteractable>().activated.RemoveAllListeners();
    }

    private void SelectSelf(ActivateEventArgs context)
    {
        GameObject.Find("PlayerManager").GetComponent<SoundscapePlayerEditor>().SelectAudioSource(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/