using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Laser Beam", fileName = "Weapon_LaserBeam")]
public class LaserBeamStrategy : WeaponStrategySO
{
    [Header("Láser")]
    [SerializeField] private float maxRange = 20f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private GameObject beamVisualPrefab;

    public override void Fire(WeaponFireContext context)
    {
        Vector2 direction = ResolveForwardDirection(context.Faction);

        RaycastHit2D hit = Physics2D.Raycast(context.Origin, direction, maxRange, hitMask);
        float beamLength = hit.collider != null ? hit.distance : maxRange;

        if (hit.collider != null &&
            hit.collider.TryGetComponent(out IDamageable damageable) &&
            damageable.Faction != context.Faction)
        {
            damageable.TakeDamage(context.Damage, context.Owner != null ? context.Owner.gameObject : null);
        }

        SpawnBeamVisual(context.Origin, direction, beamLength);
    }

    private void SpawnBeamVisual(Vector2 origin, Vector2 direction, float length)
    {
        if (beamVisualPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        GameObject instance = PoolManager.Instance.Spawn(
            beamVisualPrefab,
            origin,
            Quaternion.FromToRotation(Vector3.up, direction)
        );

        Vector3 scale = instance.transform.localScale;
        scale.y = length;
        instance.transform.localScale = scale;
    }
}
