using UnityEngine;
using CrazyMinnow.SALSA;
using UMA;
using UMA.CharacterSystem;

public class AnimationListener : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;
    Emoter emoter = null; // Referência ao componente Emoter
    Animator animator = null; // Referência ao componente Animator

    void Start()
    {

        avatar.CharacterCreated.AddListener(OnAvatarCreated);

    }

    void OnAvatarCreated(UMAData umaData){
        animator = umaData.animator;
        emoter = avatar.GetComponent<Emoter>();
    }

    void OnEnable()
    {
        EventHub.OnInterruptAnimation += OnInterruptAnimation; // inscreve no evento
        EventHub.OnAnimation += PlayAnimation;
    }

    void OnDisable()
    {
        EventHub.OnInterruptAnimation -= OnInterruptAnimation; // remove inscrição
        EventHub.OnAnimation -= PlayAnimation;
    }

    void OnInterruptAnimation(string reason)
    {
        Debug.Log($"[Listener] Animação interrompida! Motivo: {reason}");
        emoter?.TurnOffAll();
    }

    void PlayAnimation(string animName)
    {
        if (animName == "blink")
        {
            PlayBlinkAnimation();
        }
        return;
    }
   
    void PlayBlinkAnimation()
    {
        float blinkDuration = Random.Range(0.2f, 0.35f);
        emoter.ManualEmote("blink", ExpressionComponent.ExpressionHandler.RoundTrip, blinkDuration);
    }
}