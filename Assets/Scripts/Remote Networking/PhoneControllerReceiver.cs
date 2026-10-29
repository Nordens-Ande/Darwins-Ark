using JamesFrowen.SimpleWeb;
using System;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public enum ControllerPacketType
{
    None,
    Motion,
    Touch
}
public enum ControllerTouchPhase
{
    None,
    Began,
    Moved,
    Ended
}

public class PhoneControllerReceiver : SingletonManager<PhoneControllerReceiver>
{
    public event Action<ControllerPacketType> OnPacketTypeChange; // Recommend to not use this event as it gets spammed because it always changes to motion
    public event Action<ControllerTouchPhase> OnTouchPhaseChange;


    private SimpleWebServer server;


    [SerializeField, ReadOnly] private bool hasMotion;
    [SerializeField, ReadOnly] private bool hasTouch;
    [SerializeField, ReadOnly] private ControllerPacketType lastPacketType = ControllerPacketType.None;
    [SerializeField, ReadOnly] private ControllerTouchPhase touchPhase = ControllerTouchPhase.None;
    [SerializeField, ReadOnly] private long touchId;
    [SerializeField, ReadOnly] private float lastMotionRealtime;
    [SerializeField, ReadOnly] private float lastTouchRealtime;

    [Space]

    [SerializeField, ReadOnly] private float pitch;
    [SerializeField, ReadOnly] private float roll;
    [SerializeField, ReadOnly] private float yaw;
    [SerializeField, ReadOnly] private float gx;
    [SerializeField, ReadOnly] private float gy;
    [SerializeField, ReadOnly] private float gz;
    [SerializeField, ReadOnly] private float ax;
    [SerializeField, ReadOnly] private float ay;
    [SerializeField, ReadOnly] private float az;

    [Space]

    [SerializeField, ReadOnly] private float touchX;
    [SerializeField, ReadOnly] private float touchY;
    //[SerializeField, ReadOnly] private bool touchMarkerActive;
    //[SerializeField, ReadOnly] private GameObject touchMarker;
    [SerializeField, ReadOnly] private string parseError;


    public bool HasMotion => hasMotion;
    public bool HasTouch => hasTouch;
    public ControllerPacketType LastPacketType => lastPacketType;
    public ControllerTouchPhase TouchPhase => touchPhase;
    public long TouchId => touchId;
    public float LastMotionRealtime => lastMotionRealtime;
    public float LastTouchRealtime => lastTouchRealtime;

    public float Pitch => pitch;
    public float Roll => roll;
    public float Yaw => yaw;
    public float Gx => gx;
    public float Gy => gy;
    public float Gz => gz;
    public float Ax => ax;
    public float Ay => ay;
    public float Az => az;

    public float TouchX => touchX;
    public float TouchY => touchY;
    
    public string ParseError => parseError;

    
    private void Start()
    {
        TcpConfig tcpConfig = new TcpConfig(noDelay: true, sendTimeout: 5000, receiveTimeout: 20000);

        server = new SimpleWebServer(5000, tcpConfig, ushort.MaxValue, 5000, new SslConfig(), 100);

        server.onConnect += id =>
        {
            Debug.Log($"iPhone connected: {id}");
        };

        server.onDisconnect += id =>
        {
            Debug.Log($"iPhone disconnected: {id}");
        };

        server.onData += (id, data) =>
        {
            string json = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);

            VisualPhoneDebugger.Instance?.ApplyJson(json);
            ApplyJson(json);

            //Debug.Log($"Controller input from {id}: {json}");
        };

        server.onError += (id, exception) =>
        {
            Debug.LogError($"WebSocket error from {id}: {exception}");
        };

        server.Start(7777);

        Debug.Log("Controller WebSocket server started on port 7777");


        //OnPacketTypeChange += LogPacketTypeChange;
        //OnTouchPhaseChange += LogTouchPhaseChange;
    }

    //public void LogPacketTypeChange(ControllerPacketType newPacketType) => Debug.Log("Updated PacketType to: " + newPacketType);
    //public void LogTouchPhaseChange(ControllerTouchPhase newTouchPhase) => Debug.Log("Updated TouchPhase to: " + newTouchPhase);


    private void Update()
    {
        // Process queued network messages on Unity's main thread.
        server?.ProcessMessageQueue(); // THIS METHOD MUST BE HERE NO MATTER WHAT
    }

    protected override void OnDestroy()
    {
        server?.Stop();
        server = null;

        base.OnDestroy();
    }


    public void ApplyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        ControllerPacket packet;
        try
        {
            packet = JsonUtility.FromJson<ControllerPacket>(json);
        }
        catch (Exception exception)
        {
            parseError = exception.Message;
            Debug.LogWarning($"PhoneSensorDebugger: could not parse controller packet: {exception.Message}");
            return;
        }

        if (packet == null)
        {
            parseError = "JsonUtility returned null";
            return;
        }

        string packetType = (packet.type ?? string.Empty).ToLowerInvariant();
        if (packetType.Length == 0)
        {
            // Infer the packet type when the sender omits the type field.
            if (json.Contains("\"phase\""))
            {
                packetType = "touch";
            }
            else if (json.Contains("\"pitch\"") || json.Contains("\"gx\""))
            {
                packetType = "motion";
            }
        }

        ControllerPacketType newPacketType = ParsePacketType(packetType.Length == 0 ? "unknown" : packetType);
        if (lastPacketType != newPacketType)
        {
            OnPacketTypeChange?.Invoke(newPacketType);
            lastPacketType = newPacketType;
        }

        parseError = null;

        if (packetType == "motion")
        {
            UpdateMotionData(packet);
        }
        else if (packetType == "touch")
        {
            UpdateTouchData(packet);
        }
    }


    private void UpdateMotionData(ControllerPacket packet)
    {
        hasMotion = true;
        pitch = packet.pitch;
        roll = packet.roll;
        yaw = packet.yaw;
        gx = packet.gx;
        gy = packet.gy;
        gz = packet.gz;
        ax = packet.ax;
        ay = packet.ay;
        az = packet.az;
        lastMotionRealtime = Time.unscaledTime;
    }

    private void UpdateTouchData(ControllerPacket packet)
    {
        hasTouch = true;
        touchX = packet.x;
        touchY = packet.y;
        touchId = packet.id;
        lastTouchRealtime = Time.unscaledTime;

        ControllerTouchPhase newTouchPhase = ParseTouchPhase(string.IsNullOrEmpty(packet.phase) ? "unknown" : packet.phase);
        if (touchPhase != newTouchPhase)
        {
            OnTouchPhaseChange?.Invoke(newTouchPhase);
            touchPhase = ParseTouchPhase(string.IsNullOrEmpty(packet.phase) ? "unknown" : packet.phase);
        }
    }


    private ControllerPacketType ParsePacketType(string packetType)
    {
        return packetType switch
        {
            "motion" => ControllerPacketType.Motion,
            "touch" => ControllerPacketType.Touch,
            _ => ControllerPacketType.None
        };
    }

    private ControllerTouchPhase ParseTouchPhase(string touchPhase)
    {
        return touchPhase switch
        {
            "began" => ControllerTouchPhase.Began,
            "moved" => ControllerTouchPhase.Moved,
            "ended" => ControllerTouchPhase.Ended,
            _ => ControllerTouchPhase.None,
        };
    }
}