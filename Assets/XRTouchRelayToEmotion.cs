using UnityEngine;

public class XRTouchRelayToEmotion : MonoBehaviour
{
    public EmotionScoreManager score;
    int touchingCount = 0;

    bool IsHandOrController(Collider other)
    {
        if (other.GetComponentInParent<OVRHand>() != null) return true;
        if (other.GetComponentInParent<OVRGrabber>() != null) return true;

        Transform t = other.transform;
        while (t != null)
        {
            string n = t.name.ToLowerInvariant();
            if (n.Contains("hand") || n.Contains("controller") || n.Contains("interactor") || n.Contains("ovr"))
                return true;
            t = t.parent;
        }

        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TouchRelay] TriggerEnter by {other.name}");

        if (!IsHandOrController(other))
        {
            Debug.Log($"[TouchRelay] Rejected (not hand/controller): {other.name}");
            return;
        }

        touchingCount++;

        if (touchingCount == 1 && score)
        {
            score.SetTouching(true);
            score.RegisterInteractionOnce();
            Debug.Log($"[TouchRelay] Accepted -> Inter++");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TouchRelay] TriggerExit by {other.name}");

        if (!IsHandOrController(other)) return;

        touchingCount = Mathf.Max(0, touchingCount - 1);
        if (touchingCount == 0 && score)
        {
            score.SetTouching(false);
        }
    }
}
