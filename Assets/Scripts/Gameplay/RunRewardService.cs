using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunRewardService : MonoBehaviour
{
    [SerializeField] private ScoreRewardConfigSO rewardConfig;

    public event Action<int, int> OnRewardGranted;
    public int FinalScore { get; private set; }
    public int GrantedCredits { get; private set; }
    public bool IsSettled { get; private set; }

    private void OnEnable() => ServiceLocator.Instance.Register(this);

    private void OnDisable()
    {
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
    }

    public int SettleRun()
    {
        if (IsSettled) return GrantedCredits;
        IsSettled = true;

        if (ServiceLocator.Instance.TryGet(out ScoreManager scoreManager))
            FinalScore = scoreManager.CurrentScore;

        GrantedCredits = rewardConfig != null ? rewardConfig.CalculateCredits(FinalScore) : 0;
        if (GrantedCredits > 0 && ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
            inventory.AddCurrency(GachaCurrency.Soft, GrantedCredits);

        OnRewardGranted?.Invoke(FinalScore, GrantedCredits);
        return GrantedCredits;
    }
}
