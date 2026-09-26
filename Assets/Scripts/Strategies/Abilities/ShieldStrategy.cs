using UnityEngine;


[CreateAssetMenu(menuName = "Sector7/Abilities/Shield", fileName = "Ability_Shield")]
public class ShieldStrategy : SkillStrategySO
{
    [Header("Escudo")]
    [SerializeField] private float duration = 3f;

    public float Duration => duration;

    public override void Activate(Transform user)
    {
        if (user.TryGetComponent(out Health health))
        {
            health.ActivateShield(duration);
        }
    }
}
