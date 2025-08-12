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
        animator = umaData.animator; // É o Animator novo, final
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
        // StartCoroutine(PainRoutine());
    }

    public void EmotionTrigger(string emotion)
    {
        Debug.Log($"Emotion Triggered: {emotion}");

        if (emotion == "pain")
        {
            float painDuration = Random.Range(painDurationRange.x, painDurationRange.y);
            actual_emotion = emotion;
            if (facial_expression == true)
            {
                emoter.ManualEmote(painExpressionName, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
            }
            if (body_expression == true)
            {
                animator.SetTrigger("idle_pain");
            }
            new WaitForSeconds(painDuration);

            emoter.TurnOffAll();
            if (actual_emotion != "neutral")
                emoter.ManualEmote(actual_emotion, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
        }

    }


    System.Collections.IEnumerator PainRoutine(){
        while (true)
        {
            float waitTime = Random.Range(painIntervalRange.x, painIntervalRange.y);
            float painDuration = Random.Range(painDurationRange.x, painDurationRange.y);

            yield return new WaitForSeconds(waitTime);
            if (facial_expression == true){
                emoter.ManualEmote(painExpressionName, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
            }
            if (body_expression == true){
                animator.SetTrigger("idle_pain");
            }
            yield return new WaitForSeconds(painDuration);

            emoter.TurnOffAll();
            if (actual_emotion != "neutral")
                emoter.ManualEmote(actual_emotion, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
            
        }
    }

    System.Collections.IEnumerator BlinkRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(blinkIntervalRange.x, blinkIntervalRange.y);
            float blinkDuration = Random.Range(blinkDurationRange.x, blinkDurationRange.y);
            yield return new WaitForSeconds(waitTime);
            emoter.ManualEmote(blinkExpressionName, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
            yield return new WaitForSeconds(blinkDuration);

            
            emoter.TurnOffAll();
            if (actual_emotion != "neutral")
                emoter.ManualEmote(actual_emotion, ExpressionComponent.ExpressionHandler.OneWay, 0f, true);
                
        }
    }
}