using UnityEngine;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.IO;

public class SocketCaller : MonoBehaviour
{
    [Serializable] class AudioPacket { public string emotion; public string audio; } // payload mínimo

    readonly ConcurrentQueue<Action> mainThread = new();
    void OnEnable() {
        EventHub.OnSocketText += HandleText;
        EventHub.OnSocketBinary += HandleBinary; // <—
    }
    void OnDisable() {
        EventHub.OnSocketText -= HandleText;
        EventHub.OnSocketBinary -= HandleBinary; // <—
    }
    void HandleBinary(byte[] data) {
        var clip = Pcm16ToClip(data, 24000, 1, "net_bin_audio");
        if (clip != null) EventHub.RequestAudio(clip, 1f, interrupt:false);
    }

    void Update() { while (mainThread.TryDequeue(out var a)) a?.Invoke(); }

    void HandleText(string json)
    {
        AudioPacket p = null;
        try { p = JsonUtility.FromJson<AudioPacket>(json); } catch { return; }
        if (p == null || string.IsNullOrEmpty(p.audio)) return;

        // EMOÇÃO → EventHub (desacoplado)
        if (!string.IsNullOrEmpty(p.emotion))
            EventHub.EmitEmotion(p.emotion); // EmotionCaller reagirá :contentReference[oaicite:2]{index=2}

        // ÁUDIO → cria clip no main thread e pede para tocar
        var pcm = Convert.FromBase64String(p.audio);
        mainThread.Enqueue(() =>
        {
            var clip = Pcm16ToClip(pcm, 24000, 1, "net_audio");
            if (clip != null)
            {
                bool priority = p.emotion == "cough" || p.emotion == "pain";
                EventHub.RequestAudio(clip, 1f, interrupt: priority); // SpeechListener ouve :contentReference[oaicite:3]{index=3}:contentReference[oaicite:4]{index=4}
            }
        });
    }

    static AudioClip Pcm16ToClip(byte[] pcm, int sr, int ch, string name)
    {
        if (pcm == null || pcm.Length < 2) return null;
        int samples = pcm.Length / 2;
        var data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            short s = (short)(pcm[2*i] | (pcm[2*i+1] << 8));
            data[i] = s / 32768f;
        }
        int frames = samples / Mathf.Max(1, ch);
        var clip = AudioClip.Create(name, frames, ch, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
