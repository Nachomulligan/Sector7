using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    [Header("Saldo inicial de prueba (sin persistencia)")]
    [Min(0)] [SerializeField] private int initialSoftCurrency = 1000;
    [Min(0)] [SerializeField] private int initialPremiumCurrency = 100;

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
                active.GetComponent<Health>().SetBonusMaxHealth(Ascension(mecha) * mecha.HealthBonusPerAscension);
        }

        OnChanged?.Invoke();
        return duplicate;
    }

    public bool TryEquipMecha(MechaPresetSO preset)
    {
        if (!OwnsMecha(preset) || !TryGetActivePlayer(out Mecha mecha, out MechaAbility ability)) return false;
        EquippedMecha = preset;
        EquippedWeapon = preset.StartingWeapon;
        EquippedAbility = preset.StartingAbility;
        mecha.SetWeapon(EquippedWeapon);
        ability.SetAbility(EquippedAbility);
        if (preset.Appearance != null && mecha.TryGetComponent(out SpriteRenderer spriteRenderer))
            spriteRenderer.sprite = preset.Appearance;
        mecha.GetComponent<Health>().SetBonusMaxHealth(Ascension(preset) * preset.HealthBonusPerAscension);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipWeapon(WeaponStrategySO weapon)
    {
        if (!OwnsWeapon(weapon) || !TryGetActivePlayer(out Mecha mecha, out _)) return false;
        EquippedWeapon = weapon;
        mecha.SetWeapon(weapon);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryEquipAbility(SkillStrategySO ability)
    {
        if (!OwnsAbility(ability) || !TryGetActivePlayer(out _, out MechaAbility mechaAbility)) return false;
        EquippedAbility = ability;
        mechaAbility.SetAbility(ability);
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
