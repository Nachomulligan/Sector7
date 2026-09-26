using UnityEngine;


[RequireComponent(typeof(Health))]
public class Mecha : MonoBehaviour
{
    [Header("Arma")]
    [SerializeField] private WeaponStrategySO weapon;
    [SerializeField] private Transform firePoint;

    private float cooldownTimer;
    private float effectiveCooldown;
    private int damage = 10;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        effectiveCooldown = weapon != null ? weapon.Cooldown : 0f;
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;

        if (ServiceLocator.Instance.TryGet(out PlayerInventory inventory))
        {
            inventory.ApplyEquippedLoadout(this);
        }
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        if (!health.IsAlive)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (weapon != null && cooldownTimer <= 0f)
        {
            Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            weapon.Fire(new WeaponFireContext(origin, transform, health.Faction, damage));
            cooldownTimer = effectiveCooldown;
        }
    }

    public void SetWeapon(WeaponStrategySO newWeapon, float levelMultiplier = 1f)
    {
        weapon = newWeapon;
        effectiveCooldown = weapon != null ? weapon.Cooldown / Mathf.Max(0.05f, levelMultiplier) : 0f;
        cooldownTimer = 0f;
    }

    public void SetDamage(int value) => damage = Mathf.Max(1, value);

    private void HandleDeath(GameObject killer)
    {
        Debug.Log("Player mecha destruido.");
        gameObject.SetActive(false);
    }
}
