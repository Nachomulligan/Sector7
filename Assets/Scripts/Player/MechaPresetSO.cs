using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Mechas/Mecha Preset", fileName = "Mecha_")]
public sealed class MechaPresetSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite appearance;
    [SerializeField] private WeaponStrategySO startingWeapon;
    [SerializeField] private SkillStrategySO startingAbility;
    [Min(0)] [SerializeField] private int healthBonusPerAscension = 20;
    [Min(0)] [SerializeField] private int maxAscension = 5;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Appearance => appearance;
    public WeaponStrategySO StartingWeapon => startingWeapon;
    public SkillStrategySO StartingAbility => startingAbility;
    public int HealthBonusPerAscension => healthBonusPerAscension;
    public int MaxAscension => maxAscension;
}
