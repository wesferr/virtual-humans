using System;
using System.Reflection;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using CrazyMinnow.SALSA;
using System.Threading.Tasks; // mantém o namespace básico do SALSA
public class AnimationListener : MonoBehaviour
{

    Coroutine blink_routine, cough_routine, mouth_breathing_routine, chest_pain_routine, sneeze_routine, headache_routine;
    public AudioClip cough_clip;
    public AudioClip sneeze_clip;
    public DynamicCharacterAvatar avatar;
    public Eyes salsaEyes; // componente “Eyes” do SALSA
    public bool disableWhileRebinding = true;
    Emoter emoter = null; // Referência ao componente Emoter
    Animator animator = null; // Referência ao componente Animator

    [SerializeField] OVRScreenFade fader;

    void Start()
    {
        avatar.CharacterCreated.AddListener(OnAvatarCreated);
        avatar.CharacterUpdated.AddListener(OnAvatarCreated);
    }

    void OnAvatarCreated(UMAData umaData)
    {

        animator = umaData.animator;
        emoter = avatar.GetComponent<Emoter>();
        // emoter.ManualEmote("joy", ExpressionComponent.ExpressionHandler.RoundTrip, 180);
    }

    void OnEnable()
    {
        EventHub.OnFadeIn += FadeIn; // inscreve no evento
        EventHub.OnFadeOut += FadeOut; // inscreve no evento
        EventHub.OnInterruptAnimation += InterruptAnimation; // inscreve no evento
        EventHub.OnAnimation += PlayAnimation;
    }

    void OnDisable()
    {
        InterruptAnimation("disabled");
        EventHub.OnFadeIn -= FadeIn; // remove inscrição
        EventHub.OnFadeOut -= FadeOut; // remove inscrição
        EventHub.OnInterruptAnimation -= InterruptAnimation; // remove inscrição
        EventHub.OnAnimation -= PlayAnimation;
    }

    void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    void FadeOut()
    {
        fader.FadeOut();
    }

    System.Collections.IEnumerator FadeInRoutine()
    {
        yield return new WaitForSeconds(2);
        fader.FadeIn();
    }

    void InterruptAnimation(string reason)
    {
        Debug.Log($"[Listener] Animação interrompida! Motivo: {reason}");

        if(blink_routine != null) StopCoroutine(blink_routine);
        if(cough_routine != null) StopCoroutine(cough_routine);
        if(mouth_breathing_routine != null) StopCoroutine(mouth_breathing_routine);
        if(chest_pain_routine != null) StopCoroutine(chest_pain_routine);
        if(sneeze_routine != null) StopCoroutine(sneeze_routine);
        if(headache_routine != null) StopCoroutine(headache_routine);

        animator.SetTrigger("reset");
        emoter?.TurnOffAll();
    }

    void PlayAnimation(string animName) {
        StartCoroutine(PlayAnimationWithWait(animName));
    }

    System.Collections.IEnumerator PlayAnimationWithWait(string animName)
    {
        yield return new WaitUntil(() => emoter != null && animator != null);
        if (animName == "blink") blink_routine = StartCoroutine(PlayBlinkAnimation());
        if (animName == "cough") cough_routine = StartCoroutine(PlayCoughAnimation());
        if (animName == "mouth_breathing") mouth_breathing_routine = StartCoroutine(MouthBreathingAnimation());
        if (animName == "heavy_breathing") animator.SetTrigger("heavy_breathing");
        if (animName == "chest_pain") chest_pain_routine = StartCoroutine(PlayChestPainAnimation());
        if (animName == "sneeze") sneeze_routine = StartCoroutine(PlaySneezeAnimation());
        if (animName == "headache") headache_routine = StartCoroutine(PlayHeadacheAnimation());
        
    }

    System.Collections.IEnumerator PlaySneezeAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float waitTime = UnityEngine.Random.Range(5f, 10f);
            float duration = UnityEngine.Random.Range(2f, 3f);
            yield return new WaitForSeconds(waitTime);
            animator.SetTrigger("idle_to_sneeze");
            yield return new WaitForSeconds(0.5f);
            EventHub.RequestAudio(sneeze_clip, duration, interrupt: true);
            yield return new WaitForSeconds(duration);
        }
        
    }

    System.Collections.IEnumerator PlayBlinkAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float waitTime = UnityEngine.Random.Range(1f, 3f);
            float blinkDuration = UnityEngine.Random.Range(0.2f, 0.35f);
            yield return new WaitForSeconds(waitTime);
            emoter.ManualEmote("blink", ExpressionComponent.ExpressionHandler.RoundTrip, blinkDuration);
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    System.Collections.IEnumerator PlayHeadacheAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float waitTime = UnityEngine.Random.Range(20f, 30f);
            float duration = UnityEngine.Random.Range(1.5f, 2f);
            yield return new WaitForSeconds(waitTime);
            animator.SetTrigger("idle_to_headache");
            emoter.ManualEmote("pain", ExpressionComponent.ExpressionHandler.RoundTrip, duration);
            yield return new WaitForSeconds(duration);
        }
    }

    System.Collections.IEnumerator PlayChestPainAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float waitTime = UnityEngine.Random.Range(15f, 25f);
            float duration = UnityEngine.Random.Range(1.5f, 2f);
            yield return new WaitForSeconds(waitTime);
            animator.SetTrigger("idle_to_pain");
            emoter.ManualEmote("pain", ExpressionComponent.ExpressionHandler.RoundTrip, duration);
            yield return new WaitForSeconds(duration);
        }
    }

    System.Collections.IEnumerator PlayCoughAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float waitTime = UnityEngine.Random.Range(8f, 15f);
            float duration = UnityEngine.Random.Range(1f, 1.5f);
            yield return new WaitForSeconds(waitTime);
            animator.SetTrigger("idle_to_cough");
            EventHub.RequestAudio(cough_clip, duration, interrupt: true);
            yield return new WaitForSeconds(duration);
        }
    }


    System.Collections.IEnumerator MouthBreathingAnimation()
    {
        while (true)
        {
            yield return new WaitUntil(() => emoter != null && animator != null);
            float inhale_time = UnityEngine.Random.Range(2f, 2.5f);
            float exhale_time = UnityEngine.Random.Range(2.5f, 3f);
            float wait_time = UnityEngine.Random.Range(0.2f, 0.4f);
            emoter.ManualEmote("inhale_air", ExpressionComponent.ExpressionHandler.RoundTrip, inhale_time);
            yield return new WaitForSeconds(inhale_time);
            emoter.ManualEmote("exhale_air", ExpressionComponent.ExpressionHandler.RoundTrip, exhale_time);
            yield return new WaitForSeconds(exhale_time);
            yield return new WaitForSeconds(wait_time);
        }

    }
    
}