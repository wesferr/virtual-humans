using UnityEngine;

public class RecorderCaller : MonoBehaviour
{
    void Update()
    {
        // OVR (botão X)
        if (OVRInput.GetDown(OVRInput.RawButton.X)) EventHub.RecordStart();
        if (OVRInput.GetUp(OVRInput.RawButton.X))   EventHub.RecordStop();

        // Teclado (fallback): R para gravar, T para parar
        if (Input.GetKeyDown(KeyCode.R)) EventHub.RecordStart();
        if (Input.GetKeyDown(KeyCode.T)) EventHub.RecordStop();
    }
}