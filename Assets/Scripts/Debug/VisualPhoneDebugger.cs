using System;
using UnityEngine;


// This debugger has been entirely been made by LLMs,
// this was done since the tool is only meant to be used for debug/development and wont be apart of the game/final product
// therefore it is more motivated to generate this script to increase development productivity

/// <summary>
/// Visual debugger for phone-controller JSON packets.
/// Attach this component to an empty GameObject and forward packets to ApplyJson().
/// Phone coordinate convention: +X = right, +Y = front/screen, +Z = top.
/// </summary>
public class VisualPhoneDebugger : SingletonManager<VisualPhoneDebugger>
{
    public enum AngleUnits
    {
        Degrees,
        Radians
    }

    [Header("Phone visual")]
    [Tooltip("Optional existing phone cuboid. Its screen/front face should point along local +Y.")]
    public Transform phoneVisual;
    public bool createPhoneIfMissing = true;
    [Tooltip("Dimensions are width (X), thickness (Y), and height (Z).")]
    public Vector3 generatedPhoneScale = new Vector3(0.70f, 0.12f, 1.35f);

    [Header("Orientation mapping")]
    [Tooltip("Set this to match the units sent by the phone controller.")]
    public AngleUnits angleUnits = AngleUnits.Radians;
    [Tooltip("Mapping: pitch rotates around X, yaw around Y, and roll around Z.")]
    public float pitchSign = -1f;
    public float yawSign = -1f;
    public float rollSign = -1f;
    public bool applyOrientation = true;

    [Header("Debug display")]
    public bool showHud = true;
    public bool showSceneGizmos = true;
    public bool showTouchMarker = true;
    public float axisArrowLength = 0.85f;

    [Serializable]
    private class ControllerPacket
    {
        public string type;
        public string phase;
        public long id;
        public double timestamp;

        public float x;
        public float y;

        public float pitch;
        public float roll;
        public float yaw;
        public float gx;
        public float gy;
        public float gz;
        public float ax;
        public float ay;
        public float az;
    }

    private bool hasMotion;
    private bool hasTouch;
    private string lastPacketType = "none";
    private string touchPhase = "none";
    private long touchId;
    private float lastMotionRealtime;
    private float lastTouchRealtime;

    private float pitch;
    private float roll;
    private float yaw;
    private float gx;
    private float gy;
    private float gz;
    private float ax;
    private float ay;
    private float az;

    private float touchX;
    private float touchY;
    private bool touchMarkerActive;
    private GameObject touchMarker;
    private string parseError;

    private GUIStyle hudBoxStyle;
    private GUIStyle hudLabelStyle;

    protected override void Awake()
    {
        base.Awake();

        EnsurePhoneVisual();
        EnsureTouchMarker();
    }

    private void EnsurePhoneVisual()
    {
        if (phoneVisual != null || !createPhoneIfMissing)
        {
            return;
        }

        GameObject phone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        phone.name = "Debug Phone Cube (+Y = screen/front)";
        phone.transform.SetParent(transform, false);
        phone.transform.localPosition = Vector3.zero;
        phone.transform.localRotation = Quaternion.identity;
        phone.transform.localScale = generatedPhoneScale;
        phoneVisual = phone.transform;

        Renderer phoneRenderer = phone.GetComponent<Renderer>();
        if (phoneRenderer != null)
        {
            // Renderer.material creates a material instance for this debug object.
            phoneRenderer.material.color = new Color(0.18f, 0.24f, 0.31f, 1f);
        }
    }

    private void EnsureTouchMarker()
    {
        if (phoneVisual == null || touchMarker != null)
        {
            return;
        }

        touchMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        touchMarker.name = "Debug Touch Position";
        touchMarker.transform.SetParent(phoneVisual, false);

        Collider markerCollider = touchMarker.GetComponent<Collider>();
        if (markerCollider != null)
        {
            Destroy(markerCollider);
        }

        // Keep the marker close to the same world-space size at any phone scale.
        Vector3 phoneWorldScale = phoneVisual.lossyScale;
        touchMarker.transform.localScale = new Vector3(
            0.055f / Mathf.Max(Mathf.Abs(phoneWorldScale.x), 0.0001f),
            0.055f / Mathf.Max(Mathf.Abs(phoneWorldScale.y), 0.0001f),
            0.055f / Mathf.Max(Mathf.Abs(phoneWorldScale.z), 0.0001f));
        touchMarker.SetActive(false);
    }

    /// <summary>
    /// Pass one complete JSON object from the phone for each motion or touch packet.
    /// Call this from Unity's main thread.
    /// </summary>
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

        lastPacketType = packetType.Length == 0 ? "unknown" : packetType;
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

        if (applyOrientation && phoneVisual != null)
        {
            float pitchDegrees = ConvertAngleToDegrees(pitch);
            float rollDegrees = ConvertAngleToDegrees(roll);
            float yawDegrees = ConvertAngleToDegrees(yaw);

            // +Y is the screen/front direction and +Z points toward the phone's top.
            // Keep the sensor axes aligned: pitch -> X, yaw -> Y, and roll -> Z.
            phoneVisual.localRotation = Quaternion.Euler(
                pitchDegrees * pitchSign,
                yawDegrees * yawSign,
                rollDegrees * rollSign);
        }
    }

    private void UpdateTouchData(ControllerPacket packet)
    {
        hasTouch = true;
        touchX = packet.x;
        touchY = packet.y;
        touchPhase = string.IsNullOrEmpty(packet.phase) ? "unknown" : packet.phase;
        touchId = packet.id;
        lastTouchRealtime = Time.unscaledTime;

        bool touchFinished = string.Equals(touchPhase, "ended", StringComparison.OrdinalIgnoreCase)
            || string.Equals(touchPhase, "cancelled", StringComparison.OrdinalIgnoreCase)
            || string.Equals(touchPhase, "canceled", StringComparison.OrdinalIgnoreCase);

        touchMarkerActive = !touchFinished;
        UpdateTouchMarker();
    }

    private float ConvertAngleToDegrees(float angle)
    {
        return angleUnits == AngleUnits.Radians ? angle * Mathf.Rad2Deg : angle;
    }

    private void UpdateTouchMarker()
    {
        if (phoneVisual == null)
        {
            return;
        }

        EnsureTouchMarker();
        if (touchMarker == null)
        {
            return;
        }

        bool shouldShowMarker = showTouchMarker && hasTouch && touchMarkerActive;
        touchMarker.SetActive(shouldShowMarker);
        if (!shouldShowMarker)
        {
            return;
        }

        // Assumes normalized 0..1 coordinates with origin at the screen's top-left.
        // The screen lies on local +Y, and the phone top is local +Z.
        float normalizedX = Mathf.Clamp01(touchX);
        float normalizedY = Mathf.Clamp01(touchY);
        float frontSurfaceOffset = 0.51f;

        touchMarker.transform.localPosition = new Vector3(
            normalizedX - 0.5f,
            frontSurfaceOffset,
            0.5f - normalizedY);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateTouchMarker();
        }
    }

    private void OnGUI()
    {
        if (!showHud)
        {
            return;
        }

        EnsureHudStyles();

        const float panelWidth = 410f;
        const float panelHeight = 325f;
        GUI.Box(new Rect(12f, 12f, panelWidth, panelHeight), "PHONE SENSOR DEBUGGER", hudBoxStyle);
        GUILayout.BeginArea(new Rect(26f, 40f, panelWidth - 28f, panelHeight - 38f));

        GUILayout.Label($"Packet: {lastPacketType}", hudLabelStyle);
        string motionStatus = hasMotion
            ? $"received | age {Time.unscaledTime - lastMotionRealtime:F2} s"
            : "waiting";
        GUILayout.Label($"Motion: {motionStatus}", hudLabelStyle);

        string inputAngleUnit = angleUnits == AngleUnits.Radians ? "rad (input)" : "deg (input)";
        GUILayout.Label($"Orientation [{inputAngleUnit}]", hudLabelStyle);
        GUILayout.Label($"  pitch {FormatSensorValue(pitch)}   roll {FormatSensorValue(roll)}   yaw {FormatSensorValue(yaw)}", hudLabelStyle);
        GUILayout.Label("  Unity mapping: X=pitch, Y=yaw, Z=roll", hudLabelStyle);
        GUILayout.Label(
            $"  mapped Euler deg: X {FormatSensorValue(ConvertAngleToDegrees(pitch) * pitchSign)}  Y {FormatSensorValue(ConvertAngleToDegrees(yaw) * yawSign)}  Z {FormatSensorValue(ConvertAngleToDegrees(roll) * rollSign)}",
            hudLabelStyle);
        GUILayout.Label($"Gyroscope (raw):  gx {FormatSensorValue(gx)}  gy {FormatSensorValue(gy)}  gz {FormatSensorValue(gz)}", hudLabelStyle);
        GUILayout.Label($"Acceleration (raw): ax {FormatSensorValue(ax)}  ay {FormatSensorValue(ay)}  az {FormatSensorValue(az)}", hudLabelStyle);

        GUILayout.Space(7f);
        GUILayout.Label($"Touch: {(hasTouch ? touchPhase : "waiting")}", hudLabelStyle);
        if (hasTouch)
        {
            GUILayout.Label($"  x {FormatSensorValue(touchX)}   y {FormatSensorValue(touchY)}   id {touchId}", hudLabelStyle);
            GUILayout.Label($"  packet age {Time.unscaledTime - lastTouchRealtime:F2} s", hudLabelStyle);
        }
        else
        {
            GUILayout.Label("  No touch packet received", hudLabelStyle);
        }

        GUILayout.Space(7f);
        GUILayout.Label("Front/screen = local +Y   |   Top = local +Z", hudLabelStyle);
        GUILayout.Label("Scene view: colored local axes and labeled faces", hudLabelStyle);
        if (!string.IsNullOrEmpty(parseError))
        {
            GUILayout.Label($"Parse warning: {parseError}", hudLabelStyle);
        }

        GUILayout.EndArea();
    }

    private void EnsureHudStyles()
    {
        if (hudBoxStyle == null)
        {
            hudBoxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
        }

        if (hudLabelStyle == null)
        {
            hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true
            };
        }
    }

    private static string FormatSensorValue(float value)
    {
        return value.ToString("+0.0000;-0.0000;0.0000");
    }

    private void OnDrawGizmos()
    {
        if (!showSceneGizmos || phoneVisual == null)
        {
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = phoneVisual.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = previousMatrix;

        Vector3 phoneOrigin = phoneVisual.position;
        DrawAxisArrow(phoneOrigin, phoneVisual.right, axisArrowLength, Color.red);
        DrawAxisArrow(phoneOrigin, phoneVisual.up, axisArrowLength, Color.green);
        DrawAxisArrow(phoneOrigin, phoneVisual.forward, axisArrowLength, Color.blue);

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(0f, 0.58f, 0f)), "FRONT / SCREEN (+Y)");
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(0f, -0.58f, 0f)), "BACK (-Y)");
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(0f, 0f, 0.64f)), "TOP (+Z)");
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(0f, 0f, -0.64f)), "BOTTOM (-Z)");
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(0.58f, 0f, 0f)), "RIGHT (+X)");
        UnityEditor.Handles.Label(phoneVisual.TransformPoint(new Vector3(-0.58f, 0f, 0f)), "LEFT (-X)");

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(phoneOrigin + phoneVisual.right * (axisArrowLength + 0.08f), "+X RIGHT");
        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(phoneOrigin + phoneVisual.up * (axisArrowLength + 0.08f), "+Y FRONT / SCREEN");
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.Label(phoneOrigin + phoneVisual.forward * (axisArrowLength + 0.08f), "+Z TOP");
#endif

        if (hasTouch && touchMarkerActive && showTouchMarker)
        {
            Gizmos.color = Color.yellow;
            Vector3 touchPoint = phoneVisual.TransformPoint(new Vector3(
                Mathf.Clamp01(touchX) - 0.5f,
                0.51f,
                0.5f - Mathf.Clamp01(touchY)));
            Gizmos.DrawWireSphere(touchPoint, 0.075f);
        }

        Gizmos.color = previousColor;
    }

    private static void DrawAxisArrow(Vector3 origin, Vector3 direction, float length, Color color)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();
        Vector3 arrowEnd = origin + direction * length;
        float arrowHeadLength = Mathf.Min(0.16f, length * 0.22f);
        Vector3 arrowSide = Vector3.Cross(direction, Vector3.up);
        if (arrowSide.sqrMagnitude < 0.001f)
        {
            arrowSide = Vector3.Cross(direction, Vector3.right);
        }
        arrowSide.Normalize();

        Gizmos.color = color;
        Gizmos.DrawLine(origin, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd - direction * arrowHeadLength + arrowSide * arrowHeadLength * 0.5f);
        Gizmos.DrawLine(arrowEnd, arrowEnd - direction * arrowHeadLength - arrowSide * arrowHeadLength * 0.5f);
    }
}
