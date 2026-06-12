using UnityEngine;
using UnityEngine.UI;

public class InputCaller : MonoBehaviour
{
    public Button endCaseButton;

    void Start()
    {
        endCaseButton.onClick.AddListener(() => EventHub.EndCase());
    }
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger)) EventHub.RecordStart();
        if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger)) EventHub.RecordStop();

        // if (OVRInput.GetDown(OVRInput.RawButton.X)) EventHub.RecordStart();
        // if (OVRInput.GetUp(OVRInput.RawButton.X)) EventHub.RecordStop();
        // if (OVRInput.GetDown(OVRInput.RawButton.X))
        // {
        //     Debug.Log("OVRInput.RawButton.X pressed");
        //     EventHub.EndCase();
        // }

        // if (Input.GetKeyDown(KeyCode.B))
        // {
        //     EventHub.EndCase();
        // }
        
    }
}