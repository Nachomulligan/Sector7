using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[CreateAssetMenu(menuName = "Sector7/Stats/Modifier Strategy", fileName = "Modifier_")]
public sealed class StatModifierStrategySO : ScriptableObject
{
    [Serializable]
    private struct Entry
    {
        public MechaStatType stat;
        public StatModifierOperation operation;
        public float value;
    }

    [SerializeField] private List<Entry> modifiers = new List<Entry>();

    public void Apply(StatAccumulator accumulator, float levelMultiplier)
    {
        if (accumulator == null) return;
        foreach (Entry modifier in modifiers)
        {
            if (modifier.operation == StatModifierOperation.Flat)
                accumulator.Add(modifier.stat, modifier.value * levelMultiplier);
            else
                accumulator.Multiply(modifier.stat, 1f + modifier.value * levelMultiplier);
        }
    }

    public string GetStatsDescription(float levelMultiplier)
    {
        if (modifiers.Count == 0) return "Sin modificadores";
        StringBuilder builder = new StringBuilder();
        foreach (Entry modifier in modifiers)
        {
            if (builder.Length > 0) builder.AppendLine();
            float scaledValue = modifier.value * levelMultiplier;
            string statName = StatName(modifier.stat);
            if (modifier.operation == StatModifierOperation.Percent)
                builder.Append($"{statName}: {scaledValue * 100f:+0.#;-0.#;0}%");
            else
                builder.Append($"{statName}: {scaledValue:+0.##;-0.##;0}");
        }
        return builder.ToString();
    }

    private static string StatName(MechaStatType stat)
    {
        switch (stat)
        {
            case MechaStatType.MaxHealth: return "Vida máxima";
            case MechaStatType.Damage: return "Daño";
            case MechaStatType.AbilityCooldown: return "Cooldown de habilidad";
            default: return stat.ToString();
        }
    }
}
