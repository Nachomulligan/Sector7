using UnityEngine;

public abstract class WeaponStrategySO : ScriptableObject, IWeaponStrategy
{
    [Header("Stats base (compartidos por toda estrategia)")]
    [SerializeField] protected float cooldown = 0.25f;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float projectileSpeed = 12f;

    public float Cooldown => cooldown;

    public abstract void Fire(Vector2 origin, Transform firingMecha);
    protected Faction ResolveFaction(Transform firingMecha)
    {
        return firingMecha.TryGetComponent(out Health health) ? health.Faction : Faction.Player;
    }

    protected Vector2 ResolveForwardDirection(Faction faction)
    {
        return faction == Faction.Player ? Vector2.up : Vector2.down;
    }

    protected Projectile SpawnProjectile(Vector2 origin, Vector2 direction, Faction faction, Transform homingTarget = null)
    {
        GameObject instance = PoolManager.Instance.Spawn(projectilePrefab, origin, Quaternion.identity);
        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.Init(direction, projectileSpeed, damage, faction, homingTarget);
        return projectile;
    }
}
