using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEditor.VersionControl;


public class ReapyControllerCommunication : MonoBehaviour
{
    public string ServerIP;
    public int ReapyCommunicationPort = 6337;
    private TcpClient client;
    private NetworkStream stream;

    //Commands
    [SerializeField]
    private string newTrackCommand = "new_track";

    [SerializeField]
    private string sourcePositionCommand = "source_position";

    [SerializeField]
    private string deleteTrackCommand = "delete_track";

    [SerializeField]
    private string resetCommand = "RESET";

    void Start()
    {
        //TODO: start the TCP connection when a button is pressed and not on start
        client = new TcpClient(ServerIP, ReapyCommunicationPort); 
        stream = client.GetStream();
    }

    async void message_beeboop(string command) //All communication to server needs to be async to not freeze the entire game
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
    public void NewTrack(string audioID, Guid soundUUID, bool loop)
    {
        var msg = new NewTrack {command = newTrackCommand, AudioID = audioID, SoundUUID = soundUUID.ToString(), Loop = loop};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    //Update Audio Source Position
    public void UpdateSourcePosition(Guid soundUUID, float azim, float elev, float Distance)
    {
        var msg = new SourcePosition {command = sourcePositionCommand, SoundUUID = soundUUID.ToString(), azim = azim, elev = elev, distance = Distance};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    //Delete Track
    public void DeleteTrack(Guid soundUUID)
    {
        var msg = new DeleteTrack {command = deleteTrackCommand, SoundUUID = soundUUID.ToString()};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    //Reset
    public void Reset()
    {
        var msg = new ResetReapyController {command = resetCommand};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }
    
}
