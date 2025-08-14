using UnityEngine;
using System;
using System.IO;

public class RecorderListener : MonoBehaviour
{
    [Header("Config")]
    public int sampleRate = 24000;
    public int channels   = 1; // Unity só dá 1 ou 2
    public string deviceName = null; // null = default device

    private AudioClip recording;
    private float startTime;

    void OnEnable()
    {
        EventHub.OnRecordStart += HandleStart;
        EventHub.OnRecordStop  += HandleStop;
    }
    void OnDisable()
    {
        EventHub.OnRecordStart -= HandleStart;
        EventHub.OnRecordStop  -= HandleStop;
    }

    void HandleStart()
    {
        if (Microphone.IsRecording(deviceName)) return;
        recording = Microphone.Start(deviceName, false, 600, sampleRate); // até 10min
        startTime = Time.time;
        Debug.Log("[MicRecorder] Gravando...");
    }

    void HandleStop()
    {
        if (!Microphone.IsRecording(deviceName) || recording == null) return;
        int pos = Microphone.GetPosition(deviceName);
        Microphone.End(deviceName);

        // Extrai amostras reais gravadas
        var samples = new float[pos * channels];
        recording.GetData(samples, 0);

        // Converte para WAV (PCM16 LE)
        var wav = FloatToWav(samples, sampleRate, channels);
        EventHub.EmitAudioReady(wav);
        Debug.Log($"[MicRecorder] Parou. Enviando {wav.Length/1024} KB.");
        recording = null;
    }

    // Helpers
    static byte[] FloatToWav(float[] samples, int sampleRate, int channels)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        int bytesPerSample = 2;
        int subchunk2Size = samples.Length * bytesPerSample;
        int chunkSize = 36 + subchunk2Size;

        // RIFF header
        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(chunkSize);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt  subchunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);              // Subchunk1Size (16 for PCM)
        bw.Write((short)1);        // AudioFormat = PCM
        bw.Write((short)channels); // NumChannels
        bw.Write(sampleRate);      // SampleRate
        bw.Write(sampleRate * channels * bytesPerSample); // ByteRate
        bw.Write((short)(channels * bytesPerSample));     // BlockAlign
        bw.Write((short)16);       // BitsPerSample

        // data subchunk
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(subchunk2Size);

        // samples (float -1..1) -> int16
        foreach (var f in samples)
        {
            short s = (short)Mathf.Clamp(Mathf.RoundToInt(f * 32767f), -32768, 32767);
            bw.Write(s);
        }
        bw.Flush();
        return ms.ToArray();
    }
}
