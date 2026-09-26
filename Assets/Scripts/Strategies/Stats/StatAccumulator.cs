using UnityEngine;

public sealed class StatAccumulator
{
    private float maxHealth;
    private float damage;
    private float abilityCooldownMultiplier;

    public StatAccumulator(float maxHealth, float damage, float abilityCooldownMultiplier)
    {
        this.maxHealth = maxHealth;
        this.damage = damage;
        this.abilityCooldownMultiplier = abilityCooldownMultiplier;
    }

    public void Add(MechaStatType stat, float value)
    {
        switch (stat)
        {
            case MechaStatType.MaxHealth: maxHealth += value; break;
            case MechaStatType.Damage: damage += value; break;
            case MechaStatType.AbilityCooldown: abilityCooldownMultiplier += value; break;
        }
    }

    public void Multiply(MechaStatType stat, float multiplier)
    {
        switch (stat)
        {
            case MechaStatType.MaxHealth: maxHealth *= multiplier; break;
            case MechaStatType.Damage: damage *= multiplier; break;
            case MechaStatType.AbilityCooldown: abilityCooldownMultiplier *= multiplier; break;
        }
    }

    public RuntimeMechaStats Build() => new RuntimeMechaStats(
        Mathf.RoundToInt(maxHealth), Mathf.RoundToInt(damage), abilityCooldownMultiplier);
}
