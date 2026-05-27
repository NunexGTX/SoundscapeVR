using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


public class ReapyControllerCommunication : MonoBehaviour
{
    public bool EnableCommunication = true;
    public bool autoconnect = true;
    private string ServerIP;
    [SerializeField]
    private int ReapyCommunicationPort = 6337;
    public Int32 connectionTimeOutMillis = 5000;
    public VrStateData vrStateData;
    private TcpClient client;
    private NetworkStream stream;
    private string connectionStatus = "not_connected";
    private string[] connectionTypes = new string[] {"not_connected", "connecting", "connected", "failed"};
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly SemaphoreSlim _streamLock = new SemaphoreSlim(1, 1);
    private bool _isReconnecting = false;
    private const int MaxReconnectAttempts = 5;
    private const int ReadTimeoutMs = 3000;
    private const int BaseReconnectDelayMs = 1000;
    public LoadingManager loadingManager = null;

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
        if (autoconnect) setAndConnectIP(vrStateData.CurrentReaperControllerServerIP);
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

    async void ConnectAsync() => await TryConnectAsync(_cts.Token);

    private async Task<bool> TryConnectAsync(CancellationToken ct)
    {
        stream?.Close(); stream = null;
        client?.Close();

        client = new TcpClient();
        var connectTask = client.ConnectAsync(ServerIP, ReapyCommunicationPort);
        var timeoutTask = Task.Delay(connectionTimeOutMillis, ct);
        set_connection_status(connectionTypes[1]);

        if (await Task.WhenAny(connectTask, timeoutTask) == timeoutTask)
        {
            Debug.Log("Reaper Controller: Connection timed out");
            client.Close();

            _ = connectTask.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Debug.Log("Reaper Controller: Late connect fault (ignored): " + t.Exception?.GetBaseException().Message);
            }, TaskScheduler.Default);

            set_connection_status(connectionTypes[0]);
            return false;
        }

        if (connectTask.IsFaulted)
        {
            Debug.Log("Reaper Controller: Connection failed - " + connectTask.Exception?.GetBaseException().Message);
            set_connection_status(connectionTypes[3]);
            client.Close();
            return false;
        }

        stream = client.GetStream();
        stream.ReadTimeout = ReadTimeoutMs;
        Debug.Log("Reaper Controller: Connected!");
        set_connection_status(connectionTypes[2]);
        return true;
    }

    async void message_beeboop(string command)
        => await SendMessageAsync(command, _cts.Token);

    //This is for us to save a reference of the soundUUID in case the response is meaningful
    async void message_beeboop(string command, Guid soundMessangerUUID)
        => await SendMessageAsync(command, _cts.Token);

    private async Task SendMessageAsync(string command, CancellationToken ct)
    {
        if (stream == null)
        {
            Debug.LogWarning($"[SoundscapeVR] No stream. Cannot send to {ServerIP}:{ReapyCommunicationPort}");
            return;
        }
        await _streamLock.WaitAsync(ct);
        try
        {
            byte[] msg = Encoding.UTF8.GetBytes(command);
            await stream.WriteAsync(msg, 0, msg.Length, ct);
            Debug.Log($"Sent command to Reaper Controller at {ServerIP}:{ReapyCommunicationPort}");

            byte[] buf = new byte[1024];
            int n = await stream.ReadAsync(buf, 0, buf.Length, ct);
            string response = Encoding.UTF8.GetString(buf, 0, n);
            //TODO: interpret responses to be useful for something
            Debug.Log($"Reaper Controller response: {response}\nCommand: {command}");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Reaper Controller: SendMessageAsync cancelled (shutdown).");
        }
        catch (Exception ex) when (ex is IOException || ex is SocketException)
        {
            Debug.LogWarning($"Reaper Controller: I/O error — {ex.Message}. Triggering reconnect.");
            _ = ReconnectAsync();
        }
        finally
        {
            _streamLock.Release();
        }
    }

    //TODO
    private void OnReaperReceive()
    {
        //correr quando receber ack ou error de tocar uma fonte de audio
        //if(loadingManage != null) loadingManager.Increment();
    }

    private async Task ReconnectAsync()
    {
        if (_isReconnecting) return;
        _isReconnecting = true;

        stream?.Close(); stream = null;
        client?.Close(); client = null;

        int delayMs = BaseReconnectDelayMs;
        for (int attempt = 1; attempt <= MaxReconnectAttempts; attempt++)
        {
            if (_cts.IsCancellationRequested) break;
            Debug.Log($"Reaper Controller: Reconnect attempt {attempt}/{MaxReconnectAttempts} in {delayMs}ms...");
            try { await Task.Delay(delayMs, _cts.Token); }
            catch (OperationCanceledException) { break; }

            if (await TryConnectAsync(_cts.Token))
            {
                Debug.Log("Reaper Controller: Reconnected successfully.");
                _isReconnecting = false;
                return;
            }
            delayMs = Math.Min(delayMs * 2, 30000);
        }
        Debug.LogError($"Reaper Controller: Failed to reconnect after {MaxReconnectAttempts} attempts.");
        _isReconnecting = false;
    }

    void OnDestroy()
    {
        _cts.Cancel();
        stream?.Close();
        client?.Close();
    }

    //Set Server IP
    public void setServerIP(string ip)
    {
        ServerIP = ip;
        
    }

    public void setAndConnectIP(string ip)
    {
        ServerIP = ip;
        if(ip != null) startReaperCommunication();
    }

    public string getServerIP()
    {
        return ServerIP;
    }

    public int getPort()
    {
        return ReapyCommunicationPort;
    }

    public string connection_status()
    {
        return connectionStatus;
    }

    void set_connection_status(string connect_status)
    {
        connectionStatus = connect_status;

        //Set UI if available
        ConfigScript configUI = FindFirstObjectByType<ConfigScript>();
        if (configUI != null)
        {
            configUI.setConnectionMessage(connectionStatus);
        }
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
                return new string[] {"true", audioEffectInstance.occlusionEffectData.frequency_dependent.ToString(), audioEffectInstance.occlusionEffectData.occlusion_value.ToString(), audioEffectInstance.occlusionEffectData.occlusion_low_freq.ToString(),audioEffectInstance.occlusionEffectData.occlusion_mid_freq.ToString(), audioEffectInstance.occlusionEffectData.occlusion_high_freq.ToString()};
            
            //TODO: add new effects like LOW HIGH Pass
        }
    }
    
}
