using System;
using System.Collections.Generic;
using UnityEngine;

public enum GachaPullFailure { None, MissingBanner, MissingInventory, InsufficientCurrency, InvalidBanner }

public sealed class GachaPullResult
{
    public GachaRewardSO Reward { get; }
    public bool WasDuplicate { get; }
    public bool PityTriggered { get; }
    public int PullsSinceEpic { get; }

    public GachaPullResult(GachaRewardSO reward, bool wasDuplicate, bool pityTriggered, int pullsSinceEpic)
    {
        Reward = reward;
        WasDuplicate = wasDuplicate;
        PityTriggered = pityTriggered;
        PullsSinceEpic = pullsSinceEpic;
    }
}

[DisallowMultipleComponent]
public sealed class GachaService : MonoBehaviour
{
    [SerializeField] private GachaBannerSO defaultBanner;

    private readonly Dictionary<string, int> pullsSinceEpic = new Dictionary<string, int>();
    public event Action<GachaPullResult> OnPullCompleted;
    public GachaBannerSO DefaultBanner => defaultBanner;

    private void OnEnable() => ServiceLocator.Instance.Register(this);

    private void OnDisable()
    {
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
    }

    public int GetPullsSinceEpic(GachaBannerSO banner) =>
        banner != null && pullsSinceEpic.TryGetValue(banner.BannerId, out int pulls) ? pulls : 0;

    public bool TryPull(GachaBannerSO banner, GachaCurrency currency,
        out GachaPullResult result, out GachaPullFailure failure)
    {
        result = null;
        failure = GachaPullFailure.None;
        if (banner == null) { failure = GachaPullFailure.MissingBanner; return false; }
        if (!ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
        { failure = GachaPullFailure.MissingInventory; return false; }
        if (!inventory.CanSpend(currency, banner.Cost(currency)))
        { failure = GachaPullFailure.InsufficientCurrency; return false; }

        int previousPulls = GetPullsSinceEpic(banner);
        bool pity = previousPulls + 1 >= banner.EpicPityThreshold;
        if (!GachaRoller.TryRoll(banner, pity, () => UnityEngine.Random.value, out GachaRewardSO reward))
        { failure = GachaPullFailure.InvalidBanner; return false; }

        inventory.TrySpend(currency, banner.Cost(currency));
        bool duplicate = inventory.Grant(reward);
        int nextPulls = reward.Rarity >= GachaRarity.Epic ? 0 : previousPulls + 1;
        pullsSinceEpic[banner.BannerId] = nextPulls;
        result = new GachaPullResult(reward, duplicate, pity, nextPulls);
        OnPullCompleted?.Invoke(result);
        return true;
    }

    public bool TryPullDefault(GachaCurrency currency, out GachaPullResult result,
        out GachaPullFailure failure) => TryPull(defaultBanner, currency, out result, out failure);

    // Se pueden conectar directamente a botones del Inspector.
    public void PullSoft() => PullAndLog(GachaCurrency.Soft);
    public void PullPremium() => PullAndLog(GachaCurrency.Premium);

    [ContextMenu("Probar tirada soft")]
    private void TestSoftPull() => PullSoft();

    private void PullAndLog(GachaCurrency currency)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Iniciá Play Mode para probar el gacha.", this);
            return;
        }

        if (TryPullDefault(currency, out GachaPullResult result, out GachaPullFailure failure))
            Debug.Log($"Gacha: {result.Reward.DisplayName} ({result.Reward.Rarity})" +
                (result.WasDuplicate ? " - duplicado" : ""), this);
        else Debug.LogWarning($"Gacha: tirada fallida ({failure}).", this);
    }
}
