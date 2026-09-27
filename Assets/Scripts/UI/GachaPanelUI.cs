using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GachaPanelUI : MonoBehaviour
{
    [SerializeField] private Button softPullButton;
    [SerializeField] private Button premiumPullButton;
    [SerializeField] private Button equipLastRewardButton;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text pityText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text shakeInstructionText;
    [SerializeField] private AccelerometerShakeDetector shakeDetector;

    private GachaService gacha;
    private PlayerInventory inventory;
    private GachaRewardSO lastReward;

    private void Start()
    {
        ServiceLocator.Instance.TryGet(out gacha);
        ServiceLocator.Instance.TryGet(out inventory);
        if (gacha == null || inventory == null || !HasViewReferences)
        {
            Debug.LogWarning("GachaPanelUI tiene servicios o referencias de UI sin asignar.", this);
            return;
        }

        softPullButton.onClick.AddListener(PullSoft);
        premiumPullButton.onClick.AddListener(PullPremium);
        equipLastRewardButton.onClick.AddListener(EquipLastReward);
        inventory.OnChanged += Refresh;
        gacha.OnPullPending += HandlePullPending;
        gacha.OnPullCompleted += HandlePull;
        shakeDetector.OnShakeDetected += HandleShakeDetected;
        shakeDetector.OnStateChanged += HandleShakeStateChanged;
        Refresh();
        RefreshShakeState();
    }

    private void OnDestroy()
    {
        if (softPullButton != null) softPullButton.onClick.RemoveListener(PullSoft);
        if (premiumPullButton != null) premiumPullButton.onClick.RemoveListener(PullPremium);
        if (equipLastRewardButton != null) equipLastRewardButton.onClick.RemoveListener(EquipLastReward);
        if (inventory != null) inventory.OnChanged -= Refresh;
        if (gacha != null) gacha.OnPullPending -= HandlePullPending;
        if (gacha != null) gacha.OnPullCompleted -= HandlePull;
        if (shakeDetector != null)
        {
            shakeDetector.OnShakeDetected -= HandleShakeDetected;
            shakeDetector.OnStateChanged -= HandleShakeStateChanged;
        }
    }

    public void PullSoft() => Pull(GachaCurrency.Soft);
    public void PullPremium() => Pull(GachaCurrency.Premium);

    public void EquipLastReward()
    {
        if (lastReward == null || inventory == null) return;
        resultText.text = inventory.TryEquipReward(lastReward)
            ? $"EQUIPADO  •  {lastReward.DisplayName}" : "No se pudo equipar la recompensa.";
    }

    private void Pull(GachaCurrency currency)
    {
        if (!gacha.TryBeginDefaultPull(currency, out GachaPullFailure failure))
            resultText.text = FailureMessage(failure);
    }

    private void HandlePullPending(GachaPendingPull pending)
    {
        lastReward = null;
        equipLastRewardButton.interactable = false;
        resultText.color = Color.white;
        resultText.text = "CAJA DE SUMINISTROS LISTA";
        RefreshShakeState();
    }

    private void HandleShakeDetected()
    {
        if (!gacha.HasPendingPull) return;
        if (!gacha.TryCompletePendingPull(out _))
            resultText.text = "No se pudo abrir la caja de suministros";
    }

    private void HandleShakeStateChanged(ShakeDetectorState state) => RefreshShakeState();

    private void HandlePull(GachaPullResult result)
    {
        lastReward = result.Reward;
        resultText.color = RarityColor(result.Reward.Rarity);
        resultText.text = $"{result.Reward.DisplayName}\n{result.Reward.Rarity}" +
            (result.WasDuplicate ? "  •  DUPLICADO" : "  •  NUEVO") +
            (result.PityTriggered ? "  •  PITY" : string.Empty);
        equipLastRewardButton.interactable = result.Reward.IsValid;
        Refresh();
        RefreshShakeState();
    }

    private void Refresh()
    {
        balanceText.text = $"CREDITOS  {inventory.SoftCurrency:N0}     GEMAS  {inventory.PremiumCurrency:N0}";
        if (gacha.DefaultBanner != null)
            pityText.text = $"GARANTIA EPIC+  {gacha.GetPullsSinceEpic(gacha.DefaultBanner)} / {gacha.DefaultBanner.EpicPityThreshold}";
        inventoryText.text = $"Coleccion: {inventory.Owned.Count} objetos";
        bool canPull = !gacha.HasPendingPull && shakeDetector.State == ShakeDetectorState.Ready;
        softPullButton.interactable = canPull;
        premiumPullButton.interactable = canPull;
    }

    private void RefreshShakeState()
    {
        if (shakeInstructionText == null || shakeDetector == null || gacha == null) return;

        if (shakeDetector.State == ShakeDetectorState.Unavailable)
            shakeInstructionText.text = "ACELERÓMETRO NO DISPONIBLE";
        else if (shakeDetector.State == ShakeDetectorState.Calibrating)
            shakeInstructionText.text = "MANTENÉ EL DISPOSITIVO QUIETO · CALIBRANDO…";
        else if (gacha.HasPendingPull)
            shakeInstructionText.text = "AGITÁ EL DISPOSITIVO PARA ABRIR LA CAJA";
        else
            shakeInstructionText.text = "LISTO PARA REALIZAR UNA TIRADA";

        Refresh();
    }

    private bool HasViewReferences => softPullButton != null && premiumPullButton != null &&
        equipLastRewardButton != null && balanceText != null && pityText != null &&
        resultText != null && inventoryText != null && shakeInstructionText != null &&
        shakeDetector != null;

    private static Color RarityColor(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Rare: return new Color(0.13f, 0.48f, 0.9f);
            case GachaRarity.Epic: return new Color(0.58f, 0.22f, 0.86f);
            case GachaRarity.Legendary: return new Color(0.95f, 0.55f, 0.12f);
            default: return new Color(0.75f, 0.82f, 0.9f);
        }
    }

    private static string FailureMessage(GachaPullFailure failure) =>
        failure == GachaPullFailure.InsufficientCurrency ? "Saldo insuficiente" :
        failure == GachaPullFailure.InvalidBanner ? "El banner no tiene recompensas validas" :
        failure == GachaPullFailure.PendingPull ? "Primero abrí la caja pendiente" :
        "No se pudo realizar la tirada";
}
