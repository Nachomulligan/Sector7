using UnityEngine;


[RequireComponent(typeof(Health))]
public class Mecha : MonoBehaviour
{
    [Header("Arma")]
    [SerializeField] private WeaponStrategySO weapon;
    [SerializeField] private Transform firePoint;

    private float cooldownTimer;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
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
            weapon.Fire(origin, transform);
            cooldownTimer = weapon.Cooldown;
        }
    }

    public void SetWeapon(WeaponStrategySO newWeapon)
    {
        weapon = newWeapon;
        cooldownTimer = 0f;
    }

    private void HandleDeath(GameObject killer)
    {
        Debug.Log("Player mecha destruido.");
        gameObject.SetActive(false);
    }
}
