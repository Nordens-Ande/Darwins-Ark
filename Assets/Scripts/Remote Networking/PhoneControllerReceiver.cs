using JamesFrowen.SimpleWeb;
using System;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PhoneControllerReceiver : MonoBehaviour
{
    private SimpleWebServer server;

    void Start()
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

            //Debug.Log($"Controller input from {id}: {json}");
        };

        server.onError += (id, exception) =>
        {
            Debug.LogError($"WebSocket error from {id}: {exception}");
        };

        server.Start(7777);

        Debug.Log("Controller WebSocket server started on port 7777");
    }

    void Update()
    {
        // Process queued network messages on Unity's main thread.
        server?.ProcessMessageQueue(); // THIS METHOD MUST BE HERE NO MATTER WHAT
    }

    void OnDestroy()
    {
        server?.Stop();
        server = null;
    }
}