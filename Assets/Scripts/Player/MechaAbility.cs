using System;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class MechaAbility : MonoBehaviour
{
    [SerializeField] private SkillStrategySO ability;

    public event Action<float> OnCooldownChanged;

    /// <summary>Se dispara cada vez que cambia el SO de habilidad equipado (ej: al armar loadout).</summary>
    public event Action<SkillStrategySO> OnAbilityChanged;

    public bool IsReady => cooldownTimer <= 0f;
    public SkillStrategySO CurrentAbility => ability;

    private float cooldownTimer;
    private float effectiveCooldown;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
        effectiveCooldown = ability != null ? ability.Cooldown : 0f;
    }

    private void Update()
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }

        float normalized = ability != null && effectiveCooldown > 0f
            ? cooldownTimer / effectiveCooldown
            : 0f;

        OnCooldownChanged?.Invoke(normalized);
    }

    public void TryActivate()
    {
        if (ability == null || !IsReady || !health.IsAlive)
        {
            return;
        }

        ability.Activate(transform);
        cooldownTimer = effectiveCooldown;
        OnCooldownChanged?.Invoke(1f);
    }


    public void SetAbility(SkillStrategySO newAbility, float levelMultiplier = 1f,
        float cooldownMultiplier = 1f)
    {
        ability = newAbility;
        effectiveCooldown = ability != null
            ? ability.Cooldown * Mathf.Max(0.05f, cooldownMultiplier) / Mathf.Max(0.05f, levelMultiplier)
            : 0f;
        cooldownTimer = 0f;
        OnCooldownChanged?.Invoke(0f);
        OnAbilityChanged?.Invoke(ability);
    }
}
