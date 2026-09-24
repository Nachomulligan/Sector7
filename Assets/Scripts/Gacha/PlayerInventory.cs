using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    [Header("Saldo inicial de prueba (sin persistencia)")]
    [Min(0)] [SerializeField] private int initialSoftCurrency = 1000;
    [Min(0)] [SerializeField] private int initialPremiumCurrency = 100;
    [Tooltip("Recompensas que el jugador posee al iniciar una sesion nueva.")]
    [SerializeField] private List<GachaRewardSO> startingRewards = new List<GachaRewardSO>();

    private readonly Dictionary<GachaRewardSO, int> owned = new Dictionary<GachaRewardSO, int>();
    private readonly Dictionary<MechaPresetSO, int> ascensions = new Dictionary<MechaPresetSO, int>();
    private int softCurrency;
    private int premiumCurrency;

    public event Action OnChanged;
    public int SoftCurrency => softCurrency;
    public int PremiumCurrency => premiumCurrency;
    public IReadOnlyDictionary<GachaRewardSO, int> Owned => owned;
    public MechaPresetSO EquippedMecha { get; private set; }
    public WeaponStrategySO EquippedWeapon { get; private set; }
    public SkillStrategySO EquippedAbility { get; private set; }

    private void Awake()
    {
        softCurrency = initialSoftCurrency;
        premiumCurrency = initialPremiumCurrency;

        foreach (GachaRewardSO reward in startingRewards)
        {
            if (reward == null || !reward.IsValid) continue;
            owned[reward] = Count(reward) + 1;

            if (EquippedMecha == null && reward.Kind == GachaRewardKind.Mecha)
            {
                EquippedMecha = reward.Mecha;
                EquippedWeapon = reward.Mecha.StartingWeapon;
                EquippedAbility = reward.Mecha.StartingAbility;
            }
        }
    }

    private void OnEnable() => ServiceLocator.Instance.Register(this);

    private void OnDisable()
    {
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
    }

    public int Count(GachaRewardSO reward) =>
        reward != null && owned.TryGetValue(reward, out int count) ? count : 0;

    public int Ascension(MechaPresetSO mecha) =>
        mecha != null && ascensions.TryGetValue(mecha, out int level) ? level : 0;

    public bool OwnsMecha(MechaPresetSO mecha)
    {
        if (mecha == null) return false;
        foreach (KeyValuePair<GachaRewardSO, int> entry in owned)
            if (entry.Value > 0 && entry.Key.Kind == GachaRewardKind.Mecha && entry.Key.Mecha == mecha) return true;
        return false;
    }

    public bool OwnsWeapon(WeaponStrategySO weapon)
    {
        if (weapon == null) return false;
        foreach (KeyValuePair<GachaRewardSO, int> entry in owned)
            if (entry.Value > 0 && entry.Key.Kind == GachaRewardKind.Weapon && entry.Key.Weapon == weapon) return true;
        return false;
    }

    public bool OwnsAbility(SkillStrategySO ability)
    {
        if (ability == null) return false;
        foreach (KeyValuePair<GachaRewardSO, int> entry in owned)
            if (entry.Value > 0 && entry.Key.Kind == GachaRewardKind.Ability && entry.Key.Ability == ability) return true;
        return false;
    }

    public bool CanSpend(GachaCurrency currency, int amount) =>
        amount >= 0 && (currency == GachaCurrency.Soft ? softCurrency : premiumCurrency) >= amount;

    public bool TrySpend(GachaCurrency currency, int amount)
    {
        if (!CanSpend(currency, amount)) return false;
        if (currency == GachaCurrency.Soft) softCurrency -= amount;
        else premiumCurrency -= amount;
        OnChanged?.Invoke();
        return true;
    }

    public void AddCurrency(GachaCurrency currency, int amount)
    {
        if (amount <= 0) return;
        if (currency == GachaCurrency.Soft) softCurrency += amount;
        else premiumCurrency += amount;
        OnChanged?.Invoke();
    }

    public bool Grant(GachaRewardSO reward)
    {
        if (reward == null || !reward.IsValid) return false;
        bool duplicate = reward.Kind == GachaRewardKind.Mecha
            ? OwnsMecha(reward.Mecha) : Count(reward) > 0;
        owned[reward] = Count(reward) + 1;

        if (reward.Kind == GachaRewardKind.Mecha && duplicate)
        {
            MechaPresetSO mecha = reward.Mecha;
            ascensions[mecha] = Mathf.Min(Ascension(mecha) + 1, mecha.MaxAscension);
            if (EquippedMecha == mecha && TryGetActivePlayer(out Mecha active, out _))
                ApplyEquippedLoadout(active);
        }

        OnChanged?.Invoke();
        return duplicate;
    }

    public bool TryEquipMecha(MechaPresetSO preset)
    {
        if (!OwnsMecha(preset)) return false;
        EquippedMecha = preset;
        EquippedWeapon = preset.StartingWeapon;
        EquippedAbility = preset.StartingAbility;
        ApplyToActivePlayer();
        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipWeapon(WeaponStrategySO weapon)
    {
        if (!OwnsWeapon(weapon)) return false;
        EquippedWeapon = weapon;
        ApplyToActivePlayer();
        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipAbility(SkillStrategySO ability)
    {
        if (!OwnsAbility(ability)) return false;
        EquippedAbility = ability;
        ApplyToActivePlayer();
        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipReward(GachaRewardSO reward)
    {
        if (Count(reward) == 0) return false;
        switch (reward.Kind)
        {
            case GachaRewardKind.Mecha: return TryEquipMecha(reward.Mecha);
            case GachaRewardKind.Weapon: return TryEquipWeapon(reward.Weapon);
            case GachaRewardKind.Ability: return TryEquipAbility(reward.Ability);
            default: return false;
        }
    }

    /// <summary>Aplica la seleccion guardada al jugador que acaba de aparecer.</summary>
    public void ApplyEquippedLoadout(Mecha mecha)
    {
        if (mecha == null) return;

        if (EquippedWeapon != null)
            mecha.SetWeapon(EquippedWeapon);

        if (mecha.TryGetComponent(out MechaAbility mechaAbility) && EquippedAbility != null)
            mechaAbility.SetAbility(EquippedAbility);

        if (EquippedMecha != null)
        {
            if (EquippedMecha.Appearance != null && mecha.TryGetComponent(out SpriteRenderer spriteRenderer))
                spriteRenderer.sprite = EquippedMecha.Appearance;

            if (mecha.TryGetComponent(out Health health))
                health.SetBonusMaxHealth(Ascension(EquippedMecha) * EquippedMecha.HealthBonusPerAscension);
        }
    }

    private void ApplyToActivePlayer()
    {
        if (TryGetActivePlayer(out Mecha mecha, out _))
            ApplyEquippedLoadout(mecha);
    }

    private static bool TryGetActivePlayer(out Mecha mecha, out MechaAbility ability)
    {
        mecha = null;
        ability = null;
        if (!ServiceLocator.Instance.TryGet<IPlayerTarget>(out IPlayerTarget player) ||
            !player.IsAlive || player.TargetTransform == null) return false;
        return player.TargetTransform.TryGetComponent(out mecha) &&
            player.TargetTransform.TryGetComponent(out ability);
    }
}
