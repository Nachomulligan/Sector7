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
        gacha.OnPullCompleted += HandlePull;
        Refresh();
    }

    private void OnDestroy()
    {
        if (softPullButton != null) softPullButton.onClick.RemoveListener(PullSoft);
        if (premiumPullButton != null) premiumPullButton.onClick.RemoveListener(PullPremium);
        if (equipLastRewardButton != null) equipLastRewardButton.onClick.RemoveListener(EquipLastReward);
        if (inventory != null) inventory.OnChanged -= Refresh;
        if (gacha != null) gacha.OnPullCompleted -= HandlePull;
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
        if (!gacha.TryPullDefault(currency, out _, out GachaPullFailure failure))
            resultText.text = FailureMessage(failure);
    }

    private void HandlePull(GachaPullResult result)
    {
        lastReward = result.Reward;
        resultText.color = RarityColor(result.Reward.Rarity);
        resultText.text = $"{result.Reward.DisplayName}\n{result.Reward.Rarity}" +
            (result.WasDuplicate ? "  •  DUPLICADO" : "  •  NUEVO") +
            (result.PityTriggered ? "  •  PITY" : string.Empty);
        equipLastRewardButton.interactable = result.Reward.IsValid;
        Refresh();
    }

    private void Refresh()
    {
        balanceText.text = $"CREDITOS  {inventory.SoftCurrency:N0}     GEMAS  {inventory.PremiumCurrency:N0}";
        if (gacha.DefaultBanner != null)
            pityText.text = $"GARANTIA EPIC+  {gacha.GetPullsSinceEpic(gacha.DefaultBanner)} / {gacha.DefaultBanner.EpicPityThreshold}";
        inventoryText.text = $"Coleccion: {inventory.Owned.Count} objetos";
    }

    private bool HasViewReferences => softPullButton != null && premiumPullButton != null &&
        equipLastRewardButton != null && balanceText != null && pityText != null &&
        resultText != null && inventoryText != null;

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
        "No se pudo realizar la tirada";
}
