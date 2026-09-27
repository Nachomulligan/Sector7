using System;
using System.Collections.Generic;
using UnityEngine;

public enum GachaPullFailure { None, MissingBanner, MissingInventory, InsufficientCurrency, InvalidBanner, PendingPull }

public sealed class GachaPendingPull
{
    public GachaBannerSO Banner { get; }
    public GachaRewardSO Reward { get; }
    public bool PityTriggered { get; }
    public int PreviousPullsSinceEpic { get; }

    public GachaPendingPull(GachaBannerSO banner, GachaRewardSO reward,
        bool pityTriggered, int previousPullsSinceEpic)
    {
        Banner = banner;
        Reward = reward;
        PityTriggered = pityTriggered;
        PreviousPullsSinceEpic = previousPullsSinceEpic;
    }
}

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
    private GachaPendingPull pendingPull;
    public event Action<GachaPendingPull> OnPullPending;
    public event Action<GachaPullResult> OnPullCompleted;
    public GachaBannerSO DefaultBanner => defaultBanner;
    public bool HasPendingPull => pendingPull != null;

    private void OnEnable() => ServiceLocator.Instance.Register(this);

    private void OnDisable()
    {
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
    }

    public int GetPullsSinceEpic(GachaBannerSO banner) =>
        banner != null && pullsSinceEpic.TryGetValue(banner.BannerId, out int pulls) ? pulls : 0;

    public bool TryBeginPull(GachaBannerSO banner, GachaCurrency currency,
        out GachaPullFailure failure)
    {
        failure = GachaPullFailure.None;
        if (pendingPull != null) { failure = GachaPullFailure.PendingPull; return false; }
        if (banner == null) { failure = GachaPullFailure.MissingBanner; return false; }
        if (!ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
        { failure = GachaPullFailure.MissingInventory; return false; }
        if (!inventory.CanSpend(currency, banner.Cost(currency)))
        { failure = GachaPullFailure.InsufficientCurrency; return false; }

        int previousPulls = GetPullsSinceEpic(banner);
        bool pity = previousPulls + 1 >= banner.EpicPityThreshold;
        if (!GachaRoller.TryRoll(banner, pity, () => UnityEngine.Random.value, out GachaRewardSO reward))
        { failure = GachaPullFailure.InvalidBanner; return false; }

        if (!inventory.TrySpend(currency, banner.Cost(currency)))
        { failure = GachaPullFailure.InsufficientCurrency; return false; }

        pendingPull = new GachaPendingPull(banner, reward, pity, previousPulls);
        OnPullPending?.Invoke(pendingPull);
        return true;
    }

    public bool TryBeginDefaultPull(GachaCurrency currency, out GachaPullFailure failure) =>
        TryBeginPull(defaultBanner, currency, out failure);

    public bool TryCompletePendingPull(out GachaPullResult result)
    {
        result = null;
        if (pendingPull == null || !ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
            return false;

        GachaPendingPull completed = pendingPull;
        pendingPull = null;
        bool duplicate = inventory.Grant(completed.Reward);
        int nextPulls = completed.Reward.Rarity >= GachaRarity.Epic
            ? 0 : completed.PreviousPullsSinceEpic + 1;
        pullsSinceEpic[completed.Banner.BannerId] = nextPulls;
        result = new GachaPullResult(completed.Reward, duplicate,
            completed.PityTriggered, nextPulls);
        OnPullCompleted?.Invoke(result);
        return true;
    }

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

        if (TryBeginDefaultPull(currency, out GachaPullFailure failure))
            Debug.Log("Gacha: tirada reservada; agitá el dispositivo para abrirla.", this);
        else Debug.LogWarning($"Gacha: tirada fallida ({failure}).", this);
    }
}
