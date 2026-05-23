using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using UnityEditor.VersionControl;


public class ReapyControllerCommunication : MonoBehaviour
{
    public bool EnableCommunication = true;
    private string ServerIP;
    public int ReapyCommunicationPort = 6337;
    public Int32 connectionTimeOutMillis = 5000;
    public VrStateData vrStateData;
    private TcpClient client;
    private NetworkStream stream;

    //Commands
    [SerializeField]
    private string newTrackCommand = "new_track";

    [SerializeField]
    private string sourcePositionCommand = "source_position";

    [SerializeField]
    private string muteCommand = "mute";

    [SerializeField]
    private string unmuteCommand = "unmute";

    [SerializeField]
    private string deleteTrackCommand = "delete_track";

    [SerializeField]
    private string resetCommand = "RESET";

    [SerializeField]
    private string addEffectCommand = "add_effect";

    [SerializeField]
    private string removeEffectCommand = "remove_effect";

    private string newTrackCreatedSuccesfullyMessage = "Track Created in Reaper";

    void Start()
    {
        setServerIP(vrStateData.CurrentReaperControllerServerIP);
    }

    public void startReaperCommunication()
    {
        if (EnableCommunication && ServerIP != null)
        {
            ConnectAsync();
        }else if(ServerIP == null)
        {
            Debug.LogError("Reaper Communication can't start since no server IP was set");
        }
    }

    async void ConnectAsync()
    {
        client = new TcpClient();
        var connectTask = client.ConnectAsync(ServerIP, ReapyCommunicationPort);
        var timeoutTask = System.Threading.Tasks.Task.Delay(connectionTimeOutMillis);

        if (await System.Threading.Tasks.Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
        {
            Debug.Log("Reaper Controller: Connection timed out");
            client.Close();

            // Observe the background task so its exception doesn't go unhandled
            _ = connectTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.Log("Reaper Controller: Late connect fault (ignored): " + t.Exception?.GetBaseException().Message);
            }, TaskScheduler.Default);

            return;
        }

        // connectTask won — but it may have faulted (e.g. refused), so check it
        if (connectTask.IsFaulted)
        {
            Debug.Log("Reaper Controller: Connection failed - " + connectTask.Exception?.GetBaseException().Message);
            client.Close();
            return;
        }

        stream = client.GetStream();
        Debug.Log("Reaper Controller: Connected!");
    }

    async void message_beeboop(string command) //All communication to server needs to be async to not freeze the entire game
    {
        if(stream == null)
        {
            Debug.LogWarning($"[SoundscapeVR] No stream object was found. Failed to send message to Reaper Controller Server.\nAddress:{ServerIP}:{ReapyCommunicationPort}");
            return;
        }
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

    //This is for us to save a reference of the soundUUID in case the response is meaningful
    async void message_beeboop(string command, Guid soundMessangerUUID)
    {
        if(stream == null)
        {
            Debug.LogWarning($"[SoundscapeVR] No stream object was found. Failed to send message to Reaper Controller Server.\nAddress:{ServerIP}:{ReapyCommunicationPort}");
            return;
        }
        byte[] message = Encoding.UTF8.GetBytes(command);
        stream.Write(message, 0, message.Length); //Send message
        Debug.Log($"Sent {message} to Reaper Controller Server at {ServerIP}:{ReapyCommunicationPort}");

        // Wait for and read the response
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //TODO: interpret responses to be useful for something
        Debug.Log($"Reaper Controller Server response: {response} \n"
        + $"Command: {command}");
    }

    //Command functions

    //New audio source
    public void NewTrack(string audioID, Guid soundUUID, float volume, bool loop, uint delay)
    {
        var msg = new NewTrack {command = newTrackCommand, AudioID = audioID, SoundUUID = soundUUID.ToString(), Volume = volume, Loop = loop, Delay = delay};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    //Update Audio Source Position
    public void UpdateSourcePosition(Guid soundUUID, float azim, float elev, float Distance)
    {
        var msg = new SourcePosition {command = sourcePositionCommand, SoundUUID = soundUUID.ToString(), azim = azim, elev = elev, distance = Distance};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    //Mute Track
    public void MuteTrack(Guid soundUUID)
    {
        var msg = new MuteTrack {command = muteCommand, SoundUUID = soundUUID.ToString()};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    public void UnmuteTrack(Guid soundUUID)
    {
        var msg = new MuteTrack {command = unmuteCommand, SoundUUID = soundUUID.ToString()};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    //Delete Track
    public void DeleteTrack(Guid soundUUID)
    {
        var msg = new DeleteTrack {command = deleteTrackCommand, SoundUUID = soundUUID.ToString()};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    //Reset
    public void Reset()
    {
        var msg = new ResetReapyController {command = resetCommand};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    //Set Server IP
    public void setServerIP(string ip)
    {
        ServerIP = ip;
        //TODO: instead of starting the server when the ip is set, make a button that directly calls the start connection function
        startReaperCommunication();
    }

    //Effects
    public void AddEffectToAudio(Guid soundUUID, AudioEffectInstance audioEffectInstance)
    {

        string[] effectParams = trackEffectParams(audioEffectInstance); //Gather effect params that are valid to use in reaper controller

        if (effectParams == null)
        {
            return;
        }

        var msg = new AudioEffect {command = addEffectCommand, SoundUUID = soundUUID.ToString(), EffectName = audioEffectInstance.effectId, EffectParams = effectParams};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    string[] trackEffectParams(AudioEffectInstance audioEffectInstance)
    {
        switch (audioEffectInstance.effectId)
        {
            default:
                Debug.LogWarning("The effect you were trying to add to the track in Reaper was not yet implemented. Skipping...");
                return null;

            case AudioEffectInstance.Echo:
                return new string[] {"true", audioEffectInstance.echoEffectData.delay_ms.ToString(),audioEffectInstance.echoEffectData.decay_ratio.ToString(),audioEffectInstance.echoEffectData.dry_mix.ToString(),audioEffectInstance.echoEffectData.wet_mix.ToString()};

            case AudioEffectInstance.Occlusion:
                return new string[] {"true", audioEffectInstance.occlusionEffectData.frequency_dependent.ToString(), audioEffectInstance.occlusionEffectData.occlusion_low_freq.ToString(),audioEffectInstance.occlusionEffectData.occlusion_mid_freq.ToString(), audioEffectInstance.occlusionEffectData.occlusion_high_freq.ToString()};
            
        }
    }
    
}
