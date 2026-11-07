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
    [Min(1)] [SerializeField] private int attackDamage = 10;

    [Header("Recompensa")]
    [Min(0)] [SerializeField] private int scoreValue = 100;

    [Header("Movimiento")]
    [Tooltip("Patrón de vuelo de este enemigo. Cambiarlo (o crear uno nuevo) no requiere tocar esta clase.")]
    [SerializeField] private MovementStrategySO movement;

    [Header("Feedback de daño")]
    [SerializeField] private SpriteRenderer damageSprite;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.35f, 0.35f, 1f);
    [Min(0f)] [SerializeField] private float damageFlashDuration = 0.12f;

    [Tooltip("Límites horizontales del área jugable. Se aplican SIEMPRE, sin importar qué haga la estrategia, para garantizar que el enemigo nunca salga de pantalla por el costado.")]
    [SerializeField] private Vector2 boundsMin = new Vector2(-3.5f, -10f);
    [SerializeField] private Vector2 boundsMax = new Vector2(3.5f, 10f);

    [Tooltip("Y en la que el enemigo se considera 'fuera de pantalla' y se despawnea sin dar puntos.")]
    [SerializeField] private float despawnY = -8f;

    private float cooldownTimer;
    private Health health;
    private bool returnNotified;
    private Color originalColor;
    private int previousHealth;
    private float flashRemaining;
    private readonly MovementState movementState = new MovementState();

    private void Awake()
    {
        health = GetComponent<Health>();
        if (damageSprite == null) damageSprite = GetComponentInChildren<SpriteRenderer>();
        if (damageSprite != null) originalColor = damageSprite.color;
    }

    private void OnEnable()
    {
        health.OnDeath += HandleDeath;
        previousHealth = health.CurrentHealth;
        health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        health.OnHealthChanged -= HandleHealthChanged;
        RestoreDamageColor();
    }

    private void Update()
    {
        if (flashRemaining > 0f)
        {
            flashRemaining -= Time.deltaTime;
            if (flashRemaining <= 0f) RestoreDamageColor();
        }
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
            weapon.Fire(new WeaponFireContext(origin, transform, health.Faction, attackDamage));
            cooldownTimer = weapon.Cooldown;
        }
    }

    private void HandleHealthChanged(int current, int maximum)
    {
        if (current < previousHealth && current > 0 && damageSprite != null && damageFlashDuration > 0f)
        {
            damageSprite.color = damageFlashColor;
            flashRemaining = damageFlashDuration;
        }
        previousHealth = current;
    }

    private void RestoreDamageColor()
    {
        flashRemaining = 0f;
        if (damageSprite != null) damageSprite.color = originalColor;
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
        GameEvents.RaiseEnemyDied(gameObject);
        GameEvents.RaiseEnemyKilledScored(scoreValue);
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
