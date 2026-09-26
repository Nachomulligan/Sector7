using System;
using System.Collections.Generic;
using UnityEngine;

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
}
