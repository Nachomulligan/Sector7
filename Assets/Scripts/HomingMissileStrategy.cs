using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Homing Missile", fileName = "Weapon_HomingMissile")]
public class HomingMissileStrategy : WeaponStrategySO
{
    [Header("Búsqueda de objetivo")]
    [SerializeField] private float searchRadius = 15f;

    public override void Fire(Vector2 origin, Transform firingMecha)
    {
        Faction faction = ResolveFaction(firingMecha);
        Vector2 forward = ResolveForwardDirection(faction);

        Transform target = FindNearestEnemy(origin, faction);

        SpawnProjectile(origin, forward, faction, target);
    }

    private Transform FindNearestEnemy(Vector2 origin, Faction ownerFaction)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, searchRadius);

        Transform nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent(out Health health) || health.Faction == ownerFaction || !health.IsAlive)
            {
                continue;
            }

            float distance = Vector2.Distance(origin, hit.transform.position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = hit.transform;
            }
        }

        return nearest;
    }
}
