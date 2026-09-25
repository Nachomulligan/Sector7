using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private Camera worldCamera;

    [Header("UI")]
    [SerializeField] private Health health;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform selfRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Comportamiento")]
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private float hideTimer;
    private int previousHealth;
    private bool initialized;
    private Coroutine fadeRoutine;

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            previousHealth = health.CurrentHealth;
            initialized = true;

            HandleFillOnly(health.CurrentHealth, health.MaxHealth);
            SetAlphaInstant(0f);
        }
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnHealthChanged -= HandleHealthChanged;

        initialized = false;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    private void LateUpdate()
    {
        if (target == null || worldCamera == null || selfRect == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(target.position + worldOffset);
        selfRect.position = screenPos;

        if (canvasGroup != null && canvasGroup.alpha > 0f && fadeRoutine == null)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                StartFadeOut();
            }
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        HandleFillOnly(current, max);

        bool tookDamage = initialized && current < previousHealth;
        previousHealth = current;

        if (tookDamage)
        {
            ShowImmediate();
        }
    }

    private void HandleFillOnly(int current, int max)
    {
        if (fillImage == null || max <= 0) return;
        fillImage.fillAmount = Mathf.Clamp01((float)current / max);
    }

    private void ShowImmediate()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        SetAlphaInstant(1f);
        hideTimer = visibleDuration;
    }

    private void StartFadeOut()
    {
        fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        fadeRoutine = null;
    }

    private void SetAlphaInstant(float value)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = value;
        }
    }
}