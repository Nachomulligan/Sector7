using System;
using System.Collections.Generic;
using UnityEngine;

public enum GachaCurrency { Soft, Premium }

[Serializable]
public sealed class RarityWeight
{
    [SerializeField] private GachaRarity rarity;
    [Min(0f)] [SerializeField] private float weight = 1f;
    public GachaRarity Rarity => rarity;
    public float Weight => weight;
}

[Serializable]
public sealed class WeightedGachaReward
{
    [SerializeField] private GachaRewardSO reward;
    [Min(0f)] [SerializeField] private float weight = 1f;
    public GachaRewardSO Reward => reward;
    public float Weight => weight;
}

[CreateAssetMenu(menuName = "Sector7/Gacha/Banner", fileName = "Banner_")]
public sealed class GachaBannerSO : ScriptableObject
{
    [SerializeField] private string bannerId = "standard";
    [Min(0)] [SerializeField] private int softCost = 100;
    [Min(0)] [SerializeField] private int premiumCost = 10;
    [Min(1)] [SerializeField] private int epicPityThreshold = 10;
    [SerializeField] private List<RarityWeight> rarityWeights = new List<RarityWeight>();
    [SerializeField] private List<WeightedGachaReward> rewards = new List<WeightedGachaReward>();

    public string BannerId => bannerId;
    public int SoftCost => softCost;
    public int PremiumCost => premiumCost;
    public int EpicPityThreshold => Mathf.Max(1, epicPityThreshold);
    public IReadOnlyList<RarityWeight> RarityWeights => rarityWeights;
    public IReadOnlyList<WeightedGachaReward> Rewards => rewards;
    public int Cost(GachaCurrency currency) => currency == GachaCurrency.Soft ? softCost : premiumCost;
}
