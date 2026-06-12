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

    
    public static event Action<string> OnChangeStatus;
    public static void ChangeStatus(string status) => OnChangeStatus?.Invoke(status);
    
    public static event Action OnRecordStart;
    public static void RecordStart() => OnRecordStart?.Invoke();

    public static event Action OnRecordStop;
    public static void RecordStop() => OnRecordStop?.Invoke();

    public static event Action<byte[]> OnAudioReady; // bytes (wav ou pcm)
    public static void EmitAudioReady(byte[] data) => OnAudioReady?.Invoke(data);

    public static event Action OnStopAllAudio; // nome da emoção
    public static void StopAllAudio()
    {
        OnStopAllAudio?.Invoke();
    }



    //=============ANIMACAO==============
    public static event Action OnChangeCase;
    public static void ChangeCase() => OnChangeCase?.Invoke();

    public static event Action OnStartCase;
    public static void StartCase() => OnStartCase?.Invoke();
    
    public static event Action<string> OnInterruptAnimation;
    public static void InterruptAnimation(string reason) => OnInterruptAnimation?.Invoke(reason);

    public static event Action<string> OnAnimation;
    public static void PlayAnimation(string anim) => OnAnimation?.Invoke(anim);

    //=============SOCKET==============

    public static event Action<string> OnSocketText;
    public static event Action<byte[]> OnSocketBinary;
    public static void EmitSocketText(string s) => OnSocketText?.Invoke(s);
    public static void EmitSocketBinary(byte[] b) => OnSocketBinary?.Invoke(b);


    public static event Action<string, int> OutboundText;
    public static event Action<byte[]> OutboundBinary;
    public static void SendText(string scase, int subjectId) => OutboundText?.Invoke(scase, subjectId);
    public static void SendBinary(byte[] b) => OutboundBinary?.Invoke(b);


    public static event Action<long, long, long> OnOutboundTextToLs;
    public static void SendTextToLs(long start, long end, long dir) => OnOutboundTextToLs?.Invoke(start, end, dir);

    //=============LIMPAR TODOS OS EVENTOS==============

    public static event Action OnFadeIn;
    public static void FadeIn() => OnFadeIn?.Invoke();
    public static event Action OnFadeOut;
    public static void FadeOut() => OnFadeOut?.Invoke();

    public static event Action OnEndCase;
    public static void EndCase() => OnEndCase?.Invoke();

    public static event Action<int> OnDefineSubject;
    public static void DefineSubject(int subjectId) => OnDefineSubject?.Invoke(subjectId);

    public static void ClearEvents()
    {
        OnAudioRequested = null;
        OnRecordStart = null;
        OnRecordStop = null;
        OnAudioReady = null;
        OnInterruptAnimation = null;
        OnStopAllAudio = null;
        OnSocketText = null;
        OnSocketBinary = null;
        OutboundText = null;
        OutboundBinary = null;
        OnAnimation = null;
    }
    
}

