using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class InventoryPanelUI : MonoBehaviour
{
    [Header("Tarjetas")]
    [SerializeField] private RectTransform equippedContent;
    [SerializeField] private RectTransform itemContent;
    [SerializeField] private ItemCardUI itemCardPrefab;
    [SerializeField] private RarityVisualConfigSO rarityVisuals;
    [SerializeField] private ItemDetailsPanelUI detailsPanel;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private TMP_Text selectionText;

    [Header("Categorías")]
    [SerializeField] private Button mechaTab;
    [SerializeField] private Button weaponTab;
    [SerializeField] private Button abilityTab;
    [SerializeField] private Button companionTab;

    private PlayerInventory inventory;
    private GachaRewardKind currentKind = GachaRewardKind.Mecha;
    private GachaRewardSO selectedReward;
    private readonly List<GameObject> generatedItems = new List<GameObject>();
    private readonly List<GameObject> generatedEquipped = new List<GameObject>();

    private void Start()
    {
        ServiceLocator.Instance.TryGet(out inventory);
        if (inventory == null || !HasReferences)
        {
            Debug.LogWarning("InventoryPanelUI tiene servicios o referencias sin asignar.", this);
            return;
        }

        mechaTab.onClick.AddListener(ShowMechas);
        weaponTab.onClick.AddListener(ShowWeapons);
        abilityTab.onClick.AddListener(ShowAbilities);
        companionTab.onClick.AddListener(ShowCompanions);
        inventory.OnChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        if (mechaTab != null) mechaTab.onClick.RemoveListener(ShowMechas);
        if (weaponTab != null) weaponTab.onClick.RemoveListener(ShowWeapons);
        if (abilityTab != null) abilityTab.onClick.RemoveListener(ShowAbilities);
        if (companionTab != null) companionTab.onClick.RemoveListener(ShowCompanions);
    }

    public void ShowMechas() => SelectKind(GachaRewardKind.Mecha);
    public void ShowWeapons() => SelectKind(GachaRewardKind.Weapon);
    public void ShowAbilities() => SelectKind(GachaRewardKind.Ability);
    public void ShowCompanions() => SelectKind(GachaRewardKind.Companion);

    private void SelectKind(GachaRewardKind kind)
    {
        currentKind = kind;
        selectedReward = null;
        if (detailsPanel != null) detailsPanel.Hide();
        RefreshItems();
        RefreshSelection();
    }

    private void Refresh()
    {
        RefreshEquipped();
        RefreshItems();
        RefreshSelection();
    }

    private void RefreshEquipped()
    {
        ClearGenerated(generatedEquipped);
        AddEquippedCard(GachaRewardKind.Mecha, "MECHA");
        AddEquippedCard(GachaRewardKind.Weapon, "ARMA");
        AddEquippedCard(GachaRewardKind.Ability, "HABILIDAD");
        AddEquippedCard(GachaRewardKind.Companion, "APOYO");
    }

    private void AddEquippedCard(GachaRewardKind kind, string emptyLabel)
    {
        ItemCardUI card = Instantiate(itemCardPrefab, equippedContent);
        generatedEquipped.Add(card.gameObject);
        GachaRewardSO reward = FindEquippedReward(kind);
        if (reward == null)
        {
            card.Clear();
            card.name = $"Equipped_{emptyLabel}_Empty";
            return;
        }

        int level = inventory.Level(reward);
        card.name = $"Equipped_{reward.name}";
        card.Bind(reward, level, rarityVisuals, true,
            () => EquipReward(reward), false, () => ShowDetails(reward));
    }

    private void RefreshItems()
    {
        ClearGenerated(generatedItems);
        int count = 0;
        foreach (KeyValuePair<GachaRewardSO, int> entry in inventory.Owned)
        {
            GachaRewardSO reward = entry.Key;
            if (reward == null || reward.Kind != currentKind || entry.Value <= 0) continue;
            count++;
            int level = inventory.Level(reward);
            ItemCardUI card = Instantiate(itemCardPrefab, itemContent);
            card.name = $"Item_{reward.name}";
            card.Bind(reward, level, rarityVisuals, IsEquipped(reward),
                () => EquipReward(reward), true, () => ShowDetails(reward));
            generatedItems.Add(card.gameObject);
        }
        emptyText.gameObject.SetActive(count == 0);
    }

    private void ShowDetails(GachaRewardSO reward)
    {
        selectedReward = reward;
        int level = Mathf.Max(1, inventory.Level(reward));
        detailsPanel.Show(reward, level, -1,
            () => EquipRewardFromDetails(reward), IsEquipped(reward));
        RefreshSelection();
    }

    private bool EquipReward(GachaRewardSO reward)
    {
        if (reward == null) return false;
        selectedReward = reward;
        if (IsEquipped(reward))
        {
            selectionText.text = $"{reward.DisplayName.ToUpperInvariant()} YA ESTÁ EQUIPADO";
            return true;
        }

        bool equipped = inventory.TryEquipReward(reward);
        selectionText.text = equipped
            ? $"{reward.DisplayName.ToUpperInvariant()} EQUIPADO"
            : "NO SE PUDO EQUIPAR";
        return equipped;
    }

    private void EquipRewardFromDetails(GachaRewardSO reward)
    {
        if (EquipReward(reward)) detailsPanel.SetEquippedState(true);
    }

    private void RefreshSelection()
    {
        selectionText.text = selectedReward == null
            ? $"SELECCIONÁ UN {RewardPresentationUtility.KindName(currentKind)}"
            : $"{selectedReward.DisplayName.ToUpperInvariant()} · NV. {inventory.Level(selectedReward)}";
    }

    private GachaRewardSO FindEquippedReward(GachaRewardKind kind)
    {
        foreach (KeyValuePair<GachaRewardSO, int> entry in inventory.Owned)
            if (entry.Value > 0 && entry.Key != null && entry.Key.Kind == kind && IsEquipped(entry.Key))
                return entry.Key;
        return null;
    }

    private bool IsEquipped(GachaRewardSO reward)
    {
        if (reward == null) return false;
        switch (reward.Kind)
        {
            case GachaRewardKind.Mecha: return reward.Mecha == inventory.EquippedMecha;
            case GachaRewardKind.Weapon: return reward.Weapon == inventory.EquippedWeapon;
            case GachaRewardKind.Ability: return reward.Ability == inventory.EquippedAbility;
            case GachaRewardKind.Companion: return reward.Companion == inventory.EquippedCompanion;
            default: return false;
        }
    }

    private static void ClearGenerated(List<GameObject> generated)
    {
        foreach (GameObject item in generated)
            if (item != null) Destroy(item);
        generated.Clear();
    }

    private bool HasReferences => equippedContent != null && itemContent != null &&
        itemCardPrefab != null && detailsPanel != null && emptyText != null && selectionText != null &&
        mechaTab != null && weaponTab != null && abilityTab != null &&
        companionTab != null;
}
