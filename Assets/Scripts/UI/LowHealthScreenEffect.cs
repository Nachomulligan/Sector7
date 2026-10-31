using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LowHealthVignetteGraphic))]
public sealed class LowHealthScreenEffect : MonoBehaviour
{
    [Header("Vida baja")]
    [Range(0.05f, 0.95f)] [SerializeField] private float healthThreshold = 0.3f;
    [SerializeField] private Color vignetteColor = new Color(0.75f, 0f, 0f, 1f);
    [Range(0f, 1f)] [SerializeField] private float minimumAlpha = 0.2f;
    [Range(0f, 1f)] [SerializeField] private float maximumAlpha = 0.72f;

    [Header("Pulso")]
    [Min(0f)] [SerializeField] private float pulseSpeed = 1.2f;
    [Range(0f, 1f)] [SerializeField] private float pulseStrength = 0.18f;

    private LowHealthVignetteGraphic vignette;
    private Health health;
    private float healthIntensity;

    private void Awake()
    {
        vignette = GetComponent<LowHealthVignetteGraphic>();
        vignette.raycastTarget = false;
        ApplyAlpha(0f);
    }

    private void Update()
    {
        if (healthIntensity <= 0f) return;

        float baseAlpha = Mathf.Lerp(minimumAlpha, maximumAlpha, healthIntensity);
        float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        ApplyAlpha(Mathf.Clamp01(baseAlpha + pulse * pulseStrength * healthIntensity));
    }

    public void Bind(Health playerHealth)
    {
        if (health == playerHealth) return;
        Unbind();
        health = playerHealth;
        if (health == null) return;

        health.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(health.CurrentHealth, health.MaxHealth);
    }

    public void Unbind()
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
        health = null;
        healthIntensity = 0f;
        ApplyAlpha(0f);
    }

    private void OnDestroy() => Unbind();

    private void HandleHealthChanged(int current, int max)
    {
        float normalizedHealth = max > 0 ? current / (float)max : 0f;
        healthIntensity = current > 0 && normalizedHealth < healthThreshold
            ? Mathf.InverseLerp(healthThreshold, 0f, normalizedHealth)
            : 0f;

        if (healthIntensity <= 0f) ApplyAlpha(0f);
    }

    private void ApplyAlpha(float alpha)
    {
        if (vignette == null) return;
        Color color = vignetteColor;
        color.a = alpha;
        vignette.color = color;
    }
}
