using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class PlayerInventory : MonoBehaviour
{
    [Header("Saldo inicial de prueba (sin persistencia)")]
    [Min(0)] [SerializeField] private int initialSoftCurrency = 1000;
    [Min(0)] [SerializeField] private int initialPremiumCurrency = 100;
    [Tooltip("Recompensas que el jugador posee al iniciar una sesion nueva.")]
    [SerializeField] private List<GachaRewardSO> startingRewards = new List<GachaRewardSO>();
    [SerializeField] private string gameplaySceneName = "Gameplay";

    private readonly Dictionary<GachaRewardSO, int> owned = new Dictionary<GachaRewardSO, int>();
    private readonly Dictionary<GachaRewardSO, int> levels = new Dictionary<GachaRewardSO, int>();
    private int softCurrency;
    private int premiumCurrency;

    public event Action OnChanged;
    /// <summary>Se dispara cuando el loadout de la escena Gameplay ya fue resuelto.</summary>
    public event Action<bool, string> OnLoadoutReady;
    public int SoftCurrency => softCurrency;
    public int PremiumCurrency => premiumCurrency;
    public IReadOnlyDictionary<GachaRewardSO, int> Owned => owned;
    public MechaPresetSO EquippedMecha { get; private set; }
    public WeaponStrategySO EquippedWeapon { get; private set; }
    public SkillStrategySO EquippedAbility { get; private set; }
    public CompanionPresetSO EquippedCompanion { get; private set; }

    private void Awake()
    {
        softCurrency = initialSoftCurrency;
        premiumCurrency = initialPremiumCurrency;

        foreach (GachaRewardSO reward in startingRewards)
        {
            if (reward == null || !reward.IsValid) continue;
            owned[reward] = Count(reward) + 1;
            levels[reward] = Mathf.Max(1, Level(reward));

            if (EquippedMecha == null && reward.Kind == GachaRewardKind.Mecha)
                EquippedMecha = reward.Mecha;
            else if (EquippedWeapon == null && reward.Kind == GachaRewardKind.Weapon)
                EquippedWeapon = reward.Weapon;
            else if (EquippedAbility == null && reward.Kind == GachaRewardKind.Ability)
                EquippedAbility = reward.Ability;
            else if (EquippedCompanion == null && reward.Kind == GachaRewardKind.Companion)
                EquippedCompanion = reward.Companion;
        }
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Register(this);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        if (ServiceLocator.HasInstance) ServiceLocator.Instance.Unregister(this);
    }

    public int Count(GachaRewardSO reward) =>
        reward != null && owned.TryGetValue(reward, out int count) ? count : 0;

    public int Level(GachaRewardSO reward) =>
        reward != null && levels.TryGetValue(reward, out int level) ? level : 0;

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

    public bool OwnsCompanion(CompanionPresetSO companion)
    {
        if (companion == null) return false;
        foreach (KeyValuePair<GachaRewardSO, int> entry in owned)
            if (entry.Value > 0 && entry.Key.Kind == GachaRewardKind.Companion && entry.Key.Companion == companion) return true;
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
        bool duplicate = Count(reward) > 0;
        owned[reward] = Count(reward) + 1;
        levels[reward] = duplicate ? Mathf.Min(Mathf.Max(1, Level(reward)) + 1, reward.MaxLevel) : 1;
        if (duplicate) ApplyToActivePlayer();

        OnChanged?.Invoke();
        return duplicate;
    }

    public bool TryEquipMecha(MechaPresetSO preset)
    {
        if (!OwnsMecha(preset)) return false;
        EquippedMecha = preset;
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

    public bool TryEquipCompanion(CompanionPresetSO companion)
    {
        if (!OwnsCompanion(companion)) return false;
        EquippedCompanion = companion;
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
            case GachaRewardKind.Companion: return TryEquipCompanion(reward.Companion);
            default: return false;
        }
    }

    /// <summary>Aplica la seleccion guardada al jugador que acaba de aparecer.</summary>
    public void ApplyEquippedLoadout(Mecha mecha)
    {
        if (mecha == null) return;

        GachaRewardSO mechaReward = FindReward(GachaRewardKind.Mecha, EquippedMecha);
        GachaRewardSO weaponReward = FindReward(GachaRewardKind.Weapon, EquippedWeapon);
        GachaRewardSO abilityReward = FindReward(GachaRewardKind.Ability, EquippedAbility);
        GachaRewardSO companionReward = FindReward(GachaRewardKind.Companion, EquippedCompanion);

        float mechaLevel = Multiplier(mechaReward);
        float companionLevel = Multiplier(companionReward);
        MechaStatController stats = mecha.GetComponent<MechaStatController>();
        if (stats == null) stats = mecha.gameObject.AddComponent<MechaStatController>();
        stats.Recalculate(EquippedMecha, mechaLevel, EquippedCompanion, companionLevel);
        mecha.SetDamage(stats.Current.Damage);
        mecha.SetWeapon(EquippedWeapon, Multiplier(weaponReward));

        if (mecha.TryGetComponent(out MechaAbility mechaAbility))
            mechaAbility.SetAbility(EquippedAbility, Multiplier(abilityReward), stats.Current.AbilityCooldownMultiplier);

        CompanionController companionController = mecha.GetComponent<CompanionController>();
        if (companionController == null) companionController = mecha.gameObject.AddComponent<CompanionController>();
        companionController.SetCompanion(EquippedCompanion);

        if (EquippedMecha != null)
        {
            if (EquippedMecha.Appearance != null && mecha.TryGetComponent(out SpriteRenderer spriteRenderer))
                spriteRenderer.sprite = EquippedMecha.Appearance;
            if (mecha.TryGetComponent(out SpriteRenderer tintedRenderer))
                tintedRenderer.color = EquippedMecha.AppearanceColor;
        }
    }

    private float Multiplier(GachaRewardSO reward) => reward != null
        ? reward.LevelMultiplier(Mathf.Max(1, Level(reward))) : 1f;

    private GachaRewardSO FindReward(GachaRewardKind kind, UnityEngine.Object asset)
    {
        if (asset == null) return null;
        foreach (KeyValuePair<GachaRewardSO, int> entry in owned)
        {
            GachaRewardSO reward = entry.Key;
            if (reward == null || reward.Kind != kind) continue;
            UnityEngine.Object candidate = kind == GachaRewardKind.Mecha ? reward.Mecha
                : kind == GachaRewardKind.Weapon ? reward.Weapon
                : kind == GachaRewardKind.Ability ? reward.Ability : reward.Companion;
            if (candidate == asset) return reward;
        }
        return null;
    }

    private void ApplyToActivePlayer()
    {
        TryApplyToActivePlayer(out _);
    }

    public bool TryApplyToActivePlayer(out string status)
    {
        if (!ServiceLocator.Instance.TryGet<IPlayerTarget>(out IPlayerTarget player))
        {
            status = "ERROR · IPlayerTarget no está registrado en ServiceLocator";
            return false;
        }
        if (!player.IsAlive)
        {
            status = "ERROR · El Player está muerto o desactivado";
            return false;
        }
        if (player.TargetTransform == null)
        {
            status = "ERROR · IPlayerTarget no tiene Transform";
            return false;
        }
        if (!player.TargetTransform.TryGetComponent(out Mecha mecha))
        {
            status = "ERROR · El objeto registrado no tiene componente Mecha";
            return false;
        }

        ApplyEquippedLoadout(mecha);
        status = $"OK · Mecha: {AssetName(EquippedMecha)} · Arma: {AssetName(EquippedWeapon)} · " +
            $"Habilidad: {AssetName(EquippedAbility)} · Apoyo: {AssetName(EquippedCompanion)}";
        return true;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameplaySceneName)
            StartCoroutine(ApplyLoadoutAfterSceneInitialization());
    }

    private IEnumerator ApplyLoadoutAfterSceneInitialization()
    {
        yield return null;
        bool applied = TryApplyToActivePlayer(out string status);
        OnLoadoutReady?.Invoke(applied, status);
        if (applied)
            Debug.Log($"[PlayerInventory] {status}", this);
        else
            Debug.LogError($"[PlayerInventory] No se pudo aplicar el loadout al cargar Gameplay: {status}", this);
    }

    private static string AssetName(UnityEngine.Object asset)
    {
        if (asset == null) return "Ninguno";
        if (asset is MechaPresetSO mecha) return mecha.DisplayName;
        if (asset is CompanionPresetSO companion) return companion.DisplayName;
        return asset.name.Replace("Weapon_", string.Empty).Replace("Ability_", string.Empty);
    }
}
