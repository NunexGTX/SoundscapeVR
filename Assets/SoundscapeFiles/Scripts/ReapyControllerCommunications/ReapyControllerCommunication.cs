using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;


public class ReapyControllerCommunication : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TcpClient client = new TcpClient("192.168.1.69", 6337);
        NetworkStream stream = client.GetStream();

        byte[] message = Encoding.UTF8.GetBytes("hi\n");
        stream.Write(message, 0, message.Length);

        Debug.Log("'Hi' Sent");

        // Wait for and read the response
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Debug.Log("Server response: " + response);

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    
}
