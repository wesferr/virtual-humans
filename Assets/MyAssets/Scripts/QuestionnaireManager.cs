using UnityEngine;
using UnityEngine.UI; // Required for UI components like Slider
using System.IO; // Required for file operations

public class QuestionnaireManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Slider Slider1; // Reference to the UI Slider component
    public Slider Slider2; // Reference to the UI Slider component
    public Slider Slider3; // Reference to the UI Slider component
    public Button SubmitButton; // Reference to the UI Button component
    public GameObject SystemObject; // Reference to the UI Panel for the questionnaire

    [System.Serializable]
public class SliderData
    {
        public float Slider1Value;
        public float Slider2Value;
        public float Slider3Value;
    }

    void OnEnable()
    {
        // Initialize the questionnaire or perform setup tasks here
        Debug.Log("Questionnaire Initialized");
        Slider1.value = 3f; // Set initial value for Slider1
        Slider2.value = 3f; // Set initial value for Slider2
        Slider3.value = 3f; // Set initial value for Slider3
        SubmitButton.onClick.AddListener(OnSubmitButtonClicked); // Add listener for button click
        EventHub.FadeIn();
    }

    void Update()
    {
        if (OVRInput.GetUp(OVRInput.RawButton.X))
        {
            OnSubmitButtonClicked();
        }
    }

    void OnDisable()
    {
        // Clean up or reset the questionnaire when it is disabled
        Debug.Log("Questionnaire Disabled");
        SubmitButton.onClick.RemoveListener(OnSubmitButtonClicked); // Remove listener for button click
    }

    void OnSubmitButtonClicked()
    {
        Debug.Log("Questionnaire Submitted");

        StateManager stateManager = SystemObject.GetComponent<StateManager>();
        string state = stateManager.actual_state;
        int subjectId = stateManager.subjectId;

        // Crie uma instância da classe SliderData
        SliderData sliderData = new SliderData
        {
            Slider1Value = Slider1.value,
            Slider2Value = Slider2.value,
            Slider3Value = Slider3.value
        };

        // Converta a instância para JSON
        string json = JsonUtility.ToJson(sliderData, true);
        Debug.Log("JSON Data: " + json);

        string filename = subjectId.ToString("D4") + "-" + state + "-data.json";

        // Defina o caminho do arquivo
        string filePath = Path.Combine(Application.persistentDataPath, filename);

        // Escreva os dados JSON no arquivo
        File.WriteAllText(filePath, json);

        Debug.Log($"Slider values saved to file: {filePath}");

        EventHub.ChangeCase();
        EventHub.FadeOut();
    }

}
