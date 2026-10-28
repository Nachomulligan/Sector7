using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class GachaPanelUI : MonoBehaviour
{
    [Header("Tienda")]
    [SerializeField] private Button softPullButton;
    [SerializeField] private Button premiumPullButton;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text pityText;
    [SerializeField] private TMP_Text inventoryText;

    [Header("Apertura")]
    [SerializeField] private GameObject revealOverlay;
    [SerializeField] private GameObject closedChestState;
    [SerializeField] private GameObject resultState;
    [SerializeField] private Image closedChestImage;
    [SerializeField] private Image openChestImage;
    [SerializeField] private TMP_Text shakeInstructionText;
    [SerializeField] private TMP_Text resultStatusText;
    [SerializeField] private AccelerometerShakeDetector shakeDetector;

    [Header("Resultado")]
    [SerializeField] private GameObject singleRewardGroup;
    [SerializeField] private GameObject duplicateRewardGroup;
    [SerializeField] private ItemCardUI singleRewardCard;
    [SerializeField] private ItemCardUI previousLevelCard;
    [SerializeField] private ItemCardUI currentLevelCard;
    [SerializeField] private Button buyAgainButton;
    [SerializeField] private TMP_Text buyAgainButtonText;
    [SerializeField] private Button exitButton;
    [SerializeField] private ItemDetailsPanelUI detailsPanel;
    [SerializeField] private RarityVisualConfigSO rarityVisuals;

    private GachaService gacha;
    private PlayerInventory inventory;
    private GachaPullResult lastResult;
    private GachaCurrency lastCurrency = GachaCurrency.Soft;
    private bool initialized;

    private void Start()
    {
        ServiceLocator.Instance.TryGet(out gacha);
        ServiceLocator.Instance.TryGet(out inventory);
        if (gacha == null || inventory == null || !HasViewReferences)
        {
            Debug.LogWarning("GachaPanelUI tiene servicios o referencias sin asignar.", this);
            return;
        }

        softPullButton.onClick.AddListener(PullSoft);
        premiumPullButton.onClick.AddListener(PullPremium);
        buyAgainButton.onClick.AddListener(BuyAgain);
        exitButton.onClick.AddListener(CloseReveal);
        inventory.OnChanged += RefreshStore;
        gacha.OnPullPending += HandlePullPending;
        gacha.OnPullCompleted += HandlePullCompleted;
        shakeDetector.OnShakeDetected += HandleShakeDetected;
        shakeDetector.OnStateChanged += HandleShakeStateChanged;
        initialized = true;

        if (gacha.HasPendingPull) ShowWaitingForShake();
        else CloseReveal();
        RefreshStore();
    }

    private void OnDestroy()
    {
        if (!initialized) return;
        softPullButton.onClick.RemoveListener(PullSoft);
        premiumPullButton.onClick.RemoveListener(PullPremium);
        buyAgainButton.onClick.RemoveListener(BuyAgain);
        exitButton.onClick.RemoveListener(CloseReveal);
        inventory.OnChanged -= RefreshStore;
        gacha.OnPullPending -= HandlePullPending;
        gacha.OnPullCompleted -= HandlePullCompleted;
        shakeDetector.OnShakeDetected -= HandleShakeDetected;
        shakeDetector.OnStateChanged -= HandleShakeStateChanged;
    }

    public void PullSoft() => BeginPull(GachaCurrency.Soft);
    public void PullPremium() => BeginPull(GachaCurrency.Premium);

    private void BeginPull(GachaCurrency currency)
    {
        lastCurrency = currency;
        if (gacha.TryBeginDefaultPull(currency, out GachaPullFailure failure)) return;

        if (resultStatusText != null) resultStatusText.text = FailureMessage(failure).ToUpperInvariant();
        if (revealOverlay != null && !revealOverlay.activeSelf)
            Debug.LogWarning($"Gacha: {FailureMessage(failure)}", this);
    }

    private void BuyAgain()
    {
        if (detailsPanel != null) detailsPanel.Hide();
        BeginPull(lastCurrency);
    }

    private void CloseReveal()
    {
        lastResult = null;
        if (detailsPanel != null) detailsPanel.Hide();
        if (revealOverlay != null) revealOverlay.SetActive(false);
        RefreshStore();
    }

    private void HandlePullPending(GachaPendingPull pending) => ShowWaitingForShake();

    private void ShowWaitingForShake()
    {
        lastResult = null;
        revealOverlay.SetActive(true);
        closedChestState.SetActive(true);
        resultState.SetActive(false);
        RefreshShakeMessage();
        RefreshStore();
    }

    private void HandleShakeDetected()
    {
        if (!gacha.HasPendingPull) return;
        if (!gacha.TryCompletePendingPull(out _))
            resultStatusText.text = "NO SE PUDO ABRIR LA CAJA";
    }

    private void HandleShakeStateChanged(ShakeDetectorState state)
    {
        RefreshShakeMessage();
        RefreshStore();
    }

    private void HandlePullCompleted(GachaPullResult result)
    {
        lastResult = result;
        revealOverlay.SetActive(true);
        closedChestState.SetActive(false);
        resultState.SetActive(true);

        bool duplicate = result.WasDuplicate;
        singleRewardGroup.SetActive(!duplicate);
        duplicateRewardGroup.SetActive(duplicate);

        if (duplicate)
        {
            previousLevelCard.Bind(result.Reward, Mathf.Max(1, result.PreviousLevel), rarityVisuals,
                false, () => ShowResultDetails(true));
            currentLevelCard.Bind(result.Reward, result.CurrentLevel, rarityVisuals,
                false, () => ShowResultDetails(true));
            resultStatusText.text = result.LevelIncreased
                ? $"MEJORA · NIVEL {result.PreviousLevel} → {result.CurrentLevel}"
                : "DUPLICADO · NIVEL MÁXIMO";
        }
        else
        {
            singleRewardCard.Bind(result.Reward, result.CurrentLevel, rarityVisuals,
                false, () => ShowResultDetails(false));
            resultStatusText.text = "NUEVO OBJETO DESBLOQUEADO";
        }

        if (result.PityTriggered) resultStatusText.text += " · GARANTÍA";
        buyAgainButtonText.text = lastCurrency == GachaCurrency.Soft
            ? $"OTRA CAJA · {gacha.DefaultBanner.SoftCost} CR"
            : $"OTRA CAJA · {gacha.DefaultBanner.PremiumCost} GEMAS";
        RefreshStore();
    }

    private void ShowResultDetails(bool compare)
    {
        if (lastResult == null || detailsPanel == null) return;
        detailsPanel.Show(lastResult.Reward, lastResult.CurrentLevel,
            compare ? lastResult.PreviousLevel : -1);
    }

    private void RefreshShakeMessage()
    {
        if (shakeInstructionText == null || shakeDetector == null) return;
        switch (shakeDetector.State)
        {
            case ShakeDetectorState.Unavailable:
                shakeInstructionText.text = "ACELERÓMETRO NO DISPONIBLE";
                break;
            case ShakeDetectorState.Calibrating:
                shakeInstructionText.text = "MANTENÉ EL DISPOSITIVO QUIETO\nCALIBRANDO…";
                break;
            default:
                shakeInstructionText.text = "AGITÁ EL DISPOSITIVO\nPARA ABRIR LA CAJA";
                break;
        }
    }

    private void RefreshStore()
    {
        if (inventory == null || gacha == null || shakeDetector == null) return;
        balanceText.text = $"CRÉDITOS  {inventory.SoftCurrency:N0}     GEMAS  {inventory.PremiumCurrency:N0}";
        if (gacha.DefaultBanner != null)
            pityText.text = $"GARANTÍA ÉPICO+  {gacha.GetPullsSinceEpic(gacha.DefaultBanner)} / {gacha.DefaultBanner.EpicPityThreshold}";
        inventoryText.text = $"COLECCIÓN  {inventory.Owned.Count} OBJETOS";

        bool ready = !gacha.HasPendingPull && shakeDetector.State == ShakeDetectorState.Ready;
        softPullButton.interactable = ready;
        premiumPullButton.interactable = ready;
        if (buyAgainButton != null)
            buyAgainButton.interactable = ready && gacha.DefaultBanner != null &&
                inventory.CanSpend(lastCurrency, gacha.DefaultBanner.Cost(lastCurrency));
    }

    private bool HasViewReferences => softPullButton != null && premiumPullButton != null &&
        balanceText != null && pityText != null && inventoryText != null && revealOverlay != null &&
        closedChestState != null && resultState != null && closedChestImage != null && openChestImage != null &&
        shakeInstructionText != null && resultStatusText != null && shakeDetector != null &&
        singleRewardGroup != null && duplicateRewardGroup != null && singleRewardCard != null &&
        previousLevelCard != null && currentLevelCard != null && buyAgainButton != null &&
        buyAgainButtonText != null && exitButton != null && detailsPanel != null;

    private static string FailureMessage(GachaPullFailure failure) =>
        failure == GachaPullFailure.InsufficientCurrency ? "Saldo insuficiente" :
        failure == GachaPullFailure.InvalidBanner ? "El banner no tiene recompensas válidas" :
        failure == GachaPullFailure.PendingPull ? "Primero abrí la caja pendiente" :
        "No se pudo realizar la tirada";
}
