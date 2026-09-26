using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Single Shot", fileName = "Weapon_SingleShot")]
public class SingleShotStrategy : WeaponStrategySO
{
    public override void Fire(WeaponFireContext context)
    {
        SpawnProjectile(context, ResolveForwardDirection(context.Faction));
    }
}
