using UnityEngine;
using CrazyMinnow.SALSA;
using UMA;
using UMA.CharacterSystem;

public class AnimationListener : MonoBehaviour
{
    
    Coroutine blink_routine, cough_routine;
    public DynamicCharacterAvatar avatar;
    Emoter emoter = null; // Referência ao componente Emoter
    Animator animator = null; // Referência ao componente Animator

    void Start()
    {
        avatar.CharacterCreated.AddListener(OnAvatarCreated);
    }

    void OnAvatarCreated(UMAData umaData)
    {
        animator = umaData.animator;
        emoter = avatar.GetComponent<Emoter>();
    }

    void OnEnable()
    {
        EventHub.OnInterruptAnimation += InterruptAnimation; // inscreve no evento
        EventHub.OnAnimation += PlayAnimation;
    }

    void OnDisable()
    {
        EventHub.OnInterruptAnimation -= InterruptAnimation; // remove inscrição
        EventHub.OnAnimation -= PlayAnimation;
    }

    void InterruptAnimation(string reason)
    {
        Debug.Log($"[Listener] Animação interrompida! Motivo: {reason}");

        StopCoroutine(blink_routine);
        StopCoroutine(cough_routine);

        animator.SetTrigger("back_to_idle");
        emoter?.TurnOffAll();
    }

    void PlayAnimation(string animName)
    {
        if (animName == "blink")
        {
            blink_routine = StartCoroutine(PlayBlinkAnimation());
        }
        if (animName == "cough")
        {
            cough_routine = StartCoroutine(PlayCoughAnimation());
        }
        return;
    }

    System.Collections.IEnumerator PlayBlinkAnimation()
    {
        while (true){
            float waitTime = Random.Range(1f, 3f);
            float blinkDuration = Random.Range(0.2f, 0.35f);
            yield return new WaitForSeconds(waitTime);
            emoter.ManualEmote("blink", ExpressionComponent.ExpressionHandler.RoundTrip, blinkDuration);
            yield return new WaitForSeconds(blinkDuration);
        }   
    }

    System.Collections.IEnumerator PlayCoughAnimation()
    {
        float waitTime = Random.Range(5f, 10f);
        yield return new WaitForSeconds(waitTime);
        animator.SetTrigger("idle_to_cough");
        EventHub.RequestAudio(cough_clip, 1f, interrupt: true);
    }
    
}