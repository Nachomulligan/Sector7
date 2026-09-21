using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Homing Missile", fileName = "Weapon_HomingMissile")]
public class HomingMissileStrategy : WeaponStrategySO
{
    [Header("Búsqueda de objetivo")]
    [SerializeField] private float searchRadius = 15f;

    private readonly List<ICombatTarget> targets = new List<ICombatTarget>();

    public override void Fire(Vector2 origin, Transform firingMecha)
    {
        Faction faction = ResolveFaction(firingMecha);
        Vector2 forward = ResolveForwardDirection(faction);

        Transform target = FindNearestEnemy(origin, faction);

        SpawnProjectile(origin, forward, faction, target);
    }

    private Transform FindNearestEnemy(Vector2 origin, Faction ownerFaction)
    {
        ServiceLocator.Instance.GetAll(targets);

        Transform nearest = null;
        float nearestSqrDistance = searchRadius * searchRadius;

        foreach (ICombatTarget target in targets)
        {
            if (target.Faction == ownerFaction || !target.IsAlive || target.TargetTransform == null)
            {
                continue;
            }

            float sqrDistance = ((Vector2)target.TargetTransform.position - origin).sqrMagnitude;

            if (sqrDistance <= nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = target.TargetTransform;
            }
        }

        return nearest;
    }
}
