using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GachaChestAnimationUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string closedStateName = "Closed";
    [SerializeField] private string openingTriggerName = "Open";

    [Header("Audio")]
    [Tooltip("Sonido sincronizado con el momento en que el cofre termina de abrirse.")]
    [SerializeField] private AudioClip openingSound;
    [Range(0f, 1f)] [SerializeField] private float openingSoundVolume = 1f;

    private int closedStateHash;
    private int openingTriggerHash;
    private bool isOpening;

    public event Action OpeningCompleted;
    public bool IsOpening => isOpening;

    private void Awake()
    {
        closedStateHash = Animator.StringToHash(closedStateName);
        openingTriggerHash = Animator.StringToHash(openingTriggerName);
    }

    private void OnDisable() => isOpening = false;

    public void ShowClosed()
    {
        isOpening = false;
        if (animator == null || !animator.isActiveAndEnabled) return;
        animator.ResetTrigger(openingTriggerHash);
        animator.Play(closedStateHash, 0, 0f);
        animator.Update(0f);
    }

    public bool PlayOpening()
    {
        if (animator == null || !animator.isActiveAndEnabled || isOpening) return false;
        isOpening = true;
        animator.ResetTrigger(openingTriggerHash);
        animator.SetTrigger(openingTriggerHash);
        return true;
    }

    // Invocado por un Animation Event al terminar el destello del cofre.
    public void NotifyOpeningCompleted()
    {
        if (!isOpening) return;
        isOpening = false;
        OpeningCompleted?.Invoke();
    }

    // Invocado por el Animation Event del cuadro de apertura.
    public void PlayOpeningSound()
    {
        if (ServiceLocator.Instance.TryGet(out AudioService audioService))
            audioService.PlaySfx(openingSound, openingSoundVolume);
    }
}
