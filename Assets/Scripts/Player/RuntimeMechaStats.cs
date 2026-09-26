using System;

[Serializable]
public readonly struct RuntimeMechaStats
{
    public RuntimeMechaStats(int maxHealth, int damage, float abilityCooldownMultiplier)
    {
        MaxHealth = Math.Max(1, maxHealth);
        Damage = Math.Max(1, damage);
        AbilityCooldownMultiplier = Math.Max(0.05f, abilityCooldownMultiplier);
    }

    public int MaxHealth { get; }
    public int Damage { get; }
    public float AbilityCooldownMultiplier { get; }
}
