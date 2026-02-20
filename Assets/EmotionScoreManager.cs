using System;
using System.IO;
using UnityEngine;

public class EmotionScoreManager : MonoBehaviour
{
    [Header("Timing")]
    public float windowSeconds = 5f;
    public float ignoreFirstSeconds = 2f;
    public float baselineWarmupSeconds = 2f;

    [Header("Weights (sum=1 recommended)")]
    [Range(0, 1)] public float wDwell = 0.45f;
    [Range(0, 1)] public float wRot = 0.25f;
    [Range(0, 1)] public float wInter = 0.10f;
    [Range(0, 1)] public float wHR = 0.20f;

    [Header("References")]
    public Camera mainCam;

    [Header("Target (Money)")]
    public string targetTag = "money";
    public float rayDistance = 100f;

    [Header("Debug")]
    public bool logToConsole = true;

    [Header("Heart Rate")]
    public HeartRateSimulator hrSim;
    public bool includeHeartRate = true;

    [Tooltip("HR 변화율(-20%~+20%)을 0~1로 매핑. 예: 0.2 = ±20%")]
    public float hrRatioRange = 0.20f;

    public event Action<float, string> OnScoreUpdated;

    float sessionTime;
    float windowTime;

    Quaternion prevRot;
    float rotSpeedSum;
    int rotSpeedCount;

    float engageDwell;               
    bool isTouching;
    bool isGrabbing;

    int interactionCount;

    bool baselineReady;
    float baseDwellSum, baseRotSum, baseInterSum, baseHrSum;
    int baseCount;

    string logPath;

    void Start()
    {
        if (!mainCam) mainCam = Camera.main;
        if (!mainCam)
        {
            Debug.LogError("[EmotionScoreManager] Main Camera not assigned/found.");
            enabled = false;
            return;
        }

        prevRot = mainCam.transform.rotation;

        logPath = Path.Combine(Application.persistentDataPath,
            $"emotion_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        File.WriteAllText(logPath, "t,score,emotion,dwell,rot,inter,hr\n");
        Debug.Log($"[EmotionScoreManager] Logging to: {logPath}");
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        sessionTime += dt;
        windowTime += dt;

        var currRot = mainCam.transform.rotation;
        float ang = Quaternion.Angle(prevRot, currRot);
        float rotSpeed = ang / dt;
        rotSpeedSum += rotSpeed;
        rotSpeedCount++;
        prevRot = currRot;

        UpdateEngagementDwell(dt);

        if (windowTime >= windowSeconds)
        {
            ProcessWindow();
            ResetWindow();
        }
    }

    void UpdateEngagementDwell(float dt)
    {
        bool isLooking = IsLookingAtTarget();

        if (isLooking || isTouching || isGrabbing)
            engageDwell += dt;
    }

    bool IsLookingAtTarget()
    {
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.collider != null && hit.collider.CompareTag(targetTag))
                return true;
        }
        return false;
    }

    void ProcessWindow()
    {
        float hrBpm = (includeHeartRate && hrSim != null) ? hrSim.currentBpm : 0f;
        float avgRot = (rotSpeedCount > 0) ? (rotSpeedSum / rotSpeedCount) : 0f;

        if (sessionTime < ignoreFirstSeconds)
            return;

        float warmupEnd = ignoreFirstSeconds + baselineWarmupSeconds;
        if (!baselineReady && sessionTime < warmupEnd)
        {
            baseDwellSum += engageDwell;
            baseRotSum += avgRot;
            baseInterSum += interactionCount;

            if (includeHeartRate && hrBpm > 0f)
                baseHrSum += hrBpm;

            baseCount++;
            return;
        }

        if (!baselineReady)
        {
            if (baseCount <= 0)
            {
                baseDwellSum = 1f;
                baseRotSum = 30f;
                baseInterSum = 1f;
                baseHrSum = 70f;
                baseCount = 1;
            }
            baselineReady = true;
        }

        float baseDwell = baseDwellSum / baseCount;
        float baseRot = baseRotSum / baseCount;
        float baseInter = baseInterSum / baseCount;
        float baseHr = (includeHeartRate && baseHrSum > 0f) ? (baseHrSum / baseCount) : 70f;

        float dwellNorm = NormalizeRatio(engageDwell, baseDwell);           
        float rotNorm   = NormalizeInverseRatio(avgRot, baseRot);           
        float interNorm = NormalizeRatio(interactionCount, baseInter);     

        float hrScore = 0.5f;
        if (includeHeartRate && hrBpm > 0f && baseHr > 0f)
        {
            float ratio = (hrBpm - baseHr) / baseHr;
            float range = Mathf.Max(0.05f, hrRatioRange);
            float hrNorm = Mathf.InverseLerp(-range, range, ratio);
            hrScore = 1f - Mathf.Clamp01(Mathf.Abs(hrNorm - 0.5f) * 2f);
        }

        float score =
            wDwell * dwellNorm +
            wRot   * rotNorm +
            wInter * interNorm +
            (includeHeartRate ? (wHR * hrScore) : 0f);

        score = Mathf.Clamp01(score);

        string emotionClass = ClassifyEmotion(score);

        string line = $"{sessionTime:F2},{score:F3},{emotionClass},{engageDwell:F2},{avgRot:F2},{interactionCount},{hrBpm:F1}\n";
        File.AppendAllText(logPath, line);

        if (logToConsole)
        {
            Debug.Log($"[Emotion] t={sessionTime:F2}s | score={score:F3} | class={emotionClass} | dwell={engageDwell:F2}s | rot={avgRot:F2}deg/s | inter={interactionCount} | hr={hrBpm:F1}bpm");
        }

        OnScoreUpdated?.Invoke(score, emotionClass);
    }

    void ResetWindow()
    {
        windowTime = 0f;
        rotSpeedSum = 0f;
        rotSpeedCount = 0;
        engageDwell = 0f;

        interactionCount = 0;
    }

    float NormalizeRatio(float v, float baseline)
    {
        float eps = 1e-5f;
        return Mathf.Clamp01(v / (baseline + eps));
    }

    float NormalizeInverseRatio(float v, float baseline)
    {
        float eps = 1e-5f;
        return Mathf.Clamp01((baseline + eps) / (v + eps));
    }

    string ClassifyEmotion(float score)
    {
        if (score >= 0.75f) return "A_Flow";
        if (score >= 0.55f) return "B_Immersed";
        if (score >= 0.35f) return "C_Baseline";
        return "D_Tense";
    }

    public string GetLogPath() => logPath;

    public void SetTouching(bool touching) => isTouching = touching;

    public void SetGrabbing(bool grabbing) => isGrabbing = grabbing;


    public void RegisterInteractionOnce()
{
    interactionCount++;
    Debug.Log($"[Inter++] {name} => {interactionCount}");
}

}
