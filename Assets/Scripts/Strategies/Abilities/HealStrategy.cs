using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Abilities/Heal", fileName = "Ability_Heal")]
public sealed class HealStrategy : SkillStrategySO
{
    [Header("Curación")]
    [Min(1)] [SerializeField] private int healAmount = 35;

    public int HealAmount => healAmount;

    protected override bool TryActivateEffect(Transform user)
    {
        if (!user.TryGetComponent(out Health health)) return false;
        health.Heal(healAmount);
        return true;
    }
}
