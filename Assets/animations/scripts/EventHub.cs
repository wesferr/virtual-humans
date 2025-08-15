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
    
    public static event Action OnRecordStart;
    public static void RecordStart() => OnRecordStart?.Invoke();

    public static event Action OnRecordStop;
    public static void RecordStop() => OnRecordStop?.Invoke();

    public static event Action<byte[]> OnAudioReady; // bytes (wav ou pcm)
    public static void EmitAudioReady(byte[] data) => OnAudioReady?.Invoke(data);


    //=============ANIMACAO==============
    public static event Action<string> OnChangeCase;
    public static void ChangeCase() => OnChangeCase?.Invoke();
    
    public static event Action<string> OnInterruptAnimation;
    public static void InterruptAnimation(string reason) => OnInterruptAnimation?.Invoke(reason);

    public static event Action<string> OnAnimation;
    public static void EmitAnimation(string anim) => OnAnimation?.Invoke(anim);

    //=============SOCKET==============

    public static event Action<string> OnSocketText;
    public static event Action<byte[]> OnSocketBinary;
    public static void EmitSocketText(string s) => OnSocketText?.Invoke(s);
    public static void EmitSocketBinary(byte[] b) => OnSocketBinary?.Invoke(b);


    public static event Action<string> OutboundText;
    public static event Action<byte[]> OutboundBinary;
    public static void SendText(string s) => OutboundText?.Invoke(s);
    public static void SendBinary(byte[] b) => OutboundBinary?.Invoke(b);



    //=============LIMPAR TODOS OS EVENTOS==============
    public static void ClearEvents()
    {
        OnAudioRequested = null;
        OnRecordStart = null;
        OnRecordStop = null;
        OnAudioReady = null;
        OnEmotion = null;
        OnInterruptAnimation = null;
        OnInterruptSpeech = null;
        OnStopAllAudio = null;
        OnSocketText = null;
        OnSocketBinary = null;
        OutboundText = null;
        OutboundBinary = null;
        OnAnimation = null;
    }
    
}

