using UnityEngine;


[CreateAssetMenu(menuName = "Sector7/Abilities/Bomb", fileName = "Ability_Bomb")]
public class BombStrategy : SkillStrategySO
{
    [Header("Bomba")]
    [SerializeField] private int damage = 999;
    [SerializeField] private float blastRadius = 30f;

    public int Damage => damage;
    public float BlastRadius => blastRadius;

    public override void Activate(Transform user)
    {
        Faction userFaction = ResolveFaction(user);
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.position, blastRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable) || damageable.Faction == userFaction)
            {
                continue;
            }

            damageable.TakeDamage(damage, user.gameObject);
        }
    }
}
