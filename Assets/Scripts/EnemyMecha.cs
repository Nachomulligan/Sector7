using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyMecha : MonoBehaviour, IPoolable
{
    [Header("Arma")]
    [SerializeField] private WeaponStrategySO weapon;
    [SerializeField] private Transform firePoint;

    [Header("Movimiento")]
    [Tooltip("Velocidad de descenso, en unidades/seg (estilo 1945: entra por arriba y baja).")]
    [SerializeField] private float moveSpeed = 3f;

    [Tooltip("Y en la que el enemigo se considera 'fuera de pantalla' y se despawnea sin dar puntos.")]
    [SerializeField] private float despawnY = -8f;

    private float cooldownTimer;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
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

        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if (transform.position.y <= despawnY)
        {
            Despawn();
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            weapon.Fire(origin, transform);
            cooldownTimer = weapon.Cooldown;
        }
    }

    private void HandleDeath(GameObject killer)
    {
        Despawn();
    }

    private void Despawn()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnSpawn()
    {
        health.ResetHealth();
        cooldownTimer = 0f;
    }

    public void OnDespawn()
    {
    }
}
