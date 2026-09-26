using UnityEngine;

public enum GachaRarity { Common, Rare, Epic, Legendary }
public enum GachaRewardKind { Mecha, Weapon, Ability, Companion }

[CreateAssetMenu(menuName = "Sector7/Gacha/Reward", fileName = "Reward_")]
public sealed class GachaRewardSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private GachaRarity rarity;
    [SerializeField] private GachaRewardKind kind;
    [SerializeField] private MechaPresetSO mecha;
    [SerializeField] private WeaponStrategySO weapon;
    [SerializeField] private SkillStrategySO ability;
    [SerializeField] private CompanionPresetSO companion;
    [Header("Nivel por duplicados")]
    [Min(1)] [SerializeField] private int maxLevel = 10;
    [Min(0f)] [SerializeField] private float bonusPerLevelPercent = 0.05f;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Icon => icon;
    public GachaRarity Rarity => rarity;
    public GachaRewardKind Kind => kind;
    public MechaPresetSO Mecha => mecha;
    public WeaponStrategySO Weapon => weapon;
    public SkillStrategySO Ability => ability;
    public CompanionPresetSO Companion => companion;
    public int MaxLevel => maxLevel;
    public float BonusPerLevelPercent => bonusPerLevelPercent;
    public float LevelMultiplier(int level) => 1f + Mathf.Max(0, level - 1) * bonusPerLevelPercent;
    public bool IsValid => kind == GachaRewardKind.Mecha ? mecha != null
        : kind == GachaRewardKind.Weapon ? weapon != null
        : kind == GachaRewardKind.Ability ? ability != null : companion != null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!IsValid)
            Debug.LogWarning($"{name}: asigná el asset correspondiente al tipo de recompensa.", this);
    }
#endif
}
