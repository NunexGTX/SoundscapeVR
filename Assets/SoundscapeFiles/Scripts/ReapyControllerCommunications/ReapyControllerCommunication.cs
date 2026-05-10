using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEditor.VersionControl;


public class ReapyControllerCommunication : MonoBehaviour
{
    public static string ServerIP;
    public static int ReapyCommunicationPort = 6637;
    private static TcpClient client;
    private static NetworkStream stream;

    void Start()
    {
        //TODO: start the TCP connection when a button is pressed and not on start
        client = new TcpClient(ServerIP, ReapyCommunicationPort); 
        stream = client.GetStream();
    }

    async static void message_beeboop(string command) //All communication to server needs to be async to not freeze the entire game
    {
        byte[] message = Encoding.UTF8.GetBytes(command);
        stream.Write(message, 0, message.Length); //Send message
        Debug.Log($"Sent {message} to Reaper Controller Server at {ServerIP}:{ReapyCommunicationPort}");

        // Wait for and read the response
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Debug.Log($"Reaper Controller Server response: {response} \n"
        + $"Command: {command}");
    }

    //Command functions

    //New audio source
    public static void NewTrack(string audioID, Guid soundUUID)
    {
        var msg = new { command = "new_track", data = new { AudioID = audioID, SoundUUID = soundUUID } }; //TODO Make this message with Newtonsoft or dont make a dictionary inside the dictionary
        string json = JsonUtility.ToJson(msg);

        //T
    }
    
}
