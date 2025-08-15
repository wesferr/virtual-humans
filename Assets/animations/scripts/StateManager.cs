using UnityEngine;
using System.Collections.Generic;

public class StateManager : MonoBehaviour
{

        public AudioClip cough_clip;
    public List<string> interrupting = new() { "cough", "pain" };
    public List<string> anim = new() { "cough", "pain", "surprise" };

    string actual_state = "first";

    void UpdateState()
    {
        if (actual_state == "first") actual_state = "second";
        else if (actual_state == "second") actual_state = "third";
        else if (actual_state == "third") actual_state = "fourth";
        else if (actual_state == "fourth") actual_state = "fifth";
        else if (actual_state == "fifth") actual_state = "first";

    }


    void OnEnable()
    {
        EventHub.OnChangeCase += ChangeCase;
    }
    void OnDisable()
    {
        EventHub.OnChangeCase -= ChangeCase;
    }

    void ChangeCase()
    {
        UpdateState();
        EventHub.InterruptAnimation("change state");

        if (actual_state == "first")
        {
            // VOCAL
            //vós anasalada
            EventHub.SendText("first");
        }
        else if (actual_state == "second")
        {
            // FACIAL
            EventHub.SendText("second");
            EventHub.PlayAnimation("blink");
            //respirando pela boca
            //espirrando
        }
        else if (actual_state == "third")
        {
            //OS 3
            EventHub.SendText("third");
            EventHub.PlayAnimation("blink");
            EventHub.PlayAnimation("cough");

            //respiração pesada
            //dor de cabeça
            //disfonia
            //tosse durante a consulta
            //expressao facial de dor
        }
        else if (actual_state == "fourth")
        {
            // CORPORAL
            EventHub.SendText("fourth");
            EventHub.PlayAnimation("cough");
            //respiração pesada
            //tosse durante a consulta
            //dor no peiro
        }
        else if (actual_state == "fifth")
        {
            EventHub.SendText("fifth");
        }
 
    }

}