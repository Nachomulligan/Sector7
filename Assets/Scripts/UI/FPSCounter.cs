using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public sealed class FPSCounter : MonoBehaviour
{
    [Header("Medición")]
    [Min(0.1f)] [SerializeField] private float refreshInterval = 0.5f;
    [Min(1)] [SerializeField] private int targetFps = 60;

    [Header("Colores")]
    [SerializeField] private Color goodColor = new Color(0.35f, 1f, 0.55f);
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0.25f);
    [SerializeField] private Color badColor = new Color(1f, 0.35f, 0.35f);

    private TMP_Text label;
    private float accumulatedTime;
    private int accumulatedFrames;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        accumulatedTime = 0f;
        accumulatedFrames = 0;
        RefreshLabel(targetFps);
    }

    private void Update()
    {
        accumulatedTime += Time.unscaledDeltaTime;
        accumulatedFrames++;

        if (accumulatedTime < refreshInterval) return;

        float fps = accumulatedFrames / Mathf.Max(accumulatedTime, 0.0001f);
        RefreshLabel(fps);
        accumulatedTime = 0f;
        accumulatedFrames = 0;
    }

    private void RefreshLabel(float fps)
    {
        float frameMilliseconds = 1000f / Mathf.Max(fps, 0.01f);
        label.text = $"FPS {fps:0}\n{frameMilliseconds:0.0} ms";

        float ratio = fps / targetFps;
        label.color = ratio >= 0.9f ? goodColor : ratio >= 0.6f ? warningColor : badColor;
    }
}
