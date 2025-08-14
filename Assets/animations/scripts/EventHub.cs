using UnityEngine;
using System;

public static class EventHub
{
    //=============AUDIO==============
    public struct AudioRequest
    {
        public AudioClip clip;    // áudio já pronto
        public float volume;      // 0..1
        public bool interrupt;    // true = limpa fila/para o atual e toca este
    }

    public static event Action<AudioRequest> OnAudioRequested;
    public static void RequestAudio(AudioClip clip, float volume = 1.0f, bool interrupt = false)
    {
        AudioRequest request = new AudioRequest
        {
            clip = clip,
            volume = volume,
            interrupt = interrupt
        };
        OnAudioRequested?.Invoke(request);
    }

    //=============INICIAR GRAVACAO==============
    public static event Action OnRecordStart;
    public static void RecordStart() => OnRecordStart?.Invoke();

    //=============PARAR GRAVACAO==============
    public static event Action OnRecordStop;
    public static void RecordStop() => OnRecordStop?.Invoke();

    //=============AUDIO PRONTO==============
    public static event Action<byte[]> OnAudioReady; // bytes (wav ou pcm)
    public static void EmitAudioReady(byte[] data) => OnAudioReady?.Invoke(data);


    //=============EMOCAO==============
    public static event Action<string> OnEmotion;               // "cough", "pain", etc.
    public static void EmitEmotion(string emotion) => OnEmotion?.Invoke(emotion);

    //=============ANIMACAO==============
    public static event Action<string> OnInterruptAnimation;
    public static void InterruptAnimation(string reason) => OnInterruptAnimation?.Invoke(reason);

    // =============FALA==============
    public static event Action<string> OnInterruptSpeech;
    public static void InterruptSpeech(string reason) => OnInterruptSpeech?.Invoke(reason);

    //=============PARAR TODOS OS AUDIOS==============
    public static event Action OnStopAllAudio;
    public static void StopAllAudio() => OnStopAllAudio?.Invoke();

    //=============LIMPAR TODOS OS EVENTOS==============
    public static void ClearEvents()
    {
        OnInterruptAnimation = null;
        OnInterruptSpeech = null;
    }

    // Recebimento bruto do socket (opcional)
    public static event Action<string> OnSocketText;
    public static event Action<byte[]> OnSocketBinary;
    public static void EmitSocketText(string s) => OnSocketText?.Invoke(s);
    public static void EmitSocketBinary(byte[] b) => OnSocketBinary?.Invoke(b);

    // Saída: quem quiser enviar publica aqui
    public static event Action<string> OutboundText;
    public static event Action<byte[]> OutboundBinary;
    public static void SendText(string s) => OutboundText?.Invoke(s);
    public static void SendBinary(byte[] b) => OutboundBinary?.Invoke(b);

    public static event Action<string> OnAnimation;
    public static void EmitAnimation(string anim) => OnAnimation?.Invoke(anim);
}

