using UnityEngine;


[RequireComponent(typeof(Health))]
public class ShieldVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Range(0f, 1f)]
    [SerializeField] private float shieldAlpha = 0.35f;

    private Health health;
    private float defaultAlpha = 1f;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultAlpha = spriteRenderer.color.a;
        }
    }

    private void OnEnable()
    {
        health.OnShieldChanged += HandleShieldChanged;
    }

    private void OnDisable()
    {
        health.OnShieldChanged -= HandleShieldChanged;
    }

    private void HandleShieldChanged(bool isActive)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color color = spriteRenderer.color;
        color.a = isActive ? shieldAlpha : defaultAlpha;
        spriteRenderer.color = color;
    }
}
