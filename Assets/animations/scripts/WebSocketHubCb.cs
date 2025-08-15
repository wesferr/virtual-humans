using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;

public class WebSocketHubCb : MonoBehaviour
{
    [SerializeField] string serverUrlCb = "ws://143.54.85.87:8765/cb";

    ClientWebSocket wsCb;
    CancellationTokenSource ctsCb;
    bool isConnectingCb = false;
    int attemptsCb = 0;

    [Header("Backoff")]
    public float baseDelay = 2f;
    public float maxDelay  = 20f;

    void OnEnable()
    {
        EventHub.OutboundText += HandleOutboundTextToCb;

        _ = ConnectCbAsync();
    }

    void OnDisable()
    {
        EventHub.OutboundText -= HandleOutboundTextToCb;
        _ = SafeCloseCb();
    }

    async Task ConnectCbAsync()
    {
        if (isConnectingCb) return;
        isConnectingCb = true;

        while (wsCb == null || wsCb.State != WebSocketState.Open)
        {
            try
            {
                ctsCb?.Cancel();
                ctsCb = new CancellationTokenSource();

                wsCb?.Dispose();
                wsCb = new ClientWebSocket();

                Debug.Log("[WS:/cb] Conectando...");
                await wsCb.ConnectAsync(new Uri(serverUrlCb), ctsCb.Token);

                Debug.Log("[WS:/cb] Conectado!");
                attemptsCb = 0;
                _ = ReceiveLoopCb();
                break;
            }
            catch (Exception ex)
            {
                attemptsCb++;
                var delay = Mathf.Min(baseDelay * Mathf.Pow(2f, attemptsCb - 1), maxDelay);
                Debug.LogWarning($"[WS:/cb] Falha: {ex.Message}. Retentando em {delay:0.#}s.");
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        isConnectingCb = false;
    }

    async Task ReceiveLoopCb()
    {
        var buf = new byte[32 * 1024]; // geralmente só TEXT

        try
        {
            while (wsCb != null && wsCb.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult res;
                do
                {
                    res = await wsCb.ReceiveAsync(new ArraySegment<byte>(buf), ctsCb.Token);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.LogWarning("[WS:/cb] Servidor fechou.");
                        await SafeCloseCb();
                        await ConnectCbAsync();
                        return;
                    }
                    ms.Write(buf, 0, res.Count);
                } while (!res.EndOfMessage);

                var data = ms.ToArray();
                if (res.MessageType == WebSocketMessageType.Text)
                    EventHub.EmitSocketText(Encoding.UTF8.GetString(data)); // opcional
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[WS:/cb] Erro recv: {ex.Message}");
            await SafeCloseCb();
            await ConnectCbAsync();
        }
    }

    async void HandleOutboundTextToCb(string s)
    {
        s = "{\"bg\":\"" + s + "\"}";
        if (wsCb == null || wsCb.State != WebSocketState.Open) return;
        try
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            await wsCb.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ctsCb?.Token ?? CancellationToken.None);
            Debug.Log($"[WS:/cb] Enviado: {s}");
        }
        catch (Exception e) { Debug.LogError($"[WS:/cb] send text erro: {e.Message}"); }
    }

    async Task SafeCloseCb()
    {
        try
        {
            ctsCb?.Cancel();
            if (wsCb != null && (wsCb.State == WebSocketState.Open || wsCb.State == WebSocketState.CloseReceived))
                await wsCb.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch { }
        finally
        {
            try { wsCb?.Dispose(); } catch { }
            wsCb = null;
        }
    }

    async void OnApplicationQuit()
    {
        await SafeCloseCb();
    }
}