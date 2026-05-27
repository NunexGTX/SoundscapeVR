using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DistText : MonoBehaviour
{
    public PlayerManager playerManager = null;
    private Camera playerCamera = null;
    private bool renderHalfway = true;
    [SerializeField] private float lerperMult = 5f;
    private Vector3 destination = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerManager == null)
            playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        playerCamera = playerManager.GetCurrentCamera();
        gameObject.GetComponent<Canvas>().worldCamera = playerCamera;
    }

    public void SetAndPlace(Vector3 worldPos)
    {
        //magnitude > 2 so it doesnt render inside the block
        float dist = (float) Math.Round(worldPos.magnitude, 1);
        if(dist >= 3)
        {
            renderHalfway = true;
            destination = worldPos.normalized * ((-100)/(dist + 10) + 10);
        } else renderHalfway = false;
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if(text != null) text.text = $"r = {dist}";
    }

    // Update is called once per frame
    void Update()
    {
        if(!renderHalfway)
        {
            Vector3 referencePos = new Vector3(playerCamera.transform.position.x, 0, playerCamera.transform.position.z).normalized * -1 * 1.5f;
            referencePos.y = 1.5f;
            destination = referencePos;
        }
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, destination, lerperMult*Time.deltaTime);
        transform.LookAt(transform.position + (transform.position - playerCamera.transform.position));
    }
}
