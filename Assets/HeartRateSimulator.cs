using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class HeartRateSimulator : MonoBehaviour
{
    [Header("Simulation")]
    public bool useRandomWalk = true;
    public float baselineBpm = 72f;
    public float amplitude = 18f;
    public float changeSpeed = 0.6f;
    public float updateInterval = 5f;

    [Header("Manual Override (Arrow Keys)")]
    public bool enableManualKeys = true;
    public float manualStep = 5f;
    public bool freezeAutoWhenManual = false;

    [Header("Read-only")]
    public float currentBpm;

    float t;
    bool manualTouched;

    void Start() => currentBpm = baselineBpm;

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (enableManualKeys && Keyboard.current != null)
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) { currentBpm += manualStep; manualTouched = true; }
            if (Keyboard.current.downArrowKey.wasPressedThisFrame) { currentBpm -= manualStep; manualTouched = true; }
        }
#endif
        currentBpm = Mathf.Clamp(currentBpm, baselineBpm - amplitude, baselineBpm + amplitude);

        if (freezeAutoWhenManual && manualTouched) return;

        t += Time.deltaTime;
        if (t >= updateInterval)
        {
            t = 0f;

            if (useRandomWalk)
            {
                float step = Random.Range(-3f, 3f) * changeSpeed;
                currentBpm = Mathf.Clamp(currentBpm + step, baselineBpm - amplitude, baselineBpm + amplitude);
            }
            else
            {
                float phase = Time.time * changeSpeed;
                currentBpm = baselineBpm + Mathf.Sin(phase) * amplitude;
            }
        }
    }
}
