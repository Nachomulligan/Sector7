using System;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class EnemyMecha : MonoBehaviour, IPoolable
{
    public event Action<EnemyMecha> OnReturnedToPool;

    [Header("Arma opcional")]
    [Tooltip("Dejar en None para enemigos que sólo se mueven, embisten o hacen daño por contacto.")]
    [SerializeField] private WeaponStrategySO weapon;
    [SerializeField] private Transform firePoint;

    [Header("Movimiento")]
    [Tooltip("Patrón de vuelo de este enemigo. Cambiarlo (o crear uno nuevo) no requiere tocar esta clase.")]
    [SerializeField] private MovementStrategySO movement;

    [Tooltip("Límites horizontales del área jugable. Se aplican SIEMPRE, sin importar qué haga la estrategia, para garantizar que el enemigo nunca salga de pantalla por el costado.")]
    [SerializeField] private Vector2 boundsMin = new Vector2(-3.5f, -10f);
    [SerializeField] private Vector2 boundsMax = new Vector2(3.5f, 10f);

    [Tooltip("Y en la que el enemigo se considera 'fuera de pantalla' y se despawnea sin dar puntos.")]
    [SerializeField] private float despawnY = -8f;

    private float cooldownTimer;
    private Health health;
    private bool returnNotified;
    private readonly MovementState movementState = new MovementState();

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

        ApplyMovement();

        if (transform.position.y <= despawnY)
        {
            Despawn();
            return;
        }

        if (weapon == null)
        {
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

    private void ApplyMovement()
    {
        if (movement == null)
        {
            return;
        }

        movement.Move(transform, movementState, Time.deltaTime, boundsMin, boundsMax);
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        transform.position = pos;
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
            NotifyReturnedToPool();
            Destroy(gameObject);
        }
    }

    public void OnSpawn()
    {
        returnNotified = false;
        health.ResetHealth();
        cooldownTimer = 0f;
        movementState.Reset();
    }

    public void OnDespawn()
    {
        NotifyReturnedToPool();
    }

    private void OnDestroy()
    {
        NotifyReturnedToPool();
    }

    private void NotifyReturnedToPool()
    {
        if (returnNotified)
        {
            return;
        }

        returnNotified = true;
        OnReturnedToPool?.Invoke(this);
    }

}
