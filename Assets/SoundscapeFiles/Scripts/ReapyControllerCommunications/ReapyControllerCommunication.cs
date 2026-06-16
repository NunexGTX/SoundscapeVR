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

    private long t1_count = 0;
    private long offset_count = 0;

    //Commands
    [SerializeField]
    private const string ClockSyncCommand = "clock_sync";
    [SerializeField]
    private const string ClockSyncResultCommand = "clock_sync_result";
    [SerializeField]
    private const string newSoundscapeCommand = "new_soundscape";
    [SerializeField]
    private const string startSoundscapeCommand = "start_soundscape";
    [SerializeField]
    private const string skipStartSoundscapeCommand = "no_start_wait";
    [SerializeField]
    private const string endSoundscapeCommand = "end_soundscape";
    [SerializeField]
    private const string newTrackCommand = "new_track";

    [SerializeField]
    private const string sourcePositionCommand = "source_position";

    [SerializeField]
    private const string muteCommand = "mute";

    [SerializeField]
    private const string unmuteCommand = "unmute";

    [SerializeField]
    private const string deleteTrackCommand = "delete_track";

    [SerializeField]
    private const string resetCommand = "RESET";

    [SerializeField]
    private const string addEffectCommand = "add_effect";

    [SerializeField]
    private const string removeEffectCommand = "remove_effect";

    [SerializeField]
    private const string newTrackCreatedSuccesfullyMessage = "track_created";

    [SerializeField]
    private const string startSoundscapeSuccesfullyMessage = "soundscape_started";

    void Start()
    {
        string readIP = readIPFile();
        if (autoconnect)
        {
            if (readIP != null)
            {
                setAndConnectIP(readIP);
            }
            else
            {
                setAndConnectIP(vrStateData.CurrentReaperControllerServerIP);  
            }
        }
        else
        {
            if (readIP != null)
            {
                setServerIP(readIP);
            }
            else
            {
                setServerIP(vrStateData.CurrentReaperControllerServerIP);  
            }
        }
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

    async void ConnectAsync() {
        await TryConnectAsync(_cts.Token);

        //Send clock after establishing a connection
        ClockSync();
    }

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
            OnReaperReceive(response);
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

    private void OnReaperReceive(string ReaperResponse)
    {
        if (ReaperResponse.Contains("clock_sync:"))
        {
            ClockSyncResult(long.Parse(ReaperResponse.Split(':')[1]));
        }

        //correr quando receber ack ou error de tocar uma fonte de audio
        switch (ReaperResponse.Trim())
        {
            case newTrackCreatedSuccesfullyMessage:
                Debug.Log($"[SoundscapeVR] Acknowledged track creation - loading manager exists: {loadingManager != null}");
                if(loadingManager != null) loadingManager.Increment();
                break;
            case startSoundscapeSuccesfullyMessage:
                //ReapyControllerMonitor.countstartDelay();
                break;
            default:
                Debug.Log($"[SoundscapeVR] other response gotten: {ReaperResponse}");
                break;
        }
            
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

    public static string readIPFile()
    {
        string IPFile = SoundscapeLibrary.GetReaperControllerIPFilePath();

        try
        {
            string ip = File.ReadAllText(IPFile).Trim();

             if (IPTools.IsValidIP(ip))
            {
                Debug.Log($"Loaded Reaper Controller's IP:{ip} from {IPFile} successfully");
                return ip;
            }
            else
            {
                Debug.LogError($"Invalid IP address format in config: '{ip}'");
                return null;
            }
        }
        catch (IOException e)
        {
            Debug.LogWarning($"Failed to read config file: {e.Message}");
            return null;
        }
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
    public void ClockSync()
    {
        t1_count = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var msg = new ClockSync {command = ClockSyncCommand, t1 = t1_count};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    public void ClockSyncResult(long t_rx)
    {
        long t4 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        offset_count = t_rx - (t1_count + (t4-t1_count)/2);

        var msg = new ClockSyncResult {command = ClockSyncResultCommand, offset = offset_count};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    //Soundscape Commands
    public void NewSoundscape(int duration, bool loop)
    {
        var msg = new NewSoundscape {command = newSoundscapeCommand, Duration = duration, Loop = loop};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    public void StartSoundscape()
    {
        var msg = new StartSoundscape {command = startSoundscapeCommand, timestamp=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    public void SkipStartSoundscape()
    {
        var msg = new StartSoundscape {command = skipStartSoundscapeCommand};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

    public void StopSoundscape()
    {
        var msg = new StopSoundscape {command = endSoundscapeCommand};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json);
    }

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

        var msg = new AudioEffect {command = addEffectCommand, SoundUUID = soundUUID.ToString(), EffectName = audioEffectInstance.effectData.effectId, EffectParams = effectParams};
        string json = JsonUtility.ToJson(msg);

        message_beeboop(json, soundUUID);
    }

    string[] trackEffectParams(AudioEffectInstance audioEffectInstance) => audioEffectInstance.effectData.GetTrackParams();
    
}
