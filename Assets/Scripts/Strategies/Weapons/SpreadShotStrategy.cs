using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Spread Shot", fileName = "Weapon_SpreadShot")]
public class SpreadShotStrategy : WeaponStrategySO
{
    [Header("Spread")]
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 30f;

    public override void Fire(WeaponFireContext context)
    {
        Vector2 forward = ResolveForwardDirection(context.Faction);

        if (projectileCount <= 1)
        {
            SpawnProjectile(context, forward);
            return;
        }

        float halfSpread = spreadAngle * 0.5f;
        float angleStep = spreadAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = -halfSpread + (angleStep * i);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * forward;
            SpawnProjectile(context, direction);
        }
    }
}
