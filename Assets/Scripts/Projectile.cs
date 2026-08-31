using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour, IPoolable
{
    [Header("Configuración")]
    [Tooltip("Tiempo de vida máximo antes de despawnear si no impacta nada (evita balas eternas fuera de pantalla).")]
    [SerializeField] private float maxLifetime = 4f;

    [Tooltip("Velocidad de giro (grados/seg) cuando el proyectil es homing.")]
    [SerializeField] private float homingTurnSpeed = 220f;

    private Rigidbody2D body;
    private Faction ownerFaction;
    private int damage;
    private float speed;
    private Transform homingTarget;
    private float lifetimeTimer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }
    public void Init(Vector2 direction, float speed, int damage, Faction ownerFaction, Transform homingTarget = null)
    {
        this.speed = speed;
        this.damage = damage;
        this.ownerFaction = ownerFaction;
        this.homingTarget = homingTarget;

        transform.up = direction; 
        body.linearVelocity = direction.normalized * speed;

        lifetimeTimer = maxLifetime;
    }

    private void Update()
    {
        lifetimeTimer -= Time.deltaTime;

        if (lifetimeTimer <= 0f)
        {
            Despawn();
            return;
        }

        if (homingTarget != null)
        {
            SteerTowardsTarget();
        }
    }

    private void SteerTowardsTarget()
    {
        Vector2 currentDirection = body.linearVelocity.normalized;
        Vector2 desiredDirection = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;

        Vector2 newDirection = Vector3.RotateTowards(
            currentDirection,
            desiredDirection,
            homingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        body.linearVelocity = newDirection * speed;
        transform.up = newDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out IDamageable damageable))
        {
            return;
        }

        if (damageable.Faction == ownerFaction)
        {
            return; // fuego amigo desactivado
        }

        damageable.TakeDamage(damage, gameObject);
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
        homingTarget = null;
    }

    public void OnDespawn()
    {
        body.linearVelocity = Vector2.zero;
    }
}
