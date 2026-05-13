using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public PlayerManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(manager == null) manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + (transform.position - manager.GetCurrentCamera().transform.position));
    }
}
