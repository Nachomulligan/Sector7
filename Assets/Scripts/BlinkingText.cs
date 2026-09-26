using System.Collections;
using TMPro;
using UnityEngine;


[RequireComponent(typeof(TMP_Text))]
public class BlinkingText : MonoBehaviour
{

    [SerializeField] private float blinkInterval = 1.2f;

    [SerializeField][Range(0f, 1f)] private float minAlpha = 0.15f;

    private TMP_Text label;
    private Coroutine blinkRoutine;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        SetAlpha(1f);
    }

    private IEnumerator BlinkLoop()
    {
        float halfCycle = blinkInterval * 0.5f;

        while (true)
        {
            yield return FadeTo(minAlpha, halfCycle);
            yield return FadeTo(1f, halfCycle);
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = label.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color c = label.color;
        c.a = alpha;
        label.color = c;
    }
}