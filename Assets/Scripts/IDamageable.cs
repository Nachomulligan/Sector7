using UnityEngine;
public interface IDamageable
{
    Faction Faction { get; }
    bool IsAlive { get; }
    void TakeDamage(int amount, GameObject source);
}
