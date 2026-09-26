using UnityEngine;

public abstract class WeaponStrategySO : ScriptableObject, IWeaponStrategy
{
    [Header("Stats base (compartidos por toda estrategia)")]
    [SerializeField] protected float cooldown = 0.25f;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected float projectileSpeed = 12f;

    public float Cooldown => cooldown;

    public abstract void Fire(WeaponFireContext context);

    protected Vector2 ResolveForwardDirection(Faction faction)
    {
        return faction == Faction.Player ? Vector2.up : Vector2.down;
    }

    protected Projectile SpawnProjectile(WeaponFireContext context, Vector2 direction, Transform homingTarget = null)
    {
        if (projectilePrefab == null || PoolManager.Instance == null) return null;
        GameObject instance = PoolManager.Instance.Spawn(projectilePrefab, context.Origin, Quaternion.identity);
        Projectile projectile = instance.GetComponent<Projectile>();
        if (projectile == null) return null;
        projectile.Init(direction, projectileSpeed, context.Damage, context.Faction, homingTarget);
        return projectile;
    }
}
