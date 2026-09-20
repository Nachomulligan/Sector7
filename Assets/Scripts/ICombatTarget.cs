using UnityEngine;

public interface ICombatTarget
{
    Transform TargetTransform { get; }
    Faction Faction { get; }
    bool IsAlive { get; }
}
