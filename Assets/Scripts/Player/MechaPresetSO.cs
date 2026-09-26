using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Mechas/Mecha Preset", fileName = "Mecha_")]
public sealed class MechaPresetSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite appearance;
    [SerializeField] private Color appearanceColor = Color.white;
    [Min(1)] [SerializeField] private int baseMaxHealth = 100;
    [Min(1)] [SerializeField] private int baseDamage = 10;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public Sprite Appearance => appearance;
    public Color AppearanceColor => appearanceColor;
    public int BaseMaxHealth => baseMaxHealth;
    public int BaseDamage => baseDamage;
}
