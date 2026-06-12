using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public TextMeshProUGUI id;
    public Button idUp;
    public Button idDown;

    private int currentId = 1;

    public Button startButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the ID text
        id.text = currentId.ToString();

        // Add listeners to the buttons
        idUp.onClick.AddListener(IncreaseId);
        idDown.onClick.AddListener(DecreaseId);
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        EventHub.FadeOut();
        EventHub.DefineSubject(currentId-1);
        EventHub.ChangeCase();
        this.gameObject.SetActive(false);
    }

    private void IncreaseId()
    {
        currentId++;
        if (currentId > 9999) currentId = 9999;
    }
    private void DecreaseId()
    {
        currentId--;
        if (currentId < 1) currentId = 1;
    }

    // Update is called once per frame
    void Update()
    {
        id.text = currentId.ToString();
        if (OVRInput.GetUp(OVRInput.RawButton.X)) StartGame();
    }
}
