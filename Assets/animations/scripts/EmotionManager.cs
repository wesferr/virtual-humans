using UnityEngine;
using CrazyMinnow.SALSA;
using System.Collections;
using UMA;
using UMA.CharacterSystem;

public class EmotionManager : MonoBehaviour
{
    
    public DynamicCharacterAvatar avatar;

    public bool facial_expression = true; // se as expressões faciais devem ser aplicadas
    public bool body_expression = true; // se as expressões corporais devem ser aplicadas

    public string painExpressionName = "pain";
    public string blinkExpressionName = "blink";

    public Vector2 blinkIntervalRange = new Vector2(1f, 3f); // intervalo entre blinks
    public Vector2 blinkDurationRange = new Vector2(0.2f, 0.35f); // pequena espera para reaplicar a emoção após o blink

    public Vector2 painIntervalRange = new Vector2(5f, 10f); // intervalo entre emoções de dor
    public Vector2 painDurationRange = new Vector2(0.75f, 1.25f); // pequena espera para reaplicar a emoção após o blink
    


    Emoter emoter;
    Animator animator;
    string actual_emotion = "neutral";

    void Start()
    {

        avatar.CharacterCreated.AddListener(OnAvatarCreated);

    }

    void OnAvatarCreated(UMAData umaData)
    {
        animator = umaData.animator;
        emoter = avatar.GetComponent<Emoter>();

        if (emoter == null)
        {
            Debug.LogError("Emoter não atribuído.");
            return;
        }
        if (animator == null)
        {
            Debug.LogError("Animator não atribuído.");
            return;
        }

        emoter.TurnOffAll();

        StartCoroutine(BlinkRoutine());
        StartCoroutine(MouthBreathingRotine());
    }

    public void EmotionTrigger(string emotion)
    {
        Debug.Log($"Emotion Triggered: {emotion}");

        if (emotion == "pain")
        {
            TriggerPain();
        }
        if (emotion == "cough")
        {
            TriggerCough();
        }

    }

    void TriggerCough()
    {
        float painDuration = Random.Range(painDurationRange.x, painDurationRange.y);
        if (facial_expression == true)
        {
            emoter.ManualEmote("cough", ExpressionComponent.ExpressionHandler.RoundTrip, painDuration);
        }
        if (body_expression == true)
        {
            animator.SetTrigger("idle_to_cough");
        }
        new WaitForSeconds(painDuration);
    }

    void TriggerPain(){
        float painDuration = Random.Range(painDurationRange.x, painDurationRange.y);
        if (facial_expression == true)
        {
            emoter.ManualEmote("pain", ExpressionComponent.ExpressionHandler.RoundTrip, painDuration);
        }
        if (body_expression == true)
        {
            animator.SetTrigger("idle_to_pain");
        }
        new WaitForSeconds(painDuration);
    }

    System.Collections.IEnumerator MouthBreathingRotine()
    {
        while (true)
        {
            float inhale_time = Random.Range(2f, 2.5f);
            float exhale_time = Random.Range(2.5f, 3f);
            float wait_time = Random.Range(0.2f, 0.4f);
            emoter.ManualEmote("inhale_air", ExpressionComponent.ExpressionHandler.RoundTrip, inhale_time);
            yield return new WaitForSeconds(inhale_time);
            emoter.ManualEmote("exhale_air", ExpressionComponent.ExpressionHandler.RoundTrip, exhale_time);
            yield return new WaitForSeconds(exhale_time);
            yield return new WaitForSeconds(wait_time);
        }
        
    }

    System.Collections.IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
            float blinkDuration = Random.Range(blinkDurationRange.x, blinkDurationRange.y);
            yield return new WaitForSeconds(waitTime);
            emoter.ManualEmote(blinkExpressionName, ExpressionComponent.ExpressionHandler.RoundTrip, blinkDuration);
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}