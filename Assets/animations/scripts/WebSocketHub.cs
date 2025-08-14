using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;

public class WebSocketHub : MonoBehaviour
{
    [SerializeField] string serverUrl = "ws://143.54.85.87:8765/ai";

    ClientWebSocket ws;
    CancellationTokenSource cts;

    // reconexão
    bool isConnecting = false;
    float reconnectDelaySec = 2f;      // atraso base
    float reconnectMaxDelaySec = 20f;  // atraso máximo
    int reconnectAttempts = 0;

    void OnEnable()
    {
        // OUT: quem quiser enviar publica no EventHub
        EventHub.OutboundText   += HandleOutboundText;
        EventHub.OutboundBinary += HandleOutboundBinary;
        EventHub.OnAudioReady   += HandleOutboundBinary; // envia gravações locais também

        _ = ConnectAsync(); // inicia conexão
    }

    void OnDisable()
    {
        EventHub.OutboundText   -= HandleOutboundText;
        EventHub.OutboundBinary -= HandleOutboundBinary;
        EventHub.OnAudioReady   -= HandleOutboundBinary;

        _ = SafeClose();
    }

    async Task ConnectAsync()
    {
        if (isConnecting) return;
        isConnecting = true;

        while (ws == null || ws.State != WebSocketState.Open)
        {
            try
            {
                cts?.Cancel();
                cts = new CancellationTokenSource();

                ws?.Dispose();
                ws = new ClientWebSocket();

                Debug.Log("[WebSocketHub] Conectando...");
                await ws.ConnectAsync(new Uri(serverUrl), cts.Token);

                Debug.Log("[WebSocketHub] Conectado!");
                reconnectAttempts = 0; // zera backoff ao conectar
                _ = ReceiveLoop();     // começa a ouvir
                break;
            }
            catch (Exception ex)
            {
                reconnectAttempts++;
                var delay = Mathf.Min(reconnectDelaySec * Mathf.Pow(2f, reconnectAttempts - 1), reconnectMaxDelaySec);
                Debug.LogWarning($"[WebSocketHub] Falha na conexão: {ex.Message}. Tentando novamente em {delay:0.#}s.");
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        isConnecting = false;
    }

    async Task ReceiveLoop()
    {
        var buf = new byte[64 * 1024];

        try
        {
            while (ws != null && ws.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult res;
                do
                {
                    res = await ws.ReceiveAsync(new ArraySegment<byte>(buf), cts.Token);
                    if (res.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.LogWarning("[WebSocketHub] Servidor fechou a conexão.");
                        await SafeClose();
                        await ConnectAsync(); // reconecta
                        return;
                    }
                    ms.Write(buf, 0, res.Count);
                } while (!res.EndOfMessage);

                // publica no EventHub (texto/binário)
                if (res.MessageType == WebSocketMessageType.Text)
                    EventHub.EmitSocketText(Encoding.UTF8.GetString(ms.ToArray()));
                else if (res.MessageType == WebSocketMessageType.Binary)
                    EventHub.EmitSocketBinary(ms.ToArray());
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[WebSocketHub] Erro na recepção: {ex.Message}");
            await SafeClose();
            await ConnectAsync(); // reconecta em caso de erro
        }
    }

    async void HandleOutboundText(string s)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        try
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts?.Token ?? CancellationToken.None);
        }
        catch (Exception e) { Debug.LogError($"[WebSocketHub] send text erro: {e.Message}"); }
    }

    async void HandleOutboundBinary(byte[] data)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        try
        {
            await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cts?.Token ?? CancellationToken.None);
        }
        catch (Exception e) { Debug.LogError($"[WebSocketHub] send bin erro: {e.Message}"); }
    }

    async Task SafeClose()
    {
        try
        {
            cts?.Cancel();
            if (ws != null && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived))
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
        }
        catch { /* ignore */ }
        finally
        {
            try { ws?.Dispose(); } catch { }
            ws = null;
        }
    }

    async void OnApplicationQuit()
    {
        await SafeClose();
    }
}
