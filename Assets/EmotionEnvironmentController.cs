using UnityEngine;

public class EmotionEnvironmentController : MonoBehaviour
{
    [Header("Reference")]
    public EmotionScoreManager scoreManager;

    [Header("Light")]
    public Light directionalLight;

    [Header("BGM")]
    public AudioSource bgm;

    void Start()
    {
        if (!scoreManager)
        {
            Debug.LogError("[EmotionEnvironmentController] ScoreManager missing");
            enabled = false;
            return;
        }

        scoreManager.OnScoreUpdated += OnEmotionUpdated;
    }

    void OnDestroy()
    {
        if (scoreManager)
            scoreManager.OnScoreUpdated -= OnEmotionUpdated;
    }

    void OnEmotionUpdated(float score, string emotionClass)
    {
        switch (emotionClass)
        {
            case "A_Flow":
                Apply(1.2f, 0.8f, Color.white);
                break;

            case "B_Immersed":
                Apply(1.0f, 0.6f, Color.white);
                break;

            case "C_Baseline":
                Apply(0.85f, 0.4f, Color.gray);
                break;

            case "D_Tense":
                Apply(0.6f, 0.25f, new Color(0.75f, 0.8f, 1f));
                break;
        }
    }

    void Apply(float lightIntensity, float volume, Color color)
    {
        if (directionalLight)
        {
            directionalLight.intensity = lightIntensity;
            directionalLight.color = color;
            RenderSettings.ambientIntensity = lightIntensity * 0.8f;
            RenderSettings.ambientLight = color;
        }

        if (bgm)
        {
            bgm.volume = volume;
            if (!bgm.isPlaying)
                bgm.Play();
        }
    }
}