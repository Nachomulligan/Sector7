using UnityEngine;


[CreateAssetMenu(menuName = "Sector7/Abilities/Shield", fileName = "Ability_Shield")]
public class ShieldStrategy : SkillStrategySO
{
    [Header("Escudo")]
    [SerializeField] private float duration = 3f;

    public float Duration => duration;

    protected override bool TryActivateEffect(Transform user)
    {
        if (!user.TryGetComponent(out Health health)) return false;
        health.ActivateShield(duration);
        return true;
    }
}
