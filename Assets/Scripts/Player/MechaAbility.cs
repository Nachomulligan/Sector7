using System;
using UnityEngine;


[RequireComponent(typeof(Health))]
public class MechaAbility : MonoBehaviour
{
    [SerializeField] private SkillStrategySO ability;

    public event Action<float> OnCooldownChanged;

    public bool IsReady => cooldownTimer <= 0f;

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
    }
}
