using UnityEngine;

public readonly struct WeaponFireContext
{
    public WeaponFireContext(Vector2 origin, Transform owner, Faction faction, int damage)
    {
        Origin = origin;
        Owner = owner;
        Faction = faction;
        Damage = Mathf.Max(1, damage);
    }

    public Vector2 Origin { get; }
    public Transform Owner { get; }
    public Faction Faction { get; }
    public int Damage { get; }
}
