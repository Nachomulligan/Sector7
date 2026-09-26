using System;
using System.Collections;
using UnityEngine;


[DisallowMultipleComponent]
public class Health : MonoBehaviour, IDamageable, IPlayerTarget
{
    [Header("Configuración")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Faction faction = Faction.Enemy;

 
    public event Action<int, int> OnHealthChanged;


    public event Action<GameObject> OnDeath;

    public event Action<bool> OnShieldChanged;

    public Faction Faction => faction;
    public bool IsAlive => currentHealth > 0;
    public Transform TargetTransform => transform;
    public bool IsInvulnerable => isInvulnerable;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => Mathf.Max(1, maxHealth + bonusMaxHealth);

    private int currentHealth;
    private int bonusMaxHealth;
    private bool isInvulnerable;
    private Coroutine shieldRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Register<ICombatTarget>(this);

        if (faction == Faction.Player)
        {
            ServiceLocator.Instance.Register<IPlayerTarget>(this);
        }
    }

    private void OnDisable()
    {
        if (ServiceLocator.HasInstance)
        {
            ServiceLocator.Instance.Unregister<ICombatTarget>(this);

            if (faction == Faction.Player)
            {
                ServiceLocator.Instance.Unregister<IPlayerTarget>(this);
            }
        }
    }

    public void TakeDamage(int amount, GameObject source)
    {
        if (!IsAlive || isInvulnerable || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

        if (currentHealth == 0)
        {
            OnDeath?.Invoke(source);
        }
    }

    public void Heal(int amount)
    {
        if (!IsAlive || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void SetBonusMaxHealth(int bonus)
    {
        int previousMax = MaxHealth;
        bonusMaxHealth = Mathf.Max(0, bonus);
        if (currentHealth > 0)
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(currentHealth * (float)MaxHealth / previousMax), 1, MaxHealth);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    public void SetMaxHealth(int value)
    {
        int previousMax = MaxHealth;
        maxHealth = Mathf.Max(1, value);
        bonusMaxHealth = 0;
        if (currentHealth > 0)
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(currentHealth * (float)MaxHealth / Mathf.Max(1, previousMax)), 1, MaxHealth);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

  
    public void ActivateShield(float duration)
    {
        if (!IsAlive || duration <= 0f)
        {
            return;
        }

        if (shieldRoutine != null)
        {
            StopCoroutine(shieldRoutine);
        }

        shieldRoutine = StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        isInvulnerable = true;
        OnShieldChanged?.Invoke(true);

        yield return new WaitForSeconds(duration);

        isInvulnerable = false;
        shieldRoutine = null;
        OnShieldChanged?.Invoke(false);
    }
}
