using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ItemDetailsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button backdropButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image header;
    [SerializeField] private Image rarityBadge;
    [SerializeField] private Image iconFrame;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private RarityVisualConfigSO rarityVisuals;

    private void Awake()
    {
        if (backdropButton != null) backdropButton.onClick.AddListener(Hide);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    private void OnDestroy()
    {
        if (backdropButton != null) backdropButton.onClick.RemoveListener(Hide);
        if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
    }

    public void Show(GachaRewardSO reward, int level, int previousLevel = -1)
    {
        if (reward == null) return;
        RarityVisualStyle style = rarityVisuals != null ? rarityVisuals.Get(reward.Rarity)
            : new RarityVisualStyle(Color.gray, Color.white, Color.white);
        if (panelRoot != null) panelRoot.SetActive(true);
        if (header != null) header.color = style.Frame;
        if (rarityBadge != null) rarityBadge.color = style.Background;
        if (iconFrame != null) iconFrame.color = style.Frame;
        if (icon != null)
        {
            icon.sprite = reward.Icon;
            icon.color = reward.Icon != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            icon.preserveAspect = true;
        }
        if (nameText != null) nameText.text = reward.DisplayName.ToUpperInvariant();
        if (rarityText != null)
        {
            rarityText.text = $"{RewardPresentationUtility.KindName(reward.Kind)} · {RewardPresentationUtility.RarityName(reward.Rarity)}";
            rarityText.color = Color.white;
        }
        if (levelText != null) levelText.text = $"NIVEL {Mathf.Max(1, level)} / {reward.MaxLevel}";
        if (descriptionText != null) descriptionText.text = reward.Description;
        if (statsText != null) statsText.text = RewardPresentationUtility.BuildStats(reward, level, previousLevel);
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
