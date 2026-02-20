using System;
using System.Reflection;
using UnityEngine;

public class MetaTouchHandGrabRelayToEmotion : MonoBehaviour
{
    public EmotionScoreManager score;

    Component touchGrab;   
    bool prevGrabbed;

    void Awake()
    {
        if (!score) score = FindFirstObjectByType<EmotionScoreManager>();

        foreach (var c in GetComponents<Component>())
        {
            if (c == null) continue;
            string tn = c.GetType().Name;
            if (tn.Contains("TouchHandGrab") || tn.Contains("HandGrab"))
            {
                touchGrab = c;
                break;
            }
        }

        Debug.Log($"[MetaGrabRelay] Awake on {name} | score={(score?score.name:"NULL")} | comp={(touchGrab?touchGrab.GetType().Name:"NULL")}");
    }

    void Update()
    {
        if (!score || touchGrab == null) return;

        bool grabbedNow = ReadGrabState(touchGrab);

        if (grabbedNow == prevGrabbed) return;
        prevGrabbed = grabbedNow;

        score.SetGrabbing(grabbedNow);

        if (grabbedNow)
        {
            score.RegisterInteractionOnce();
            Debug.Log("[MetaGrabRelay] GRAB -> Inter++");
        }
    }

    bool ReadGrabState(Component c)
    {
        var t = c.GetType();

        string[] boolNames = { "IsSelected", "Selected", "isSelected", "IsGrabbed", "Grabbed", "isGrabbed", "IsInteracting" };
        foreach (var n in boolNames)
        {
            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(bool))
                try { return (bool)p.GetValue(c); } catch { }

            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(bool))
                try { return (bool)f.GetValue(c); } catch { }
        }

        string[] listNames = { "SelectingInteractors", "selectingInteractors", "Interactors", "interactors" };
        foreach (var n in listNames)
        {
            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null)
                try { if (p.GetValue(c) is System.Collections.ICollection col) return col.Count > 0; } catch { }

            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null)
                try { if (f.GetValue(c) is System.Collections.ICollection col) return col.Count > 0; } catch { }
        }

        return false;
    }
}
