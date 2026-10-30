using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GachaChestAnimationUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string closedStateName = "Closed";
    [SerializeField] private string openingTriggerName = "Open";

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
}
