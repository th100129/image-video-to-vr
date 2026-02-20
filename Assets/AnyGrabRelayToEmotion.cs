using System;
using System.Reflection;
using UnityEngine;

public class AnyGrabRelayToEmotion : MonoBehaviour
{
public EmotionScoreManager score;
private OVRGrabbable ovr;

private Component grabbableComp;
private Func<bool> getGrabbableSelected; 

private bool prevGrab;

void Awake()
{
    ovr = GetComponent<OVRGrabbable>();

    grabbableComp = GetComponent("Grabbable") as Component;

    getGrabbableSelected = BuildSelectedGetter(grabbableComp);

    prevGrab = IsAnyGrabbed();
}

void OnEnable()
{
    prevGrab = IsAnyGrabbed();
    if (score) score.SetGrabbing(prevGrab);
}

void Update()
{
    bool now = IsAnyGrabbed();
    if (now == prevGrab) return;

    prevGrab = now;

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

bool IsAnyGrabbed()
{
    bool a = (ovr != null) && ovr.isGrabbed;

    bool b = (getGrabbableSelected != null) && getGrabbableSelected();

    return a || b;
}

Func<bool> BuildSelectedGetter(Component c)
{
    if (c == null) return null;

    var t = c.GetType();

    string[] boolProps = { "IsSelected", "isSelected", "Selected", "IsGrabbed", "isGrabbed", "IsHeld", "isHeld" };
    foreach (var name in boolProps)
    {
        var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(bool))
            return () => (bool)p.GetValue(c);

        var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool))
            return () => (bool)f.GetValue(c);
    }

    string[] methods = { "IsSelected", "get_IsSelected", "IsGrabbed", "IsHeld" };
    foreach (var mname in methods)
    {
        var m = t.GetMethod(mname, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (m != null && m.ReturnType == typeof(bool))
            return () => (bool)m.Invoke(c, null);
    }

    return null;
}
}