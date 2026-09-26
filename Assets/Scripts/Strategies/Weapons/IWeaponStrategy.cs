public interface IWeaponStrategy
{
    void Fire(WeaponFireContext context);
    float Cooldown { get; }
}
