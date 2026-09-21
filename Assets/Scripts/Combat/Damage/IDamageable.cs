using UnityEngine;
public interface IDamageable : ICombatTarget
{
    void TakeDamage(int amount, GameObject source);
}
