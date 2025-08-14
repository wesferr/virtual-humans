using UnityEngine;
using System.Collections.Generic;

public class EmotionCaller : MonoBehaviour
{

    public List<string> interrupting = new() { "cough", "pain" };
    public List<string> anim = new() { "cough", "pain", "surprise" };

    void OnEnable() => EventHub.OnEmotion += Handle;
    void OnDisable() => EventHub.OnEmotion -= Handle;

    void Handle(string e)
    {
        if (string.IsNullOrEmpty(e)) return;
        if (anim.Contains(e)) EventHub.InterruptAnimation(e);
        if (interrupting.Contains(e)) EventHub.InterruptSpeech(e);
    }

    void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    System.Collections.IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(waitTime);
            EventHub.EmitAnimation("blink");
        }
    }
}