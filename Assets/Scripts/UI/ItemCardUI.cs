using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ItemCardUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IPointerMoveHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image frame;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private GameObject equippedBadge;
    [SerializeField] private Sprite fallbackIcon;
    [Header("Interacción táctil")]
    [Min(0.1f)] [SerializeField] private float longPressDuration = 0.45f;
    [Min(1f)] [SerializeField] private float movementCancelDistance = 18f;

    private Action tapAction;
    private Action longPressAction;
    private bool trackingPress;
    private bool longPressTriggered;
    private int trackedPointerId;
    private float pressStartTime;
    private Vector2 pressStartPosition;
    public GachaRewardSO Reward { get; private set; }

    private void Update()
    {
        if (!trackingPress || longPressTriggered || longPressAction == null) return;
        if (Time.unscaledTime - pressStartTime < longPressDuration) return;
        longPressTriggered = true;
        longPressAction.Invoke();
    }

    private void OnDisable() => CancelPress();

    public void Bind(GachaRewardSO reward, int level, RarityVisualConfigSO visuals,
        bool isEquipped, Action onTap, bool showName = true, Action onLongPress = null)
    {
        Reward = reward;
        tapAction = onTap;
        longPressAction = onLongPress;
        CancelPress();
        if (reward == null)
        {
            Clear();
            return;
        }

        RarityVisualStyle style = visuals != null ? visuals.Get(reward.Rarity) : DefaultStyle(reward.Rarity);
        if (background != null) background.color = style.Background;
        if (frame != null) frame.color = style.Frame;
        if (icon != null)
        {
            icon.sprite = reward.Icon != null ? reward.Icon : fallbackIcon;
            icon.color = icon.sprite != null ? reward.IconColor : new Color(1f, 1f, 1f, 0f);
            icon.preserveAspect = true;
        }
        if (nameText != null)
        {
            nameText.gameObject.SetActive(showName);
            nameText.text = reward.DisplayName.ToUpperInvariant();
        }
        if (levelText != null) levelText.text = level > 0 ? $"NV. {level}" : string.Empty;
        if (rarityText != null)
        {
            rarityText.text = RewardPresentationUtility.RarityName(reward.Rarity);
            rarityText.color = style.Text;
        }
        if (equippedBadge != null) equippedBadge.SetActive(isEquipped);
        if (button != null) button.interactable = true;
    }

    public void Clear()
    {
        Reward = null;
        tapAction = null;
        longPressAction = null;
        CancelPress();
        if (icon != null) { icon.sprite = fallbackIcon; icon.color = new Color(1f, 1f, 1f, 0.18f); }
        if (nameText != null) nameText.text = "VACÍO";
        if (levelText != null) levelText.text = string.Empty;
        if (rarityText != null) rarityText.text = string.Empty;
        if (equippedBadge != null) equippedBadge.SetActive(false);
        if (button != null) button.interactable = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Reward == null || button == null || !button.interactable) return;
        trackingPress = true;
        longPressTriggered = false;
        trackedPointerId = eventData.pointerId;
        pressStartTime = Time.unscaledTime;
        pressStartPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!trackingPress || eventData.pointerId != trackedPointerId) return;
        bool shouldTap = !longPressTriggered;
        CancelPress();
        if (shouldTap) tapAction?.Invoke();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!trackingPress || eventData.pointerId != trackedPointerId) return;
        if ((eventData.position - pressStartPosition).sqrMagnitude >
            movementCancelDistance * movementCancelDistance)
            CancelPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (trackingPress && eventData.pointerId == trackedPointerId) CancelPress();
    }

    private void CancelPress()
    {
        trackingPress = false;
        longPressTriggered = false;
        trackedPointerId = int.MinValue;
    }

    private static RarityVisualStyle DefaultStyle(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Rare:
                return new RarityVisualStyle(new Color(0.08f, 0.25f, 0.55f), new Color(0.12f, 0.58f, 1f), new Color(0.35f, 0.78f, 1f));
            case GachaRarity.Epic:
                return new RarityVisualStyle(new Color(0.28f, 0.08f, 0.48f), new Color(0.72f, 0.24f, 1f), new Color(0.82f, 0.52f, 1f));
            case GachaRarity.Legendary:
                return new RarityVisualStyle(new Color(0.48f, 0.22f, 0.04f), new Color(1f, 0.66f, 0.08f), new Color(1f, 0.78f, 0.22f));
            default:
                return new RarityVisualStyle(new Color(0.22f, 0.28f, 0.36f), new Color(0.62f, 0.7f, 0.8f), Color.white);
        }
    }
}
