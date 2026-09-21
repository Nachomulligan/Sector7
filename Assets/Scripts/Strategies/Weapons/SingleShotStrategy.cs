using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Weapons/Single Shot", fileName = "Weapon_SingleShot")]
public class SingleShotStrategy : WeaponStrategySO
{
    public override void Fire(Vector2 origin, Transform firingMecha)
    {
        Faction faction = ResolveFaction(firingMecha);
        Vector2 direction = ResolveForwardDirection(faction);

        SpawnProjectile(origin, direction, faction);
    }
}
