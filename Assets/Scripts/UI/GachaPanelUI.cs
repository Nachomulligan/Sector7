using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Adaptador opcional: la lógica de tiradas e inventario funciona sin esta pantalla.
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

        if (gacha == null || inventory == null)
        {
            Debug.LogWarning("GachaPanelUI requiere GachaService y PlayerInventory activos.", this);
            return;
        }

        if (softPullButton != null) softPullButton.onClick.AddListener(PullSoft);
        if (premiumPullButton != null) premiumPullButton.onClick.AddListener(PullPremium);
        if (equipLastRewardButton != null) equipLastRewardButton.onClick.AddListener(EquipLastReward);
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
        if (resultText != null)
            resultText.text = inventory.TryEquipReward(lastReward)
                ? $"Equipado: {lastReward.DisplayName}"
                : "No se pudo equipar en este momento.";
    }

    private void Pull(GachaCurrency currency)
    {
        if (gacha == null) return;
        if (!gacha.TryPullDefault(currency, out _, out GachaPullFailure failure) && resultText != null)
            resultText.text = $"No se pudo tirar: {failure}";
    }

    private void HandlePull(GachaPullResult result)
    {
        lastReward = result.Reward;
        if (resultText != null)
            resultText.text = $"{result.Reward.DisplayName} · {result.Reward.Rarity}" +
                (result.WasDuplicate ? " · duplicado" : "") +
                (result.PityTriggered ? " · pity" : "");
        Refresh();
    }

    private void Refresh()
    {
        if (inventory == null || gacha == null) return;
        if (balanceText != null)
            balanceText.text = $"Soft: {inventory.SoftCurrency}  Premium: {inventory.PremiumCurrency}";
        if (pityText != null && gacha.DefaultBanner != null)
            pityText.text = $"Pity Epic+: {gacha.GetPullsSinceEpic(gacha.DefaultBanner)}/{gacha.DefaultBanner.EpicPityThreshold}";
        if (inventoryText != null)
        {
            StringBuilder summary = new StringBuilder("Inventario");
            foreach (KeyValuePair<GachaRewardSO, int> item in inventory.Owned)
            {
                if (item.Key == null) continue;
                summary.Append('\n').Append(item.Key.DisplayName).Append(" x").Append(item.Value);
                if (item.Key.Kind == GachaRewardKind.Mecha)
                    summary.Append(" · ascensión ").Append(inventory.Ascension(item.Key.Mecha));
            }
            inventoryText.text = summary.ToString();
        }
    }
}
