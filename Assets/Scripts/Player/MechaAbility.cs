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
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
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

        float normalized = ability != null && ability.Cooldown > 0f
            ? cooldownTimer / ability.Cooldown
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
        cooldownTimer = ability.Cooldown;
        OnCooldownChanged?.Invoke(1f);
    }


    public void SetAbility(SkillStrategySO newAbility)
    {
        ability = newAbility;
        cooldownTimer = 0f;
        OnCooldownChanged?.Invoke(0f);
        OnAbilityChanged?.Invoke(ability);
    }
}