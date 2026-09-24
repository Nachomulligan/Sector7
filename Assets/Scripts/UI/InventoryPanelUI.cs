using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text equippedText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private RectTransform itemContent;
    [SerializeField] private Button itemTemplate;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button mechaTab;
    [SerializeField] private Button weaponTab;
    [SerializeField] private Button abilityTab;

    private PlayerInventory inventory;
    private GachaRewardKind currentKind = GachaRewardKind.Mecha;
    private GachaRewardSO selectedReward;
    private readonly List<GameObject> generatedItems = new List<GameObject>();

    private void Start()
    {
        ServiceLocator.Instance.TryGet(out inventory);
        if (inventory == null || !HasReferences)
        {
            Debug.LogWarning("InventoryPanelUI tiene referencias sin asignar.", this);
            return;
        }

        mechaTab.onClick.AddListener(ShowMechas);
        weaponTab.onClick.AddListener(ShowWeapons);
        abilityTab.onClick.AddListener(ShowAbilities);
        equipButton.onClick.AddListener(EquipSelected);
        inventory.OnChanged += Refresh;
        itemTemplate.gameObject.SetActive(false);
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        if (mechaTab != null) mechaTab.onClick.RemoveListener(ShowMechas);
        if (weaponTab != null) weaponTab.onClick.RemoveListener(ShowWeapons);
        if (abilityTab != null) abilityTab.onClick.RemoveListener(ShowAbilities);
        if (equipButton != null) equipButton.onClick.RemoveListener(EquipSelected);
    }

    public void ShowMechas() => SelectKind(GachaRewardKind.Mecha);
    public void ShowWeapons() => SelectKind(GachaRewardKind.Weapon);
    public void ShowAbilities() => SelectKind(GachaRewardKind.Ability);

    private void SelectKind(GachaRewardKind kind)
    {
        currentKind = kind;
        selectedReward = null;
        RefreshItems();
        RefreshDetails();
    }

    private void Refresh()
    {
        equippedText.text = $"Mecha: {NameOf(inventory.EquippedMecha)}   •   " +
            $"Arma: {NameOf(inventory.EquippedWeapon)}   •   " +
            $"Habilidad: {NameOf(inventory.EquippedAbility)}";
        RefreshItems();
        RefreshDetails();
    }

    private void RefreshItems()
    {
        foreach (GameObject item in generatedItems)
            if (item != null) Destroy(item);
        generatedItems.Clear();

        int count = 0;
        foreach (KeyValuePair<GachaRewardSO, int> item in inventory.Owned)
        {
            GachaRewardSO reward = item.Key;
            if (reward == null || reward.Kind != currentKind || item.Value <= 0) continue;
            count++;
            Button card = Instantiate(itemTemplate, itemContent);
            card.name = $"Item_{reward.name}";
            card.gameObject.SetActive(true);
            card.image.color = RarityColor(reward.Rarity);
            TMP_Text label = card.GetComponentInChildren<TMP_Text>(true);
            string suffix = reward.Kind == GachaRewardKind.Mecha
                ? $"  • Asc. {inventory.Ascension(reward.Mecha)}"
                : item.Value > 1 ? $"  • x{item.Value}" : string.Empty;
            label.text = $"{reward.DisplayName}  [{reward.Rarity}]{suffix}";
            GachaRewardSO captured = reward;
            card.onClick.AddListener(() => SelectReward(captured));
            generatedItems.Add(card.gameObject);
        }

        emptyText.gameObject.SetActive(count == 0);
        itemTemplate.transform.SetAsFirstSibling();
    }

    private void SelectReward(GachaRewardSO reward)
    {
        selectedReward = reward;
        RefreshDetails();
    }

    private void EquipSelected()
    {
        if (selectedReward == null) return;
        detailText.text = inventory.TryEquipReward(selectedReward)
            ? $"{selectedReward.DisplayName} equipado" : "No se pudo equipar este objeto";
    }

    private void RefreshDetails()
    {
        equipButton.interactable = selectedReward != null;
        detailText.text = selectedReward == null ? "Elegi un objeto desbloqueado"
            : $"{selectedReward.DisplayName}\n{selectedReward.Rarity}";
    }

    private static string NameOf(Object asset)
    {
        if (asset == null) return "Sin equipar";
        if (asset is MechaPresetSO mecha) return mecha.DisplayName;
        return asset.name.Replace("Weapon_", string.Empty).Replace("Ability_", string.Empty);
    }

    private static Color RarityColor(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Rare: return new Color(0.13f, 0.48f, 0.9f);
            case GachaRarity.Epic: return new Color(0.58f, 0.22f, 0.86f);
            case GachaRarity.Legendary: return new Color(0.95f, 0.55f, 0.12f);
            default: return new Color(0.25f, 0.31f, 0.4f);
        }
    }

    private bool HasReferences => equippedText != null && detailText != null && itemContent != null &&
        itemTemplate != null && emptyText != null && equipButton != null && mechaTab != null &&
        weaponTab != null && abilityTab != null;
}
