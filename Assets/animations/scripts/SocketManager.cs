using UnityEngine;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;
using System.Text;

public class SocketManager : MonoBehaviour
{
    private string serverUrl = "ws://143.54.85.87:8765/ai"; // Endpoint do paciente principal
    private string contextServerUrl = "ws://143.54.85.87:8765/cb"; // Novo endpoint para troca de contexto
    public GameObject canvas;
    private AudioRecorder audioRecorder;
    private ClientWebSocket webSocket;
    private ClientWebSocket contextWebSocket; // Novo WebSocket para contexto
    private AudioSource audioSource;
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private bool isPlayingAudio = false;

    private string lastContext = "first";
    public GameObject paciente;
    public EmotionManager emotionManager; // Referência ao EmotionManager

    void Start()
    {
        audioRecorder = gameObject.AddComponent<AudioRecorder>();
        webSocket = new ClientWebSocket();
        contextWebSocket = new ClientWebSocket(); // Inicializando o novo WebSocket
        audioSource = paciente.GetComponent<AudioSource>();
        ConnectWebSocket();
        ConnectContextWebSocket(); // Conecta ao novo WebSocket de contexto
    }

    async void ConnectWebSocket()
    {
        try
        {
            if (webSocket.State == WebSocketState.None || webSocket.State == WebSocketState.Closed)
                await webSocket.ConnectAsync(new Uri(serverUrl), CancellationToken.None);

            _ = ReceiveAudioData(); // Inicia o recebimento de áudio assíncrono
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao conectar WebSocket: {ex.Message}");
        }
    }

    async void ConnectContextWebSocket()
    {
        try
        {
            if (contextWebSocket.State == WebSocketState.None || contextWebSocket.State == WebSocketState.Closed)
                await contextWebSocket.ConnectAsync(new Uri(contextServerUrl), CancellationToken.None);

            Debug.Log("Conectado ao WebSocket de contexto.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao conectar WebSocket de contexto: {ex.Message}");
        }
    }

    void Update()
    {
        HandleInput();
        ReconnectIfNeeded();
        //if bool ver
        PlayNextAudio();
    }

    void HandleInput()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.X)) // Iniciar gravação
        {
            StartRecording();
        }

        if (OVRInput.GetUp(OVRInput.RawButton.X)) // Parar gravação e enviar áudio
        {
            StopRecordingAndSend();
        }

        // Caso queira enviar algum contexto específico para o segundo WebSocket
        if (OVRInput.GetDown(OVRInput.RawButton.Y)) // Exemplo de troca de contexto
        {
            if (lastContext == "first")
            {
                lastContext = "second";
            }
            else
            {
                lastContext = "first";
                SendContextData("{\"bg\":\"first\"}");
            }
             // Envia dados para o segundo WebSocket
        }
    }

    void StartRecording()
    {
        audioRecorder.StartRecording();
        Debug.Log("Gravação iniciada.");
    }

    void StopRecordingAndSend()
    {
        audioRecorder.StopRecording();
        Debug.Log("Gravação parada.");
        SendAudioFile(audioRecorder.GetFilePath());
    }

    async void ReconnectIfNeeded()
    {
        if (webSocket.State == WebSocketState.Closed)
        {
            Debug.LogWarning("WebSocket desconectado. Reconectando...");
            ConnectWebSocket();
        }

        if (contextWebSocket.State == WebSocketState.Closed)
        {
            Debug.LogWarning("WebSocket de contexto desconectado. Reconectando...");
            ConnectContextWebSocket();
        }
    }

    async void SendAudioFile(string filePath)
    {
        try
        {
            byte[] audioData = File.ReadAllBytes(filePath);

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(audioData), WebSocketMessageType.Binary, true, CancellationToken.None);
                Debug.Log("Arquivo de áudio enviado.");
            }
            else
            {
                Debug.LogWarning("WebSocket não está aberto.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Falha ao enviar arquivo de áudio: {ex.Message}");
        }
    }

    async void SendContextData(string context)
    {
        try
        {
            byte[] contextData = System.Text.Encoding.UTF8.GetBytes(context);

            if (contextWebSocket.State == WebSocketState.Open)
            {
                await contextWebSocket.SendAsync(new ArraySegment<byte>(contextData), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log($"Contexto '{context}' enviado.");
            }
            else
            {
                Debug.LogWarning("WebSocket de contexto não está aberto.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Falha ao enviar contexto: {ex.Message}");
        }
    }

    [System.Serializable]
    public class AudioPacket
    {
        public string emotion;
        public string audio;
    }


    async Task ReceiveAudioData()
    {
        var buf = new byte[64 * 1024];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                // junta a mensagem de texto completa (pode vir fragmentada)
                using var ms = new MemoryStream();
                WebSocketReceiveResult res;
                do
                {
                    res = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
                    if (res.MessageType == WebSocketMessageType.Close) return;
                    if (res.MessageType != WebSocketMessageType.Text) { ms.SetLength(0); break; }
                    ms.Write(buf, 0, res.Count);
                }
                while (!res.EndOfMessage);
                Debug.Log($"Recebido: {ms.Length} bytes, tipo: {res.MessageType}");

                if (ms.Length == 0 || res.MessageType != WebSocketMessageType.Text) continue;

                var json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                var p = JsonUtility.FromJson<AudioPacket>(json);
                if (p == null || string.IsNullOrEmpty(p.audio)) continue;

                var pcm = Convert.FromBase64String(p.audio);
                Debug.Log("emotion: " + p.emotion);
                emotionManager.EmotionTrigger(p.emotion); // Chama o método PainTrigger do EmotionManager
                var clip = ConvertToAudioClip(pcm, pcm.Length);
                if (clip != null) audioQueue.Enqueue(clip);

                if (!string.IsNullOrEmpty(p.emotion)) Debug.LogWarning(p.emotion);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    void PlayNextAudio()
    {
        if (isPlayingAudio || audioQueue.Count == 0)
            return;

        isPlayingAudio = true;
        AudioClip nextClip = audioQueue.Dequeue();
        audioSource.clip = nextClip;
        audioSource.Play();
        Debug.Log($"Tocando áudio de tamanho: {nextClip.length}");

        StartCoroutine(WaitForAudioToFinish(nextClip.length));
    }

    System.Collections.IEnumerator WaitForAudioToFinish(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        isPlayingAudio = false;
    }

    AudioClip ConvertToAudioClip(byte[] audioData, int dataSize)
    {
        try
        {
            float[] samples = new float[dataSize / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = BitConverter.ToInt16(audioData, i * 2);
                samples[i] = sample / 32768.0f;
            }

            AudioClip clip = AudioClip.Create("ReceivedClip", samples.Length, 1, 24000, false);
            clip.SetData(samples, 0);
            return clip;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao converter dados de áudio para AudioClip: {ex.Message}");
            return null;
        }
    }

    void OnApplicationQuit()
    {
        CloseWebSocket();
        CloseContextWebSocket();
    }

    async void CloseWebSocket()
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Aplicação finalizada", CancellationToken.None);
                Debug.Log("WebSocket fechado.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao fechar WebSocket: {ex.Message}");
            }
        }
    }

    async void CloseContextWebSocket()
    {
        if (contextWebSocket.State == WebSocketState.Open)
        {
            try
            {
                await contextWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Aplicação finalizada", CancellationToken.None);
                Debug.Log("WebSocket de contexto fechado.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao fechar WebSocket de contexto: {ex.Message}");
            }
        }
    }
}
