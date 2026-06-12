using UnityEngine;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using System.IO;

public class WebSocketHubLs : MonoBehaviour
{
    [SerializeField] string serverUrlLs = "ws://143.54.85.87:8765/ls";

    ClientWebSocket wsLs;
    CancellationTokenSource lifeCts;   // token de vida do componente
    bool runningLoop;

    [Header("Backoff")]
    public float baseDelay = 2f;
    public float maxDelay  = 20f;

    void OnEnable()
    {
        lifeCts = new CancellationTokenSource();
        EventHub.OnOutboundTextToLs += HandleOutboundTextToLs;
        _ = RunConnectionLoopAsync(lifeCts.Token);
    }

    void OnDisable()
    {
        EventHub.OnOutboundTextToLs -= HandleOutboundTextToLs;
        if (lifeCts != null && !lifeCts.IsCancellationRequested)
            lifeCts.Cancel();
        // não await aqui; Unity não permite. O loop vai encerrar sozinho.
    }

    async Task RunConnectionLoopAsync(CancellationToken token)
    {
        if (runningLoop) return;
        runningLoop = true;

        float delay = baseDelay;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // --- Conectar ---
                wsLs = new ClientWebSocket();
                // Pings periódicos (ajude a detectar disconnects imprevistos)
                wsLs.Options.KeepAliveInterval = TimeSpan.FromSeconds(15);

                await wsLs.ConnectAsync(new Uri(serverUrlLs), token);
                UnityEngine.Debug.Log("[WS:/ls] Connected");

                // Envia timestamp binário (igual ao seu código)
                long ns = (long)((double)Stopwatch.GetTimestamp() * 1e9 / Stopwatch.Frequency);
                byte[] bytes = BitConverter.GetBytes(ns);
                await wsLs.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, token);

                // Reset do backoff após conexão bem-sucedida
                delay = baseDelay;

                // --- Ouvir até cair ---
                await ListenToSocketAsync(token);

                // Se saiu do listen sem exception, foi fechamento “limpo”.
                // Vamos fechar recursos e cair no backoff para reconectar.
                await SafeCloseWsOnlyAsync();
            }
            catch (OperationCanceledException)
            {
                // Saiu porque o componente foi desabilitado / cena trocada
                break;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[WS:/ls] loop error: {ex.Message}");
                await SafeCloseWsOnlyAsync();
            }

            // --- Backoff (com teto), mas sem desistir ---
            if (!token.IsCancellationRequested)
            {
                UnityEngine.Debug.Log($"[WS:/ls] Reconnecting in {delay:0.0}s...");
                try { await Task.Delay(TimeSpan.FromSeconds(delay), token); }
                catch (OperationCanceledException) { break; }

                delay = Mathf.Min(delay * 2f, maxDelay);
            }
        }

        runningLoop = false;
    }

    async Task ListenToSocketAsync(CancellationToken token)
    {
        var buffer = new byte[8192];
        using var ms = new MemoryStream();

        while (!token.IsCancellationRequested && wsLs != null && wsLs.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await wsLs.ReceiveAsync(new ArraySegment<byte>(buffer), token);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[WS:/ls] listen error: {ex.Message}");
                // Deixa o loop externo tratar reconexão
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                UnityEngine.Debug.Log("[WS:/ls] Closed by server");
                // Deixa o loop externo tratar reconexão
                return;
            }

            ms.Write(buffer, 0, result.Count);

            if (result.EndOfMessage)
            {
                try
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(ms.ToArray());
                        EventHub.ChangeStatus(message);
                        UnityEngine.Debug.Log($"[WS:/ls] Message: {message}");
                    }
                    // (Se vier binário e você quiser tratar, faça aqui.)
                }
                finally
                {
                    ms.SetLength(0);
                }
            }
        }
    }

    public class TimeStampMsg
    {
        public long t_start_ns;
        public long t_end_ns;
        public long dir;
    }

    async void HandleOutboundTextToLs(long start, long end, long dir)
    {
        // Envia somente se aberto
        if (wsLs == null || wsLs.State != WebSocketState.Open) return;

        var msg = new TimeStampMsg { t_start_ns = start, t_end_ns = end, dir = dir };
        string json = JsonUtility.ToJson(msg);

        try
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            await wsLs.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, lifeCts?.Token ?? CancellationToken.None);
            UnityEngine.Debug.Log($"[WS:/ls] Enviado: {json}");
        }
        catch (OperationCanceledException) { /* ignorar ao desligar */ }
        catch (Exception e) { UnityEngine.Debug.LogError($"[WS:/ls] send error: {e.Message}"); }
    }

    // Fecha só o socket atual (sem cancelar o lifeCts)
    async Task SafeCloseWsOnlyAsync()
    {
        var ws = wsLs;
        wsLs = null;

        if (ws != null)
        {
            try
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch { /* ignore */ }
            finally
            {
                ws.Dispose();
            }
        }
    }
}
