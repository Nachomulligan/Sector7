using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class ScreenFlashUI : MonoBehaviour
{
    [Header("Flash")]
    [Range(0f, 1f)] [SerializeField] private float peakAlpha = 0.9f;
    [Min(0f)] [SerializeField] private float fadeInDuration = 0.04f;
    [Min(0f)] [SerializeField] private float holdDuration = 0.03f;
    [Min(0f)] [SerializeField] private float fadeOutDuration = 0.18f;

    private CanvasGroup canvasGroup;
    private Coroutine flashRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnDisable()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = null;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    public void Play()
    {
        if (!isActiveAndEnabled) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        yield return Fade(0f, peakAlpha, fadeInDuration);
        if (holdDuration > 0f) yield return new WaitForSecondsRealtime(holdDuration);
        yield return Fade(peakAlpha, 0f, fadeOutDuration);
        flashRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}
