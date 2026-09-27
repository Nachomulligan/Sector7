using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Abilities/Heal", fileName = "Ability_Heal")]
public sealed class HealStrategy : SkillStrategySO
{
    [Header("Curación")]
    [Min(1)] [SerializeField] private int healAmount = 35;

    public int HealAmount => healAmount;

    public override void Activate(Transform user)
    {
        if (user != null && user.TryGetComponent(out Health health))
            health.Heal(healAmount);
    }
}
