using UnityEngine;

[DisallowMultipleComponent]
public class ZoneVolume : MonoBehaviour
{
    [Header("Zone Identity")]
    [Tooltip("비우면 오브젝트 이름을 사용")]
    public string zoneId;

    [Header("Safety")]
    [Tooltip("Zone 오브젝트가 money 태그일 때만 존으로 인정할지")]
    public bool requireMoneyTag = false;

    [Tooltip("requireMoneyTag=true일 때 확인할 태그 이름")]
    public string moneyTag = "money";

    void Reset()
    {
        if (string.IsNullOrEmpty(zoneId)) zoneId = gameObject.name;

        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    public bool IsValidZone()
    {
        if (!requireMoneyTag) return true;
        return CompareTag(moneyTag);
    }

    public string Id => string.IsNullOrEmpty(zoneId) ? gameObject.name : zoneId;
}
