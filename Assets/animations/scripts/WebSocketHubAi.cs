using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;

public class WebSocketHubAi : MonoBehaviour
{
    [SerializeField] string serverUrlAi = "ws://143.54.85.87:8765/ai";

    ClientWebSocket wsAi;
    CancellationTokenSource ctsAi;
    bool isConnectingAi = false;
    int attemptsAi = 0;

    [Header("Backoff")]
    public float baseDelay = 2f;
    public float maxDelay  = 20f;

    void OnEnable()
    {
        EventHub.OutboundBinary += HandleOutboundBinaryToAi;
        EventHub.OnAudioReady   += HandleOutboundBinaryToAi;
        // (opcional) EventHub.OutboundText += HandleOutboundTextToAi;

        _ = ConnectAiAsync();
    }

    void OnDisable()
    {
        EventHub.OutboundBinary -= HandleOutboundBinaryToAi;
        EventHub.OnAudioReady   -= HandleOutboundBinaryToAi;
        // (opcional) EventHub.OutboundText -= HandleOutboundTextToAi;

        _ = SafeCloseAi();
    }

    async Task ConnectAiAsync()
    {
        if (isConnectingAi) return;
        isConnectingAi = true;

        while (wsAi == null || wsAi.State != WebSocketState.Open)
        {
            try
            {
                ctsAi?.Cancel();
                ctsAi = new CancellationTokenSource();

                wsAi?.Dispose();
                wsAi = new ClientWebSocket();

                Debug.Log("[WS:/ai] Conectando...");
                await wsAi.ConnectAsync(new Uri(serverUrlAi), ctsAi.Token);

                Debug.Log("[WS:/ai] Conectado!");
                attemptsAi = 0;
                _ = ReceiveLoopAi();
                break;
            }
            catch (Exception ex)
            {
                attemptsAi++;
                var delay = Mathf.Min(baseDelay * Mathf.Pow(2f, attemptsAi - 1), maxDelay);
                Debug.LogWarning($"[WS:/ai] Falha: {ex.Message}. Retentando em {delay:0.#}s.");
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        isConnectingAi = false;
    }

    async Task ReceiveLoopAi()
    {
        var buf = new byte[64 * 1024];

        try
        {
            while (wsAi != null && wsAi.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult res;
                do
                {
                    res = await wsAi.ReceiveAsync(new ArraySegment<byte>(buf), ctsAi.Token);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.LogWarning("[WS:/ai] Servidor fechou.");
                        await SafeCloseAi();
                        await ConnectAiAsync();
                        return;
                    }
                    ms.Write(buf, 0, res.Count);
                } while (!res.EndOfMessage);

                var data = ms.ToArray();
                if (res.MessageType == WebSocketMessageType.Text)
                    EventHub.EmitSocketText(Encoding.UTF8.GetString(data));
                else if (res.MessageType == WebSocketMessageType.Binary)
                    EventHub.EmitSocketBinary(data);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[WS:/ai] Erro recv: {ex.Message}");
            await SafeCloseAi();
            await ConnectAiAsync();
        }
    }

    async void HandleOutboundBinaryToAi(byte[] data)
    {
        if (wsAi == null || wsAi.State != WebSocketState.Open) return;
        try
        {
            await wsAi.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, ctsAi?.Token ?? CancellationToken.None);
        }
        catch (Exception e) { Debug.LogError($"[WS:/ai] send bin erro: {e.Message}"); }
    }

    async void HandleOutboundTextToAi(string s)
    {
        if (wsAi == null || wsAi.State != WebSocketState.Open) return;
        try
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            await wsAi.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ctsAi?.Token ?? CancellationToken.None);
        }
        catch (Exception e) { Debug.LogError($"[WS:/ai] send text erro: {e.Message}"); }
    }

    async Task SafeCloseAi()
    {
        try
        {
            ctsAi?.Cancel();
            if (wsAi != null && (wsAi.State == WebSocketState.Open || wsAi.State == WebSocketState.CloseReceived))
                await wsAi.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch { }
        finally
        {
            try { wsAi?.Dispose(); } catch { }
            wsAi = null;
        }
    }

    async void OnApplicationQuit()
    {
        await SafeCloseAi();
    }
}