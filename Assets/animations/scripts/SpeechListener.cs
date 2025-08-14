using System.Collections.Generic;
using UnityEngine;

public class SpeechListener : MonoBehaviour
{
    public AudioSource source; // arraste no Inspector
    private readonly Queue<(AudioClip clip, float vol)> queue = new();

    private AudioClip pausedClip;
    private float pausedTime;
    private bool playingPriority;
    void OnEnable()
    {
        EventHub.OnAudioRequested += HandleEnqueue;   // <-- era OnEnqueueAudio
        EventHub.OnInterruptSpeech += HandleSpeechInterrupt; // para pausar fala por emoção
        EventHub.OnStopAllAudio += HandleStopAll;
    }
    void OnDisable()
    {
        EventHub.OnAudioRequested -= HandleEnqueue;
        EventHub.OnInterruptSpeech -= HandleSpeechInterrupt;
        EventHub.OnStopAllAudio -= HandleStopAll;
    }

    // adicione:
    void HandleSpeechInterrupt(string reason)
    {
        if (source.isPlaying)
        {
            // pausa o atual; Audio prioritário (interrupt:true) entrará em seguida
            pausedClip = source.clip;
            pausedTime = source.time;
            source.Stop();
        }
    }

    void HandleEnqueue(EventHub.AudioRequest req)
    {
        if (req.interrupt && !playingPriority)
        {
            // Salva o que estava tocando
            if (source.isPlaying)
            {
                pausedClip = source.clip;
                pausedTime = source.time;
            }
            playingPriority = true;
            PlayNow(req.clip, req.volume);
        }
        else if (!req.interrupt)
        {
            queue.Enqueue((req.clip, Mathf.Clamp01(req.volume)));
        }
    }

    void Update()
    {
        if (!source.isPlaying)
        {
            if (playingPriority)
            {
                // Voltou do áudio prioritário
                playingPriority = false;
                if (pausedClip != null)
                {
                    source.clip = pausedClip;
                    source.time = pausedTime;
                    source.Play();
                    pausedClip = null;
                    return;
                }
            }

            // Se não tem prioridade, toca da fila normal
            if (queue.Count > 0)
            {
                var (clip, vol) = queue.Dequeue();
                PlayNow(clip, vol);
            }
        }
    }

    void PlayNow(AudioClip clip, float vol)
    {
        if (clip == null) return;
        source.clip = clip;
        source.volume = Mathf.Clamp01(vol);
        source.Play();
    }
    
    void HandleStopAll() {
        queue.Clear();
        if (source.isPlaying) source.Stop();
        playingPriority = false;
        pausedClip = null;
        pausedTime = 0f;
    }
}