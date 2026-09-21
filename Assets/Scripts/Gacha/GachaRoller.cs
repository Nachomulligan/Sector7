using System;
using UnityEngine;

// La selección no toca inventario ni UI; el servicio coordina esas partes.
public static class GachaRoller
{
    public static bool TryRoll(GachaBannerSO banner, bool forceEpicOrBetter, Func<float> random01,
        out GachaRewardSO reward)
    {
        reward = null;
        if (banner == null || random01 == null) return false;

        float totalRarityWeight = 0f;
        foreach (RarityWeight entry in banner.RarityWeights)
        {
            if (entry == null || entry.Weight <= 0f ||
                (forceEpicOrBetter && entry.Rarity < GachaRarity.Epic) ||
                !HasReward(banner, entry.Rarity)) continue;
            totalRarityWeight += entry.Weight;
        }
        if (totalRarityWeight <= 0f) return false;

        float rarityRoll = Mathf.Clamp01(random01()) * totalRarityWeight;
        GachaRarity selectedRarity = GachaRarity.Common;
        foreach (RarityWeight entry in banner.RarityWeights)
        {
            if (entry == null || entry.Weight <= 0f ||
                (forceEpicOrBetter && entry.Rarity < GachaRarity.Epic) ||
                !HasReward(banner, entry.Rarity)) continue;
            selectedRarity = entry.Rarity;
            rarityRoll -= entry.Weight;
            if (rarityRoll <= 0f) break;
        }

        float totalRewardWeight = 0f;
        foreach (WeightedGachaReward entry in banner.Rewards)
            if (IsEligible(entry, selectedRarity)) totalRewardWeight += entry.Weight;

        float rewardRoll = Mathf.Clamp01(random01()) * totalRewardWeight;
        foreach (WeightedGachaReward entry in banner.Rewards)
        {
            if (!IsEligible(entry, selectedRarity)) continue;
            reward = entry.Reward;
            rewardRoll -= entry.Weight;
            if (rewardRoll <= 0f) return true;
        }
        return reward != null;
    }

    private static bool HasReward(GachaBannerSO banner, GachaRarity rarity)
    {
        foreach (WeightedGachaReward entry in banner.Rewards)
            if (IsEligible(entry, rarity)) return true;
        return false;
    }

    private static bool IsEligible(WeightedGachaReward entry, GachaRarity rarity) =>
        entry != null && entry.Weight > 0f && entry.Reward != null &&
        entry.Reward.IsValid && entry.Reward.Rarity == rarity;
}
