using UnityEngine;

[RequireComponent(typeof(OVRGrabbable))]
public class XRGrabRelayToEmotion : MonoBehaviour
{
    public EmotionScoreManager score;

    OVRGrabbable grabbable;
    bool prevGrabbed;

    void Awake()
    {
        grabbable = GetComponent<OVRGrabbable>();
        prevGrabbed = grabbable.isGrabbed;
    }

    void OnEnable()
    {
        if (score) score.SetGrabbing(grabbable.isGrabbed);
        prevGrabbed = grabbable.isGrabbed;
    }

    void Update()
    {
        bool now = grabbable.isGrabbed;
        if (now == prevGrabbed) return;

        prevGrabbed = now;

        if (!score) return;

        if (now)
        {
            score.SetGrabbing(true);
            score.RegisterInteractionOnce(); 
        }
        else
        {
            score.SetGrabbing(false);
        }
    }
}
