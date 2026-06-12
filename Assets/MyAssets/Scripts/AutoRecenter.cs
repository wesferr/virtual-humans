using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class AutoRecenter : MonoBehaviour
{
    void OnEnable()
    {
#if META_SDK
        OVRManager.InputFocusAcquired += ReapplyFloorAndRecenter;
        OVRManager.TrackingAcquired    += ReapplyFloorAndRecenter;
        OVRManager.HMDMounted          += ReapplyFloorAndRecenter; // se disponível na sua versão
#endif
    }

    void OnDisable()
    {
#if META_SDK
        OVRManager.InputFocusAcquired -= ReapplyFloorAndRecenter;
        OVRManager.TrackingAcquired    -= ReapplyFloorAndRecenter;
        OVRManager.HMDMounted          -= ReapplyFloorAndRecenter;
#endif
    }

    void Start() { ReapplyFloorAndRecenter(); }            // também no boot
    void OnApplicationFocus(bool f) { if (f) ReapplyFloorAndRecenter(); }

    void ReapplyFloorAndRecenter()
    {
        var subs = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs)
        {
            s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            s.TryRecenter();
        }
#if META_SDK
        // Se estiver usando OVR:
        OVRManager.display?.RecenterPose();
#endif
    }
}
