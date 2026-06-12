using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UMA;
using UMA.CharacterSystem;
using CrazyMinnow.SALSA;
public class StateManager : MonoBehaviour
{

    public Image clock;
    Material clock_fill;

    public TextMeshProUGUI time;
    public TextMeshProUGUI status;
    public UMATextRecipe[] presets;

    public DynamicCharacterAvatar avatar;

    public Emoter emoter = null; // Referência ao componente Emoter

    // public Canvas canvas;


    Coroutine manage_clock_corotine;

    float duration = 240f;
    float pre_fill = 0f;

    public string actual_state;
    public int subjectId;

    public GameObject caseposition;
    
    bool _endingCase = false;

    [SerializeField] SalsaEyesRebinder rebinder;

    Vector3[] positions = new Vector3[]
    {
        new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        // new Vector3(0.008767692f, 0.90376091f, 1.1120187f),
        // new Vector3(0.008767692f, 0.90376091f, 0.61000001f),
        // new Vector3(-0.02f,       0.90376091f, 0.5f),
        // new Vector3(-0.02f,       0.90376091f, 0.76999998f),
    };

    Quaternion[] rotations = new Quaternion[]
    {
        new Quaternion(0f,  0f,         0f,        1f),
        new Quaternion(0f,  0f,         0f,        1f),
        new Quaternion(0f,  0f,         0f,        1f),
        new Quaternion(0f,  0f,         0f,        1f),
        new Quaternion(0f,  0f,         0f,        1f),
        // new Quaternion(0f, -0.10773913f,0f,        0.99417925f),
        // new Quaternion(0f,  0.09827766f,0f,        0.99515903f),
        // new Quaternion(0f,  0.11242281f,0f,        0.99366051f),
        // new Quaternion(0f,  0.03489949f,0f,        0.99939084f),
    };

    string[] cases = { "first", "second", "third", "fourth", "fifth" };
    int stepIndex = 0;      // índice de progresso (0..n)

    bool ended = false;

    public GameObject EndPanel;

    public AudioClip myclip;

    public void UpdateState()
    {
        int next_index = stepIndex + 1;

        if (next_index > cases.Length)
        {
            EventHub.FadeOut();
            StartCoroutine(DisableInterface());
            ended = true;
        }
        else
        {
            // aplica offset do sujeito (gera o quadrado latino em runtime)
            int index = (subjectId + stepIndex) % cases.Length;
            // pega o estado correspondente
            actual_state = cases[index];
            // avança para o próximo passo
            stepIndex = stepIndex + 1;
        }
    }

    System.Collections.IEnumerator DisableInterface()
    {
        yield return new WaitForSeconds(0.5f);
        Case.SetActive(false);
        HUD.SetActive(false);
        EndCasePanel.SetActive(false);
        Panel.SetActive(false);
        EndPanel.SetActive(true);
        EventHub.FadeIn();
    }

    void Awake()
    {
        clock_fill = clock.material;
    }

    void OnEnable()
    {  
        // emoter.ManualEmote("joy", ExpressionComponent.ExpressionHandler.RoundTrip, 60);
        EventHub.OnChangeCase += UpdateState;
        EventHub.OnChangeCase += LoadCase;
        EventHub.OnChangeCase += ManageCase;
        // EventHub.OnStartCase += ManageCase;
        EventHub.OnEndCase += EndCase;
        EventHub.OnChangeStatus += ChangeStatus;
        EventHub.OnDefineSubject += DefineSubject;
    }
    void OnDisable()
    {
        EventHub.OnChangeCase -= UpdateState;
        EventHub.OnChangeCase -= LoadCase;
        EventHub.OnChangeCase -= ManageCase;
        // EventHub.OnStartCase -= ManageCase;
        EventHub.OnEndCase -= EndCase;
        EventHub.OnChangeStatus -= ChangeStatus;
        EventHub.OnDefineSubject -= DefineSubject;
    }

    void DefineSubject(int subjectId)
    {
        this.subjectId = subjectId;
    }

    void LoadCase()
    {
        if (ended) return;
        StartCoroutine(LoadCaseWithDelay());
    }


    System.Collections.IEnumerator playAudioCoroutine()
    {
        yield return new WaitForSeconds(2f);
        EventHub.RequestAudio(myclip, 3f, interrupt: false);
    }


    System.Collections.IEnumerator LoadCaseWithDelay()
    {
        yield return new WaitForSeconds(0.5f); // delay to allow other events to finish
        Case.SetActive(true);
        HUD.SetActive(true);
        EndCasePanel.SetActive(true);
        Panel.SetActive(false);

        Debug.Log($"[StateManager] Changing case to {actual_state}");

        if (actual_state == "first") // VOCAL
        {
            StartCoroutine(LoadAvatar(0));
            PositionCase(0); // posiciona o caso
            EventHub.SendText("first", subjectId); // voz anasalada
            StartCoroutine(playAudioCoroutine());
            
        }
        else if (actual_state == "second") // FACIAL
        {
            StartCoroutine(LoadAvatar(1));
            PositionCase(1); // posiciona o caso
            EventHub.SendText("second", subjectId); // voz neutra
            EventHub.PlayAnimation("blink"); // piscar
            EventHub.PlayAnimation("mouth_breathing"); // respiração bucal
            EventHub.PlayAnimation("sneeze"); // espirrando
            StartCoroutine(playAudioCoroutine());
        }
        else if (actual_state == "third") // OS 3
        {
            StartCoroutine(LoadAvatar(2));
            PositionCase(2); // posiciona o caso
            EventHub.SendText("third", subjectId); // disfonia
            EventHub.PlayAnimation("blink"); // piscar
            EventHub.PlayAnimation("cough"); // tosse durante a consulta e expressao facial de dor
            EventHub.PlayAnimation("heavy_breathing"); // respiração pesada
            StartCoroutine(playAudioCoroutine());
            // dor de cabeça
        }
        else if (actual_state == "fourth") // CORPORAL
        {
            StartCoroutine(LoadAvatar(3));
            PositionCase(3); // posiciona o caso
            EventHub.SendText("fourth", subjectId); // voz neutra
            EventHub.PlayAnimation("cough"); // tosse durante a consulta
            EventHub.PlayAnimation("heavy_breathing"); // respiração pesada
            EventHub.PlayAnimation("chest_pain"); // dor no peito
            StartCoroutine(playAudioCoroutine());
        }
        else if (actual_state == "fifth")
        {

            StartCoroutine(LoadAvatar(4));
            PositionCase(4);// posiciona o caso
            EventHub.SendText("fifth", subjectId); // voz neutra
            StartCoroutine(playAudioCoroutine());
        }

        EventHub.FadeIn();
    }

    System.Collections.IEnumerator LoadAvatar(int index)
    {
        avatar.ChangeRace("HumanMale");
        avatar.LoadFromRecipe(presets[index]);
        yield return null; // 1 frame
        avatar.BuildCharacter();
        rebinder.RebuildSalsaEyesOnly();
    }

    void PositionCase(int index)
    {
        caseposition.transform.position = positions[index];
        caseposition.transform.rotation = rotations[index];
    }

    void ManageCase()
    {
        if (ended) return;
        if (manage_clock_corotine != null) StopCoroutine(manage_clock_corotine);
        manage_clock_corotine = StartCoroutine(ManageClock());
    }

    System.Collections.IEnumerator ManageClock()
    {
        var start_time = Time.time;
        float fill = 0f;
        clock_fill.SetFloat("_Fill", fill);
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            pre_fill = fill;
            fill = (Time.time - start_time) % duration / duration;
            float timeRemaining = (1f - fill) * duration;
            System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(timeRemaining);
            time.text = timeSpan.ToString("m\\:ss");
            clock_fill.SetFloat("_Fill", fill);
            if (pre_fill > fill) // finish case
            {
                Debug.Log($"[StateManager] Case finished: {actual_state}");
                EventHub.EndCase();
                break;
            }
        }
    }

    public GameObject Scenario;
    public GameObject Case;
    public GameObject Panel;

    public GameObject EndCasePanel;
    public GameObject HUD;

    void EndCase() 
    {
        if (_endingCase) return;
        _endingCase = true;
        StartCoroutine(EndCaseWithDelay());
    }
    System.Collections.IEnumerator EndCaseWithDelay()
    {
        EventHub.FadeOut();
        yield return new WaitForSeconds(0.5f); // delay to allow other events to finish
        if (manage_clock_corotine != null) StopCoroutine(manage_clock_corotine);
        clock.fillAmount = 0f;
        pre_fill = 0f;
        EventHub.InterruptAnimation("change state");
        EventHub.StopAllAudio();
        Debug.Log($"[StateManager] Case ended: {actual_state}");

        Case.SetActive(false);
        HUD.SetActive(false);
        EndCasePanel.SetActive(false);
        Panel.SetActive(true);
        _endingCase = false;
    }

    void ChangeStatus(string status_text)
    {
        status.text = status_text;
    }

}
