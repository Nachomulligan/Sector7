using UnityEngine;
public interface IWeaponStrategy
{
    void Fire(Vector2 origin, Transform firingMecha);
    float Cooldown { get; }
}
