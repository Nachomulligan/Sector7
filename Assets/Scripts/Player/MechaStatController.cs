using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public sealed class MechaStatController : MonoBehaviour
{
    public event Action<RuntimeMechaStats> OnStatsChanged;
    public RuntimeMechaStats Current { get; private set; } = new RuntimeMechaStats(100, 10, 1f);

    private Health health;

    private void Awake() => health = GetComponent<Health>();

    public void Recalculate(MechaPresetSO mecha, float mechaLevelMultiplier,
        CompanionPresetSO companion, float companionLevelMultiplier)
    {
        int healthValue = mecha != null ? mecha.BaseMaxHealth : 100;
        int damageValue = mecha != null ? mecha.BaseDamage : 10;
        StatAccumulator accumulator = new StatAccumulator(
            healthValue * mechaLevelMultiplier,
            damageValue * mechaLevelMultiplier,
            1f);

        if (companion != null && companion.Modifier != null)
            companion.Modifier.Apply(accumulator, companionLevelMultiplier);

        Current = accumulator.Build();
        health.SetMaxHealth(Current.MaxHealth);
        OnStatsChanged?.Invoke(Current);
    }
}
